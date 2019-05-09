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
            string cn = @"Server=tcp:gzgillespie.database.windows.net,1433;Initial Catalog=RacingData;Persist Security Info=False;User ID=garydev;Password=Parkplace!2345;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"; //todo: add password
            optionsBuilder.UseSqlServer(cn);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
