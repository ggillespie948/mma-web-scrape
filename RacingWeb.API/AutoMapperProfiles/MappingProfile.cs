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
            CreateMap<ResultEntry, ResultEntryDTO>().ReverseMap()
                .ForMember(d => d.MeetingResult, opt => opt.MapFrom(s => s));

            CreateMap<MeetingResult, MeetingResultDTO>().ReverseMap()
                    .ForMember(d => d.ResultEntries, opt => opt.MapFrom(s => s));
        }
    }
}
