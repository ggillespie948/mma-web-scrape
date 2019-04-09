using HtmlAgilityPack;
using RacingWebScrape.Models;
using RacingWebScrape.Models.Courses;
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

            ProcessDailyMeetingContentNodes();

            Console.WriteLine("Daily Meeting content nodes: " + DailyMeetingContentNodes.Count);
            Console.ReadLine();
        }

        private static void ProcessDailyMeetingContentNodes()
        {
            var nodeCount = 0;
            foreach(HtmlNode node in DailyMeetingContentNodes)
            {


                var course = new Course();
                course.Name = DailyMeetingTitles[nodeCount];

                Console.WriteLine("");
                Console.WriteLine("///////////////");
                Console.WriteLine(course.Name);
                Console.WriteLine("///////////////");
                //Console.WriteLine("Children divs: " + node.Descendants("div").ToList().Count());

                if(!CourseNameIsRegistered())
                {
                    //Register new course into system
                }

                if(node.Descendants("div").Any())
                {
                    foreach(var meetingResult in node.Descendants("div").ToList())
                    {
                        var newMeetingResult = new MeetingResult();

                        //Collect Content Nodes
                        var titleNode = meetingResult.Descendants("a").FirstOrDefault();
                        var raceTimeNode = meetingResult.Descendants("span").FirstOrDefault();

                        if(meetingResult.SelectNodes(".//tr") != null)
                        {

                            var resultsNodes = meetingResult.SelectNodes(".//tr").Where(n => n.HasClass("results-place")).ToList();
                            Console.WriteLine(" ");
                            Console.WriteLine(titleNode.InnerHtml.ToString());
                            Console.WriteLine("Time: " + raceTimeNode.InnerHtml.ToString());

                            foreach (var horseResultNode in resultsNodes)
                            {
                                var newResultsEntry = new ResultEntry();

                                var placeNode = horseResultNode.Descendants("td").FirstOrDefault();
                                // jockey silks node is 2nd tr node, not extracting picture for obvious reasons
                                var nameNode = horseResultNode.Descendants("td").Last().FirstChild;
                                var fractionNode = horseResultNode.Descendants().Where(n => n.HasClass("price-fractional")).FirstOrDefault();
                                var decimalNode = horseResultNode.Descendants().Where(n => n.HasClass("price-decimal")).FirstOrDefault();

                                //todo: collect the " f" text in the horeResultNode indicatin favourite or not

                                Console.WriteLine(placeNode.InnerHtml + "  " + nameNode.InnerHtml + "  " + fractionNode.InnerHtml);
                            }
                        }

                    }

                }
                
                Console.WriteLine(" ");

                nodeCount++;
            }
            
        }

        /// <summary>
        /// This method checks for a course meeting with the same name
        /// </summary>
        private static bool CourseNameIsRegistered()
        {
            return true;
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
            Console.WriteLine("There are " + DailyMeetingTitles.Count + " meetings Today");

            int counter = 0;
            foreach(var meeting in DailyMeetingTitles)
            {
                counter++;
                Console.WriteLine(counter+". " + meeting);
            }
        }

        private static void ProcessMeetingContentNode()
        {

        }

        private static void PrintDailyResults()
        {
            
        }



    }
}
