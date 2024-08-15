using DatabaseApp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseApp.Persistence.EntityTypeConfiguration;

public class QueueConfiguration : IEntityTypeConfiguration<Queue>
{
    public void Configure(EntityTypeBuilder<Queue> builder)
    {
        builder.ToTable("QUEUES");
        
        builder.HasKey(e => e.Id).HasName("Queue_pkey");

        builder.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        builder.Property(e => e.ClassId)
            .HasColumnName("class_id");
        builder.Property(e => e.UserId)
            .HasColumnName("user_id");
        builder.Property(e => e.QueueNum)
            .HasColumnName("queue_num");

        builder.HasOne(q => q.Class).WithMany(c => c.Queues)
            .HasPrincipalKey(c => c.Id)
            .HasForeignKey(q => q.ClassId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Queue_classes_id_fkey");

        builder.HasOne(q => q.User).WithMany(u => u.Queues)
            .HasPrincipalKey(u => u.Id)
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Queue_user_id_fkey");
    }
}