using RacingWebScrape.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RacingWebScrape.Interfaces.MeetingResults
{
    public interface ICourseMeetingRepository
    {
        void Add(CourseMeeting courseMeeting);
        void Update(CourseMeeting courseMeeting);
        void Delete(CourseMeeting courseMeeting);

        IEnumerable<CourseMeeting> GetTodaysMeetings();

        CourseMeeting Get(int id);
        IEnumerable<CourseMeeting> Get();

        bool DoesCourseMeetingExist(CourseMeeting courseMeeting);
    }
}
