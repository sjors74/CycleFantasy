using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Domain.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleManager.Services
{
    public class EventScrapeSchedulerService : IEventScrapeSchedulerService
    {
        private readonly ApplicationDbContext _db;
        private readonly IScrapeOrchestratorService _orchestrator;
        private readonly ILogger<EventScrapeSchedulerService> _logger;

        public EventScrapeSchedulerService(
            ApplicationDbContext db,
            IScrapeOrchestratorService orchestrator,
            ILogger<EventScrapeSchedulerService> logger)
        {
            _db = db;
            _orchestrator = orchestrator;
            _logger = logger;
        }

        public async Task RunEventScrapeAsync(int eventId)
        {
            var stage = await _db.Stages
                .Include(s => s.Event)
                .Where(s =>
                    s.EventId == eventId &&
                    s.ScrapeStatus == ScrapeStatus.Pending)
                .OrderBy(s => s.StageOrder)
                .FirstOrDefaultAsync();

            if (stage == null)
            {
                _logger.LogInformation(
                    "Geen pending stages gevonden voor EventId {EventId}",
                    eventId);

                return;
            }

            if (stage.NoScore)
            {
                stage.ScrapeStatus = ScrapeStatus.Skipped;

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Stage {Stage} geskipt wegens NoScore",
                    stage.StageName);

                return;
            }

            stage.LastScrapeAttempt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            try
            {
                await _orchestrator.RunStageScrapeAsync(
                    stage.EventId,
                    stage.Event.EventCode,
                    stage.Id,
                    int.Parse(stage.StageName),
                    stage.Event.EventYear);

                var hasResults = await _db.Results
                    .AnyAsync(r => r.StageId == stage.Id);

                if (hasResults)
                {
                    stage.ScrapeStatus = ScrapeStatus.Completed;
                    stage.LastSuccessfulScrape = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Stage {Stage} succesvol verwerkt",
                        stage.StageName);
                }
                else
                {
                    _logger.LogInformation(
                        "Nog geen resultaten voor stage {Stage}",
                        stage.StageName);
                }
            }
            catch (Exception ex)
            {
                stage.ScrapeStatus = ScrapeStatus.Failed;

                _logger.LogError(ex,
                    "Fout tijdens scrapen stage {Stage}",
                    stage.StageName);
            }

            await _db.SaveChangesAsync();
        }
    }
}
