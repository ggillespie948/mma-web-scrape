using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET;
using Microsoft.IdentityModel.Protocols;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Alexa.NET.Security.Functions;
using MMAWeb.Db;

namespace MMAWebScrapeAlexaAzureFunction
{
    public class AlexaFunction
    {
        public static MMADbContext DbContext;
        public AlexaFunction(MMADbContext unitOfWork)
        {
            DbContext = unitOfWork;
        }

        [FunctionName("Alexa")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed an alexa post request.");

            string json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);

            //Validate Request
            // Verifies that the request is a valid request from Amazon Alexa 
            var isValid = await skillRequest.ValidateRequestAsync(req, log);
            if (!isValid)
                return new BadRequestResult();

            SkillResponse response = null;

            Session session = skillRequest.Session;

            switch (skillRequest.Request)
            {
                case LaunchRequest launchRequest: return HandleLaunch(launchRequest, log);
                case IntentRequest intentRequest: return HandleIntent(intentRequest, session, log);
            }
            
            return new OkObjectResult(response);
        }

        private static ObjectResult HandleLaunch(LaunchRequest launchRequest, ILogger log)
        {
            log.LogInformation("Handle launch request");
            var responseSpeach = "";
            responseSpeach += "Welcome to MMA Results! "; //add in a question for repropt

            Session session = new Session();

            if (session.Attributes == null)
                session.Attributes = new Dictionary<string, object>();

            Reprompt reprompt = new Reprompt(responseSpeach);

            return BuildResponse(responseSpeach, true, session, reprompt);
        }

        private static ObjectResult HandleIntent(IntentRequest intentRequest, Session session, ILogger log)
        {
            log.LogInformation("Handle intent request");
            log.LogInformation("Init HTTP Client..");

            //Check intentRequest for user session
            log.LogInformation("Checking session..");
            if (session == null)
            {
                log.LogInformation("Session was null");
                session = new Session();
            }
            else
            {
                log.LogInformation("Session not null");
                // !!!!!!! replace promotion.Value with session promotion
                // process session to see if where u asked a reprompt for something
            }


            //handle intent request
            switch (intentRequest.Intent.Name)
            {
                case "QuickResults":
                    return HandleQuickResultsIntent(intentRequest, session, log);

                case "Amazon.YesIntent":
                    return HandleYesNoIntent(intentRequest, session, log);

                case "Amazon.NoIntent":
                    return HandleYesNoIntent(intentRequest, session, log);

                case "Amazon.StopIntent":
                    break;

                case "Amazon.HelpIntent":
                    break;

                default:
                    //Could not parse any meaningful variables, reprompt
                    var reprompt = new Reprompt();
                    return BuildErrorResponse(session, reprompt, true);
            }

            var errorReprompt = new Reprompt();
            return BuildErrorResponse(session, errorReprompt, true);
        }

        public static ObjectResult HandleYesNoIntent(IntentRequest intentRequest, Session session, ILogger log)
        {
            if(session.Attributes["isMostRecentPromotionEventPrompt"] != null)
            {
                // Set this session attribute to null so this yes/no intent is not 
                // trigger during future yesNo in same session
                session.Attributes["isMostRecentPromotionEventPrompt"] = null;

                if (session.Attributes["promotionId"] != null)
                    return GetMostRecentPromotionEvent(int.Parse(session.Attributes["promotionId"].ToString()));

                return BuildErrorResponse(session, null, true);


            } else if (session.Attributes["somethingelse"] != null) //*
            {
                return BuildErrorResponse(session, null, true);//*

            } else if (session.Attributes["something"] != null)//*
            {
                return BuildErrorResponse(session, null, true);//*
            } else
            {
                return BuildErrorResponse(session, null, true);//*
            }
        }

        
       

