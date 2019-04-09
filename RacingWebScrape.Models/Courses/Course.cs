using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RacingWebScrape.Models.Courses
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string RaceTitleContentId { get; set; }

        /// <summary>
        /// This is the html element ID used to obtain the results node
        /// </summary>
        public string ResultsContentId { get; set; }
    }
}
