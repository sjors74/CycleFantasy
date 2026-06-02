using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleManager.Domain.Interfaces
{
    public interface IEventScrapeSchedulerService
    {
        Task RunEventScrapeAsync(int eventId);
        Task RunStartlistSyncAsync(int eventId);
    }
}
