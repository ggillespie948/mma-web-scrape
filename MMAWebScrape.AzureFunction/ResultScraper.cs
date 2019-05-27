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
            EventDateNodes = EventContainer.SelectNodes(".//h3");

            ProcessEventResults();

            Console.WriteLine("Reslts complete");
        }

        /// <summary>
        /// This method takes the globally available EventResultsContent nodes and iterates over them, in the process attempting
        /// to create a new PromotionMeeting instance, checking for an existing record, before then creating or updating both the maincard and undercard 
        /// results. To be split up at a future point.
        /// </summary>
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
                    } 

                }

                //under card
                Console.WriteLine("adding under card nodes..");
                for (int i=1; i< (fightResultNodes.Count()-1); i+=2)
                {
                    if (fightResultNodes.Count >= i)
                    {
                        underCardNode.Add(fightResultNodes[i]);
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

        /// <summary>
        /// Parse the DateTime from given html node in '01/05/2019 00:00:00' format (sql)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private DateTime ProcessTimeNode(HtmlNode node)
        {
            string str = node.Descendants().FirstOrDefault().InnerHtml;
            return DateTime.Parse(str);
        }

        /// <summary>
        /// Method which returns the parsed title of the event E.G. Fight Night 123: Jose Aldo vs Joe Bloggs along
        /// with an out parameter which returns the storage ID for the represented promotion.
        /// </summary>
        /// <param name="titleNode"></param> - the HtmlNode element containing the title for the event
        /// <param name="promotionId"></param>
        /// <returns></returns>
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
       
    }
}
