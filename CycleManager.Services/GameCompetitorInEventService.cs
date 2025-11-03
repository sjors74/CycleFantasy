using CycleManager.Domain.Dto;
using CycleManager.Domain.Interfaces;
using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CycleManager.Services
{
    public class GameCompetitorInEventService : IGameCompetitorInEventService
    {
        private readonly IGameCompetitorInEventRepository _repo;
        private readonly IGameCompetitorEventPickRepository _pickRepository;
        private readonly ICompetitorsInEventRepository _competitorRepo;

        public GameCompetitorInEventService(IGameCompetitorInEventRepository repo, IGameCompetitorEventPickRepository pickRepository, 
            ICompetitorsInEventRepository competitorRepo)
        {
            _repo = repo;
            _pickRepository = pickRepository;
            _competitorRepo = competitorRepo;
        }

        /// <summary>
        /// Create a new gamecompetitor for an event and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Create(GameCompetitorEvent entity)
        {
            _repo.Add(entity);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a gamecompetitor for an event and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Delete(GameCompetitorEvent entity)
        {
            _repo.Remove(entity);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Get a list of all game competitors for an event by event id
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<GameCompetitorEvent>> GetAllCompetitorsInEvent(int eventId)
        {
            return await _repo.GetAllGameCompetitorsInEventByEventId(eventId);
        }

        /// <summary>
        /// Get a gamecompettor for an event by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GameCompetitorEvent?> GetGameCompetitorEventById(int id)
        {
            return await _repo.GetGameCompetitorInEventById(id);
        }


        /// <summary>
        /// Get a list of all gamecompetitors in an event and its picks (competitors for this event)
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public IQueryable<GameCompetitorEventPick> GetPicks(int eventId)
        {
            return _pickRepository.GetCompetitorEventPicksByEventId(eventId);

        }

        /// <summary>
        /// Get all picks for a gamecompetitor in event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IEnumerable<GameCompetitorEventPick>> GetAllPicks(int id)
        {
            return await _pickRepository.GetCompetitorEventPicksById(id);
        }

        /// <summary>
        /// Get the number of picks for a gamecompetitor
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="gameCompetitorId"></param>
        /// <returns></returns>
        public async Task<int> GetNumberOfPicks(int eventId, int gameCompetitorId)
        {
            var picks = await GetAllPicks(gameCompetitorId);
            return picks.Count();
        }

        /// <summary>
        /// Update a game competitor for an event and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task UpdateAsync(DeelnemerEditDto dto)
        {
            var entity = await _repo.GetById(dto.Id);
            if (entity == null) throw new Exception("Deelnemer niet gevonden.");

            entity.TeamName = dto.TeamName;
            entity.UserId = dto.UserId;

            await _repo.SaveChangesAsync();
        }

        public async Task<IEnumerable<CompetitorsInEvent>> GetCompetitors(int id, int number)
        {
            return await _competitorRepo.GetRandomNumberofCompetitors(id, number);
        }

        public async Task<IEnumerable<int>> GetAllPicksAsCompetitorIds(int id)
        {
            var competitorPicks = new List<int>();
            var picks = await _pickRepository.GetCompetitorEventPicksById(id);
            foreach (var pick in picks)
            {
                competitorPicks.Add(pick.CompetitorsInEvent.CompetitorInTeamId);
            }

            return competitorPicks;
        }

        public async Task<CompetitorsInEvent> GetCompetitorInEventById(int id)
        {
            return await _competitorRepo.GetById(id);
        }

        public async Task<GameCompetitorEvent> CreateGameCompetitorEventAsync(DeelnemerCreateDto dto)
        {
            return await _repo.CreateGameCompetitorEventAsync(dto);
        }

        public async Task RemovePickFromEvent(int id)
        {
            await _pickRepository.RemovePickFromEvent(id);
            await _pickRepository.SaveChangesAsync();
        }

        public async Task AddPicks(List<GameCompetitorEventPick> picks)
        {
            _pickRepository.AddRange(picks);
            await _pickRepository.SaveChangesAsync();
        }

        public async Task DeleteGameCompetitorEventAsync(int id)
        {
            await _pickRepository.DeleteGameCompetitorEventAsync(id);
            await _pickRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<SelectListItem>> GetDropdownListAsync(int eventId)
        {
            var competitors = new List<SelectListItem>();
            var competitorsDb = await _competitorRepo.GetCompetitorsInEventList(eventId);
            var groupedCompetitors = competitorsDb
                .GroupBy(x => x.CompetitorInTeam?.Team?.CurrentTeamName ?? "onbekend");

            foreach (var group in groupedCompetitors)
            {
                var optionGroup = new SelectListGroup { Name = group.Key };
                foreach (var item in group)
                {
                    competitors.Add(new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.CompetitorInTeam.Competitor.CompetitorName,
                        Group = optionGroup
                    });
                }
            }
            return competitors;
        }
    }
}
