using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using IdGen;
using Dapper;
using Newtonsoft.Json;

using Core.Domain.Enums;
using Core.Domain.Entities;
using Core.Management.Common;
using Core.Domain.Infrastructure.Database;
using Core.Domain.Infrastructure.Services;

namespace Core.Management.Infrastructure.IntegrationEvents.EventHandling
{
    public class NotificationHandler : INotificationHandler
    {
        private readonly HttpClient _client;
        private readonly IConnection _connection;
        private readonly NotificationSetting _setting;
        private readonly IIdGenerator<long> _idGenerator;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<NotificationHandler> _logger;

        public NotificationHandler(IConnection connection, IDateTimeService dateTimeService, IOptions<NotificationSetting> setting, IIdGenerator<long> idGenerator, ILogger<NotificationHandler> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _setting = setting.Value;
            _connection = connection;
            _idGenerator = idGenerator;
            _dateTimeService = dateTimeService;
            _client = httpClientFactory.CreateClient();
        }

        public async Task Handle()
        {
            using SqlConnection sqlConnection = new SqlConnection(_connection.ConnectionString);
            long bucketId = _idGenerator.CreateId();

            //Create processing bucket
            int allocatedBucketSize = await sqlConnection.ExecuteAsync(Queries.BATCH_UNPROCESSED_NOTIFICATIONS, new
            {
                batchSize = _setting.BatchSize,
                bucketId = 0,
                queuedStatusId = (int)NotificationStates.Queued,
                processingStatusId = (int)NotificationStates.Processing,
                processingId = bucketId
            }).ConfigureAwait(false);

            if (allocatedBucketSize < 1) return;

            //Fetch processing bucket
            List<Notification> notifications = (await sqlConnection.QueryAsync<Notification>(Queries.GET_BATCHED_NOTIFICATIONS, new { bucketId }).ConfigureAwait(false)).ToList();
            if (notifications.Count < 1)
            {
                _logger.LogWarning($"{nameof(NotificationHandler) } unable to fetch bucket {bucketId}");
                return;
            }

            //Queue to service provider
            (List<(long Id, string ExternalId, string Message)> Successful, List<(long Id, string ExternalId, string Error)> Failed) = await QueueBatchedNotifications(notifications).ConfigureAwait(false);

            if (Successful is { } successfulEvents)
            {
                foreach ((long Id, string ExternalId, string Message) in successfulEvents)
                {
                    try
                    {
                        await sqlConnection.ExecuteAsync(Queries.UPDATE_SUCCESSFUL_NOTIFICATIONS, new
                        {
                            submittedStatusId = (int)NotificationStates.Submitted,
                            numberOfSends = 1,
                            queueId = ExternalId,
                            bucketId = 0,
                            message = Message,
                            modifiedAt = _dateTimeService.Now,
                            notificationId = Id
                        }).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating successfully queued notification {Id} : {ExternalId}");
                    }
                }

            }

            if (Failed is { } failedEvents)
            {
                if (failedEvents.Count < 1) return;

                _logger.LogCritical($"Failed notifications for action - {JsonConvert.SerializeObject(failedEvents)}");

                foreach ((long Id, string ExternalId, string Error) in failedEvents)
                {
                    try
                    {
                        await sqlConnection.ExecuteAsync(Queries.UPDATE_FAILED_NOTIFICATIONS, new
                        {
                            failedStatusId = (int)NotificationStates.Retry,
                            numberOfSends = 1,
                            bucketId = 0,
                            error = Error,
                            modifiedAt = _dateTimeService.Now,
                            notificationId = Id
                        }).ConfigureAwait(false);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating failed and queued notification {Id} : {Error}");
                    }
                }
            }
        }

