﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RacingWebScrape.Db;

namespace RacingWebScrape.Db.Migrations
{
    [DbContext(typeof(RacingDbContext))]
    partial class RacingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("RacingWebScrape.Models.CourseMeeting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CourseId");

                    b.Property<DateTime>("MeetingDate");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.ToTable("CourseMeetings");
                });

            modelBuilder.Entity("RacingWebScrape.Models.Courses.Course", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name");

                    b.Property<string>("RaceTitleContentId");

                    b.Property<string>("ResultsContentId");

                    b.HasKey("Id");

                    b.ToTable("Courses");
                });

            modelBuilder.Entity("RacingWebScrape.Models.MeetingResult", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CourseMeetingId");

                    b.Property<string>("FullResultURL");

                    b.Property<DateTime>("RaceTime");

                    b.Property<string>("RaceTitle");

                    b.Property<string>("RunnerInformation");

                    b.Property<string>("WinningJockey");

                    b.Property<string>("WinningTrainer");

                    b.HasKey("Id");

                    b.HasIndex("CourseMeetingId");

                    b.ToTable("MeetingResults");
                });

            modelBuilder.Entity("RacingWebScrape.Models.ResultEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("HorseName");

                    b.Property<string>("HorseNumber");

                    b.Property<string>("JockeyName");

                    b.Property<int>("MeetingResultId");

                    b.Property<string>("Place");

                    b.Property<decimal>("PriceDecimal");

                    b.Property<string>("PriceFraction");

                    b.Property<string>("TrainerName");

                    b.HasKey("Id");

                    b.HasIndex("MeetingResultId");

                    b.ToTable("ResultEntires");
                });

            modelBuilder.Entity("RacingWebScrape.Models.CourseMeeting", b =>
                {
                    b.HasOne("RacingWebScrape.Models.Courses.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RacingWebScrape.Models.MeetingResult", b =>
                {
                    b.HasOne("RacingWebScrape.Models.CourseMeeting", "CourseMeeting")
                        .WithMany("MeetingResults")
                        .HasForeignKey("CourseMeetingId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RacingWebScrape.Models.ResultEntry", b =>
                {
                    b.HasOne("RacingWebScrape.Models.MeetingResult", "MeetingResult")
                        .WithMany("ResultEntries")
                        .HasForeignKey("MeetingResultId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
