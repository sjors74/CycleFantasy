using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using DataAccessEF.Extensions;
using Domain.Dto;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Helpers;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class CompetitorsController : Controller
    {
        private readonly ICompetitorService _competitorService;
        private readonly ITeamService _teamService;
        private readonly ICountryService _countryService;

        public CompetitorsController(ICompetitorService competitorService, ITeamService teamService, ICountryService countryService)
        {
            _competitorService = competitorService;
            _teamService = teamService;
            _countryService = countryService;
        }

        // GET: Competitors
        public async Task<IActionResult> Index(string currentFilter, string searchString, int? pageNumber, int? year)
        {
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;
            var availableYears = await _competitorService.GetAvailableYears();
            ViewData["AvailableYears"] = availableYears;
            
            var selectedYear = year ?? DateTime.Now.Year;
            ViewData["SelectedYear"] = selectedYear;

            var pageSize = ConfigurationConstants.PageSize;

            var competitors = await _competitorService.GetAllCompetitors(selectedYear);

            if (!string.IsNullOrEmpty(searchString))
            {
                competitors = competitors
                        .Where(s => s.LastName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                                    s.FirstName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList(); // materialiseer direct naar List

                if (!competitors.Any())
                {
                    return NotFound();
                }
            }

            var orderedList = competitors
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName);

            return View(PaginatedList<CompetitorDto>.Create(
                orderedList, pageNumber ?? 1, pageSize));
        }

        // GET: Competitors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competitor =  await _competitorService.GetCompetitorById((int)id);
            if (competitor == null)
            {
                return NotFound();
            }

            return View(competitor);
        }

        // GET: Competitors/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Competitors = await GetCompetitorSelectListAsync();
            ViewData["TeamId"] = new SelectList((await _teamService.GetAll()).OrderBy(t => t.TeamName), "TeamId", "TeamName");
            ViewData["CountryId"] = new SelectList(await CountrySelectListHelper.GetOrderedCountries(_countryService), "CountryId", "CountryNameLong");
            return View(new CreateCompetitorViewModel());
        }

        // POST: Competitors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCompetitorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Competitors = await GetCompetitorSelectListAsync();
                ViewData["TeamId"] = new SelectList((await _teamService.GetAll()).OrderBy(t => t.TeamName), "TeamId", "TeamName");
                ViewData["CountryId"] = new SelectList(await CountrySelectListHelper.GetOrderedCountries(_countryService), "CountryId", "CountryNameLong");
                return View(model);
            }

            Competitor competitor;
            if (model.CompetitorId > 0)
            {
                competitor = await _competitorService.GetCompetitorById(model.CompetitorId);
                if (competitor == null)
                {
                    ModelState.AddModelError("", "Geselecteerde renner bestaat niet.");
                    return View(model);
                }
            }
            else
            {
                if (model.CompetitorId == 0) // nieuwe renner
                {
                    if (string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName))
                    {
                        ModelState.AddModelError("", "Vul naam in voor nieuwe renner.");
                        return View(model);
                    }
                }

                competitor = await _competitorService.GetCompetitorByName(model.FirstName, model.LastName, model.CountryId);

                if (competitor == null)
                {
                    competitor = new Competitor
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PcsName = model.PcsName,
                        CountryId = model.CountryId
                    };
                    await _competitorService.Create(competitor);
                }
            }
            bool alreadyExists = await _competitorService.CheckCompetitorInTeam(model.CompetitorId, model.TeamId, model.Year);
            if (!alreadyExists)
            {
                var competitorInTeam = new CompetitorInTeam
                {
                    CompetitorId = competitor.CompetitorId,
                    TeamId = model.TeamId,
                    Year = model.Year,
                    IsNationalChampion = model.IsNationalChampion
                };

                await _competitorService.CreateCompetitorInTeam(competitorInTeam);
            }
            else
            {
                ModelState.AddModelError("", "Deze renner zit al in dit team voor dit jaar.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Competitors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competitor = await _competitorService.GetCompetitorById((int)id);
            if (competitor == null)
            {
                return NotFound();
            }
            var allTeams = (await _teamService.GetAll()).OrderBy(t => t.TeamName);

            // Kies het eerste team van de competitor (of filter op een specifiek jaar)
            var selectedTeamId = competitor.CompetitorInTeams.FirstOrDefault()?.TeamId ?? 0;

            ViewData["TeamId"] = new SelectList(allTeams, "TeamId", "TeamName", selectedTeamId);
            ViewData["CountryId"] = new SelectList(await CountrySelectListHelper.GetOrderedCountries(_countryService), "CountryId", "CountryNameLong", competitor.CountryId);
            return View(competitor);
        }

        // POST: Competitors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CompetitorId,FirstName,LastName,PcsName,IsNationalChampion,TeamId,CountryId")] Competitor competitor)
        {
            if (id != competitor.CompetitorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _competitorService.Update(competitor);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompetitorExists(competitor.CompetitorId))
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
            var allTeams = (await _teamService.GetAll()).OrderBy(t => t.TeamName);

            // Kies het eerste team van de competitor (of filter op een specifiek jaar)
            var selectedTeamId = competitor.CompetitorInTeams.FirstOrDefault()?.TeamId ?? 0;

            ViewData["TeamId"] = new SelectList(allTeams, "TeamId", "TeamName", selectedTeamId);
            ViewData["CountryId"] = new SelectList(await CountrySelectListHelper.GetOrderedCountries(_countryService), "CountryId", "CountryNameLong", competitor.CountryId);
            return View(competitor);
        }

        // GET: Competitors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var competitor = await _competitorService.GetCompetitorById((int)id);
            if (competitor == null)
            {
                return NotFound();
            }

            return View(competitor);
        }

        // POST: Competitors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var competitor = await _competitorService.GetCompetitorById((int)id);
            if (competitor != null)
            {
                await _competitorService.Delete(competitor);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> SearchCompetitors(string term)
        {
            var competitors = await _competitorService.GetCompetitorsByTerm(term)
                .Select(c => new
                {
                    id = c.CompetitorId,
                    label = c.FirstName + " " + c.LastName,
                    value = c.FirstName + " " + c.LastName
                })
                .ToListAsync();

            return Json(competitors);
        }

        [HttpGet]
        public async Task<IActionResult> GetCompetitorInfo(int id, int year)
        {
            var competitor = await _competitorService.GetCompetitorById(id);
            if (competitor == null) return NotFound();

            var team = competitor.CompetitorInTeams
                .FirstOrDefault(cit => cit.Year == year)?.Team;

            return Json(new
            {
                TeamName = team?.TeamName ?? "Onbekend",
                Country = competitor.Country?.CountryNameLong ?? "Onbekend",
                PcsName = competitor.PcsName ?? ""
            });
        }

        private bool CompetitorExists(int id)
        {
          return _competitorService.GetCompetitorById(id) != null;
        }

        private async Task<List<SelectListItem>> GetCompetitorSelectListAsync()
        {
            var competitors = await _competitorService.GetAllCompetitors(DateTime.Now.Year);
            var selectList = competitors.Select(c => new SelectListItem
            {
                Value = c.CompetitorId.ToString(),
                Text = $"{c.FirstName} {c.LastName}"
            }).ToList();

            selectList.Insert(0, new SelectListItem { Value = "0", Text = "-- Nieuwe renner --" });
            return selectList;
        }
    }
}
