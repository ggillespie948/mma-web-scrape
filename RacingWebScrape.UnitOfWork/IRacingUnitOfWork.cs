using RacingWebScrape.Interfaces;
using RacingWebScrape.Interfaces.MeetingResults;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RacingWebScrape.UnitOfWork
{
    public interface IRacingUnitOfWork
    {
        ICourseRepository Courses { get; }
        IMeetingResultsRepository CourseMeetings {get;}

        void Complete();
        Task CompleteAsync();
    }
}
