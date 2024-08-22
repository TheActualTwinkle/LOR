using DatabaseApp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseApp.Persistence.EntityTypeConfiguration;

public class SubscriberConfiguration : IEntityTypeConfiguration<Subscriber>
{
    public void Configure(EntityTypeBuilder<Subscriber> builder)
    {
        builder.ToTable("SUBSCRIBERS");
        
        builder.HasKey(s => s.Id).HasName("Subscriber_pkey");

        builder.HasIndex(s => s.TelegramId, "tg_id_check").IsUnique();

        builder.Property(s => s.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        builder.Property(s => s.TelegramId)
            .HasColumnName("telegram_id");

        builder.HasOne(s => s.User).WithOne(u => u.Subscriber)
            .HasPrincipalKey<User>(u => u.TelegramId)
            .HasForeignKey<Subscriber>(s => s.TelegramId)
            .HasConstraintName("Subscriber_user_id_fkey");
    }
}