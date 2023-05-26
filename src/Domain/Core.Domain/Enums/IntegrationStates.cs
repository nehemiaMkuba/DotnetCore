namespace Core.Domain.Enums
{
    public enum IntegrationStates
    {
        Pending = 1,
        Processing,
        Published,
        Retry,
        Failed,
        Complete      
    }
}
