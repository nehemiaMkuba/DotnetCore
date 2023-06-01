namespace Core.Management.Infrastructure
{
    public partial class Queries
    {
        public const string GET_ENTITY_BY_COLUMN_NAME = "SELECT * FROM gospel.{EntityName} WHERE {ColumnName} = @value";
        public const string GET_ENTITY_COLUMN = "SELECT {Column} FROM gospel.{EntityName} WHERE {ColumnName} = @value";

        public const string BATCH_UNPROCESSED_NOTIFICATIONS = "WITH CTE(NotificationId) AS " +
       "(SELECT TOP(@batchSize) NotificationId FROM IPN.Notifications WHERE NotificationStatusId = @queuedStatusId AND BucketId = @bucketId ORDER BY NotificationId ASC) " +
       "UPDATE IPN.Notifications SET BucketId = @processingId, NotificationStatusId = @processingStatusId FROM CTE WHERE Notifications.NotificationId = CTE.NotificationId";
        public const string GET_BATCHED_NOTIFICATIONS = "SELECT NotificationId, Msisdn, TextBody, SenderId, Priority, InformationModeId, Email, InCopyRecipients, Subject FROM IPN.Notifications WHERE BucketId = @bucketId ORDER BY NotificationId ASC";
        public const string UPDATE_SUCCESSFUL_NOTIFICATIONS = "UPDATE IPN.Notifications SET NotificationStatusId = @submittedStatusId, QueueId = @queueId, ProviderId = @queueId, NumberOfSends += @numberOfSends, ErrorMessage = @message, BucketId = @bucketId, QueuedAt = @modifiedAt, ModifiedAt = @modifiedAt WHERE NotificationId = @notificationId";
        public const string UPDATE_FAILED_NOTIFICATIONS = "UPDATE IPN.Notifications SET NotificationStatusId = @failedStatusId, NumberOfSends += @numberOfSends, ErrorMessage = @error, BucketId = @bucketId, QueuedAt = @modifiedAt, ModifiedAt = @modifiedAt WHERE NotificationId = @notificationId";

    }
}
