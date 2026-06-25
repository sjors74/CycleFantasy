using CycleManager.Domain.Dto;
using CycleManager.Domain.Enums;
using CycleManager.Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IPcsScraper
    {
        Task<List<ScrapedStageResult>> ScrapeStageResultsAsync(string url, int topN, int eventId);
        Task<List<int>> ScrapeDropoutBibsAsync(string url);
        Task<List<ScrapedCompetitor>> ScrapeCompetitorsAsync(string url, int teamId, int year);
        Task<List<ScrapedStartlistEntry>> ScrapeStartlistAsync(string url);
        Task<ScrapedStageSpecialResult?> ScrapeClassificationWinnerAsync(
            string baseUrl,
            int stageId,
            QuestionType questionType);

        Task<ScrapedStageSpecialResult?> ScrapeClassificationWinnerWithRetryAsync(
            string baseUrl,
            int stageId,
            QuestionType questionType);

    }
}
