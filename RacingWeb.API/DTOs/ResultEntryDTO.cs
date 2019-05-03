using RacingWebScrape.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RacingWeb.API.DTOs
{
    public class ResultEntryDTO
    {
        public int Id { get; set; } //remove?

        public int MeetingResultId { get; set; } //remove?
        public MeetingResult MeetingResult { get; set; }

        public string Place { get; set; } 

        public string HorseName { get; set; }

        public string HorseNumber { get; set; }

        public string JockeyName { get; set; }

        public string TrainerName { get; set; }

        public string PriceFraction { get; set; }

        public decimal PriceDecimal { get; set; }
    }
}
