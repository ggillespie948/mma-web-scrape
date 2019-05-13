using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using RacingWebScrape;
using RacingWebScrape.UnitOfWork;

namespace RacingWeb.WebScrapeAzureFunction
{
    public class ScrapeResultsFunction
    {
        public static IRacingUnitOfWork UnitOfWork;
        public ScrapeResultsFunction(IRacingUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        [FunctionName("ScrapeResultsFunction")]
        public async Task Run([TimerTrigger("0 */15 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            DateTime startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0 );
            DateTime endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 21, 0, 0 );

            if(DateTime.Now > startTime && DateTime.Now < endTime)
            {
                var HTML_TARGET = Environment.GetEnvironmentVariable("HTML_TARGET", EnvironmentVariableTarget.Process); 
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(HTML_TARGET);
                var pageContents = await response.Content.ReadAsStringAsync();
                HtmlDocument pageDocument = new HtmlDocument();
                pageDocument.LoadHtml(pageContents);

                ResultScraper Scraper = new ResultScraper(UnitOfWork);
                Scraper.ScrapeQuickResults(pageDocument);

            } else
            {
                log.LogInformation("Web Scrape only active 12:00-21:00");
            }


            log.LogInformation($"C# Timer trigger function completed at: {DateTime.Now}");
        }
    }
}
