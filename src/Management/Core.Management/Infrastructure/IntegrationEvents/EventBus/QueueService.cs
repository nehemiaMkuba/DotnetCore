using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

using Amazon.SQS;
using Amazon.SQS.Model;

using Core.Management.Common;

namespace Core.Management.Infrastructure.IntegrationEvents.EventBus
{
    public class QueueService : IQueueService
    {
        private readonly EventSetting _setting;
        private readonly AmazonSQSClient _sqsClient;
        private readonly ILogger<QueueService> logger;

        //public QueueService(ILogger<QueueService> logger, IOptions<EventSetting> options, AmazonSQSClient sqsClient)
        //{
        //    this.logger = logger;
        //    _setting = options.Value;
        //    this.sqsClient = sqsClient;
        //}

        public async Task<(bool successful, string messageId)> EnqueueMessage(dynamic payload)
        {
            try
            {
                SendMessageResponse response = await _sqsClient.SendMessageAsync(_setting.QueueUrl,
                    JsonSerializer.Serialize(payload, options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } })).ConfigureAwait(false);

                return (response.HttpStatusCode == HttpStatusCode.OK, response.MessageId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error queueing action event - {ex?.Message}");
                throw;
            }
        }
    }
}
