using RacingWebScrape.Models;
using RacingWebScrape.Models.Courses;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace RacingWebScrape.Db
{
    public class RacingDbContext : DbContext
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseMeeting> CourseMeetings { get; set; }
        public DbSet<MeetingResult> MeetingResults { get; set; }
        public DbSet<ResultEntry> ResultEntires { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string cn = @"Server=DESKTOP-BREO199\SQLEXPRESS;Database=RacingData;Trusted_Connection=False;MultipleActiveResultSets=true;User ID=garydev; Password="; //todo: add password
            optionsBuilder.UseSqlServer(cn);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
