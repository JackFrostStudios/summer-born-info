﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SummerBorn.Infrastructure.Data;

#nullable disable

namespace SummerBorn.Infrastructure.Migrations
{
    [DbContext(typeof(SchoolContext))]
    [Migration("20241024214656_InitialSchema")]
    partial class InitialSchema
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SummerBorn.Core.Entity.Address", b =>
                {
                    b.Property<Guid>("SchoolId")
                        .HasColumnType("uuid")
                        .HasColumnName("Id");

                    b.Property<string>("AddressThree")
                        .HasColumnType("text");

                    b.Property<string>("County")
                        .HasColumnType("text");

                    b.Property<string>("Locality")
                        .HasColumnType("text");

                    b.Property<string>("PostCode")
                        .HasColumnType("text");

                    b.Property<string>("Street")
                        .HasColumnType("text");

                    b.Property<string>("Town")
                        .HasColumnType("text");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("SchoolId");

                    b.HasIndex("AddressThree");

                    b.HasIndex("County");

                    b.HasIndex("Locality");

                    b.HasIndex("PostCode");

                    b.HasIndex("Street");

                    b.HasIndex("Town");

                    b.ToTable("school", (string)null);
                });

            modelBuilder.Entity("SummerBorn.Core.Entity.Establishment.EstablishmentGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Code")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.ToTable("establishment_group", (string)null);
                });

            modelBuilder.Entity("SummerBorn.Core.Entity.Establishment.EstablishmentStatus", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Code")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.ToTable("establishment_status", (string)null);
                });

            modelBuilder.Entity("SummerBorn.Core.Entity.Establishment.EstablishmentType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Code")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.ToTable("establishment_type", (string)null);
                });

            modelBuilder.Entity("SummerBorn.Core.Entity.LocalAuthority", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Code")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.ToTable("local_authority", (string)null);
                });

            modelBuilder.Entity("SummerBorn.Core.Entity.PhaseOfEducation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Code")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.ToTable("phase_of_education", (string)null);
                });

            modelBuilder.Entity("SummerBorn.Core.Entity.School", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateOnly>("CloseDate")
                        .HasColumnType("date");

                    b.Property<Guid>("EstablishmentGroupId")
                        .HasColumnType("uuid");

                    b.Property<int>("EstablishmentNumber")
                        .HasColumnType("integer");

                    b.Property<Guid>("EstablishmentStatusId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("EstablishmentTypeId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("LocalAuthorityId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<DateOnly>("OpenDate")
                        .HasColumnType("date");

                    b.Property<Guid>("PhaseOfEducationId")
                        .HasColumnType("uuid");

                    b.Property<int>("UKPRN")
                        .HasColumnType("integer");

                    b.Property<int>("URN")
                        .HasColumnType("integer");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.HasIndex("EstablishmentGroupId");

                    b.HasIndex("EstablishmentNumber");

                    b.HasIndex("EstablishmentStatusId");

                    b.HasIndex("EstablishmentTypeId");

                    b.HasIndex("LocalAuthorityId");

                    b.HasIndex("Name");

                    b.HasIndex("PhaseOfEducationId");

                    b.HasIndex("UKPRN");

                    b.HasIndex("URN");

                    b.ToTable("school", (string)null);
                });

            modelBuilder.Entity("SummerBorn.Core.Entity.Address", b =>
                {
                    b.HasOne("SummerBorn.Core.Entity.School", null)
                        .WithOne("Address")
                        .HasForeignKey("SummerBorn.Core.Entity.Address", "SchoolId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("SummerBorn.Core.Entity.School", b =>
                {
                    b.HasOne("SummerBorn.Core.Entity.Establishment.EstablishmentGroup", "EstablishmentGroup")
                        .WithMany()
                        .HasForeignKey("EstablishmentGroupId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("SummerBorn.Core.Entity.Establishment.EstablishmentStatus", "EstablishmentStatus")
                        .WithMany()
                        .HasForeignKey("EstablishmentStatusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("SummerBorn.Core.Entity.Establishment.EstablishmentType", "EstablishmentType")
                        .WithMany()
                        .HasForeignKey("EstablishmentTypeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("SummerBorn.Core.Entity.LocalAuthority", "LocalAuthority")
                        .WithMany()
                        .HasForeignKey("LocalAuthorityId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("SummerBorn.Core.Entity.PhaseOfEducation", "PhaseOfEducation")
                        .WithMany()
                        .HasForeignKey("PhaseOfEducationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("EstablishmentGroup");

                    b.Navigation("EstablishmentStatus");

                    b.Navigation("EstablishmentType");

                    b.Navigation("LocalAuthority");

                    b.Navigation("PhaseOfEducation");
                });

            modelBuilder.Entity("SummerBorn.Core.Entity.School", b =>
                {
                    b.Navigation("Address")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
