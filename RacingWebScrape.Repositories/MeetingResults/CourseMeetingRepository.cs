using Microsoft.EntityFrameworkCore;
using RacingWebScrape.Db;
using RacingWebScrape.Interfaces.MeetingResults;
using RacingWebScrape.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RacingWebScrape.Repositories.MeetingResults
{
    public class CourseMeetingRepository : ICourseMeetingRepository
    {
        private readonly RacingDbContext _context;
        public CourseMeetingRepository(RacingDbContext context)
        {
            _context = context;
        }

        public void Add(CourseMeeting courseMeeting)
        {
            _context.CourseMeetings.Add(courseMeeting);
        }

        public void Delete(CourseMeeting courseMeeting)
        {
            _context.Entry(courseMeeting).State = EntityState.Deleted;
        }

        public bool DoesCourseMeetingExist(CourseMeeting courseMeeting)
        {
            if(_context.CourseMeetings.Where(i=>i.MeetingDate.Date == courseMeeting.MeetingDate.Date && i.CourseId == courseMeeting.CourseId).Any())
            {
                return true;
            } else
            {
                return false;
            }
        }

        public CourseMeeting Get(int id)
        {
            return _context.CourseMeetings
                .Include(i => i.MeetingResults)
                .Include(i => i.Course)
                .Where(i => i.Id == id)
                .FirstOrDefault();
        }

        public IEnumerable<CourseMeeting> Get()
        {
            return _context.CourseMeetings
                .Include(i => i.MeetingResults)
                .Include(i => i.Course)
                .ToList();
        }

        public IEnumerable<CourseMeeting> GetTodaysMeetings()
        {
            return _context.CourseMeetings
                .Include(i => i.MeetingResults)
                .Include(i => i.Course)
                .Where(i => i.MeetingDate == DateTime.Today)
                .ToList();
        }

        public void Update(CourseMeeting courseMeeting)
        {
            _context.Entry(courseMeeting).State = EntityState.Modified;
        }
    }
}
