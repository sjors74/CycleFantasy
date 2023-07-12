using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
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
    }
}
