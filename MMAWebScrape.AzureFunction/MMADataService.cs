using Microsoft.EntityFrameworkCore;
using MMAWeb.Db;
using MMAWeb.Models.PromotionMeetings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMAWebScrape.AzureFunction
{
    public static class MMADataService
    {
        public static bool DoesPromotionMeetingExist(DateTime date, int promotionId, ref MMADbContext context)
        {
            if (context.PromotionMeetings.Where(i => i.Date == date && i.PromotionId == promotionId).Any())
                return true;
            else
                return false;
        }

        public static bool AddPromotionMeeting(PromotionMeeting promotionMeeting, ref MMADbContext context)
        {
            context.Add(promotionMeeting);
            return false;
        }

        public static bool UpdatePromotionMeeting(PromotionMeeting promotionMeeting, ref MMADbContext context)
        {
            context.Entry(promotionMeeting).State = EntityState.Modified;
            return true;
        }

        public static PromotionMeeting GetPromotionMeetingByDate(DateTime dateTime, int promotionId, ref MMADbContext context)
        {
            return context.PromotionMeetings.Where(i => i.Date == dateTime && i.PromotionId == promotionId).FirstOrDefault();
        }

        public static void SaveChanges(ref MMADbContext context)
        {
            context.SaveChanges();
        }

    }
}
