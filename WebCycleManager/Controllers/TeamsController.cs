using CycleManager.Services.Interfaces;
using Domain.Context;
using Domain.Interfaces;
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
        private readonly ITeamRepository _teamRepository;
        private readonly ICountryService _countryService;
        public TeamsController(ITeamRepository teamRepository, ICountryRepository countryRepository, ICountryService countryService)
        {
            _teamRepository = teamRepository;
            _countryService = countryService;
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            var _teamViewModels = new List<TeamViewModel>();
            var teams = await _teamRepository.GetAll();
            foreach (var team in teams.OrderBy(t => t.TeamName))
            {
                _teamViewModels.Add(new TeamViewModel
                {
                    Id = team.TeamId,
                    TeamName = team.TeamName,
                    CountryNameShort = team.Country == null ? string.Empty : team.Country.CountryNameShort,
                    CompetitorsInTeam = team.Competitors == null ? 0 : team.Competitors.Count,
                });
            }
            return View(_teamViewModels);
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var team = await _teamRepository.GetById((int)id);
            if (team == null)
            {
                return NotFound();
            }
            var competitorsList = new List<CompetitorViewModel>();
            var vm = new TeamViewModel();
            vm.Id = team.TeamId;
            vm.TeamName = team.TeamName;
            vm.CountryNameShort = team.Country == null ? string.Empty : team.Country.CountryNameShort;

            if (team.Competitors != null)
            {
                var orderedCompetitors = team.Competitors.OrderBy(c => c.LastName);
                foreach (var comp in orderedCompetitors)
                {
                    var compViewModel = new CompetitorViewModel { CompetitorId = comp.CompetitorId, FirstName = comp.FirstName, LastName = comp.LastName, Land = comp.Country == null ? string.Empty : comp.Country.CountryNameShort };
                    competitorsList.Add(compViewModel);
                }
                vm = new TeamViewModel {  CompetitorsInTeam = team.Competitors.Count, Competitors = competitorsList };
            }
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
        public async Task<IActionResult> Create([Bind("TeamId,TeamName,CountryId")] Team team)
        {
            if (ModelState.IsValid)
            {
                _teamRepository.Add(team);
                await _teamRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _teamRepository.GetById((int)id);
            if (team == null)
            {
                return NotFound();
            }
            ViewData["CountryId"] = new SelectList(await CountrySelectListHelper.GetOrderedCountries(_countryService), "CountryId", "CountryNameLong");

            return View(team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamId,TeamName,CountryId")] Team team)
        {
            if (id != team.TeamId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _teamRepository.Update(team);
                    await _teamRepository.SaveChangesAsync();
                }
                catch
                {
                    if (!TeamExists(team.TeamId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        // GET: Teams/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = _teamRepository.GetById((int)id);
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
            var team = await _teamRepository.GetById((int)id);
            if (team != null)
            {
                _teamRepository.Remove(team);
            }
            
            await _teamRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return (_teamRepository.GetById(id) != null);
        }


    }
}
