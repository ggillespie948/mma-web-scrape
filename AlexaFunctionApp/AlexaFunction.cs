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

            //gather {slots}
            intentRequest.Intent.Slots.TryGetValue("Promotion", out var promotion);
            intentRequest.Intent.Slots.TryGetValue("EventDate", out var eventDate);
            intentRequest.Intent.Slots.TryGetValue("Day", out var day);

            var responseSpeach = "";

            log.LogInformation("Checking session..");
            if (session == null)
            {
                log.LogInformation("Session was null");
                session = new Session();
            } else
            {
                log.LogInformation("Session not null");
                // !!!!!!! replace promotion.Value with session promotion
                // process session to see if where u asked a reprompt for something
            }


            if (promotion.Value != null && promotion.Value != "" && promotion.Value != " " && promotion.Value.Length > 2)
            {
                if (eventDate.Value != null)
                {
                    DateTime date = DateTime.Now; // temp!!!
                    return GetPromotionEventByDate(log, date, session);
                }
                else if (day.Value != null)
                {
                    //get promotion results on most recent day

                    //if no promotion can be found on nearest given day date, then return a reprompt asking if they mean results title on given date

                    //if no promotion result can be found then reprompt 404 error response
                    return BuildNotFoundErrorResponse(session, true);

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
            else if(eventDate != null)
            {
                //get promotion results on eventDate
                DateTime date = DateTime.Now; // temp!!!!;
                return GetPromotionEventByDate(log, date, session);

                //if no promotion result can be found then reprompt 404 error response
                

            } else
            {
                //Could not parse any meaningful variables, reprompt
                var reprompt = new Reprompt();
                return BuildErrorResponse(session, reprompt, true);
            }

        }

        public static ObjectResult GetPromotionEventByDate(ILogger logger, DateTime date, Session session)
        {
            return BuildNotFoundErrorResponse(session, true);
        }

        //public static string GetDailyMeetings(ILogger log, out int numberOfMeetings)
        //{
        //    log.LogInformation("Get Daily Meetings..  ");
        //    var httpRequest = MakeRequest("/quickresults/dailyMeetings");
        //    log.LogInformation(httpRequest.Result);
        //    var result = JsonConvert.DeserializeObject<IEnumerable<CourseMeetingDTO>>(httpRequest.Result);

        //    string response = "";

        //    numberOfMeetings = result.Count();

        //    foreach(var courseMeeting in result)
        //    {
        //        response += " " + courseMeeting.Course.Name + ", ";
        //    }

        //    return response;
        //}


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
