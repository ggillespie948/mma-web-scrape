using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MMAWeb.Interfaces.PromotionMeetings;
using MMAWeb.Interfaces.Promotions;

namespace MMAWeb.Db
{
    public class MMADbContext : DbContext
    {
        public DbSet<Promotion> Promotions {get; set;}
        public DbSet<PromotionMeeting> PromotionMeetings {get; set;}
        public DbSet<FightResult> FightResults {get; set;}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string cn = @"Server=tcp:gzgillespie.database.windows.net,1433;Initial Catalog=MMAData;Persist Security Info=False;User ID=garydev;Password=;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"; //todo: add password
            optionsBuilder.UseSqlServer(cn);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
