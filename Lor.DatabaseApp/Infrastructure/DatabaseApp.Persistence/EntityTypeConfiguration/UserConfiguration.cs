using DatabaseApp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseApp.Persistence.EntityTypeConfiguration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("USERS");
        
        builder.HasKey(e => e.Id).HasName("Users_pkey");

        builder.HasIndex(e => e.FullName, "full_name_check").IsUnique();

        builder.HasIndex(e => e.TelegramId, "tg_id_check").IsUnique();

        builder.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        builder.Property(e => e.FullName)
            .HasColumnName("full_name");
        builder.Property(e => e.TelegramId)
            .HasColumnName("telegram_id");
        builder.Property(e => e.GroupId)
            .HasColumnName("group_id");
        
        builder.HasOne(u => u.Group).WithMany(g => g.Users)
            .HasPrincipalKey(g => g.Id)
            .HasForeignKey(u => u.GroupId)
            .HasConstraintName("User_group_id_fkey");
    }
}