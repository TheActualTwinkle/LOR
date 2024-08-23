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

        builder.HasIndex(s => s.UserId, "tg_id_check").IsUnique();

        builder.Property(s => s.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        builder.Property(s => s.UserId)
            .HasColumnName("user_id");

        builder.HasOne(s => s.User)
            .WithOne(u => u.Subscriber)
            .HasForeignKey<Subscriber>(s => s.UserId)
            .HasConstraintName("Subscriber_user_id_fkey");
    }
}