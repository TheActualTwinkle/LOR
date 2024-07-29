﻿// <auto-generated />
using System;
using DatabaseApp.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DatabaseApp.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DatabaseApp.Domain.Models.Class", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<string>("ClassName")
                        .IsRequired()
                        .HasColumnType("character varying")
                        .HasColumnName("class_name");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date")
                        .HasColumnName("date");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer")
                        .HasColumnName("group_id");

                    b.HasKey("Id")
                        .HasName("Classes_pkey");

                    b.HasIndex("GroupId");

                    b.ToTable("Classes");
                });

            modelBuilder.Entity("DatabaseApp.Domain.Models.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<string>("GroupName")
                        .IsRequired()
                        .HasColumnType("character varying")
                        .HasColumnName("group_name");

                    b.HasKey("Id")
                        .HasName("Groups_pkey");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("DatabaseApp.Domain.Models.Queue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<int>("ClassId")
                        .HasColumnType("integer")
                        .HasColumnName("class_id");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer")
                        .HasColumnName("group_id");

                    b.Property<long>("QueueNum")
                        .HasColumnType("bigint")
                        .HasColumnName("queue_num");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("Queue_pkey");

                    b.HasIndex("ClassId");

                    b.HasIndex("GroupId");

                    b.HasIndex("UserId");

                    b.ToTable("Queue", (string)null);
                });

            modelBuilder.Entity("DatabaseApp.Domain.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("character varying")
                        .HasColumnName("full_name");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer");

                    b.Property<long>("TelegramId")
                        .HasColumnType("bigint")
                        .HasColumnName("telegram_id");

                    b.HasKey("Id")
                        .HasName("Users_pkey");

                    b.HasIndex("GroupId");

                    b.HasIndex(new[] { "FullName" }, "full_name_check")
                        .IsUnique();

                    b.HasIndex(new[] { "TelegramId" }, "tg_id_check")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DatabaseApp.Domain.Models.Class", b =>
                {
                    b.HasOne("DatabaseApp.Domain.Models.Group", "Group")
                        .WithMany("Classes")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("DatabaseApp.Domain.Models.Queue", b =>
                {
                    b.HasOne("DatabaseApp.Domain.Models.Class", "Class")
                        .WithMany("Queues")
                        .HasForeignKey("ClassId")
                        .IsRequired()
                        .HasConstraintName("Queue_classes_id_fkey");

                    b.HasOne("DatabaseApp.Domain.Models.Group", "Group")
                        .WithMany("Queues")
                        .HasForeignKey("GroupId")
                        .IsRequired()
                        .HasConstraintName("Queue_group_id_fkey");

                    b.HasOne("DatabaseApp.Domain.Models.User", "User")
                        .WithMany("Queues")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("Queue_user_id_fkey");

                    b.Navigation("Class");

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DatabaseApp.Domain.Models.User", b =>
                {
                    b.HasOne("DatabaseApp.Domain.Models.Group", "Group")
                        .WithMany("Users")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("DatabaseApp.Domain.Models.Class", b =>
                {
                    b.Navigation("Queues");
                });

            modelBuilder.Entity("DatabaseApp.Domain.Models.Group", b =>
                {
                    b.Navigation("Classes");

                    b.Navigation("Queues");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("DatabaseApp.Domain.Models.User", b =>
                {
                    b.Navigation("Queues");
                });
#pragma warning restore 612, 618
        }
    }
}
