using DatabaseApp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseApp.Persistence.EntityTypeConfiguration;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("CLASSES");
        
        builder.HasKey(e => e.Id).HasName("Classes_pkey");
        
        builder.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        builder.Property(e => e.GroupId)
            .HasColumnName("group_id");
        builder.Property(e => e.ClassName)
            .HasColumnName("class_name");
        builder.Property(e => e.Date)
            .HasColumnName("date");
    }
}