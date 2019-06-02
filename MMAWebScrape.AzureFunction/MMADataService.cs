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
            if (context.PromotionMeetings.Where(i => i.Date.Date == date.Date && i.PromotionId == promotionId).Any())
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
            return context.PromotionMeetings.Where(i => i.Date == dateTime && i.PromotionId == promotionId)
                .Include(i => i.FightResults)
                .Include(i => i.Promotion)
                .FirstOrDefault();
        }

        public static IEnumerable<PromotionMeeting> GetEventsByDate(DateTime dateTime, ref MMADbContext context)
        {
            return context.PromotionMeetings.Where(i => i.Date == dateTime)
                .Include(i => i.FightResults)
                .Include(i => i.Promotion)
                .ToList();
        }

        public static PromotionMeeting GetMostRecentPromotionEvent(int promotionId, ref MMADbContext context)
        {
            return context.PromotionMeetings.Where(i => i.PromotionId == promotionId)
                .Include(i => i.FightResults)
                .FirstOrDefault();
        }

        public static void SaveChanges(ref MMADbContext context)
        {
            context.SaveChanges();
        }

    }
}
