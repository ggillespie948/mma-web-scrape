using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RacingWebScrape.Models.Jockeys
{
    public class Jockey
    {
        [Key]
        public int Id { get; set; }

        public string ResultName { get; set; }

        public string FirstName { get; set; }

        public string Surname { get; set; }

        public ICollection<ResultEntry> ResultHistory {get; set;}
    }
}
