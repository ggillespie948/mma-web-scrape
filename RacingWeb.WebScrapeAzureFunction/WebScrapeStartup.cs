using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using RacingWebScrape.Db;
using RacingWebScrape.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;


[assembly: FunctionsStartup(typeof(RacingWeb.WebScrapeAzureFunction.WebScrapeStartup))]
namespace RacingWeb.WebScrapeAzureFunction
{
    public class WebScrapeStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

            builder.Services
                .AddLogging()
                .AddDbContext<RacingDbContext>()
                .AddTransient<IRacingUnitOfWork, UnitOfWork>()
                .BuildServiceProvider();
        }
    }
}
