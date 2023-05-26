using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class Language : AuditableEntity
    {
        public byte LanguageId { get; set; }
        public string Name { get; set; }
        public string IsoCode { get; set; }
        public bool Default { get; set; } = false;
        public bool IsActive { get; set; } = false;
    }
}