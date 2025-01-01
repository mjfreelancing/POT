﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pot.Data;

#nullable disable

namespace Pot.Data.Migrations
{
    [DbContext(typeof(PotDbContext))]
    partial class PotDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Pot.Data.Entities.AccountEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<double>("Allocated")
                        .HasColumnType("double precision");

                    b.Property<double>("Balance")
                        .HasColumnType("double precision");

                    b.Property<string>("Bsb")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("character varying(7)");

                    b.Property<double>("DailyAccrual")
                        .HasColumnType("double precision");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<long>("Etag")
                        .HasColumnType("bigint");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<double>("Reserved")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.HasIndex("Description")
                        .IsUnique();

                    b.ToTable("Account");
                });

            modelBuilder.Entity("Pot.Data.Entities.ExpenseEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("integer");

                    b.Property<DateOnly>("AccrualStart")
                        .HasColumnType("date");

                    b.Property<double>("Allocated")
                        .HasColumnType("double precision");

                    b.Property<double>("Amount")
                        .HasColumnType("double precision");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<long>("Etag")
                        .HasColumnType("bigint");

                    b.Property<string>("Frequency")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("FrequencyCount")
                        .HasColumnType("integer");

                    b.Property<DateOnly>("NextDue")
                        .HasColumnType("date");

                    b.Property<bool>("Recurring")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("Description")
                        .IsUnique();

                    b.HasIndex("NextDue");

                    b.ToTable("Expense");
                });

            modelBuilder.Entity("Pot.Data.Entities.ExpenseEntity", b =>
                {
                    b.HasOne("Pot.Data.Entities.AccountEntity", "Account")
                        .WithMany("Expenses")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Pot.Data.Entities.AccountEntity", b =>
                {
                    b.Navigation("Expenses");
                });
#pragma warning restore 612, 618
        }
    }
}
