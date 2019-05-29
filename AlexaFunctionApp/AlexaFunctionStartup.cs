using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MMAWeb.Db;
using System;
using System.Collections.Generic;
using System.Text;


[assembly: FunctionsStartup(typeof(RacingWeb.AlexaAzureFunction.AlexaFunctionStartup))]
namespace RacingWeb.AlexaAzureFunction
{
    public class AlexaFunctionStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddLogging()
                .AddDbContext<MMADbContext>()
                .BuildServiceProvider();
        }
    }
}
