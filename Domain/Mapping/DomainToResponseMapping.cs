using AutoMapper;
using Domain.Dto;
using Domain.Models;

namespace Domain.Mapping
{
    public class DomainToResponseMappingProfile : Profile
    {
        public DomainToResponseMappingProfile()
        {
            CreateMap<Event, EventDto>();
            CreateMap<Competitor, CompetitorDto>()
                .ForMember(c => c.TeamName, d => d.MapFrom(s => s.Team.TeamName))
                .ForMember(c => c.CountryShort, d => d.MapFrom(s => s.Country.CountryNameShort));
            //TODO: add eventnumber

            CreateMap<GameCompetitorEvent, DeelnemerDto>()
                .ForMember(c => c.DeelnemerNaam, d => d.MapFrom(s => s.GameCompetitor.FirstName + " " + s.GameCompetitor.LastName))
                .ForMember(c => c.PoolNaam, d => d.MapFrom(s => s.TeamName));

        }
    }
}
