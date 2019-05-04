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

            public void UpdateCourseMeetingResults(CourseMeeting courseMeeting)
            {
                _context.Entry(courseMeeting).State = EntityState.Modified;
            }

            public CourseMeeting GetCourseMeeting(int id)
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

            /// <summary>
            /// Action which returns the today's meeting names
            /// </summary>
            /// <returns></returns>
            public IEnumerable<CourseMeeting> GetTodaysMeetings()
            {
                return _context.CourseMeetings
                    .Include(i => i.Course)
                    .Where(i => i.MeetingDate == DateTime.Today)
                    .ToList();
            }

            public CourseMeeting GetCourseMeetingByDate(int courseId, DateTime date)
            {
                return _context.CourseMeetings
                .Include(i => i.MeetingResults)
                .Include(i => i.Course)
                .Where(i => i.MeetingDate == date && i.CourseId == courseId)
                .FirstOrDefault();
            }
        #endregion


        #region Meeting Results
        public void AddMeetingResult(MeetingResult meetingResult)
            {
                _context.Add(meetingResult);
            }

            public void DeleteMeetingResult(MeetingResult meetingResult)
            {
                _context.Entry(meetingResult).State = EntityState.Deleted;
            }

            public void UpdateMeetingResult(MeetingResult meetingResult)
            {
                _context.Entry(meetingResult).State = EntityState.Modified;
            }

            public MeetingResult GetMeetingResultByRaceNo(int courseId, int raceNo)
            {
                return _context.MeetingResults
                    .Include(i => i.CourseMeeting)
                    .Include(i => i.ResultEntries)
                    .Where(i => i.CourseMeeting.CourseId == courseId 
                            && i.RaceNumber == raceNo && i.RaceTime.Date == DateTime.Today )
                    .FirstOrDefault();
            }

            public MeetingResult GetDailyMeetingResultByTime(DateTime time)
            {
                return _context.MeetingResults
                    .Include(i => i.CourseMeeting)
                    .Include(i => i.ResultEntries)
                    .Where(i => i.RaceTime == time && i.RaceTime.Date == DateTime.Today)
                    .FirstOrDefault();
            }

            public IEnumerable<MeetingResult> GetTodaysMeetingResults()
            {
                return _context.MeetingResults
                    .Include(i => i.CourseMeeting)
                    .Include(i => i.ResultEntries)
                    .Where(i => i.CourseMeeting.MeetingDate == DateTime.Today)
                    .ToList();
            }

            public IEnumerable<MeetingResult> GetMeetingResultsByCourse(int courseId, DateTime? Date)
            {
                return _context.MeetingResults
                    .Include(i => i.CourseMeeting)
                    .Include(i => i.ResultEntries)
                    .Where(i => i.CourseMeeting.MeetingDate == DateTime.Today)
                    .ToList();
            }

            public MeetingResult GetCourseMeetingResultByTime(int courseMeetingId, DateTime time)
            {
                return _context.MeetingResults
                .Include(i => i.CourseMeeting)
                .Include(i => i.ResultEntries)
                .Where(i => i.RaceTime == time && i.CourseMeetingId == courseMeetingId)
                .FirstOrDefault();
            }
        #endregion


        #region Results entry
        public void AddResultEntry(ResultEntry resultEntry)
            {
                _context.ResultEntires.Add(resultEntry);
            }
        
            public void UpdateResultEntry(ResultEntry resultEntry)
            {
                _context.Entry(resultEntry).State = EntityState.Modified;
            }

            public void DeleteResultEntry(ResultEntry resultEntry)
            {
                _context.Entry(resultEntry).State = EntityState.Deleted;
            }
            public ResultEntry GetResultEntry(int id)
            {
                return _context.ResultEntires
                    .Include(i => i.MeetingResult)
                    .Where(i => i.Id == id)
                    .FirstOrDefault();
            }

            public IEnumerable<ResultEntry> GetMeetingResultEntries(int meetingResultId)
            {
                return _context.ResultEntires
                    .Include(i => i.MeetingResult)
                    .Where(i => i.MeetingResultId == meetingResultId)
                    .ToList();
            }

      




        #endregion
    }
}
