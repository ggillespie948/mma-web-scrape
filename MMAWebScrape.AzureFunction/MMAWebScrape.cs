using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MMAWeb;
using MMAWeb.Db;

namespace MMAWeb.AzureFunction
{
    public class MMAWebScrape
    {
        public static string WebApiAddress { get; set; }

        public static MMADbContext DbContext;
        public MMAWebScrape(MMADbContext unitOfWork)
        {
            DbContext = unitOfWork;
        }

        [FunctionName("ScrapeResultsFunction")]
        public async Task Run([TimerTrigger("0 */10 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            DateTime startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 0, 0);
            DateTime endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 21, 0, 0);

            if (DateTime.Now > startTime && DateTime.Now < endTime)
            {
                var HTML_TARGET = Environment.GetEnvironmentVariable("HTML_TARGET", EnvironmentVariableTarget.Process);
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(HTML_TARGET);
                var pageContents = await response.Content.ReadAsStringAsync();
                HtmlDocument pageDocument = new HtmlDocument();
                pageDocument.LoadHtml(pageContents);

                //ResultScraper Scraper = new ResultScraper(UnitOfWork);
                //Scraper.ScrapeQuickResults(pageDocument);

            }
            else
            {
                log.LogInformation("Web Scrape only active 13:00-21:00");
            }


            //make call to Web API to keep it warm
            WebApiAddress = Environment.GetEnvironmentVariable("WEB_API_ADDRESS", EnvironmentVariableTarget.Process);

            if (WebApiAddress == null)
            {
                WebApiAddress = "";
                log.LogError("Local web api address has not been set.");
            }

            var httpRequest = MakeRequest("/quickresults/dailyMeetings");

            log.LogInformation("Keeping Web API Warm.. :" + httpRequest.ToString());

            log.LogInformation($"C# Timer trigger function completed at: {DateTime.Now}");
        }

        private static async Task<string> MakeRequest(string parameter)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(WebApiAddress + parameter);

            return await response.Content.ReadAsStringAsync();
        }
    }
}
