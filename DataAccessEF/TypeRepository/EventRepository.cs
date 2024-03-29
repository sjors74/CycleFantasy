﻿using Domain.Context;
using Domain.Interfaces;
using Domain.Models;

namespace DataAccessEF.TypeRepository
{
    class EventRepository : GenericRepository<Event>, IEventRepository
    {
        public EventRepository(DatabaseContext context) : base(context) { }
    }
}
