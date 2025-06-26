using CycleManager.Domain.Dto;
using CycleManager.Domain.Interfaces;
using CycleManager.Domain.ViewModel;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IStageRepository _stageRepository;
        private readonly IResultsRepository _resultRepository;
        private readonly IGameCompetitorInEventRepository _deelnemersRepository;
        private readonly IGameCompetitorEventPickRepository _picksRepository;
        private readonly ICompetitorInEventService _competitorService;
        private readonly IResultService _resultService;

        public EventService(IEventRepository eventRepository, IStageRepository stageRepository, 
            IResultsRepository resultsRepository, IGameCompetitorInEventRepository deelnemersRepository,
            IGameCompetitorEventPickRepository picksRepository, ICompetitorInEventService competitorService, 
            IResultService resultService)
        {
            _eventRepository = eventRepository;
            _stageRepository = stageRepository;
            _resultRepository = resultsRepository;
            _deelnemersRepository = deelnemersRepository;
            _picksRepository = picksRepository;
            _competitorService = competitorService;
            _resultService = resultService;
            _resultService = resultService;
        }

        public async Task Create(Event entity)
        {
            _eventRepository.Add(entity);
            await _eventRepository.SaveChangesAsync();
        }

        public async Task Delete(Event entity)
        {
            _eventRepository.Remove(entity);
            await _eventRepository.SaveChangesAsync();
        }

        public Task<IEnumerable<Event>> GetAllEvents()
        {
            return _eventRepository.GetAllEvents();
        }

        public Task<Event> GetEventById(int id)
        {
            return _eventRepository.GetEventById(id);
        }

        public async Task Update(Event entity)
        {
            _eventRepository.Update(entity);
            await _eventRepository.SaveChangesAsync();

        }

        public async Task<int> GetAllStagesForEvent(int id)
        {
            var stages = await _stageRepository.GetByEventId(id);
            var numberOfStages = stages.Count();
            return numberOfStages;
        }

        public async Task<IEnumerable<StageResultDto>> GetStagesWithResultsForEvent(int eventId)
        {
            var resultList = new List<StageResultDto>();
            var stages = await _stageRepository.GetByEventId(eventId);
            foreach (var stage in stages)
            {
                var result = await _resultRepository.GetResultsByStageId(stage.Id);
                if (result == 0)
                {
                    resultList.Add(new StageResultDto { StageNumber = int.Parse(stage.StageName), HasResult = false });
                }
                else
                {
                    resultList.Add(new StageResultDto { StageNumber = int.Parse(stage.StageName), HasResult = true });
                }
            }
            return resultList;
        }

        public async Task<IEnumerable<EventForUserDto>> GetEventsByUserId(string userId)
        {
            var events = await _deelnemersRepository.GetEventsByUserId(userId);
            var eventsForUserDto = new List<EventForUserDto>();
            foreach(var ev in events)
            {
                var deelnemers = new List<DeelnemerDto>();

                foreach (var gce in ev.GameCompetitorEvents)
                {
                    var renners = new List<CompetitorDto>();

                    foreach (var renner in gce.Renners)
                    {
                        var results = await _resultService.GetCompetitorResultsByEventId(renner.CompetitorsInEvent.EventId, renner.CompetitorsInEventId);
                        var punten = results != null ? results.TotalScore : 0;
                        renners.Add(new CompetitorDto
                        {
                            CompetitorId = renner.CompetitorsInEvent.CompetitorId,
                            CompetitorName = renner.CompetitorsInEvent.CompetitorName,
                            CountryShort = renner.CompetitorsInEvent.Competitor.Country.CountryNameShort,
                            EventNumber = renner.CompetitorsInEvent.EventNumber.ToString(),
                            TeamName = renner.CompetitorsInEvent.Competitor.Team.TeamName,
                            Punten = punten
                        });
                    }
                    deelnemers.Add(new DeelnemerDto
                    {
                        Id = gce.Id,
                        PoolNaam = gce.TeamName,
                        DeelnemerNaam = $"{gce.User.FirstName} {gce.User.LastName}",
                        Renners = renners
                    });

                }
                eventsForUserDto.Add(
                new EventForUserDto
                {
                    EventId = ev.EventId,
                    EventName = ev.EventName,
                    StartDate = ev.StartDate.GetValueOrDefault(),
                    EndDate = ev.EndDate.GetValueOrDefault(),
                    Slogan = ev.Slogan,
                    CountryCode = ev.CountryCode,
                    ColorName = ev.CountryCode,
                    UserId = userId,
                    Deelnemers = deelnemers 
                });
                
            }
            return eventsForUserDto;
        }

        public async Task SaveSelectie(SelectieDto selectie)
        {
            var gamePicks = new List<GameCompetitorEventPick>();
            foreach(var geselecteerde_renner in selectie.RennerIds)
            {
                var cie = await _competitorService.FindOrCreate(selectie.EventId, geselecteerde_renner);

                gamePicks.Add(
                new GameCompetitorEventPick
                {
                    CompetitorsInEventId = cie.Id,
                    GameCompetitorEventId = selectie.DeelnemerId
                });
            }

            await _picksRepository.CreateGamePicksAsync(gamePicks);
        }

        public async Task<DeelnemerDto> CreatePoolAsync(DeelnemerDto deelnemerDto)
        {
            var gameCompetitorEvent = new GameCompetitorEvent
            {
                TeamName = deelnemerDto.PoolNaam,
                UserId = deelnemerDto.UserId,
                EventId = deelnemerDto.EventId,
            };

            var createdEvent = await _deelnemersRepository.CreateGameCompetitorEventAsync(gameCompetitorEvent);
            deelnemerDto.Id = createdEvent.Id;
            return deelnemerDto;
        }

        public async Task DeletePoolAsync(int id)
        {
            var deelnemer = await _deelnemersRepository.GetyCompetitorWithPicksById(id);
            if(deelnemer != null)
            {
                if(deelnemer.Renners.Any())
                {
                    _picksRepository.RemoveRange(deelnemer.Renners);
                    await _picksRepository.SaveChangesAsync();
                }
                _deelnemersRepository.Remove(deelnemer);
                await _deelnemersRepository.SaveChangesAsync();
            }
        }

        public async Task<EventDetailsViewModel?> GetEventDetailsViewModelById(int eventId)
        {
            return await _eventRepository.GetEventDetailsViewModelById(eventId);
        }

        public async Task<IEnumerable<TeamDto>> GetTeamsForEvent(int eventId)
        {
            return await _eventRepository.GetTeamsForEvent(eventId);
        }

        /// <summary>
        /// Return number of participants for an event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> GetAantalDeelnemers(int id)
        {
            return await _eventRepository.GetAantalDeelnemers(id);
        }
    }
}
