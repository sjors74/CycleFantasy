using Domain.Interfaces;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IScoreRepository
    {
        Task<List<DeelnemerScore>> GetScoresByEventIdAsync(int eventId);
    }
}
