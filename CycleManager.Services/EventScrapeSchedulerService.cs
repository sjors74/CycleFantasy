using CycleManager.Domain.Enums;
using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Domain.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
                    (s.ScrapeStatus == ScrapeStatus.Pending || s.ScrapeStatus == ScrapeStatus.Partial))
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

                var expectedResults = await GetExpectedResultsAsync(stage.Event.ConfigurationId);

                var actualResults = await _db.Results
                    .CountAsync(r => r.StageId == stage.Id);

                if (actualResults >= expectedResults)
                {
                    stage.ScrapeStatus = ScrapeStatus.Completed;
                    stage.LastSuccessfulScrape = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Stage {Stage} volledig verwerkt ({Actual}/{Expected} resultaten)",
                        stage.StageName,
                        actualResults,
                        expectedResults);
                }
                else if (actualResults > 0)
                {
                    stage.ScrapeStatus = ScrapeStatus.Partial;

                    _logger.LogInformation(
                        "Stage {Stage} gedeeltelijk verwerkt ({Actual}/{Expected} resultaten)",
                        stage.StageName,
                        actualResults,
                        expectedResults);
                }
                else
                {
                    stage.ScrapeStatus = ScrapeStatus.Pending;

                    _logger.LogInformation(
                        "Nog geen resultaten voor stage {Stage} (0/{Expected})",
                        stage.StageName,
                        expectedResults);
                }
            }
            catch (Exception ex)
            {
                stage.ScrapeStatus = ScrapeStatus.Failed;

                _logger.LogError(
                    ex,
                    "Fout tijdens verwerken van stage {Stage}",
                    stage.StageName);

                throw;
            }

            await _db.SaveChangesAsync();
        }

        public async Task RunStartlistSyncAsync(int eventId)
        {
            try
            {
                await _orchestrator.RefreshStartlistAsync(eventId);

                _logger.LogInformation(
                    "Startlist sync succesvol voor EventId {EventId}",
                    eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Fout tijdens startlist sync voor EventId {EventId}",
                    eventId);

                if (ex.InnerException != null)
                {
                    _logger.LogError(
                        "InnerException: {Message}",
                        ex.InnerException.Message);
                }

                throw;
            }
        }

        private async Task<int> GetExpectedResultsAsync(int? configurationId)
        {
            if (configurationId == null)
            {
                return 0;
            }

            return await _db.ConfigurationItems
                .CountAsync(ci => ci.ConfigurationId == configurationId);
        }
    }
}