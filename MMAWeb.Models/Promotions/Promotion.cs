using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using MMAWeb.Interfaces.PromotionMeetings;

namespace MMAWeb.Interfaces.Promotions
{
    public class Promotion
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<PromotionMeeting> PromotionMeetings {get; set;}
    }
}
