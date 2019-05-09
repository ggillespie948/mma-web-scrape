using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RacingWeb.API.DTOs
{
    public class CourseMeetingDTO
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public virtual CourseDTO Course { get; set; }
        public DateTime MeetingDate { get; set; }
    }
}
