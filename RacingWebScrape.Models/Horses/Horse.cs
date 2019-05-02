using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RacingWebScrape.Models.Horses
{
    public class Horse
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Country { get; set; }

        public ICollection<ResultEntry> ResultHistory {get; set;}

    }
}
