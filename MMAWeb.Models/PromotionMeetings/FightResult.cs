using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MMAWeb.Interfaces.PromotionMeetings
{
    public class FightResult
    {
        [Key]
        public int Id { get; set; }
        public int PromotionMeetingId { get; set; }
        public virtual PromotionMeeting PromotionMeeting { get; set; }
        public bool IsMainCard { get; set; }
        public string FighterNameA { get; set; }
        public string FighterNameB { get; set; }
        public string DecisionSummary { get; set; }
    }
}
