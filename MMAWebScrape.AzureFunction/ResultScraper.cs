using MMAWeb.Db;
using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using MMAWeb.Models;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using MMAWeb.Models.PromotionMeetings;

namespace MMAWebScrape
{
    /// <summary>
    /// Collection of methods and data which scrape racing results from a given html document
    /// </summary>
    public class ResultScraper
    {
        public static MMADbContext DbContext;
        public ResultScraper(MMADbContext unitOfWork)
        {
            DbContext = unitOfWork;
        }
        private static List<string> DailyMeetingTitles { get; set; }
        //Raw html node collections, unprocessed
        private static HtmlNodeCollection DailyMeetingTitleNodes { get; set; } //to be deleted



        private static HtmlNode EventContainer { get; set; }
        private static HtmlNodeCollection EventResultsContentNodes { get; set; }
        private static HtmlNodeCollection EventTitleNodes { get; set; }
        private static HtmlNodeCollection EventDateNodes { get; set; }

        public void ScrapeQuickResults(HtmlDocument pageDocument)
        {
            //meeting titles are used to retrieve the id's for retrieving content nodes
            EventContainer = pageDocument.DocumentNode.SelectSingleNode("//div[@class='m-mmaf-pte-event-list']");
            EventResultsContentNodes = EventContainer.SelectNodes(".//div[@class='m-mmaf-pte-event-list__split-item']");
            EventTitleNodes = EventContainer.SelectNodes(".//h2");
            EventTitleNodes = EventContainer.SelectNodes(".//h3");

            ProcessEventResults();
        }

        private void ProcessEventResults()
        {
            var counter = 0;
            foreach(var node in EventResultsContentNodes.Elements())
            {
                var promotionMeeting = new PromotionMeeting();
                promotionMeeting.Title = ProcessEventTitle(EventTitleNodes.ElementAt(counter), out int promotionId);
                promotionMeeting.PromotionId = promotionId;
                // assign promoiton via id search
                promotionMeeting.Date = ProcessTimeNode(EventDateNodes.ElementAt(counter));
                promotionMeeting.FightResults = new List<FightResult>();

                var mainCardNode = node.SelectSingleNode("//div[@class='m-mmaf-pte-event-list'][1]");
                var underCardNode = node.SelectSingleNode("//div[@class='m-mmaf-pte-event-list'][2]");

                //process maincard
                foreach(var fightResultNode in mainCardNode.Descendants().ElementAt(1).Descendants())
                {
                    var fightResult = new FightResult();

                    fightResult.IsMainCard = true;

                    if(ProcessFighterNames(fightResultNode.Descendants().ElementAt(0).InnerHtml, out string[] fighterNames))
                    {
                        fightResult.FighterNameA = fighterNames[0];
                        fightResult.FighterNameB = fighterNames[1];
                    }

                    //sometimes there is an additional <span> if the fight is a title fight
                    if(fightResultNode.Descendants().Count() == 3)
                    {
                        fightResult.DecisionSummary = ProcessFightDecision(fightResultNode.Descendants().ElementAt(2).InnerHtml);

                    } else if (fightResultNode.Descendants().Count() == 2)
                    {
                        fightResult.DecisionSummary = ProcessFightDecision(fightResultNode.Descendants().ElementAt(2).InnerHtml);
                    }

                    promotionMeeting.FightResults.Add(fightResult);
                }

                //process undercard
                foreach (var fightResultNode in mainCardNode.Descendants().ElementAt(1).Descendants())
                {
                    var fightResult = new FightResult();

                    fightResult.IsMainCard = false;

                    if (ProcessFighterNames(fightResultNode.Descendants().ElementAt(0).InnerHtml, out string[] fighterNames))
                    {
                        fightResult.FighterNameA = fighterNames[0];
                        fightResult.FighterNameB = fighterNames[1];
                    }

                    if (fightResultNode.Descendants().Count() == 3)
                    {
                        fightResult.DecisionSummary = ProcessFightDecision(fightResultNode.Descendants().ElementAt(2).InnerHtml);

                    }
                    else if (fightResultNode.Descendants().Count() == 2)
                    {
                        fightResult.DecisionSummary = ProcessFightDecision(fightResultNode.Descendants().ElementAt(2).InnerHtml);
                    }

                    promotionMeeting.FightResults.Add(fightResult);
                }
            }
        }

