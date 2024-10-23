﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Persistence;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241023190645_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("Application.Common.OutboxMessage", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("TEXT")
                        .HasColumnName("content");

                    b.Property<string>("Error")
                        .HasMaxLength(1024)
                        .HasColumnType("TEXT")
                        .HasColumnName("error");

                    b.Property<DateTime>("OccuredOnUtc")
                        .HasColumnType("TEXT")
                        .HasColumnName("occured_on_utc");

                    b.Property<DateTime?>("ProcessedOnUtc")
                        .HasColumnType("TEXT")
                        .HasColumnName("processed_on_utc");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("p_k_outbox_message");

                    b.ToTable("outbox_message");
                });

            modelBuilder.Entity("Domain.Aggregates.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("TEXT")
                        .HasColumnName("created_by");

                    b.Property<DateTime?>("Deleted")
                        .HasColumnType("TEXT")
                        .HasColumnName("deleted");

                    b.Property<string>("DeletedBy")
                        .HasColumnType("TEXT")
                        .HasColumnName("deleted_by");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT")
                        .HasColumnName("email");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("TEXT")
                        .HasColumnName("last_modified");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("TEXT")
                        .HasColumnName("last_modified_by");

                    b.Property<decimal>("Money")
                        .HasColumnType("TEXT")
                        .HasColumnName("money");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT")
                        .HasColumnName("password_hash");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT")
                        .HasColumnName("username");

                    b.HasKey("Id")
                        .HasName("p_k_user");

                    b.ToTable("user");
                });
#pragma warning restore 612, 618
        }
    }
}
