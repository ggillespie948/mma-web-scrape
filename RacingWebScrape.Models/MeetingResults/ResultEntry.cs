using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RacingWebScrape.Models
{
    public class ResultEntry
    {
        [Key]
        public int Id { get; set; }

        public int MeetingResultId { get; set; }
        public MeetingResult MeetingResult { get; set; }

        public string Place { get; set; } // e.g. "1st" "2nd"

        public int Position { get; set; }

        public string HorseName { get; set; }

        public string HorseNumber { get; set; }

        public string JockeyName { get; set; }

        public string TrainerName { get; set; }

        public string PriceFraction { get; set; }

        public decimal PriceDecimal { get; set; }
    }
}
