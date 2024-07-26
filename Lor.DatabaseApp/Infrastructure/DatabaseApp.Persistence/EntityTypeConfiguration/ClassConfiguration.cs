﻿using DatabaseApp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseApp.Persistence.EntityTypeConfiguration;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.HasKey(e => e.Id).HasName("Classes_pkey");

        builder.HasIndex(e => e.GroupId, "Group_check").IsUnique();

        builder.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        builder.Property(e => e.ClassName)
            .HasColumnType("character varying")
            .HasColumnName("class_name");
        builder.Property(e => e.Date).HasColumnName("date");
        builder.Property(e => e.GroupId).HasColumnName("group_id");
    }
}