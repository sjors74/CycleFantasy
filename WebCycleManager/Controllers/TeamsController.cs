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
        private readonly ISeasonYearService _seasonYearService;
        public TeamsController(ITeamService teamService, ICountryService countryService, ISeasonYearService seasonYearService)
        {
            _teamService = teamService;
            _countryService = countryService;
            _seasonYearService = seasonYearService;
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
                    CompetitorsInTeam = team.TeamYears.FirstOrDefault(ty => ty.SeasonYear.Active)?.CompetitorInTeams.Count ?? 0
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

            var competitors = team.TeamYears
                .Where(ty => ty.SeasonYear.Year == selectedYear)
                .SelectMany(ty => ty.CompetitorInTeams)
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
                                    .Select(ty => ty.SeasonYear.Year)
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
            if (team == null)  
                return NotFound();

            var availableYears = await _seasonYearService.GetAllAsync();

            var countries = (await _countryService.GetAll())
                .OrderBy(c => c.CountryNameLong)
                .Select(c => new SelectListItem
                {
                    Value = c.CountryId.ToString(),
                    Text = c.CountryNameLong,
                    Selected = c.CountryId == team.CountryId
                })
                .ToList();
            
            var model = new TeamEditViewModel
            {
                TeamId = team.TeamId,
                CurrentTeamName = team.CurrentTeamName,
                CountryId = team.CountryId,
                PcsName = team.PcsName,
                Countries = countries,
                TeamYears = team.TeamYears
                            .OrderBy(ty => ty.SeasonYear.Year)
                            .Select(ty => new TeamYearViewModel
                            {
                                TeamYearId = ty.TeamYearId,
                                SeasonYearId = ty.SeasonYearId,
                                Year = ty.SeasonYear.Year,
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
            if (!ModelState.IsValid)
            { 
               model.Countries = (await _countryService.GetAll())
                .OrderBy(c => c.CountryNameLong)
                .Select(c => new SelectListItem
                {
                    Value = c.CountryId.ToString(),
                    Text = c.CountryNameLong
                })
                .ToList();

                return View(model);
            }

            var team = await _teamService.GetTeamById(model.TeamId);
            if (team == null) 
                return NotFound();

            team.CurrentTeamName = model.CurrentTeamName;
            team.PcsName = model.PcsName;
            team.CountryId = model.CountryId;

            foreach (var posted in model.TeamYears)
            {
                var existing = team.TeamYears
                    .FirstOrDefault(ty => ty.SeasonYearId == posted.SeasonYearId);

                if (existing == null)
                {
                    // Dit zou eigenlijk nooit meer mogen gebeuren.
                    throw new InvalidOperationException(
                        $"Geen TeamYear gevonden voor seizoen {posted.SeasonYearId}.");
                }

                existing.Name = posted.Name;
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
