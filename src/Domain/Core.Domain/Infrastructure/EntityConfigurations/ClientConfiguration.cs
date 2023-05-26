using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Domain.Infrastructure.EntityConfigurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients", nameof(Schemas.IPN));
            builder.HasKey(x => x.ClientId);

            builder.Property(x => x.ClientId).ValueGeneratedNever();
            builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Secret).HasMaxLength(128);
            builder.Property(x => x.Description).HasMaxLength(250);
            builder.Property(x => x.ContactEmail).HasMaxLength(150);
        }
    }
}