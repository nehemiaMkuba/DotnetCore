using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Core.Domain.Enums;
using Core.Domain.Entities;

namespace Core.Domain.Infrastructure.EntityConfigurations
{
    public class SettingConfiguration : IEntityTypeConfiguration<Setting>
    {
        public void Configure(EntityTypeBuilder<Setting> builder)
        {
            builder.ToTable("Settings", nameof(Schemas.IPN));
            builder.HasKey(x => x.SettingId);

            builder.Property(x => x.SettingId).UseHiLo($"{nameof(Setting)}_HiLo", schema: nameof(Schemas.IPN)).IsRequired();
            builder.Property(x => x.Key).HasMaxLength(50).IsRequired();

            builder.HasIndex(x => x.Key).IsUnique();
        }
    }
}