        /// <summary>
        /// Parses two fighter names from decision summary and returns true if two parsed names can currently be returned
        /// the array of parsed fighter names is is returned via an out parameter in conjunction with a success boolean.
        /// </summary>
        private bool ProcessFighterNames(string htmlContent, out string[]fighternames)
        {
            //search for "def." or "vs."
            if(htmlContent.Contains("def."))
            {
                fighternames = htmlContent.Split("def.");
                if(fighternames.Length == 2) return true;

            } else if (htmlContent.Contains("vs."))
            {
                fighternames = htmlContent.Split("vs.");
                if (fighternames.Length == 2) return true;

            } else
            {
                fighternames = null;
            }

            return false;
        }


        /// <summary>
        /// process decision string to make more readable by Alexa
        /// </summary>
        /// <param name="decisionString"></param>
        /// <returns></returns>
        private string ProcessFightDecision(string decisionString)
        {
            decisionString = decisionString.Replace("def.", "defeated");
            return decisionString;
        }

        private DateTime ProcessTimeNode(HtmlNode node)
        {
            return DateTime.Parse(node.Descendants("a").FirstOrDefault().InnerHtml);
        }

        private string ProcessEventTitle(HtmlNode titleNode, out int promotionId)
        {
            string eventTitle = titleNode.Descendants("a").FirstOrDefault().InnerHtml;
            if (eventTitle.Contains("UFC"))
            {
                promotionId = 1;
            } else if (eventTitle.Contains("Bellator"))
            {
                promotionId = 2;
            } else if (eventTitle.Contains("One"))
            {
                promotionId = 3;
            } else if (eventTitle.Contains("Invicta"))
            {
                promotionId = 4;
            } else if (eventTitle.Contains("PFL"))
            {
                promotionId = 5;
            } else
            {
                promotionId = 0;
                return null;
            }

            return eventTitle;
        }


        //private void ProcessDailyMeetingContentNodes()
        //{
        //    var nodeCount = 0;
        //    foreach (HtmlNode node in DailyMeetingContentNodes)
        //    {
        //        var course = new Course();
        //        course.Name = DailyMeetingTitles[nodeCount];

        //        if (!CourseNameIsRegistered(course.Name))
        //        {
        //            UnitOfWork.Courses.Add(course);
        //            UnitOfWork.Complete();
        //        }
        //        else
        //        {
        //            course = UnitOfWork.Courses.GetByName(course.Name);
        //        }

        //        if (node.Descendants("div").Any())
        //        {
        //            CourseMeeting newCourseMeeting = new CourseMeeting();
        //            newCourseMeeting.Course = course;
        //            newCourseMeeting.CourseId = course.Id;
        //            newCourseMeeting.MeetingDate = DateTime.Today;
        //            newCourseMeeting.MeetingResults = new List<MeetingResult>();

        //            if (!UnitOfWork.CourseMeetings.DoesCourseMeetingExist(newCourseMeeting))
        //            {
        //                UnitOfWork.CourseMeetings.AddMeeting(newCourseMeeting);
        //                UnitOfWork.Complete();
        //            }
        //            else
        //            {
        //                newCourseMeeting = UnitOfWork.CourseMeetings.GetCourseMeetingByDate(course.Id, DateTime.Today);
        //                newCourseMeeting.MeetingResults.Clear(); //clear results-> update with newest results
        //            }

        //            int raceCounter = 0;
        //            foreach (var meetingResultNode in node.Descendants("div").ToList())
        //            {
        //                //Collect Content Nodes
        //                var titleNode = meetingResultNode.Descendants().Where(i => i.HasClass("results-title")).FirstOrDefault();
        //                var fullResultUrl = titleNode.Attributes["href"] != null ? titleNode.Attributes["href"].Value : "PENDING";

        //                var raceTimeNode = meetingResultNode.Descendants().Where(i => i.HasClass("results-time")).FirstOrDefault();
        //                var jockeyTrainerNode = meetingResultNode.SelectNodes(".//tr")?.Where(n => n.HasClass("results-jt"))?.ToList();
        //                var runnerInfoNode = meetingResultNode.SelectNodes(".//tr")?.Where(n => n.HasClass("results-info")).FirstOrDefault();

        //                //Init meeting result instance
        //                var newMeetingResult = new MeetingResult();
        //                newMeetingResult.RaceNumber = ++raceCounter;
        //                newMeetingResult.CourseMeetingId = newCourseMeeting.Id;
        //                newMeetingResult.CourseMeeting = newCourseMeeting;
        //                newMeetingResult.FullResultURL = fullResultUrl;
        //                newMeetingResult.ResultEntries = new List<ResultEntry>();

        //                newMeetingResult.RaceTitle = HttpUtility.HtmlDecode(titleNode?.InnerHtml.ToString());
        //                newMeetingResult.FullResultURL = fullResultUrl;
        //                newMeetingResult.RaceTime = DateTime.Parse(raceTimeNode.InnerHtml);

