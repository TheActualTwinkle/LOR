using DatabaseApp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseApp.Persistence.EntityTypeConfiguration;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("GROUPS");
        
        builder.HasKey(e => e.Id).HasName("Groups_pkey");

        builder.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        builder.Property(e => e.Name)
            .HasColumnName("name");
    }
}