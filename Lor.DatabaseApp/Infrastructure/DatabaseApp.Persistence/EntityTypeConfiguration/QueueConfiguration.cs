using DatabaseApp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseApp.Persistence.EntityTypeConfiguration;

public class QueueConfiguration : IEntityTypeConfiguration<Queue>
{
    public void Configure(EntityTypeBuilder<Queue> builder)
    {
        builder.HasKey(e => e.Id).HasName("Queue_pkey");

        builder.ToTable("Queue");

        builder.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        builder.Property(e => e.ClassId).HasColumnName("class_id");
        builder.Property(e => e.GroupId).HasColumnName("group_id");
        builder.Property(e => e.QueueNum)
            .HasColumnName("queue_num");
        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.HasOne(d => d.Class).WithMany(p => p.Queues)
            .HasPrincipalKey(p => p.Id)
            .HasForeignKey(d => d.ClassId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Queue_classes_id_fkey");

        builder.HasOne(d => d.Group).WithMany(p => p.Queues)
            .HasForeignKey(d => d.GroupId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Queue_group_id_fkey");

        builder.HasOne(d => d.User).WithMany(p => p.Queues)
            .HasPrincipalKey(p => p.Id)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Queue_user_id_fkey");
    }
}