        //                newMeetingResult.WinningJockey = jockeyTrainerNode?.First().Descendants("a").FirstOrDefault().InnerHtml;
        //                newMeetingResult.WinningTrainer = jockeyTrainerNode?.Last().Descendants("a").FirstOrDefault().InnerHtml;
        //                newMeetingResult.RunnerInformation = HttpUtility.HtmlDecode(runnerInfoNode?.Descendants("td").FirstOrDefault().InnerHtml?.ToString());

        //                newCourseMeeting.MeetingResults.Add(newMeetingResult);

        //                //Output race title and time
        //                Console.WriteLine(" ");
        //                Console.WriteLine(HttpUtility.HtmlDecode(titleNode?.InnerHtml.ToString()));
        //                Console.WriteLine("Time: " + raceTimeNode.InnerHtml.ToString());

        //                if (ProcessMeetingResultEntries(meetingResultNode, ref newMeetingResult))
        //                {
        //                    //Meeting result processed
        //                    UnitOfWork.CourseMeetings.UpdateCourseMeetingResults(newCourseMeeting);

        //                }
        //                else
        //                {
        //                    //Meeting results still pending..
        //                    Console.WriteLine(" ");
        //                    titleNode = meetingResultNode.Descendants("span").ElementAt(1);
        //                    Console.WriteLine(HttpUtility.HtmlDecode(titleNode?.InnerHtml.ToString()));
        //                    Console.WriteLine("Time: " + raceTimeNode?.InnerHtml.ToString());
        //                    Console.WriteLine("Results TBC");
        //                }
        //            }
        //        }

        //        nodeCount++;
        //    }

        //    Console.WriteLine("Daily meeting content nodes proccessing complete.");
        //    UnitOfWork.Complete();
        //}

        private static bool ProcessMeetingResultEntries(HtmlNode meetingResultNode)
        {
            //if (meetingResultNode.SelectNodes(".//tr") != null)
            //{
            //    var resultsNodes = meetingResultNode.SelectNodes(".//tr").Where(n => n.HasClass("results-place")).ToList();
            //    foreach (var horseResultNode in resultsNodes)
            //    {
            //        var newResultsEntry = new ResultEntry();
            //        //gather result entry nodes nodes
            //        var placeNode = horseResultNode.Descendants("td").FirstOrDefault();
            //        // jockey silks node is 2nd tr node, not extracting picture for obvious reasons
            //        var nameNode = horseResultNode.Descendants("td").Last().FirstChild;
            //        var fractionNode = horseResultNode.Descendants().Where(n => n.HasClass("price-fractional")).FirstOrDefault();
            //        var decimalNode = horseResultNode.Descendants().Where(n => n.HasClass("price-decimal")).FirstOrDefault();

            //        var horseName = ProcessHorseNameNode(HttpUtility.HtmlDecode(nameNode?.InnerHtml.ToString()), out var horseNo);

            //        newResultsEntry.HorseName = horseName;
            //        newResultsEntry.HorseNumber = horseNo;
            //        string processPos = Regex.Match(placeNode?.InnerHtml.ToString(), @"\d+").Value;
            //        newResultsEntry.Position = int.Parse(processPos);
            //        newResultsEntry.Place = placeNode?.InnerHtml.ToString();
            //        if (decimalNode != null)
            //            newResultsEntry.PriceDecimal = decimal.Parse(decimalNode?.InnerHtml.ToString());
            //        newResultsEntry.PriceFraction = fractionNode?.InnerHtml.ToString();
            //        newResultsEntry.MeetingResultId = meetingResult.Id;
            //        newResultsEntry.MeetingResult = meetingResult;

            //        meetingResult.ResultEntries.Add(newResultsEntry);

            //        Console.WriteLine(placeNode?.InnerHtml + "  " + HttpUtility.HtmlDecode(nameNode?.InnerHtml.ToString()) + "  " + fractionNode?.InnerHtml);
            //    }

            //}
            //else
            //{
            //    return false;
            //}

            return true;
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
                var horseName = horseTitleHtml.Substring(2, horseTitleHtml.Length - 2);
                var pattern = @" \((.*?)\)";
                return Regex.Replace(horseName, pattern, string.Empty);

            }
            else if (horseTitleHtml.ElementAt(2).ToString() == ".")
            {
                horseNumber = horseTitleHtml.Substring(0, 3);
                var horseName = horseTitleHtml.Substring(3, horseTitleHtml.Length - 3);
                var pattern = @" \((.*?)\)";
                return Regex.Replace(horseName, pattern, string.Empty);
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
            DailyMeetingTitles = dailyMeetingTitles;
        }


    }
}
