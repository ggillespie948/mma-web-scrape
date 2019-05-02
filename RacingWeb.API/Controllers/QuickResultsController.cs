using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public QuickResultsController(IRacingUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
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
                    return Json(UnitOfWork.CourseMeetings.GetTodaysMeetingResults());

                default:
                    return Json("no sub action hit.. try a different parameter");
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
