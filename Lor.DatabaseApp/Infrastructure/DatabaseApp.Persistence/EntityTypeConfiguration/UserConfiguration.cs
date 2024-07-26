using DatabaseApp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseApp.Persistence.EntityTypeConfiguration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id).HasName("Users_pkey");

        builder.HasIndex(e => e.FullName, "full_name_check").IsUnique();

        builder.HasIndex(e => e.TelegramId, "tg_id_check").IsUnique();

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");
        builder.Property(e => e.FullName)
            .HasDefaultValueSql("0")
            .HasColumnType("character varying")
            .HasColumnName("full_name");
        builder.Property(e => e.TelegramId)
            .HasDefaultValue(0L)
            .HasColumnName("telegramm_id");
    }
}