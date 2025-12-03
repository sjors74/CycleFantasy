using CycleManager.Domain.Dto;
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
using WebCycleManager.Models.ViewModel;

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
                        .ToList();
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
            ViewData["TeamId"] = new SelectList((await _teamService.GetAllTeams()).OrderBy(t => t.CurrentTeamName), "TeamId", "TeamName");
            ViewData["CountryId"] = new SelectList(await CountrySelectListHelper.GetOrderedCountries(_countryService), "CountryId", "CountryNameLong");
            return View(new CreateCompetitorViewModel());
        }

        // POST: Competitors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCompetitorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Competitors = await GetCompetitorSelectListAsync();
                ViewData["TeamId"] = new SelectList((await _teamService.GetAllTeams()).OrderBy(t => t.CurrentTeamName), "TeamId", "TeamName");
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
        public async Task<IActionResult> Edit(int id, string? returnUrl)
        {
            var dto = await _competitorService.GetCompetitorForEdit(id);
            if (dto == null) return NotFound();

            var vm = new CompetitorEditViewModel
            {
                CompetitorId = dto.CompetitorId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PcsName = dto.PcsName,
                ScraperName = dto.ScraperName,
                CountryId = dto.CountryId,
                SelectedTeamId = dto.SelectedTeamId,
                SelectedYear = dto.SelectedYear,
                ReturnUrl = returnUrl,
                
                Countries = dto.Countries.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CountryNameLong,
                    Selected = (c.Id == dto.CountryId)
                }),

                Teams = dto.Teams.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Naam,
                    Selected = (t.Id == dto.SelectedTeamId)
                }),

                AvailableYears = dto.AvailableYears.Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString(),
                    Selected = (y == dto.SelectedYear)
                }),

                CompetitorInTeams = dto.CompetitorInTeams.Select(cit => new CompetitorInTeamEditModel
                {
                    CompetitorInTeamId = cit.CompetitorInTeamId,
                    TeamName = cit.TeamName,
                    TeamNameForYear  = cit.TeamNameForYear,
                    Year = cit.Year,
                    IsNationalChampion = cit.IsNationalChampion,
                    TeamId = cit.TeamId
                }).ToList()
            };

            return View(vm);
        }

        // POST: Competitors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CompetitorEditInputModel input)
        {
            if (!ModelState.IsValid)
            {
                var dto = await _competitorService.GetCompetitorForEdit(input.CompetitorId);
                var vm = MapDtoToViewModel(dto);
                foreach (var cit in vm.CompetitorInTeams)
                {
                    var matchingInput = input.CompetitorInTeams.FirstOrDefault(c => c.CompetitorInTeamId == cit.CompetitorInTeamId);
                    if (matchingInput != null)
                    {
                        cit.IsNationalChampion = matchingInput.IsNationalChampion;
                    }
                }

                return View(vm);
            }
            var dtoUpdate = new CompetitorEditDto
            {
                CompetitorId = input.CompetitorId,
                FirstName = input.FirstName,
                LastName = input.LastName,
                PcsName = input.PcsName,
                ScraperName = input.ScraperName,
                CountryId = input.CountryId,
                CompetitorInTeams = input.CompetitorInTeams.Select(c => new CompetitorInTeamDto
                {
                    CompetitorInTeamId = c.CompetitorInTeamId,
                    Year = c.Year,
                    IsNationalChampion = c.IsNationalChampion,
                    TeamId = c.TeamId
                }).ToList()
            };

            await _competitorService.UpdateCompetitorWithTeam(dtoUpdate);

            return RedirectToAction(nameof(Index));
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
                TeamName = team?.CompetitorInTeams.FirstOrDefault()?.Team.CurrentTeamName ?? "Onbekend",
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
        private Task<IEnumerable<SelectListItem>> GetAvailableYears(int selectedYear)
        {
            var currentYear = DateTime.Now.Year;
            var years = Enumerable.Range(currentYear - 3, 7); // range -3 tot +3
            return Task.FromResult(years.Select(y => new SelectListItem
            {
                Value = y.ToString(),
                Text = y.ToString(),
                Selected = (y == selectedYear)
            }));
        }
        private CompetitorEditViewModel MapDtoToViewModel(CompetitorEditDto dto)
        {
            return new CompetitorEditViewModel
            {
                CompetitorId = dto.CompetitorId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PcsName = dto.PcsName,
                ScraperName = dto.ScraperName,
                CountryId = dto.CountryId,
                SelectedTeamId = dto.SelectedTeamId,
                SelectedYear = dto.SelectedYear,

                Countries = dto.Countries.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CountryNameLong
                }),

                Teams = dto.Teams.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Naam
                }),

                AvailableYears = dto.AvailableYears.Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString()
                }),

                CompetitorInTeams = dto.CompetitorInTeams.Select(cit => new CompetitorInTeamEditModel
                {
                    CompetitorInTeamId = cit.CompetitorInTeamId,
                    TeamName = cit.TeamName,
                    Year = cit.Year,
                    IsNationalChampion = cit.IsNationalChampion,
                    TeamId = cit.TeamId
                }).ToList()
            };
        }

        private CompetitorEditInputModel MapViewModelToInputModel(CompetitorEditViewModel vm)
        {
            return new CompetitorEditInputModel
            {
                CompetitorId = vm.CompetitorId,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                PcsName = vm.PcsName,
                ScraperName = vm.ScraperName,
                CountryId = vm.CountryId,
                CompetitorInTeams = vm.CompetitorInTeams.Select(cit => new CompetitorInTeamInputModel
                {
                    CompetitorInTeamId = cit.CompetitorInTeamId,
                    Year = cit.Year,
                    IsNationalChampion = cit.IsNationalChampion,
                    TeamId = cit.TeamId
                }).ToList()
            };
        }

    }
}
