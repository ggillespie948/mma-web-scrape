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
            var response = mapper.Map<IEnumerable<MeetingResult>, IEnumerable<MeetingResultDTO>>(meetingResults);

            return response;
        }

        public MeetingResultDTO MapMeetingResult(MeetingResult meetingResult,
            ref IMapper mapper)
        {
            var response = mapper.Map<MeetingResult, MeetingResultDTO>(meetingResult);

            return response;
        }

        public ResultEntryDTO MapResultEntryDTO(ResultEntry resultEntry,
            ref IMapper mapper)
        {
            var response = mapper.Map<ResultEntry, ResultEntryDTO>(resultEntry);

            return response;
        }

        
    }

}
