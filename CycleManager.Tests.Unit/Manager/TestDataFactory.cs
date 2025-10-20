using CycleManager.Domain.Models;
using Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public static class TestDataFactory
    {
        public static List<Team> FakeTeams() => new()
        {
            new Team { TeamId = 1, CurrentTeamName = "Team A", Country = new Country { CountryNameShort = "NL" }, CompetitorInTeams = new List<CompetitorInTeam>() },
            new Team { TeamId = 2, CurrentTeamName = "Team B", Country = new Country { CountryNameShort = "BE" }, CompetitorInTeams = new List<CompetitorInTeam>() }
        };

        public static Team FakeTeamWithCompetitors(int year) => new()
        {
            TeamId = 1,
            CurrentTeamName = "TestTeam",
            Country = new Country { CountryNameShort = "NL" },
            CompetitorInTeams = new List<CompetitorInTeam>
            {
                new CompetitorInTeam
                {
                    Year = year,
                    Competitor = new Competitor
                    {
                        FirstName = "Jan",
                        LastName = "Jansen",
                        Country = new Country { CountryNameShort = "NL" }
                    }
                }
            }
        };

        public static Team FakeTeamWithYears() => new()
        {
            TeamId = 1,
            CurrentTeamName = "Team Edit",
            CountryId = 2,
            Country = new Country { CountryId = 2, CountryNameLong = "België" },
            TeamYears = new List<TeamYear>
            {
                new TeamYear { TeamYearId = 1, Year = 2025, Name = "EditTeam2025" }
            }
        };

        public static List<Country> FakeCountries() => new()
        {
            new Country { CountryId = 1, CountryNameLong = "Nederland", CountryNameShort = "NL" },
            new Country { CountryId = 2, CountryNameLong = "België", CountryNameShort = "BE" },
            new Country { CountryId = 3, CountryNameLong = "Duitsland", CountryNameShort = "DE" }
        };

        public static List<Event> CreateEvents(int count = 1)
        {
            return Enumerable.Range(1, count)
                .Select(i => new Event
                {
                    EventId = i,
                    EventName = $"Event {i}",
                    EventYear = 2020 + i,
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddDays(10)
                })
                .ToList();
        }

        public static Stage CreateStage(int id = 1)
        {
            return new Stage
            {
                Id = id,
                StageName = $"Stage {id}",
                StageOrder = id,
                StageDate = DateTime.Now,
                StartLocation = "Start City",
                FinishLocation = "Finish City",
                EventId = 1,
                Event = new Event
                {
                    EventId = 1,
                    EventName = "Tour de Test",
                    StartDate = DateTime.Today.AddDays(-1),
                    EndDate = DateTime.Today.AddDays(3),
                    EventYear = DateTime.Today.Year
                }
            };
        }

        public static StageCreateViewModel CreateStageCreateViewModel(int eventId = 1)
        {
            return new StageCreateViewModel
            {
                EventId = eventId,
                StageName = "Test Stage",
                StageDate = DateTime.Now,
                StageOrder = 1,
                StartLocation = "Start City",
                FinishLocation = "Finish City",
                Events = CreateEvents(2)
                    .Select(e => new SelectListItem { Value = e.EventId.ToString(), Text = e.EventName })
            };
        }

        public static StageViewModel CreateStageViewModel(Stage stage)
        {
            return new StageViewModel
            {
                StageId = stage.Id,
                EventId = stage.EventId,
                StageName = stage.StageName,
                StageOrder = stage.StageOrder,
                StageDate = DateOnly.FromDateTime(stage.StageDate),
                StartLocation = stage.StartLocation,
                FinishLocation = stage.FinishLocation,
                NoScore = stage.NoScore,
                NoScoreDescription = stage.NoScoreDescription
            };
        }
    }
}