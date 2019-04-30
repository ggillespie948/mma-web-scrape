using RacingWebScrape.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RacingWebScrape.Interfaces.MeetingResults
{
    public interface IMeetingResultsRepository
    {
        //Course Meetings 
        void AddMeeting(CourseMeeting courseMeeting);
        void DeleteMeeting(CourseMeeting courseMeeting);
        bool DoesCourseMeetingExist(CourseMeeting courseMeeting);
        CourseMeeting GetCourseMeeting(int id);
        CourseMeeting GetCourseMeetingByDate(int courseId, DateTime date);
        void UpdateCourseMeetingResults(CourseMeeting courseMeeting);
        IEnumerable<CourseMeeting> GetTodaysMeetings();
        IEnumerable<CourseMeeting> GetMeetings();


        //Meeting Result
        void AddMeetingResult(MeetingResult meetingResult);
        void UpdateMeetingResult(MeetingResult meetingResult);
        void DeleteMeetingResult(MeetingResult meetingResult);
        MeetingResult GetDailyMeetingResultByTime(DateTime time);
        MeetingResult GetCourseMeetingResultByTime(int courseMeetingId, DateTime time);
        MeetingResult GetMeetingResultByRaceNo(int courseId, int raceNo);
        IEnumerable<MeetingResult> GetTodaysMeetingResults();
        IEnumerable<MeetingResult> GetMeetingResultsByCourse(int courseId, DateTime? Date);


        //Result Entries
        void AddResultEntry(ResultEntry resultEntry);
        void UpdateResultEntry(ResultEntry resultEntry);
        void DeleteResultEntry(ResultEntry resultEntry);
        ResultEntry GetResultEntry(int id);
        IEnumerable<ResultEntry> GetMeetingResultEntries(int meetingResultId);
    }
}
