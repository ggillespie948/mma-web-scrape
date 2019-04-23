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
            HttpClient client = new HttpClient();
            var response = await client.GetAsync("https://www.timeform.com/horse-racing/results/yesterday");

            var pageContents = await response.Content.ReadAsStringAsync();
            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(pageContents);

            

            Console.WriteLine("Daily Meeting content nodes: " + DailyMeetingContentNodes.Count);
            Console.ReadLine();
        }

        



    }
}
