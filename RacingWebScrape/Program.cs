using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RacingWebScrape
{
    class Program
    {
        // Processed content
        private static List<string> DailyMeetingTitles {get; set;}



        //Raw html node collections, unprocessed
        private static HtmlNodeCollection DailyMeetingTitleNodes {get; set;}
        private static List<HtmlNode> DailyMeetingContentNodes {get; set;}


        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        async static Task MainAsync(string[] args)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync("https://www.timeform.com/horse-racing/results/today");

            var pageContents = await response.Content.ReadAsStringAsync();
            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(pageContents);

            //meeting titles are used to retrieve the id's for retrieving content nodes
            DailyMeetingTitleNodes = pageDocument.DocumentNode.SelectNodes("(//div[contains(@class,'w-results-holder')][1]//a)");
            
            RetrieveDailyMeetingContentNodes(ref pageDocument);

            PrintDailyMeetingTitles();
            PrintDailyResults();

            Console.WriteLine("Daily Meeting content nodes: " + DailyMeetingContentNodes.Count);
            Console.ReadLine();
        }

        private static void PrintNodeClasses(HtmlNode node)
        {
            var classes = node.GetClasses();
            foreach(var c in classes)
            {
                Console.WriteLine(c.ToString());
            }
        }

        private static void RetrieveDailyMeetingContentNodes(ref HtmlDocument pageDocument)
        {
            //Daily Meetings Titles
            var dailyMeetingTitles = new List<string>();
            var dailyMeetingContentNodes = new List<HtmlNode>();

            foreach (var meeting in DailyMeetingTitleNodes)
            {
                if(meeting.GetClasses().Count() == 1)
                {
                    if(meeting.GetClasses().ElementAt(0) != "")
                    {
                        if (!dailyMeetingTitles.Contains(meeting.InnerHtml))
                        {
                            //Process title
                            var s = meeting.InnerHtml.ToString();
                            s = s.Replace(">", "");
                            dailyMeetingTitles.Add(s);
                            
                            //Process content Id
                            var meetingContentId = (meeting.Attributes["href"].Value + "-results");
                            meetingContentId = meetingContentId.Remove(0,1);
                            var meetingContentNode = (pageDocument.DocumentNode.SelectSingleNode("//*[@id=\"" + meetingContentId + "\"]"));
                            if(meetingContentNode != null)
                            {
                                dailyMeetingContentNodes.Add(meetingContentNode);
                            }
                        }
                    }
                }
            }
            DailyMeetingContentNodes = dailyMeetingContentNodes;
            DailyMeetingTitles = dailyMeetingTitles;
        }

        private static void PrintDailyMeetingTitles()
        {
            Console.WriteLine("There are " + DailyMeetingTitles.Count + " Meetings In The UK Today");

            int counter = 0;
            foreach(var meeting in DailyMeetingTitles)
            {
                counter++;
                Console.WriteLine(counter+". " + meeting);
            }
        }

        private static void PrintDailyResults()
        {
            

        }



    }
}
