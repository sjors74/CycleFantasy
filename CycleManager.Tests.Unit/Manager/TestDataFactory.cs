using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using Domain.Dto;
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

        public static List<CompetitorDto> CreateCompetitorDtos(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new CompetitorDto
                {
                    CompetitorId = i,
                    FirstName = $"Rider{i}",
                    LastName = $"Lastname{i}",
                    PcsName = $"PCS{i}", 
                    CountryShort = $"Country{i}", 
                    CurrentTeamName = $"Team{i}"
                })
                .ToList();
        }

        public static Competitor CreateCompetitor()
        {
            return new Competitor
            {
                CompetitorId = 1,
                FirstName = "Remco",
                LastName = "Evenepoel",
                CountryId = 1,
                PcsName = "EvenepoelR",
                Country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Belgium"
                },
                CompetitorInTeams = new List<CompetitorInTeam>()
            };
        }

        public static List<Competitor> CreateCompetitors(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new Competitor
                {
                    CompetitorId = i,
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    CountryId = i,
                    Country = new Country
                    {
                        CountryId = i,
                        CountryNameLong = $"Country{i}"
                    }
                }).ToList();
        }

        public static Competitor CreateCompetitorWithTeam()
        {
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Soudal Quick-Step"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Remco",
                LastName = "Evenepoel",
                Country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Belgium"
                },
                PcsName = "EvenepoelR",
                CompetitorInTeams = new List<CompetitorInTeam>
                {
                    new CompetitorInTeam
                    {
                        Id = 1,
                        CompetitorId = 1,
                        TeamId = 1,
                        Year = DateTime.Now.Year,
                        Team = team,
                        IsNationalChampion = true
                    }
                }
            };

            team.CompetitorInTeams = new List<CompetitorInTeam> { competitor.CompetitorInTeams.First() };

            return competitor;
        }

        public static CreateCompetitorViewModel CreateValidCreateCompetitorViewModel()
        {
            return new CreateCompetitorViewModel
            {
                CompetitorId = 0,
                FirstName = "Wout",
                LastName = "Van Aert",
                PcsName = "VanAertW",
                CountryId = 1,
                TeamId = 1,
                Year = DateTime.Now.Year,
                IsNationalChampion = false
            };
        }

        public static CompetitorEditDto CreateCompetitorEditDto()
        {
            return new CompetitorEditDto
            {
                CompetitorId = 1,
                FirstName = "Tadej",
                LastName = "Pogacar",
                PcsName = "PogacarT",
                ScraperName = "TadejP",
                CountryId = 1,
                SelectedTeamId = 1,
                SelectedYear = DateTime.Now.Year,
                Countries = new List<CountryDto>
                {
                    new CountryDto { Id = 1, CountryNameLong = "Slovenia" },
                    new CountryDto { Id = 2, CountryNameLong = "Belgium" }
                },
                Teams = new List<TeamDto>
                {
                    new TeamDto { Id = 1, Naam = "UAE Team Emirates", Renners = new List<CompetitorDto>() },
                    new TeamDto { Id = 2, Naam = "Jumbo-Visma", Renners = new List<CompetitorDto>() }
                },
                AvailableYears = new List<int> { 2023, 2024, 2025 },
                CompetitorInTeams = new List<CompetitorInTeamDto>
                {
                    new CompetitorInTeamDto
                    {
                        CompetitorInTeamId = 1,
                        TeamId = 1,
                        Year = 2024,
                        IsNationalChampion = false,
                        TeamName = "UAE Team Emirates"
                    }
                }
            };
        }
    }
}