        public static ObjectResult HandleQuickResultsIntent(IntentRequest intentRequest, Session session, ILogger log)
        {
            //gather {slots}
            intentRequest.Intent.Slots.TryGetValue("Promotion", out var promotion);
            intentRequest.Intent.Slots.TryGetValue("EventDate", out var eventDate);
            intentRequest.Intent.Slots.TryGetValue("Day", out var day);

            if (promotion.Value != null && promotion.Value != "" && promotion.Value != " " && promotion.Value.Length > 2)
            {
                if (eventDate.Value != null)
                {
                    DateTime date = DateTime.Now; // temp!!!
                    return GetPromotionEventByDate(log, date, session);
                }
                else if (day.Value != null)
                {
                    return GetPromotionResultsByDay(day.Value.ToString());
                }
                else
                {
                    var reprompt = new Reprompt();
                    reprompt.OutputSpeech = new PlainTextOutputSpeech
                    {
                        Text = "Would you like to hear the most recent " + promotion.Value.ToString() + " results?"
                    };

                    return BuildResponse(reprompt.OutputSpeech.ToString(), false, session, reprompt);
                }

            }
            else if (eventDate != null)
            {
                //get promotion results on eventDate
                DateTime date = DateTime.Now; // temp!!!!;
                return GetPromotionEventByDate(log, date, session);
            }
            else
            {
                //Could not parse any meaningful variables, reprompt
                var reprompt = new Reprompt();
                return BuildErrorResponse(session, reprompt, true);
            }
        }



        #region MMADataServiceCalls

        public static ObjectResult GetPromotionEventByDate(ILogger logger, DateTime date, Session session)
        {
            return BuildNotFoundErrorResponse(session, true);
        }

        public static ObjectResult GetMostRecentPromotionEvent(int promotionId)
        {
            throw new NotImplementedException();
            // make call to method in MMA data serivce and retunr the results
        }

        public static ObjectResult GetPromotionResultsByDay (string day)
        {
            throw new NotImplementedException();
            //return BuildNotFoundErrorResponse(session, true);

            //parse day from string value payoff
            // make call to method in MMA data serivce and retunr the results
            //get promotion results on most recent day
            //if no promotion can be found on nearest given day date, then return a reprompt asking if they mean results title on given date
            //if no promotion result can be found then reprompt 404 error response
        }

        #endregion

        #region Generic
        private static string ConvertTimeTo24Hour(string raceTime, ref ILogger log)
        {
            //check if numbers left of colon parsed are less than 12
            try
            {
                log.LogInformation("trying to parse:   " + raceTime.Substring(0, 2));
                int givenTime = int.Parse(raceTime.Substring(0, 2), NumberStyles.Any);

                if (givenTime < 12)
                    givenTime = givenTime + 12;
                else
                    return raceTime;

                string newRaceTime = givenTime.ToString() + ":" + raceTime.Substring(3, 2);
                log.LogInformation("Converted race time string >" + newRaceTime + "<");

                return newRaceTime;

            } catch (Exception e)
            {
                log.LogError("Error Line Time Conversion: Could not parse time value ");
                return raceTime;
            }
        }

        private static ObjectResult BuildErrorResponse(Session sessionAttributes, Reprompt reprompt, bool endSession)
        {
            SkillResponse skillResponse = new SkillResponse();

            skillResponse = ResponseBuilder.Ask(
                new PlainTextOutputSpeech
                {
                    Text = "Sorry, I couldn't understand that. Try asking me again or in a different way.",
                },
                reprompt,
                sessionAttributes
            );

            skillResponse.Response.ShouldEndSession = endSession;

            return new BadRequestObjectResult(skillResponse);
        }

        private static ObjectResult BuildNotFoundErrorResponse(Session sessionAttributes, bool endSession)
        {
            SkillResponse skillResponse = new SkillResponse();

            skillResponse = ResponseBuilder.Tell(
                new PlainTextOutputSpeech
                {
                    Text = "Sorry, I couldn't find any results based on the information you provided. Try asking my again. ",
                }
            );

            skillResponse.Response.ShouldEndSession = endSession;

            return new BadRequestObjectResult(skillResponse);
        }

        private static ObjectResult BuildResponse(string responseSpeach, bool endSession, Session sessionAttributes, Reprompt reprompt)
        {
            SkillResponse skillResponse = new SkillResponse();

            skillResponse = ResponseBuilder.Ask(
                new PlainTextOutputSpeech
                {
                    Text = responseSpeach,
                },
                reprompt,
                sessionAttributes
            );

            skillResponse.Response.ShouldEndSession = endSession;

            return new OkObjectResult(skillResponse);
        }

        private static ObjectResult BuildResponse(string responseSpeach)
        {
            SkillResponse skillResponse = new SkillResponse();
            skillResponse = ResponseBuilder.Tell(new PlainTextOutputSpeech()
            {
                Text = responseSpeach,
            });

            skillResponse.Response.ShouldEndSession = true;

            return new OkObjectResult(skillResponse);
        }

        #endregion

    }
}
