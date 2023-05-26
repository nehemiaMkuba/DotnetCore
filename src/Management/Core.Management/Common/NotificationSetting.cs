namespace Core.Management.Common
{
    public class NotificationSetting
    {
        public int BatchSize { get; set; }
        public string SenderId { get; set; }
        public string Sms { get; set; }
        public string PlainMail { get; set; }
        public string HtmlMail { get; set; }
        public string SenderMail { get; set; }
        public string Key { get; set; }
        public string NotificationUrl { get; set; }
    }
}
