using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RacingWeb.API.Helpers;
using RacingWebScrape.Models;
using RacingWebScrape.UnitOfWork;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RacingWeb.API.Controllers
{
    /// <summary>
    /// This controller handles HTML requests primarily from an alexa skill which returns basic overview result quieries
    /// such as: Today's racing results, The names of today's meetings, the result 
    /// </summary>
    [Route("api/[controller]")]
    public class QuickResultsController : Controller
    {
        public static IRacingUnitOfWork UnitOfWork;
        private static IMapper _mapper;
        public QuickResultsController(IRacingUnitOfWork unitOfWork, IMapper mapper)
        {
            UnitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get()
        {
            return Json(DTOHelper.Instance.MapMeetingResultsDTO(UnitOfWork.CourseMeetings.GetTodaysMeetingResults(), ref _mapper));
        }

        // GET api/<controller>/meetingnames
        [HttpGet("{subAction}")]
        public IActionResult Get(string subAction)
        {
            switch(subAction)
            {
                case "dailyMeetings":
                    return Json(UnitOfWork.CourseMeetings.GetTodaysMeetings());

                case "dailyResults":
                    return Json(DTOHelper.Instance.MapMeetingResultsDTO(UnitOfWork.CourseMeetings.GetTodaysMeetingResults(), ref _mapper));

                default:
                    return Json("Error: 404");
            }
        }


        // GET api/<controller>/racetime/15:40
        [HttpGet("{subAction}/{parameter}")]
        public IActionResult Get(string subAction, string parameter)
        {
            switch (subAction)
            {
                case "racetime":
                    if(DateTime.Parse(parameter) == null)
                    {
                        return Json("Error 300: null time Parameter");
                    } else
                    {
                        return Json(DTOHelper.Instance.MapMeetingResult(UnitOfWork.CourseMeetings.GetDailyMeetingResultByTime(DateTime.Parse(parameter)), ref _mapper));
                    }

                case "course":
                    var course = UnitOfWork.Courses.GetByName(parameter);
                    if (course != null)
                    {
                        return Json(DTOHelper.Instance.MapMeetingResultsDTO(UnitOfWork.CourseMeetings.GetMeetingResultsByCourse(course.Id, DateTime.Today), ref _mapper));
                    } else { return Json("Error: course meeting not found"); }

                        default:
                    return Json(null);
            }
        }

        // course/kempton/race/1
        // course/kelso/racetime/12:00
        [HttpGet("{subAction}/{parameter}/{subAction2}/{parameter2}")]
        public IActionResult Get(string subAction, string parameter, string subAction2, string parameter2)
        {
            switch (subAction)
            {
                case "course":
                    if(parameter != null) 
                    {
                        var course = UnitOfWork.Courses.GetByName(parameter);
                        if (course != null)
                        {
                            switch(subAction2)
                            {
                                case "racetime":
                                    if (DateTime.Parse(parameter2) == null)
                                    {
                                        return Json("Error 300: null time Parameter");
                                    }
                                    else
                                    {
                                        return Json(DTOHelper.Instance.MapMeetingResult(UnitOfWork.CourseMeetings.GetDailyMeetingResultByTime(DateTime.Parse(parameter2)), ref _mapper));
                                    }

                                case "race":
                                    if(parameter2 != null)
                                    {
                                        try
                                        {
                                            int raceNo = int.Parse(parameter2);
                                            return Json(DTOHelper.Instance.MapMeetingResult(UnitOfWork.CourseMeetings.GetMeetingResultByRaceNo(course.Id, raceNo), ref _mapper));
                                        } catch (Exception e)
                                        {
                                            return Json("Race number parameter could not be parsed. Try '1' ");
                                        }

                                    } else
                                    {
                                        return Json("Error 300: null race number parameter");
                                    }

                                case null:
                                    return Json("Null request");
                            }
                        }
                    }
                    return Json(UnitOfWork.CourseMeetings.GetTodaysMeetings());

                default:
                    return Json("Error: 404");
            }
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