        private async Task<(List<(long Id, string ExternalId, string Message)> Successful, List<(long Id, string ExternalId, string Error)> Failed)> QueueBatchedNotifications(List<Notification> notifications)
        {
            List<(long Id, string ExternalId, string Message)> successful = new List<(long Id, string ExternalId, string Message)>();
            List<(long Id, string ExternalId, string Error)> failed = new List<(long Id, string ExternalId, string Error)>();

            foreach (Notification notification in notifications)
            {
                switch (notification.InformationMode)
                {
                    case InformationModes.None:
                        continue;
                    case InformationModes.Sms:
                        {
                            (bool success, string externalId, string message) = await SendSms(notification).ConfigureAwait(false);
                            if (success) successful.Add((notification.NotificationId, externalId, message));
                            else failed.Add((notification.NotificationId, externalId, message));

                            continue;
                        }
                    case InformationModes.Email:
                        {
                            (bool success, string externalId, string message) = string.IsNullOrEmpty(notification.Email) ? await SendSms(notification).ConfigureAwait(false) : await SendEmail(notification).ConfigureAwait(false);
                            if (success) successful.Add((notification.NotificationId, externalId, message));
                            else failed.Add((notification.NotificationId, externalId, message));

                            continue;
                        }
                    case InformationModes.SmsAndEmail:
                        {
                            if (string.IsNullOrEmpty(notification.Email))
                            {
                                (bool success, string externalId, string message) = await SendSms(notification).ConfigureAwait(false);
                                if (success) successful.Add((notification.NotificationId, externalId, message));
                                else failed.Add((notification.NotificationId, externalId, message));

                                continue;
                            }

                            Task<(bool success, string externalId, string message)> smsTask = SendSms(notification);
                            Task<(bool success, string externalId, string message)> emailTask = SendEmail(notification);

                            (bool success, string externalId, string message) sms = await smsTask.ConfigureAwait(false);
                            (bool success, string externalId, string message) email = await emailTask.ConfigureAwait(false);

                            if (sms.success) successful.Add((notification.NotificationId, $"S:{sms.externalId}|E:{email.externalId}", $"S:{sms.message}|E:{email.message}"));
                            else failed.Add((notification.NotificationId, $"S:{sms.externalId}|E:{email.externalId}", $"S:{sms.message}|E:{email.message}"));

                            continue;
                        }
                }
            }

            return (successful, failed);
        }

        private async Task<(bool success, string externalId, string message)> SendSms(Notification notification)
        {
            string reasonPhrase = "";
            try
            {
                string smsObject = JsonConvert.SerializeObject(new
                {
                    phone = notification.Msisdn.ToString(),
                    message = notification.TextBody,
                    senderID = _setting.SenderId,
                    priority = notification.Priority,
                    product = "ipn",
                    notificationURL = _setting.NotificationUrl,
                    externalId = notification.NotificationId.ToString()
                });

                HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(_setting.Sms),
                    Method = HttpMethod.Post,
                    Content = new StringContent(smsObject, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("x-app-id", _setting.Key);

                HttpResponseMessage responseMessage = await _client.SendAsync(request);
                reasonPhrase = $"SMS Notification Id: {notification.NotificationId}|{responseMessage?.ReasonPhrase}";
                responseMessage.EnsureSuccessStatusCode();

                dynamic result = JsonConvert.DeserializeObject<dynamic>(await responseMessage.Content.ReadAsStringAsync());
                if (result is null) return (false, null, reasonPhrase);

                return (result.success, result?.data?.id, result?.message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Exception while queueing SMS to gateway : {reasonPhrase}");
                return (false, null, ex?.Message);
            }
        }

        private async Task<(bool success, string externalId, string message)> SendEmail(Notification notification)
        {
            string reasonPhrase = "";
            try
            {
                string emailObject = JsonConvert.SerializeObject(new
                {
                    to = notification.Email,
                    cc = notification.InCopyRecipients,
                    from = _setting.SenderMail,
                    message = notification.TextBody,
                    subject = notification.Subject,
                    priority = notification.Priority,
                    product = "ipn",
                    notificationURL = _setting.NotificationUrl,
                    externalId = notification.NotificationId.ToString()
                });

                HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(_setting.PlainMail),
                    Method = HttpMethod.Post,
                    Content = new StringContent(emailObject, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("x-app-id", _setting.Key);

                HttpResponseMessage responseMessage = await _client.SendAsync(request);
                reasonPhrase = $"Email Notification Id: {notification.NotificationId}|{responseMessage?.ReasonPhrase}";
                responseMessage.EnsureSuccessStatusCode();

                dynamic result = JsonConvert.DeserializeObject<dynamic>(await responseMessage.Content.ReadAsStringAsync());
                if (result is null) return (false, null, reasonPhrase);

                return (result.success, result?.data?.id, result?.message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while queueing Email to gateway : {reasonPhrase}");
                return (false, null, ex?.Message);
            }
        }       

        
    }
}
