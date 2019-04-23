using RacingWebScrape.Db;
using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using RacingWebScrape.Models;
using RacingWebScrape.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using RacingWebScrape.UnitOfWork;

namespace RacingWebScrape
{
    /// <summary>
    /// Collection of methods and data which scrape racing results from a given html document
    /// </summary>
    public class ResultScraper
    {
        public static IRacingUnitOfWork UnitOfWork;
        public ResultScraper(IRacingUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }
        private static List<string> DailyMeetingTitles { get; set; }
        //Raw html node collections, unprocessed
        private static HtmlNodeCollection DailyMeetingTitleNodes { get; set; }
        private static List<HtmlNode> DailyMeetingContentNodes { get; set; }

        public void ScrapeQuickResults(HtmlDocument pageDocument)
        {
            //meeting titles are used to retrieve the id's for retrieving content nodes
            DailyMeetingTitleNodes = pageDocument.DocumentNode.SelectNodes("(//div[contains(@class,'w-results-holder')][1]//a)");
            RetrieveDailyMeetingContentNodes(ref pageDocument);
            ProcessDailyMeetingContentNodes();

            PrintDailyMeetingTitles();
            PrintDailyResults();
        }

        private void ProcessDailyMeetingContentNodes()
        {
            var nodeCount = 0;
            foreach (HtmlNode node in DailyMeetingContentNodes)
            {
                var course = new Course();
                course.Name = DailyMeetingTitles[nodeCount];

                if (!CourseNameIsRegistered(course.Name))
                {
                    UnitOfWork.Courses.Add(course);
                    UnitOfWork.Complete();
                } else
                {
                    course = UnitOfWork.Courses.GetByName(course.Name);
                }

                if (node.Descendants("div").Any())
                {
                    CourseMeeting newCourseMeeting = new CourseMeeting();
                    newCourseMeeting.Course = course;
                    newCourseMeeting.MeetingDate = DateTime.Today;
                    newCourseMeeting.MeetingResults = new List<MeetingResult>();

                    if(!UnitOfWork.CourseMeetings.DoesCourseMeetingExist(newCourseMeeting))
                    {
                        UnitOfWork.CourseMeetings.Add(newCourseMeeting);
                        UnitOfWork.Complete();
                    }

                    foreach (var meetingResult in node.Descendants("div").ToList())
                    {
                        //Collect Content Nodes
                        var titleNode = meetingResult.Descendants("a").FirstOrDefault();
                        var fullResultUrl = titleNode.Attributes["href"].Value;
                        var raceTimeNode = meetingResult.Descendants("span").FirstOrDefault();

                        var newMeetingResult = new MeetingResult();
                        newMeetingResult.RaceTitle = HttpUtility.HtmlDecode(titleNode?.InnerHtml.ToString());
                        newMeetingResult.FullResultURL = fullResultUrl;
                        newMeetingResult.RaceTime = DateTime.Parse(raceTimeNode.InnerHtml);

                        if (meetingResult.SelectNodes(".//tr") != null)
                        {
                            Console.WriteLine(" ");
                            Console.WriteLine(titleNode.InnerHtml.ToString());
                            Console.WriteLine("Time: " + raceTimeNode.InnerHtml.ToString());

                            var resultsNodes = meetingResult.SelectNodes(".//tr").Where(n => n.HasClass("results-place")).ToList();
                            var jockeyTrainerNode = meetingResult.SelectNodes(".//tr").Where(n => n.HasClass("results-jt")).ToList();
                            var runnerInfoNode = meetingResult.SelectNodes(".//tr").Where(n => n.HasClass("results-info")).FirstOrDefault();

                            var jockeyName = jockeyTrainerNode.First().Descendants("a").FirstOrDefault().InnerHtml;
                            var trainerName = jockeyTrainerNode.Last().Descendants("a").FirstOrDefault().InnerHtml;
                            var runnerInfo = HttpUtility.HtmlDecode(runnerInfoNode?.InnerHtml.ToString());

                            // horse places
                            foreach (var horseResultNode in resultsNodes)
                            {
                                var newResultsEntry = new ResultEntry();

                                //gather nodes
                                var placeNode = horseResultNode.Descendants("td").FirstOrDefault();
                                // jockey silks node is 2nd tr node, not extracting picture for obvious reasons
                                var nameNode = horseResultNode.Descendants("td").Last().FirstChild;
                                var fractionNode = horseResultNode.Descendants().Where(n => n.HasClass("price-fractional")).FirstOrDefault();
                                var decimalNode = horseResultNode.Descendants().Where(n => n.HasClass("price-decimal")).FirstOrDefault();

                                var horseName = ProcessHorseNameNode(HttpUtility.HtmlDecode(nameNode?.InnerHtml.ToString()), out var horseNo);
                                newResultsEntry.HorseName = horseName;
                                newResultsEntry.HorseNumber = horseNo;
                                newResultsEntry.Place = placeNode?.InnerHtml.ToString();
                                if(decimalNode != null)
                                    newResultsEntry.PriceDecimal = decimal.Parse(decimalNode?.InnerHtml.ToString());
                                newResultsEntry.PriceFraction = fractionNode?.InnerHtml.ToString();
                                newResultsEntry.MeetingResultId = newMeetingResult.Id;
                                newResultsEntry.MeetingResult = newMeetingResult;

                                Console.WriteLine(placeNode?.InnerHtml + "  " + HttpUtility.HtmlDecode(nameNode?.InnerHtml.ToString()) + "  " + fractionNode?.InnerHtml);
                                //todo: collect the " f" text in the horeResultNode indicatin favourite or not
                            }

                        }
                        else
                        {
                            Console.WriteLine(" ");
                            titleNode = meetingResult.Descendants("span").ElementAt(1);
                            Console.WriteLine(HttpUtility.HtmlDecode(titleNode?.InnerHtml.ToString()));
                            Console.WriteLine("Time: " + raceTimeNode?.InnerHtml.ToString());
                            Console.WriteLine("Results TBC");
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
        private static bool CourseNameIsRegistered(string name)
        {
            if(UnitOfWork.Courses.Get().Where(i=>i.Name == name).Any())
            {
                return true;
            } else
            {
                return false;
            }
        }

        private static void PrintNodeClasses(HtmlNode node)
        {
            var classes = node.GetClasses();
            foreach (var c in classes)
            {
                Console.WriteLine(c.ToString());
            }
        }

        private static string ProcessHorseNameNode(string horseTitleHtml, out string horseNumber)
        {
            if (horseTitleHtml.ElementAt(1).ToString() == ".")
            {
                horseNumber = horseTitleHtml.Substring(0, 2);
                return horseTitleHtml.Substring(2, horseTitleHtml.Length-2);
            }
            else
            {
                horseNumber = "";
                return horseTitleHtml;
            }
        }

        private static void RetrieveDailyMeetingContentNodes(ref HtmlDocument pageDocument)
        {
            //Daily Meetings Titles
            var dailyMeetingTitles = new List<string>();
            var dailyMeetingContentNodes = new List<HtmlNode>();

            foreach (var meeting in DailyMeetingTitleNodes)
            {
                if (meeting.GetClasses().Count() == 1)
                {
                    if (meeting.GetClasses().ElementAt(0) != "")
                    {
                        if (!dailyMeetingTitles.Contains(meeting.InnerHtml))
                        {
                            //Process title
                            var s = meeting.InnerHtml.ToString();
                            s = s.Replace(">", "");
                            dailyMeetingTitles.Add(s);

                            //Process content Id
                            var meetingContentId = (meeting.Attributes["href"].Value + "-results");
                            meetingContentId = meetingContentId.Remove(0, 1);
                            var meetingContentNode = (pageDocument.DocumentNode.SelectSingleNode("//*[@id=\"" + meetingContentId + "\"]"));
                            if (meetingContentNode != null)
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
            foreach (var meeting in DailyMeetingTitles)
            {
                counter++;
                Console.WriteLine(counter + ". " + meeting);
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
