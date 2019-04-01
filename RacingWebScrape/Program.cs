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
        private static object tags;

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

            string[] dailyMeeting = new string[pageDocument.DocumentNode.SelectSingleNode("(//div[contains(@class,'w-results-holder')]//a)").ChildNodes.Count];
            var headlineText = pageDocument.DocumentNode.SelectSingleNode("(//div[contains(@class,'w-results-holder')]//a)[1]").InnerText;
            var dailyMeetingNodes = pageDocument.DocumentNode.SelectNodes("(//div[contains(@class,'w-results-holder')][1]//a)");

            //Daily Meetings List - Refactor into method
            var dailyMeetingTitles = new List<string>();

            foreach (var meeting in dailyMeetingNodes)
            {
                if(meeting.GetClasses().Count() == 1)
                {
                    if(meeting.GetClasses().ElementAt(0) != "")
                    {
                        if (!dailyMeetingTitles.Contains(meeting.InnerHtml))
                        {
                            var s = meeting.InnerHtml.ToString();
                            s = s.Replace(">", "");
                            dailyMeetingTitles.Add(s);
                        }
                    }
                }
            }

            //Intro
            Console.WriteLine("There are : " + dailyMeetingTitles.Count + " Meetings In The UK Today");

            int counter = 0;
            foreach(var meeting in dailyMeetingTitles)
            {
                counter++;
                Console.WriteLine(counter+". " + meeting);
            }

            Console.ReadLine();
        }

        private static void PrintClasses(HtmlNode node)
        {
            var classes = node.GetClasses();
            foreach(var c in classes)
            {
                Console.WriteLine(c.ToString());
            }
        }

    }
}
