using AutoMapper;
using CycleManager.Domain.Dto;
using Domain.Dto;
using Domain.Models;

namespace Domain.Mapping
{
    public class DomainToResponseMappingProfile : Profile
    {
        public DomainToResponseMappingProfile()
        {
            CreateMap<Event, EventDto>();
            CreateMap<Event, EventForUserDto>();
            CreateMap<Competitor, CompetitorDto>()
                .ForMember(c => c.TeamName, d => d.MapFrom(s => s.Team.TeamName))
                .ForMember(c => c.CountryShort, d => d.MapFrom(s => s.Country.CountryNameShort));
            
            CreateMap<CompetitorsInEvent, CompetitorDto>()
                .ForMember(c => c.CompetitorName, d => d.MapFrom(s => s.Competitor.CompetitorName))
                .ForMember(c => c.TeamName, d => d.MapFrom(s => s.Competitor.Team.TeamName))
                .ForMember(c => c.CountryShort, d => d.MapFrom(s => s.Competitor.Country.CountryNameShort));

            CreateMap<GameCompetitorEvent, DeelnemerDto>()
                .ForMember(c => c.DeelnemerNaam, d => d.MapFrom(s => s.User.FirstName + " " + s.User.LastName))
                .ForMember(c => c.PoolNaam, d => d.MapFrom(s => s.TeamName));
            CreateMap<GameCompetitorEventPick, ResultDto>()
                            .ForMember(c => c.CompetitorName, d => d.MapFrom(s => s.CompetitorsInEvent.Competitor.CompetitorName))
                            .ForMember(c => c.CompetitorTeam, d => d.MapFrom(s => s.CompetitorsInEvent.Competitor.Team.TeamName))
                            .ForMember(c => c.CompetitorInEventId, d => d.MapFrom(s => s.CompetitorsInEvent.Id))
                            .ForMember(c => c.OutOfCompetition, d => d.MapFrom(s => s.CompetitorsInEvent.OutOfCompetition))
                            .ForMember(c => c.CountryCode, d => d.MapFrom(s => s.CompetitorsInEvent.Competitor.Country.CountryNameShort));
        }
    }
}
