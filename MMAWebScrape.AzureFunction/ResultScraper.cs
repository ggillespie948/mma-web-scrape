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
using MMAWebScrape.AzureFunction;

namespace MMAWebScrape
{
    /// <summary>
    /// Collection of methods and data which scrape racing results from a given html document
    /// </summary>
    public class ResultScraper
    {
        private static MMADbContext DbContext;
        public ResultScraper(MMADbContext unitOfWork)
        {
            DbContext = unitOfWork;
        }
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
             
            // try adding the split searches here ??? ***

            EventResultsContentNodes = EventContainer.SelectNodes(".//div[@class='m-mmaf-pte-event-list__split-item']");
            EventTitleNodes = EventContainer.SelectNodes(".//h2");
            EventDateNodes = EventContainer.SelectNodes(".//h3");

            ProcessEventResults();

            Console.WriteLine("Reslts complete");
        }

        private void ProcessEventResults()
        {
            var counter = 0;
            foreach(var node in EventResultsContentNodes.Elements())
            {
                Console.WriteLine("Promotion meeting process entry");
                var promotionMeeting = new PromotionMeeting();

                if(!(EventDateNodes.Count > counter))
                {
                    Console.WriteLine("End of results.");
                    break;
                }

                DateTime promotionDate = ProcessTimeNode(EventDateNodes.ElementAt(counter));
                promotionMeeting.Title = ProcessEventTitle(EventTitleNodes.ElementAt(counter), out int promotionId);


                Console.WriteLine("Checking for existing promotion meeting..");
                if (MMADataService.DoesPromotionMeetingExist(promotionDate, promotionId, ref DbContext))
                {
                    promotionMeeting = MMADataService.GetPromotionMeetingByDate(promotionDate, promotionId, ref DbContext);
                    if (promotionMeeting.FightResults != null)
                    {
                        promotionMeeting.FightResults.Clear(); //update with latest results
                    } else
                    {
                        promotionMeeting.FightResults = new List<FightResult>();
                    }
                    Console.WriteLine("found..");

                } else
                {
                    Console.WriteLine("new promotion meeting..");
                    // assign promoiton via id search
                    promotionMeeting.PromotionId = promotionId;
                    promotionMeeting.Date = promotionDate;
                    promotionMeeting.FightResults = new List<FightResult>();

                    MMADataService.AddPromotionMeeting(promotionMeeting, ref DbContext);
                    MMADataService.SaveChanges(ref DbContext);
                }

                Console.WriteLine("Get fight result nodes..");
                // get results nodes
                var fightResultNodes = node.SelectNodes("//div[@class='m-mmaf-pte-event-list__split']"); //[1] mc, [2] uc [3] mc 

                var mainCardNode = new List<HtmlNode>();
                var  underCardNode = new List<HtmlNode>();

                //main card
                Console.WriteLine("adding main card nodes..");
                for (int i = 0; i < fightResultNodes.Count(); i += 2)
                {
                    if(fightResultNodes.Count >= i)
                    {
                        mainCardNode.Add(fightResultNodes[i]);
                    } else
                    {
                        Console.WriteLine("wtf");
                    }

                }

                //under card
                Console.WriteLine("adding under card nodes..");
                for (int i=1; i< (fightResultNodes.Count()-1); i+=2)
                {
                    if (fightResultNodes.Count >= i)
                    {
                        underCardNode.Add(fightResultNodes[i]);
                    } else
                    {
                        Console.WriteLine("wtf");
                    }
                }

                if (mainCardNode.Count > counter)
                {
                    //process maincard
                    foreach (var fightResultNode in mainCardNode[counter].Descendants("li"))
                    {
                        var fightResult = new FightResult();

                        fightResult.PromotionMeeting = promotionMeeting;
                        fightResult.IsMainCard = true;

                        if(ProcessFighterNames(fightResultNode.Descendants("a").FirstOrDefault().InnerHtml, out string[] fighterNames))
                        {
                            fightResult.FighterNameA = fighterNames[0];
                            fightResult.FighterNameB = fighterNames[1];
                        }

                        //sometimes there is an additional <span> if the fight is a title fight
                        if(fightResultNode.Descendants("span").Count() == 1)
                        {
                            fightResult.DecisionSummary = ProcessFightDecision(fightResultNode.Descendants("span").FirstOrDefault().InnerHtml);

                        } else if (fightResultNode.Descendants("span").Count() == 2)
                        {
                            fightResult.DecisionSummary = ProcessFightDecision(fightResultNode.Descendants("span").LastOrDefault().InnerHtml);
                        }

                        promotionMeeting.FightResults.Add(fightResult);
                    
                    }
                }


                Console.WriteLine("main card processed");

                if (underCardNode.Count > counter)
                {
                    //process undercard
                    foreach (var fightResultNode in underCardNode[counter].Descendants("li"))
                    {
                        var fightResult = new FightResult();

                        fightResult.PromotionMeeting = promotionMeeting;
                        fightResult.IsMainCard = false;

                        if (ProcessFighterNames(fightResultNode.Descendants("a").FirstOrDefault().InnerHtml, out string[] fighterNames))
                        {
                            fightResult.FighterNameA = fighterNames[0];
                            fightResult.FighterNameB = fighterNames[1];
                        }

                        //sometimes there is an additional <span> if the fight is a title fight
                        if (fightResultNode.Descendants("span").Count() == 1)
                        {
                            fightResult.DecisionSummary = ProcessFightDecision(fightResultNode.Descendants("span").FirstOrDefault().InnerHtml);

                        }
                        else if (fightResultNode.Descendants("span").Count() == 2)
                        {
                            fightResult.DecisionSummary = ProcessFightDecision(fightResultNode.Descendants("span").LastOrDefault().InnerHtml);
                        }

                        promotionMeeting.FightResults.Add(fightResult);
                    }
                }

                Console.WriteLine("under card processed");

                MMADataService.UpdatePromotionMeeting(promotionMeeting, ref DbContext);
                MMADataService.SaveChanges(ref DbContext);

                counter++;
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
            decisionString = decisionString.Replace("def ", "defeated ");
            return decisionString;
        }

        private DateTime ProcessTimeNode(HtmlNode node)
        {
            string str = node.Descendants().FirstOrDefault().InnerHtml;
            return DateTime.Parse(str);
        }

        private string ProcessEventTitle(HtmlNode titleNode, out int promotionId)
        {
            string eventTitle = titleNode.Descendants("a").FirstOrDefault().InnerHtml;
            if (eventTitle.Contains("UFC"))
            {
                promotionId = 6;
            } else if (eventTitle.Contains("Bellator"))
            {
                promotionId = 7;
            } else if (eventTitle.Contains("One"))
            {
                promotionId = 8;
            } else if (eventTitle.Contains("Invicta"))
            {
                promotionId = 9;
            } else if (eventTitle.Contains("PFL"))
            {
                promotionId = 10;
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
        }


    }
}
