namespace Core.Domain.Enums
{
    public enum NotificationStates
    {
        Queued = 1,
        Processing,
        Submitted,
        Retry,
        Cancelled,
        Failed,
        Sent
    }
}
