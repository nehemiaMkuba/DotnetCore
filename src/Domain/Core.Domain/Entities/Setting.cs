using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class Setting : AuditableEntity
    {
        public int SettingId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}