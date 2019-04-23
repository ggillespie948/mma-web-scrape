using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RacingWebScrape.Db;
using RacingWebScrape.Interfaces;
using RacingWebScrape.Repositories;

namespace RacingWebScrape.UnitOfWork
{
    public class UnitOfWork : IRacingUnitOfWork
    {
        private readonly RacingDbContext _context;

        public UnitOfWork(RacingDbContext context)
        {
            _context = context;
            Courses = new CourseRepository(context);
        }

        public ICourseRepository Courses {get; set;}

        public void Complete()
        {
            _context.SaveChanges();
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
