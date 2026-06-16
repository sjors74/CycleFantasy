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
                .ForMember(c => c.Stages, d => d.MapFrom(s => s.Stages.OrderBy(s => s.StageOrder)))
                .ForMember(c => c.Deelnemers, d => d.MapFrom(s => s.GameCompetitorEvents));

            CreateMap<Event, EventForUserDto>()
                .ForMember(d => d.Stages, o => o.MapFrom(s => s.Stages.OrderBy(s => s.StageOrder)))
                .ForMember(d => d.Deelnemers, o => o.MapFrom(s => s.GameCompetitorEvents))
                .ForMember(d => d.UserId, o => o.Ignore())
                .ForMember(d => d.CompetitorInEventId, o => o.Ignore())
                .ForMember(d => d.Renners, o => o.Ignore())
                .ForMember(d => d.IsIngeschreven, o => o.Ignore());

            CreateMap<Competitor, CompetitorDto>()
                .ForMember(c => c.CountryShort, d => d.MapFrom(s => s.Country.CountryNameShort))
                .ForMember(c => c.CompetitorId, o => o.Ignore())
                .ForMember(c => c.CompetitorInTeamId, o => o.Ignore())
                .ForMember(c => c.EventNumber, o => o.Ignore())
                .ForMember(c => c.Punten, o => o.Ignore())
                .ForMember(c => c.InSelectie, o => o.Ignore())
                .ForMember(c => c.CurrentTeamName, o => o.Ignore())
                .ForMember(c => c.IsNationalChampion, o => o.Ignore())
                .ForMember(c => c.Teams, o => o.Ignore());

            CreateMap<CompetitorsInEvent, CompetitorDto>()
                .ForMember(d => d.CompetitorId,
                    o => o.MapFrom(s => s.CompetitorInTeam.Competitor.CompetitorId))
                .ForMember(d => d.FirstName,
                    o => o.MapFrom(s => s.CompetitorInTeam.Competitor.FirstName))
                .ForMember(d => d.LastName,
                    o => o.MapFrom(s => s.CompetitorInTeam.Competitor.LastName))
                .ForMember(d => d.PcsName,
                    o => o.MapFrom(s => s.CompetitorInTeam.Competitor.PcsName))
                .ForMember(d => d.CountryShort,
                    o => o.MapFrom(s => s.CompetitorInTeam.Competitor.Country.CountryNameShort))
                .ForMember(d => d.ScraperName, o => o.Ignore())
                .ForMember(d => d.Punten, o => o.Ignore())
                .ForMember(d => d.CurrentTeamName, o => o.Ignore())
                .ForMember(d => d.IsNationalChampion, o => o.Ignore())
                .ForMember(d => d.Teams, o => o.Ignore())
                //.ForMember(d => d.CompetitorInTeamId, o => o.Ignore())
                .ForMember(d => d.EventNumber, o => o.Ignore())
                .ForMember(d => d.InSelectie, o => o.Ignore())
                .ForMember(d => d.RemovedFromStartlist, o => o.Ignore());


            CreateMap<GameCompetitorEvent, DeelnemerCreateDto>().ReverseMap();

            CreateMap<GameCompetitorEventPick, ResultDto>()
                .ForMember(c => c.CompetitorName, d => d.MapFrom(s => s.CompetitorsInEvent.CompetitorInTeam.Competitor.CompetitorName))
                .ForMember(c => c.CompetitorTeam, d => d.MapFrom(s => s.CompetitorsInEvent.CompetitorInTeam.Team.CurrentTeamName))
                .ForMember(c => c.CompetitorInEventId, d => d.MapFrom(s => s.CompetitorsInEvent.Id))
                .ForMember(c => c.OutOfCompetition, d => d.MapFrom(s => s.CompetitorsInEvent.OutOfCompetition))
                .ForMember(c => c.CountryCode, d => d.MapFrom(s => s.CompetitorsInEvent.CompetitorInTeam.Competitor.Country.CountryNameShort))
                .ForMember(c => c.PcsName, d => d.MapFrom(s => s.CompetitorsInEvent.CompetitorInTeam.Competitor.PcsName))
                .ForMember(c => c.IsNationalChampion, d => d.MapFrom(s => s.CompetitorsInEvent.CompetitorInTeam.IsNationalChampion))
                .ForMember(d => d.StageNumber, o => o.Ignore())
                .ForMember(d => d.Position, o => o.Ignore())
                .ForMember(d => d.Points, o => o.Ignore())  
                .ForMember(d => d.LatestPoints, o => o.Ignore())
                .ForMember(d => d.ConfigurationItems, o => o.Ignore())
                .ForMember(d => d.EventId, o => o.Ignore());

            CreateMap<GameCompetitorEventPick, CompetitorDto>()
                .ForMember(c => c.FirstName, d => d.MapFrom(s => s.CompetitorsInEvent.CompetitorInTeam.Competitor.FirstName))
                .ForMember(c => c.LastName, d => d.MapFrom(c => c.CompetitorsInEvent.CompetitorInTeam.Competitor.LastName))
                .ForMember(d => d.CompetitorId, o => o.Ignore())
                .ForMember(d => d.CompetitorInTeamId, o => o.Ignore())
                .ForMember(d => d.PcsName, o => o.Ignore())
                .ForMember(d => d.ScraperName, o => o.Ignore())
                .ForMember(d => d.CountryShort, o => o.Ignore())
                .ForMember(d => d.EventNumber, o => o.Ignore())
                .ForMember(d => d.Punten, o => o.Ignore())
                .ForMember(d => d.InSelectie, o => o.Ignore())
                .ForMember(d => d.CurrentTeamName, o => o.Ignore())
                .ForMember(d => d.IsNationalChampion, o => o.Ignore())
                .ForMember(d => d.Teams, o => o.Ignore());

            CreateMap<Stage, StageResultDto>()
                .ForMember(c => c.StageId, d => d.MapFrom(s => s.Id))
                .ForMember(c => c.StageNumber, d => d.MapFrom(s => s.StageName))
                .ForMember(c => c.NoScore, d => d.MapFrom(s => s.NoScore))
                .ForMember(c => c.HasResult, d => d.MapFrom(s => s.Results.Any()))
                .ForMember(c => c.VanNaar, d => d.MapFrom(s => $"{s.StartLocation}-{s.FinishLocation}"));

            CreateMap<GameCompetitorEvent, DeelnemerDto>()
                .ForMember(c => c.DeelnemerNaam, d => d.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}"))
                .ForMember(c => c.PoolNaam, d => d.MapFrom(s => s.TeamName))
                .ForMember(c => c.Renners, d => d.MapFrom(s => s.Renners))
                .ForMember(d => d.Punten, o => o.Ignore())
                .ForMember(d => d.LaatsteScore, o => o.Ignore());

            CreateMap<NewsItem, NewsItemDto>();

        }
    }
}
