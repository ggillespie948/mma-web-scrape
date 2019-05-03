using AutoMapper;
using RacingWeb.API.DTOs;
using RacingWebScrape.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RacingWeb.API.Helpers
{
    public static class DTOHelper
    {
        public static void ConvertToDTo(Object obj, ref Mapper mapper)
        {
            switch (obj)
            {
                case MeetingResult meetingResult: return;
                case MeetingResult meetingResults: return;
                case ResultEntry meetingResult: return;
            }


        }

        private static IEnumerable<MeetingResultDTO> MapMeetingResultsDTO(IEnumerable<MeetingResult> meetingResults, ref IMapper mapper)
        {
            

            foreach(var meetingResult in meetingResults)
            {
                mapper.Map<MeetingResult>
                
            }

        }

        private static void MapMeetingResultDTO()
        {

        }
    }
