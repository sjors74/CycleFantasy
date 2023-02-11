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
            var rvm = new ResultViewModel();
            //first get stage-data
            var stage = _context.Stages.FirstOrDefault(s => s.Id.Equals(stageId));
            if (stage != null)
            {
                rvm.StageId = stage.Id;
                rvm.StageName = $"Etappe {stage.StageName}: {stage.StartLocation}-{stage.FinishLocation}";

                //then get all available results for the current stage
                var results = _context.Results.Include(r => r.Competitor).Include(r => r.Stage).Include(r => r.ConfigurationItem)
                    .Where(r => r.Stage.Id.Equals(stageId));

                var resultItems = new List<ResultItemViewModel>();
                foreach (var result in results)
                {
                    var rivm = new ResultItemViewModel
                    {
                        Id = result.Id,
                        Position = result.ConfigurationItem.Position,
                        CompetitorName = $"{result.Competitor.FirstName} {result.Competitor.LastName}"
                    };
                    resultItems.Add(rivm);
                }
                rvm.Results = resultItems;

                return View(rvm);
            }
            return NotFound();
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
            var rvm = new ResultViewModel();
            var stage = _context.Stages.FirstOrDefault(s => s.Id == stageId);
            if (stage != null)
            {
                rvm.StageId = stage.Id;
                rvm.StageName = $"Etappe {stage.StageName}: {stage.StartLocation}-{stage.FinishLocation}";

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
                rvm.Results = resultItems;
            }
            ViewData["CompetitorId"] = new SelectList(_context.Competitors, "CompetitorId", "FirstName");
            
            return View(rvm);
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
            ViewData["CompetitorId"] = new SelectList(_context.Competitors, "CompetitorId", "FirstName", result.CompetitorId);
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
            ViewData["CompetitorId"] = new SelectList(_context.Competitors, "CompetitorId", "FirstName", result.CompetitorId);
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
            ViewData["CompetitorId"] = new SelectList(_context.Competitors, "CompetitorId", "FirstName", result.CompetitorId);
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
    }
}
