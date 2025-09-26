using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Helpers;
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
            var teams = await _teamService.GetAll();

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

            return View(_teamViewModels);
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int id, int? year)
        {
            var team = await _teamService.GetTeamById(id);

            if (team == null)
            {
                return NotFound();
            }

            var selectedYear = year ?? DateTime.Now.Year;
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
                AvailableYears = team.CompetitorInTeams.Select(c => c.Year).Distinct().OrderByDescending(y => y).ToList(),
                Competitors = competitors
            };
            return View(vm);
        }

        // GET: Teams/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CountryId"] = new SelectList(await CountrySelectListHelper.GetOrderedCountries(_countryService), "CountryId", "CountryNameLong");
            return View();
        }

        // POST: Teams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamId,TeamName,PcsName,CountryId")] Team team)
        {
            if (ModelState.IsValid)
            {
                _teamService.Update(team);
                //await _teamService.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var team = await _teamService.GetTeamById(id);
            if (team == null)  return NotFound();

            var availableYears = Enumerable.Range(2025, 6).ToList();
            var model = new TeamEditViewModel
            {
                TeamId = team.TeamId,
                CurrentTeamName = team.CurrentTeamName,
                CountryId = team.CountryId,
                PcsName = team.PcsName,
                Countries = _countryService.GetAll().Result
                .OrderBy(c => c.CountryNameLong)
                .Select(c => new SelectListItem
                {
                    Value = c.CountryId.ToString(),
                    Text = c.CountryNameLong
                })
                .ToList(),
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
        public async Task<IActionResult> Edit(int id, TeamEditViewModel model)
        {
            if (id != model.TeamId) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Countries = _countryService.GetAll().Result
                .OrderBy(c => c.CountryNameLong)
                .Select(c => new SelectListItem
                {
                    Value = c.CountryId.ToString(),
                    Text = c.CountryNameLong
                })
                .ToList();
                return View(model);
            }

            var team = await _teamService.GetTeamById(id);
            if (team == null) return NotFound();

            team.CurrentTeamName = model.CurrentTeamName;
            team.PcsName = model.PcsName;
            team.CountryId = model.CountryId;

            try
            {
                await _teamService.Update(team);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeamExists(team.TeamId))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _teamService.GetTeamById((int)id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
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
            
            //await _teamRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return (_teamService.GetTeamById(id) != null);
        }
    }
}
