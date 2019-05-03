using AutoMapper;
using RacingWeb.API.DTOs;
using RacingWebScrape.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RacingWeb.API.MappingProfile
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<MeetingResult, MeetingResultDTO>().ReverseMap();
            CreateMap<ResultEntry, ResultEntryDTO>().ReverseMap();
        }
    }
}
