using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RacingWebScrape.Models
{
    public class MeetingResult
    {
        [Key]
        public int Id { get; set; }

        public DateTime RaceTime { get; set; }

        public string RaceTitle { get; set; }

        public int CourseMeetingId { get; set; }
        public CourseMeeting CourseMeeting { get; set; }

        /// <summary>
        /// Object containing each horse, position, jockey, trainer, price
        /// </summary>
        public virtual IList<ResultEntry> ResultEntries { get; set; }

        /// <summary>
        /// String containing information of number of runners, name of non-runners
        /// </summary>
        public string RunnerInformation { get; set; }

        public string WinningJockey { get; set; }

        public string WinningTrainer { get; set; }

        //Reference to the url where the full result information can be scraped
        public string FullResultURL { get; set; }

    }
}
