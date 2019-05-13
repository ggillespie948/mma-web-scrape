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
using RacingWeb.API.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace RacingWebScrapeAlexaAzureFunction
{
    public static class Function1
    {
        private static string WebApiAddress { get; set; }

        [FunctionName("Alexa")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed an alexa post request.");

            string json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);

            SkillResponse response = null;

            WebApiAddress = Environment.GetEnvironmentVariable("WEB_API_ADDRESS", EnvironmentVariableTarget.Process);

            if (WebApiAddress == null)
            { 
                WebApiAddress = "";
                log.LogError("Local web api address has not been set.");
            }

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
            responseSpeach += "Welcome to Horse Racing Results! ";

            var dailyMeetings = GetDailyMeetings(log, out int count);

            responseSpeach += "There are " + count + " meetings in the UK today. They go at " + dailyMeetings + ".  ";
            responseSpeach += "For results, ask me for a specific time, course, or race number. ";

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
            intentRequest.Intent.Slots.TryGetValue("Course", out var course);
            intentRequest.Intent.Slots.TryGetValue("RaceTime", out var raceTime);
            intentRequest.Intent.Slots.TryGetValue("RaceNumber", out var raceNo);

            var responseSpeach = "";

            // Decison Matrix Variables
            // C T N - Course, raceTime, raceNumber

            // Intent Base Options
            // -----------------
            // C - summary of course results
            // C+T - {result}
            // C+N - {result}
            // T - {result}
            // [NONE] - give message explaining and repromt for a query

            // Intent Repromprt Options
            // ------------------------
            // N - reprompt course
            // repromt yes to inital launch request - {what racing is on today?}


            log.LogInformation("Checking session..");
            if (session == null)
            {
                log.LogInformation("Session was null");
                session = new Session();
            } else
            {
                log.LogInformation("Session not null");
            }

            if (course.Value != null && course.Value != "" && course.Value != " " && course.Value != "race" && course.Value.Length > 2)
            {
                if(raceTime.Value != null)
                {
                    return GetResultByRaceTime(ConvertTimeTo24Hour(raceTime.Value, ref log), log);

                } else if (raceNo.Value != null)
                {
                    return GetResultByRaceNumberAtCourse(raceNo.Value, course.Value, log);

                } else
                {
                    return GetResultsSummaryAtCourse(course.Value, log);
                }

            } else if(raceTime.Value != null)
            {
                return GetResultByRaceTime(ConvertTimeTo24Hour(raceTime.Value, ref log), log);

            } else if(raceNo != null)
            {
                // *** prompt the {raceNo} at which course?    *** 
                log.LogInformation("Race time not null, which course prompt!!! ");
                intentRequest.DialogState = DialogState.InProgress;
                responseSpeach += "Race " + raceNo.Value + " at which course?";

                if (session.Attributes == null)
                    session.Attributes = new Dictionary<string, object>();

                session.Attributes["raceNumberCourseReprompt"] = raceNo.Value;
                Reprompt reprompt = new Reprompt(responseSpeach);

                return BuildResponse(responseSpeach, false, session, reprompt);

            } else
            {
                //Could not handle intent
                log.LogInformation("Could not handle intent");
                responseSpeach += "Sorry I couldn't understand that. Ask for help for a list of commands.";
                return BuildResponse(responseSpeach);
            }

        }

        public static ObjectResult GetResultByRaceTime(string raceTime, ILogger log)
        {
            log.LogInformation("Get Results by race time:  Q: " + raceTime );
            var responseSpeach = "";

            //search for race at specific time
            var httpRequest = MakeRequest("/quickresults/racetime/" + raceTime);
            log.LogInformation(httpRequest.Result);
            var result = JsonConvert.DeserializeObject<MeetingResultDTO>(httpRequest.Result);

            bool winnerAnnounced = false;
            if(result != null)
            {
                foreach (var resultEntry in result.ResultEntries.OrderBy(i => i.Position))
                {
                    if (winnerAnnounced)
                    {
                        responseSpeach += "In " + resultEntry.Place.Substring(0, resultEntry.Place.Length) + " place was number " + resultEntry.HorseNumber + " " + resultEntry.HorseName + ".  ";
                    }
                    else
                    {
                        responseSpeach += (" The winner of the race was number " + resultEntry.HorseNumber + ", " + resultEntry.HorseName + ". ");
                        responseSpeach += (" Trained by " + result.WinningTrainer + ", Ridden by " + result.WinningJockey + ". ");
                        winnerAnnounced = true;
                    }
                }
            } else
            {
                // null result, convert time to 24 hr format?
                responseSpeach += "Sorry, there are no results for the " + raceTime + " yet. ";
                return BuildResponse(responseSpeach);
            }

            if(!winnerAnnounced)
            {
                responseSpeach += "Sorry, there are no results for the " + raceTime + " yet.";
                return BuildResponse(responseSpeach);

            } else
            {
                return BuildResponse(responseSpeach);
            }

        }

        public static ObjectResult GetResultByRaceNumberAtCourse(string raceNumber, string course, ILogger log)
        {
            log.LogInformation("Get Results by race number at course:>" + course + "< N:" + raceNumber);
            var responseSpeach = "";

            var httpRequest = MakeRequest("/quickresults/course/" + course + "/race/" + raceNumber);
            log.LogInformation(httpRequest.Result);
            var result = JsonConvert.DeserializeObject<MeetingResultDTO>(httpRequest.Result);

            if (result != null && result.ResultEntries.Any())
            {
                responseSpeach += "The results of race " + raceNumber + " at " + course + " today are: ";

                bool winnerAnnounced = false;
                foreach (var resultEntry in result.ResultEntries.OrderBy(i => i.Position))
                {
                    if (winnerAnnounced)
                    {
                        responseSpeach += "In " + resultEntry.Place.Substring(0, resultEntry.Place.Length) + " place was number " + resultEntry.HorseNumber + " " + resultEntry.HorseName + ".  ";
                    }
                    else
                    {
                        responseSpeach += (" The winner of the race was number " + resultEntry.HorseNumber + ", " + resultEntry.HorseName + ". ");
                        responseSpeach += (" Trained by " + result.WinningTrainer + ", Ridden by " + result.WinningJockey + ". ");
                        winnerAnnounced = true;
                    }
                }
            }
            else
            {
                responseSpeach += "There are no results for race " + raceNumber + "at " + course + "yet. Would you like to hear an overview of the results at " + course +" so far today?";
                return BuildResponse(responseSpeach);
            }

            return BuildResponse(responseSpeach);
        }

        public static string GetDailyMeetings(ILogger log, out int numberOfMeetings)
        {
            log.LogInformation("Get Daily Meetings..  ");
            var httpRequest = MakeRequest("/quickresults/dailyMeetings");
            log.LogInformation(httpRequest.Result);
            var result = JsonConvert.DeserializeObject<IEnumerable<CourseMeetingDTO>>(httpRequest.Result);

            string response = "";

            numberOfMeetings = result.Count();

            foreach(var courseMeeting in result)
            {
                response += " " + courseMeeting.Course.Name + ", ";
            }

            return response;
        }

        public static ObjectResult GetResultsSummaryAtCourse(string course, ILogger log)
        {
            log.LogInformation("Get reslts summary at course  Q: " + course);
            var responseSpeach = "";

            var httpRequest = MakeRequest("/quickresults/course/" + course);
            log.LogInformation(httpRequest.Result);

            if(httpRequest.Result.ToString() == "\"Error: course meeting not found\"")
            {
                responseSpeach += "The course, " + course + ", could not be found in todays meetings. Try asking me again, or ask me what meetings are on today."; 
                return BuildResponse(responseSpeach);

            } else
            {
                var result = JsonConvert.DeserializeObject<IEnumerable<MeetingResultDTO>>(httpRequest.Result);

                if (result != null)
                {
                    if (result.Any())
                    {
                        responseSpeach += "The results at" + course + " today are: ";

                        foreach (var meetingResult in result.Where(i => i.ResultEntries != null && i.ResultEntries.Count > 0))
                        {
                            responseSpeach += (" The winner of the " + meetingResult.RaceTime.ToShortTimeString() + " was number " + meetingResult.ResultEntries.OrderBy(i => i.Position).First().HorseNumber + ", " + meetingResult.ResultEntries.OrderBy(i => i.Position).First().HorseName + ".");
                        }
                    } else
                    {
                        responseSpeach += "There are no results at " + course + "yet.";  //" Would you like to hear an overview of the races at " + course + " today?";

                        return BuildResponse(responseSpeach);

                    }

                }
                else
                {
                    responseSpeach += "There are no results at " + course + "yet.";  //" Would you like to hear an overview of the races at " + course + " today?";

                    return BuildResponse(responseSpeach);
                }


                //Read results
                return BuildResponse(responseSpeach);
            }
        }


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

        private static async Task<string> MakeRequest(string parameter)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(WebApiAddress + parameter);

            return await response.Content.ReadAsStringAsync();
        }
        #endregion

    }
}
