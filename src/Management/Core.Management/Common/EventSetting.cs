namespace Core.Management.Common
{
    public class EventSetting
    {
        public string ServiceUrl { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
        public string QueueUrl { get; set; }
        public int BatchSize { get; set; }
    }
}
