using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Domain.Context;
using Domain.Models;
using WebCycleManager.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography.X509Certificates;

namespace WebCycleManager.Controllers
{
    public class ResultsController : Controller
    {
        private readonly DatabaseContext _context;

        public ResultsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Results
        public async Task<IActionResult> Index(int stageId)
        {
            //first get stage-data
            var stage = _context.Stages.FirstOrDefault(s => s.Id.Equals(stageId));
            if (stage != null)
            {
                //rvm.StageId = stage.Id;
                //rvm.StageName = $"Etappe {stage.StageName}: {stage.StartLocation}-{stage.FinishLocation}";
                //rvm.EventId = stage.EventId;
                //then get all available results for the current stage
                var results = _context.Results.Include(r => r.Competitor).Include(r => r.Stage).Include(r => r.ConfigurationItem)
                    .Where(r => r.Stage.Id.Equals(stageId)).ToList();
                var resultDict = new Dictionary<int, int>();
                resultDict = results.ToDictionary(r => r.ConfigurationItem.Position, r => r.CompetitorId);
                var currentEvent = _context.Events.FirstOrDefault(e => e.EventId.Equals(stage.EventId));
                var config= currentEvent.Configuration;
                var numberOfconfigItems = _context.ConfigurationItems.Where(l => l.ConfigurationId.Equals(config.Id)).Count();
                //rvm.ConfigurationId = config.Id;
                //rvm.ConfigurationItems = numberOfconfigItems;

                var resultItems = new List<ResultItemViewModel>();
                for(int i=0; i < numberOfconfigItems; i++)
                {
                    var position = i + 1;
                    resultDict.TryGetValue(position, out int compId);
                    var rivm = new ResultItemViewModel
                    {
                        Position = position,
                        CompetitorName = GetCompetitorFullName(compId),
                        SelectedCompetitorId = compId
                        
                    };
                    resultItems.Add(rivm);
                }
                //rvm.Results = resultItems;
                var rvm = new ResultViewModel(stage.Id, stage.EventId, config.Id, $"Etappe {stage.StageName}: {stage.StartLocation}-{stage.FinishLocation}", numberOfconfigItems, resultItems);

                ViewData["CompetitorId"] = new SelectList(_context.Competitors.OrderBy(c => c.FirstName), "CompetitorId", "CompetitorName"); //TODO: only get event-competitors!
                return View(rvm);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int stageId, IFormCollection formCollection)
        {
            var resultList = new List<Result>();
            foreach (var key in formCollection.Keys)
            {
                if (key.Contains("SelectedCompetitorId"))
                {
                    var value = formCollection[key];

                    int.TryParse(value, out var competitorId);
                    if (competitorId > 0)
                    {
                        var position = GetPositionFromKey(key);
                        var configurationId = int.TryParse(formCollection["configurationId"], out var configId);
                        var configurationItem = await _context.ConfigurationItems.FirstOrDefaultAsync(c => c.ConfigurationId.Equals(configId) && c.Position.Equals(position));
                        if (position > 0 && configurationItem != null)
                        {
                            resultList.Add(new Result
                            {
                                CompetitorId = competitorId,
                                StageId = stageId,
                                ConfigurationItemId = configurationItem.Id
                            });
                        }
                    }
                }
            }
            _context.Results.AddRange(resultList);
            _context.SaveChanges();
            return RedirectToAction("Index", "Results", new { stageId });
        }

        private int GetPositionFromKey(string key)
        {
            key = key.Substring(key.IndexOf("[") + 1);
            key = key.Substring(0, key.IndexOf("]"));
            var positon = int.TryParse(key, out var positonNumber);
            return positonNumber + 1;
        }

        // GET: Results/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Results == null)
            {
                return NotFound();
            }

            var result = await _context.Results
                .Include(r => r.Competitor)
                .Include(r => r.Stage)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        // GET: Results/Create
        public IActionResult Create(int stageId)
        {
            var stage = _context.Stages.FirstOrDefault(s => s.Id == stageId);
            if (stage != null)
            {
                //rvm.StageId = stage.Id;
                //rvm.StageName = $"Etappe {stage.StageName}: {stage.StartLocation}-{stage.FinishLocation}";

                var resultItems = new List<ResultItemViewModel>();
                //first, get the event from stage, and it's configuration
                var config = stage.Event.Configuration;
                //then we create a resultListItem for every configurationitem found
                foreach(var configItem in config.ConfigurationItems)
                {
                    var rivm = new ResultItemViewModel
                    {
                        Id = configItem.Id,
                        Position = configItem.Position,
                        CompetitorName = string.Empty
                    };
                    resultItems.Add(rivm);
                }

                //rvm.Results = resultItems;
                var rvm = new ResultViewModel(stage.Id, stage.EventId, config.Id, $"Etappe {stage.StageName}: {stage.StartLocation}-{stage.FinishLocation}", config.ConfigurationItems.Count, resultItems);
                ViewData["CompetitorId"] = new SelectList(_context.Competitors.OrderBy(c => c.FirstName), "CompetitorId", "CompetitorName");
                return View(rvm);
            }


            return NotFound();
        }

        // POST: Results/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StageId,CompetitorId")] Result result)
        {
            if (ModelState.IsValid)
            {
                _context.Add(result);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompetitorId"] = new SelectList(_context.Competitors.OrderBy(c => c.FirstName), "CompetitorId", "CompetitorName", result.CompetitorId);
            ViewData["StageId"] = new SelectList(_context.Stages, "Id", "FinishLocation", result.StageId);
            return View(result);
        }

        // GET: Results/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Results == null)
            {
                return NotFound();
            }

            var result = await _context.Results.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            ViewData["CompetitorId"] = new SelectList(_context.Competitors.OrderBy(c => c.FirstName), "CompetitorId", "CompetitorName", result.CompetitorId);
            ViewData["StageId"] = new SelectList(_context.Stages, "Id", "FinishLocation", result.StageId);
            return View(result);
        }

        // POST: Results/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StageId,CompetitorId")] Result result)
        {
            if (id != result.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(result);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResultExists(result.Id))
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
            ViewData["CompetitorId"] = new SelectList(_context.Competitors.OrderBy(c => c.FirstName), "CompetitorId", "CompetitorName", result.CompetitorId);
            ViewData["StageId"] = new SelectList(_context.Stages, "Id", "FinishLocation", result.StageId);
            return View(result);
        }

        // GET: Results/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Results == null)
            {
                return NotFound();
            }

            var result = await _context.Results
                .Include(r => r.Competitor)
                .Include(r => r.Stage)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        // POST: Results/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Results == null)
            {
                return Problem("Entity set 'DatabaseContext.Results'  is null.");
            }
            var result = await _context.Results.FindAsync(id);
            if (result != null)
            {
                _context.Results.Remove(result);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ResultExists(int id)
        {
          return (_context.Results?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private string GetCompetitorFullName(int competitorId)
        {
            var competitor = _context.Competitors.FirstOrDefault(c => c.CompetitorId.Equals(competitorId));
            if (competitor != null)
            {
                return $"{competitor.FirstName} {competitor.LastName}";
            }
            return string.Empty;
        }
    }
}
