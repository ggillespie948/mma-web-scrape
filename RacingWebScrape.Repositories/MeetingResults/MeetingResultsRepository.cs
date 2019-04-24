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
    public class MeetingResultsRepository : IMeetingResultsRepository
    {
        private readonly RacingDbContext _context;
        public MeetingResultsRepository(RacingDbContext context)
        {
            _context = context;
        }

        #region Course Meetings
            public void AddMeeting(CourseMeeting courseMeeting)
            {
                _context.CourseMeetings.Add(courseMeeting);
            }

            public void DeleteMeeting(CourseMeeting courseMeeting)
            {
                _context.Entry(courseMeeting).State = EntityState.Deleted;
            }

            public bool DoesCourseMeetingExist(CourseMeeting courseMeeting)
            {
                if (_context.CourseMeetings.Where(i => i.MeetingDate.Date == courseMeeting.MeetingDate.Date && i.CourseId == courseMeeting.CourseId).Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public CourseMeeting GetMeeting(int id)
            {
                return _context.CourseMeetings
                    .Include(i => i.MeetingResults)
                    .Include(i => i.Course)
                    .Where(i => i.Id == id)
                    .FirstOrDefault();
            }

            public IEnumerable<CourseMeeting> GetMeetings()
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
        #endregion


        #region Meeting Results
            public void AddMeetingResult(MeetingResult meetingResult)
            {
                throw new NotImplementedException();
            }

            public void DeleteMeetingResult(MeetingResult meetingResult)
            {
                throw new NotImplementedException();
            }

            public void UpdateMeetingResult(MeetingResult meetingResult)
            {
                throw new NotImplementedException();
            }

            public MeetingResult GetMeetingResultByRaceNo(int courseId, int raceNo)
            {
                throw new NotImplementedException();
            }

            public MeetingResult GetMeetingResultByTime(DateTime time)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<MeetingResult> GetTodaysMeetingResults(MeetingResult meetingResult)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<MeetingResult> GetMeetingResultsByCourse(int courseId, DateTime? Date)
            {
                throw new NotImplementedException();
            }
        #endregion


        #region Results entry
            public void AddResultEntry(ResultEntry resultEntry)
            {
                throw new NotImplementedException();
            }
        
            public void UpdateResultEntry(ResultEntry resultEntry)
            {
                throw new NotImplementedException();
            }

            public void DeleteResultEntry(ResultEntry resultEntry)
            {
                throw new NotImplementedException();
            }
            public ResultEntry GetResultEntry(int id)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ResultEntry> GetMeetingResultEntries(int meetingResultId)
            {
                throw new NotImplementedException();
            }
        #endregion
    }
}
