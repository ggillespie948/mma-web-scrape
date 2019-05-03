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
            return Json(UnitOfWork.CourseMeetings.GetTodaysMeetingResults());
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
