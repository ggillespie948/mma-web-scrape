using Microsoft.EntityFrameworkCore;
using MMAWeb.Db;
using MMAWeb.Models.PromotionMeetings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMAWebScrape.AzureFunction
{
    public class MMADataService
    {
        public bool DoesPromotionMeetingExist(PromotionMeeting promotionMeeting,ref MMADbContext context)
        {
            if (context.PromotionMeetings.Where(i => i.Date == promotionMeeting.Date).Any())
                return true;
            else
                return false;
        }

        public bool AddPromotionMeeting(PromotionMeeting promotionMeeting, ref MMADbContext context)
        {
            context.Add(promotionMeeting);
            return false;
        }

        public bool UpdatePromotionMeeting(PromotionMeeting promotionMeeting, ref MMADbContext context)
        {
            context.Entry(promotionMeeting).State = EntityState.Modified;
            return true;
        }

        public PromotionMeeting GetPromotionMeetingByDate(DateTime dateTime, int promotionId, ref MMADbContext context)
        {
            return context.PromotionMeetings.Where(i => i.Date == dateTime && i.PromotionId == promotionId).FirstOrDefault();
        }

        public void SaveChanges(ref MMADbContext context)
        {
            context.SaveChanges();
        }

    }
}
