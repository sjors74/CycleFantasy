using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ITeamService _teamService;
        private readonly ICountryService _countryService;
        public TeamsController(ITeamService teamService, ICountryService countryService)
        {
            _teamService = teamService;
            _countryService = countryService;
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            var _teamViewModels = new List<TeamViewModel>();
            var teams = await _teamService.GetAllTeams();

            bool hasUnprocessedScraped = await _teamService.HasUnprocessedScrapedTeams();
            int unprocessedCount = await _teamService.CountUnprocessedScrapedCompetitors();

            foreach (var team in teams.OrderBy(t => t.CurrentTeamName))
            {
                _teamViewModels.Add(new TeamViewModel
                {
                    Id = team.TeamId,
                    TeamName = team.CurrentTeamName,
                    PcsName = team.PcsName,
                    CountryNameShort = team.Country?.CountryNameShort ?? string.Empty,
                    CompetitorsInTeam = team.CompetitorInTeams?.Count ?? 0, // telt via CompetitorInTeams
                });
            }

            ViewBag.HasUnprocessedScraped = hasUnprocessedScraped;
            ViewBag.UnprocessedScrapedCount = unprocessedCount;
            return View(_teamViewModels);
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int id, int? year)
        {
            var selectedYear = year ?? DateTime.Now.Year;

            var team = await _teamService.GetTeamForCurrentYear(id, selectedYear);

            if (team == null)
                return NotFound();

            var competitors = team.CompetitorInTeams
                .Where(cit => cit.Year == selectedYear)
                .Select(cit => new CompetitorViewModel
                {
                    CompetitorId = cit.CompetitorId,
                    FirstName = cit.Competitor.FirstName,
                    LastName = cit.Competitor.LastName,
                    Land = cit.Competitor.Country?.CountryNameShort ?? "onbekend",
                    IsNationalChampion = cit.IsNationalChampion
                })
                .OrderBy(c => c.LastName)
                .ToList();

            var vm = new TeamDetailsViewModel
            {
                TeamId = team.TeamId,
                TeamName = team.CurrentTeamName,
                Country = team.Country?.CountryNameShort ?? "onbekend",
                SelectedYear = selectedYear,
                AvailableYears = team.TeamYears
                                    .Select(ty => ty.Year)
                                    .Distinct()
                                    .OrderByDescending(y => y)
                                    .ToList(),
                Competitors = competitors
            };
            return View(vm);
        }

        // GET: Teams/Create
        public async Task<IActionResult> Create()
        {
            var countries = await _countryService.GetAll();
            var vm = new TeamCreateViewModel
            {
                Countries = countries.Select(c => new SelectListItem
                {
                    Value = c.CountryId.ToString(),
                    Text = c.CountryNameLong
                })
            };
            return View(vm);
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var countries = await _countryService.GetAll();
                vm.Countries = countries.Select(c => new SelectListItem
                {
                    Value = c.CountryId.ToString(),
                    Text = c.CountryNameLong
                });
                return View(vm);
            }

            var team = new Team
            {
                CurrentTeamName = vm.CurrentTeamName,
                PcsName = vm.PcsName ?? string.Empty,
                CountryId = vm.CountryId
            };

            await _teamService.Add(team);
            return RedirectToAction(nameof(Index));
        }        
            
        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var team = await _teamService.GetTeamById(id);
            if (team == null)  return NotFound();

            var availableYears = Enumerable.Range(2025, 4).ToList();
            
            var countries = _countryService.GetAll().Result
                .OrderBy(c => c.CountryNameLong)
                .Select(c => new SelectListItem
                {
                    Value = c.CountryId.ToString(),
                    Text = c.CountryNameLong,
                    Selected = c.CountryId == team.CountryId // hier de geselecteerde waarde instellen
                })
                .ToList();
            
            var model = new TeamEditViewModel
            {
                TeamId = team.TeamId,
                CurrentTeamName = team.CurrentTeamName,
                CountryId = team.CountryId,
                PcsName = team.PcsName,
                Countries = countries,
                AvailableYears = availableYears,
                TeamYears = team.TeamYears
                            .OrderBy(ty => ty.Year)
                            .Select(ty => new TeamYearViewModel
                            {
                                TeamYearId = ty.TeamYearId,
                                Year = ty.Year,
                                Name = ty.Name
                            }).ToList()
            };

            return View(model);
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TeamEditViewModel model)
        {
            model.AvailableYears = Enumerable.Range(2025, 4).ToList();
            model.Countries = _countryService.GetAll().Result
                .OrderBy(c => c.CountryNameLong)
                .Select(c => new SelectListItem
                {
                    Value = c.CountryId.ToString(),
                    Text = c.CountryNameLong
                })
                .ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var team = await _teamService.GetTeamById(model.TeamId);
            if (team == null) return NotFound();

            team.CurrentTeamName = model.CurrentTeamName;
            team.PcsName = model.PcsName;
            team.CountryId = model.CountryId;

            foreach (var year in model.AvailableYears)
            {
                var posted = model.TeamYears.FirstOrDefault(x => x.Year == year);
                var existing = team.TeamYears.FirstOrDefault(x => x.Year == year);

                if (posted == null || string.IsNullOrWhiteSpace(posted.Name))
                {
                    if (existing != null)
                    {
                        team.TeamYears.Remove(existing);
                    }
                }
                else
                {
                    if (existing != null)
                    {
                        existing.Name = posted.Name;
                    }
                    else
                    {
                        team.TeamYears.Add(new TeamYear
                        {
                            Year = posted.Year,
                            Name = posted.Name
                        });
                    }
                }
            }

            await _teamService.Update(team);
            return RedirectToAction(nameof(Index));
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            
            var team = await _teamService.GetTeamById(id.Value);
            if (team == null) return NotFound();

            var vm = new TeamDeleteViewModel
            {
                TeamId = team.TeamId,
                CurrentTeamName = team.CurrentTeamName
            };

            return View(vm);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _teamService.GetTeamById(id);
            if (team != null)
            {
                await _teamService.Delete(team);
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
