using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;
using MMAWeb.Interfaces.Promotions;

namespace MMAWeb.Interfaces.PromotionMeetings
{
    public class PromotionMeeting
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; } //e.g. UFC 237: Rose Namajunes vs Model Crowe
        public DateTime Date { get; set; }
        public int PromotionId { get; set; }
        public virtual Promotion Promotion { get; set; }
        public ICollection<FightResult> FightResults { get; set; }
    }
}
