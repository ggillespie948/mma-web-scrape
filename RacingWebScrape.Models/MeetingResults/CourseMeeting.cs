using RacingWebScrape.Models.Courses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RacingWebScrape.Models
{
    public class CourseMeeting
    {
        [Key]
        public int Id { get; set; }

        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        public DateTime MeetingDate { get; set; }

        public ICollection<MeetingResult> MeetingResults { get; set; }
    }
}
