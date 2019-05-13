using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using RacingWebScrape.Db;
using RacingWebScrape.Models;
using RacingWebScrape.Models.Courses;
using RacingWebScrape.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RacingWebScrape
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        async static Task MainAsync(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddDbContext<RacingDbContext>()
                .AddTransient<IRacingUnitOfWork, UnitOfWork.UnitOfWork>()
                .BuildServiceProvider();

            Console.WriteLine("Enter target scrape URL: ");
            var HTML_TARGET = Console.ReadLine();


            if(HTML_TARGET == "" || HTML_TARGET == " ")
            {
                Console.WriteLine("ERROR: NULL HTML TARGET PARAMETER. PRESS ANY KET TO EXIT");

            } else
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(HTML_TARGET);
                var pageContents = await response.Content.ReadAsStringAsync();
                HtmlDocument pageDocument = new HtmlDocument();
                pageDocument.LoadHtml(pageContents);

                ResultScraper Scraper = new ResultScraper(serviceProvider.GetService<IRacingUnitOfWork>());
                Scraper.ScrapeQuickResults(pageDocument);

                Console.ReadLine();
            }

        }
    }
}
