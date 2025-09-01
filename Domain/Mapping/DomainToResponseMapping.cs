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
            CreateMap<Event, EventDto>()
                .ForMember(c => c.Stages, d => d.MapFrom(s => s.Stages))
                .ForMember(c => c.Deelnemers, d => d.MapFrom(s => s.GameCompetitorEvents));
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
                            .ForMember(c => c.CountryCode, d => d.MapFrom(s => s.CompetitorsInEvent.Competitor.Country.CountryNameShort))
                            .ForMember(c => c.PcsName, d => d.MapFrom(s => s.CompetitorsInEvent.Competitor.PcsName));

            CreateMap<GameCompetitorEventPick, CompetitorDto>()
                .ForMember(c => c.CompetitorName, d => d.MapFrom(s => s.CompetitorsInEvent.CompetitorName));
            CreateMap<Stage, StageResultDto>()
                .ForMember(c => c.StageId, d => d.MapFrom(s => s.Id))
                .ForMember(c => c.StageNumber, d => d.MapFrom(s => s.StageName))
                .ForMember(c => c.HasResult, d => d.MapFrom(s => s.Results.Any()))
                .ForMember(c => c.VanNaar, d => d.MapFrom(s => $"{s.StartLocation}-{s.FinishLocation}"));
            CreateMap<GameCompetitorEvent, DeelnemerDto>()
                .ForMember(c => c.DeelnemerNaam, d => d.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}"))
                .ForMember(c => c.PoolNaam, d => d.MapFrom(s => s.TeamName))
                .ForMember(c => c.Renners, d => d.MapFrom(s => s.Renners));
            CreateMap<NewsItem, NewsItemDto>();
        }
    }
}
