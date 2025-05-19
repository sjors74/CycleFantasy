using CycleManager.Domain.Dto;
using CycleManager.Domain.Interfaces;
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

        public EventService(IEventRepository eventRepository, IStageRepository stageRepository, IResultsRepository resultsRepository, IGameCompetitorInEventRepository deelnemersRepository)
        {
            _eventRepository = eventRepository;
            _stageRepository = stageRepository;
            _resultRepository = resultsRepository;
            _deelnemersRepository = deelnemersRepository;
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
            return _eventRepository.GetById(id);
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
            var resultList = new List<Event>();
            var eventDtos = new List<EventForUserDto>();
            var events = await _eventRepository.GetAllEvents();
            foreach(var e in events)
            {
                var gameCompetitorsInEvent = await _deelnemersRepository.GetAllGameCompetitorsInEventByEventId(e.EventId);
                if(gameCompetitorsInEvent.Any())
                {
                    var eventsForUser = gameCompetitorsInEvent.Where(u => !string.IsNullOrEmpty(u.UserId) && u.UserId.Equals(userId));
                    if (eventsForUser.Any())
                    {
                        foreach(var gamecompetitorInEvent in eventsForUser)
                        {
                            eventDtos.Add(new EventForUserDto
                            {
                                ColorName = gamecompetitorInEvent.Event.ColorName,
                                CompetitorInEventId = gamecompetitorInEvent.Id,
                                CountryCode = gamecompetitorInEvent.Event.CountryCode,
                                EndDate = (DateTime)gamecompetitorInEvent.Event.EndDate,
                                EventId = gamecompetitorInEvent.Event.EventId,
                                EventName = gamecompetitorInEvent.Event.EventName,
                                Slogan = gamecompetitorInEvent.Event.Slogan,
                                StartDate = (DateTime)gamecompetitorInEvent.Event.StartDate,
                                UserId = gamecompetitorInEvent.UserId
                            });
                        }
                    }
                }
            }
            return eventDtos;
        }
    }
}
