﻿using Domain.Models;

namespace Domain.Interfaces
{
    public interface IEventRepository : IGenericRepository<Event>
    {
        Task<IEnumerable<Event>> GetActiveEvents(int id);
        Task<IEnumerable<Event>> GetAllEvents();
    }
}
