﻿// <auto-generated />
using System;
using Domain.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DataAccessEF.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Domain.Models.Competitor", b =>
                {
                    b.Property<int>("CompetitorId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CompetitorId"));

                    b.Property<int>("CountryId")
                        .HasColumnType("int");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.HasKey("CompetitorId");

                    b.HasIndex("CountryId");

                    b.HasIndex("TeamId");

                    b.ToTable("Competitors", (string)null);
                });

            modelBuilder.Entity("Domain.Models.CompetitorsInEvent", b =>
                {
                    b.Property<int>("CompetitorInEventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CompetitorInEventId"));

                    b.Property<int>("CompetitorId")
                        .HasColumnType("int");

                    b.Property<int>("EventId")
                        .HasColumnType("int");

                    b.Property<int>("EventNumber")
                        .HasColumnType("int");

                    b.HasKey("CompetitorInEventId");

                    b.HasIndex("CompetitorId");

                    b.HasIndex("EventId");

                    b.ToTable("CompetitorsInEvent", (string)null);
                });

            modelBuilder.Entity("Domain.Models.Configuration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ConfigurationType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Configurations", (string)null);
                });

            modelBuilder.Entity("Domain.Models.ConfigurationItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ConfigurationId")
                        .HasColumnType("int");

                    b.Property<int>("Position")
                        .HasColumnType("int");

                    b.Property<int>("Score")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ConfigurationId");

                    b.ToTable("ConfigurationItems", (string)null);
                });

            modelBuilder.Entity("Domain.Models.Country", b =>
                {
                    b.Property<int>("CountryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CountryId"));

                    b.Property<string>("CountryNameLong")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CountryNameShort")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CountryId");

                    b.ToTable("Country", (string)null);
                });

            modelBuilder.Entity("Domain.Models.Event", b =>
                {
                    b.Property<int>("EventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("EventId"));

                    b.Property<int?>("ConfigurationId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("EventName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EventYear")
                        .HasColumnType("int");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("datetime2");

                    b.HasKey("EventId");

                    b.HasIndex("ConfigurationId");

                    b.ToTable("Events", (string)null);
                });

            modelBuilder.Entity("Domain.Models.Result", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CompetitorId")
                        .HasColumnType("int");

                    b.Property<int?>("ConfigurationItemId")
                        .HasColumnType("int");

                    b.Property<int>("StageId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CompetitorId");

                    b.HasIndex("ConfigurationItemId");

                    b.HasIndex("StageId");

                    b.ToTable("Results", (string)null);
                });

            modelBuilder.Entity("Domain.Models.Stage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("EventId")
                        .HasColumnType("int");

                    b.Property<string>("FinishLocation")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StageName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("StageOrder")
                        .HasColumnType("int");

                    b.Property<string>("StartLocation")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.ToTable("Stages", (string)null);
                });

            modelBuilder.Entity("Domain.Models.Team", b =>
                {
                    b.Property<int>("TeamId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TeamId"));

                    b.Property<int?>("CountryId")
                        .HasColumnType("int");

                    b.Property<string>("TeamName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TeamId");

                    b.HasIndex("CountryId");

                    b.ToTable("Teams", (string)null);
                });

            modelBuilder.Entity("Domain.Models.Competitor", b =>
                {
                    b.HasOne("Domain.Models.Country", "Country")
                        .WithMany()
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Models.Team", "Team")
                        .WithMany("Competitors")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Country");

                    b.Navigation("Team");
                });

            modelBuilder.Entity("Domain.Models.CompetitorsInEvent", b =>
                {
                    b.HasOne("Domain.Models.Competitor", "Competitor")
                        .WithMany()
                        .HasForeignKey("CompetitorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Models.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Competitor");

                    b.Navigation("Event");
                });

            modelBuilder.Entity("Domain.Models.ConfigurationItem", b =>
                {
                    b.HasOne("Domain.Models.Configuration", "Configuration")
                        .WithMany()
                        .HasForeignKey("ConfigurationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Configuration");
                });

            modelBuilder.Entity("Domain.Models.Event", b =>
                {
                    b.HasOne("Domain.Models.Configuration", "Configuration")
                        .WithMany()
                        .HasForeignKey("ConfigurationId");

                    b.Navigation("Configuration");
                });

            modelBuilder.Entity("Domain.Models.Result", b =>
                {
                    b.HasOne("Domain.Models.Competitor", "Competitor")
                        .WithMany()
                        .HasForeignKey("CompetitorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Models.ConfigurationItem", "ConfigurationItem")
                        .WithMany()
                        .HasForeignKey("ConfigurationItemId");

                    b.HasOne("Domain.Models.Stage", "Stage")
                        .WithMany()
                        .HasForeignKey("StageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Competitor");

                    b.Navigation("ConfigurationItem");

                    b.Navigation("Stage");
                });

            modelBuilder.Entity("Domain.Models.Stage", b =>
                {
                    b.HasOne("Domain.Models.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");
                });

            modelBuilder.Entity("Domain.Models.Team", b =>
                {
                    b.HasOne("Domain.Models.Country", "Country")
                        .WithMany()
                        .HasForeignKey("CountryId");

                    b.Navigation("Country");
                });

            modelBuilder.Entity("Domain.Models.Team", b =>
                {
                    b.Navigation("Competitors");
                });
#pragma warning restore 612, 618
        }
    }
}
