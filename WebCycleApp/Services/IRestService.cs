using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCycleApp.Models;

namespace WebCycleApp.Services
{
    public interface IRestService
    {
        Task<List<Event>> GetActiveEvents();
    }
}
