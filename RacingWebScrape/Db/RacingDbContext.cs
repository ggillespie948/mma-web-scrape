using Microsoft.EntityFrameworkCore;
using RacingWebScrape.Models;
using RacingWebScrape.Models.Courses;
using System;
using System.Collections.Generic;
using System.Text;

namespace RacingWebScrape.Db
{
    public class RacingDbContext : DbContext
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseMeeting> CourseMeetings { get; set; }
        public DbSet<MeetingResult> MeetingResults { get; set; }
        public DbSet<ResultEntry> ResultEntires { get; set; }
    }
}
