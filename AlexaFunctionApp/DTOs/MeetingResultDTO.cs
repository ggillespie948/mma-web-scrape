using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RacingWeb.API.DTOs
{
    public class MeetingResultDTO
    {
        public int Id { get; set; }

        public DateTime RaceTime { get; set; }

        public string RaceTitle { get; set; }

        public int CourseMeetingId { get; set; }

        /// <summary>
        /// Object containing each horse, position, jockey, trainer, price
        /// </summary>
        public virtual IList<ResultEntryDTO> ResultEntries { get; set; }

        /// <summary>
        /// String containing information of number of runners, name of non-runners
        /// </summary>
        public string RunnerInformation { get; set; }

        public string WinningJockey { get; set; }

        public string WinningTrainer { get; set; }

        //Race number in meeting result
        public int RaceNumber { get; set; }
    }
}
