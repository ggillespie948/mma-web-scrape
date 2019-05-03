using AutoMapper;
using RacingWeb.API.DTOs;
using RacingWebScrape.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RacingWeb.API.Helpers
{
    public sealed class DTOHelper
    {
        private static DTOHelper instance = null;

        private DTOHelper()
        {

        }

        public static DTOHelper Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new DTOHelper();
                }
                return instance;
            }
        }

        public IEnumerable<MeetingResultDTO> MapMeetingResultsDTO(IEnumerable<MeetingResult> meetingResults,
            ref IMapper mapper)
        {
            return mapper.Map<IEnumerable<MeetingResult>, IEnumerable<MeetingResultDTO>>(meetingResults);
        }
    }

}
