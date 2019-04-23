using RacingWebScrape.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RacingWebScrape.UnitOfWork
{
    public interface IRacingUnitOfWork
    {
        ICourseRepository Courses { get; }

        void Complete();
        Task CompleteAsync();
    }
}
