using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Core.Domain.Enums;
using Core.Domain.Entities;

namespace Core.Domain.Infrastructure.EntityConfigurations
{

    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications", nameof(Schemas.IPN));
            builder.HasKey(x => x.NotificationId);

            builder.Property(x => x.NotificationId).ValueGeneratedNever();
            builder.Property(x => x.Email).HasMaxLength(100);
            builder.Property(x => x.QueueId).HasMaxLength(100);
            builder.Property(x => x.SenderId).HasMaxLength(50);
            builder.Property(x => x.ProviderId).HasMaxLength(100);

            builder.Property(x => x.NotificationStatus).HasConversion<string>().HasMaxLength(15);
            builder.Property(x => x.InformationMode).HasConversion<string>().HasMaxLength(15);
        }
    }
}