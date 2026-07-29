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
        private readonly ISeasonYearService _seasonYearService;

        public CompetitorsController(ICompetitorService competitorService, ITeamService teamService, ICountryService countryService, ISeasonYearService seasonYearService)
        {
            _competitorService = competitorService;
            _teamService = teamService;
            _countryService = countryService;
            _seasonYearService = seasonYearService;
        }

        // GET: Competitors
        public async Task<IActionResult> Index(string currentFilter, string? searchString, int? pageNumber, int? seasonYearId)
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

            var activeSeason = availableYears.SingleOrDefault(a => a.Active);

            if (activeSeason == null)
            {
                return View("ConfigurationError");
            }
            var selectedSeasonYearId = seasonYearId ?? activeSeason.SeasonYearId;

            ViewData["SelectedSeasonYearId"] = selectedSeasonYearId;

            var pageSize = ConfigurationConstants.PageSize;

            var competitors = await _competitorService.GetAllCompetitors(selectedSeasonYearId);

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

            var vm = new CompetitorIndexViewModel
            {
                Competitors = PaginatedList<CompetitorDto>.Create(
                    orderedList,
                    pageNumber ?? 1,
                    pageSize),
                AvailableYears = availableYears
                    .OrderByDescending(y => y.Year)
                    .Select(y => new SeasonYearViewModel
                    {
                        SeasonYearId = y.SeasonYearId,
                        Year = y.Year,
                        Active = y.Active
                    })
                    .ToList(),
                SelectedSeasonYearId = selectedSeasonYearId,
                CurrentFilter = searchString
            };

            return View(vm);
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
        public async Task<IActionResult> Create(int? seasonYearId)
        {
            var seasonYears = await _seasonYearService.GetAllAsync();

            var activeSeason = seasonYears.Single(s => s.Active);
            var selectedSeasonYearId = seasonYearId ?? activeSeason.SeasonYearId;
            var selectedSeason = seasonYears.Single(s => s.SeasonYearId == selectedSeasonYearId);

            var teams = (await _teamService.GetTeamYears(selectedSeasonYearId))
                .OrderBy(t => t.Name)
                .Select(ty => new SelectListItem {  Value = ty.TeamYearId.ToString(), Text = ty.Name })
                .ToList();

            teams.Insert(0, new SelectListItem { Value = "", Text = "-- Kies een team --" });
            
            var countries = (await CountrySelectListHelper.GetOrderedCountries(_countryService))
                .Select(c => new SelectListItem { Value = c.CountryId.ToString(), Text = c.CountryNameLong })
                .ToList();
            countries.Insert(0, new SelectListItem { Value = "", Text = "-- Kies een land --" });
            
            var vm = new CreateCompetitorViewModel
            {
                CompetitorId = 0,
                SeasonYearId = selectedSeasonYearId,
                SeasonYear = selectedSeason.Year,
                Teams = teams,
                Countries = countries
            };

            return View(vm);
        }

        // POST: Competitors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCompetitorViewModel model)
        {

            async Task PopulateDropdownsAsync()
            {
                var season = await _seasonYearService.GetByIdAsync(model.SeasonYearId);
                if (season != null)
                {
                    model.SeasonYear = season.Year;
                }

                model.Teams = (await _teamService.GetTeamYears(model.SeasonYearId))
                    .OrderBy(t => t.Name)
                    .Select(t => new SelectListItem
                    {
                        Value = t.TeamYearId.ToString(),
                        Text = t.Name
                    })
                    .ToList();

                model.Teams.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Kies een team --"
                });

                model.Countries = (await CountrySelectListHelper.GetOrderedCountries(_countryService))
                    .Select(c => new SelectListItem
                    {
                        Value = c.CountryId.ToString(),
                        Text = c.CountryNameLong
                    })
                    .ToList();

                model.Countries.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Kies een land --"
                });
            }

            async Task<IActionResult> ReturnViewAsync()
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Competitors = await GetCompetitorSelectListAsync(model.SeasonYearId);
                return await ReturnViewAsync();
            }

            Competitor competitor;
            if (model.CompetitorId > 0)
            {
                competitor = await _competitorService.GetCompetitorById(model.CompetitorId);
                if (competitor == null)
                {
                    ModelState.AddModelError("", "Geselecteerde renner bestaat niet.");
                    return await ReturnViewAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName))
                {
                    ModelState.AddModelError("", "Vul naam in voor nieuwe renner.");
                    return await ReturnViewAsync();
                }

                // If PCS name is missing, default it (prevents NULL DB insert)
                var pcsName = string.IsNullOrWhiteSpace(model.PcsName)
                    ? $"{model.FirstName} {model.LastName}"
                    : model.PcsName;

                competitor = await _competitorService.GetCompetitorByName(model.FirstName, model.LastName, model.CountryId);

                if (competitor == null)
                {
                    competitor = new Competitor
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PcsName = pcsName,
                        CountryId = model.CountryId
                    };
                    await _competitorService.Create(competitor);
                }
            }
            bool alreadyExists = await _competitorService.CheckCompetitorInTeam(competitor.CompetitorId, model.TeamYearId!.Value);
            if (!alreadyExists)
            {
                var competitorInTeam = new CompetitorInTeam
                {
                    CompetitorId = competitor.CompetitorId,
                    TeamYearId = model.TeamYearId!.Value,
                    IsNationalChampion = model.IsNationalChampion
                };

                await _competitorService.CreateCompetitorInTeam(competitorInTeam);
            }
            else
            {
                ModelState.AddModelError("", "Deze renner zit al in dit team voor dit seizoen.");
                return await ReturnViewAsync();
            }

            return RedirectToAction(nameof(Index), new { seasonYearId = model.SeasonYearId});
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
                SelectedTeamYearId = dto.SelectedTeamYearId,
                SelectedSeasonYearId = dto.SelectedSeasonYearId,
                ReturnUrl = returnUrl,
                
                Countries = dto.Countries.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CountryNameLong,
                    Selected = (c.Id == dto.CountryId)
                }),

                Teams = dto.Teams.Select(t => new SelectListItem
                {
                    Value = t.TeamYearId.ToString(),
                    Text = t.Name,
                    Selected = (t.TeamYearId == dto.SelectedTeamYearId)
                }),
                RatingCategories = dto.RatingCategories,
                Ratings = dto.Ratings,
                AvailableYears = dto.AvailableYears.Select(y => new SeasonYearViewModel
                {
                    SeasonYearId = y.SeasonYearId,
                    Year = y.Year
                })
                .OrderByDescending(y => y.Year)
                .ToList(),
                CompetitorInTeams = dto.CompetitorInTeams
                    .Select(cit => new CompetitorInTeamEditModel
                    {
                        CompetitorInTeamId = cit.CompetitorInTeamId,
                        TeamYearId = cit.TeamYearId,
                        TeamName = cit.TeamName,
                        SeasonYearId = cit.SeasonYearId,
                        Year = cit.Year,
                        IsNationalChampion = cit.IsNationalChampion
                    })
                    .ToList()
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

                CompetitorInTeams = input.CompetitorInTeams
                    .Select(c => new CompetitorInTeamDto
                    {
                        CompetitorInTeamId = c.CompetitorInTeamId,
                        TeamYearId = c.TeamYearId,
                        SeasonYearId = c.SeasonYearId,
                        Year = c.Year,
                        IsNationalChampion = c.IsNationalChampion
                    })
                    .ToList()
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
        public async Task<IActionResult> GetCompetitorInfo(int id, int seasonYearId)
        {
            var competitor = await _competitorService.GetCompetitorById(id);
            if (competitor == null) return NotFound();

            var competitorInTeam = competitor.CompetitorInTeams
                .FirstOrDefault(cit => cit.TeamYear.SeasonYearId == seasonYearId);

            return Json(new
            {
                TeamName = competitorInTeam?.TeamYear.Name ?? "Onbekend",
                Country = competitor.Country?.CountryNameLong ?? "Onbekend",
                PcsName = competitor.PcsName ?? ""
            });
        }

        private bool CompetitorExists(int id)
        {
          return _competitorService.GetCompetitorById(id) != null;
        }

        private async Task<List<SelectListItem>> GetCompetitorSelectListAsync(int seasonYearId)
        {
            var competitors = await _competitorService.GetAllCompetitors(seasonYearId);
            var selectList = competitors
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Select(c => new SelectListItem
                {
                    Value = c.CompetitorId.ToString(),
                    Text = $"{c.FirstName} {c.LastName}"
                })
                .ToList();

            selectList.Insert(0, new SelectListItem
            { 
                Value = "0", 
                Text = "-- Nieuwe renner --" 
            });

            return selectList;
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

                Countries = dto.Countries.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CountryNameLong,
                    Selected = c.Id == dto.CountryId
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
                CountryId = vm.CountryId
            };
        }

    }
}
