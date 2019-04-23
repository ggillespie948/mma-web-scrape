using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RacingWebScrape.Db;
using RacingWebScrape.Interfaces;
using RacingWebScrape.Interfaces.MeetingResults;
using RacingWebScrape.Repositories;
using RacingWebScrape.Repositories.MeetingResults;

namespace RacingWebScrape.UnitOfWork
{
    public class UnitOfWork : IRacingUnitOfWork
    {
        private readonly RacingDbContext _context;

        public UnitOfWork(RacingDbContext context)
        {
            _context = context;
            Courses = new CourseRepository(context);
            CourseMeetings = new CourseMeetingRepository(context);
        }

        public ICourseRepository Courses {get; set;}
        public ICourseMeetingRepository CourseMeetings { get; set; }

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
