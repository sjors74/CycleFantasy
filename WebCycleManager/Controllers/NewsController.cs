using Domain.Context;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebCycleManager.Controllers
{
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public NewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: News
        public async Task<IActionResult> Index()
        {
            var newsItems = await _context.NewsItems
                .OrderByDescending(n => n.DatePosted)
                .ToListAsync();
            return View(newsItems);
        }

        // GET: News/Create
        public IActionResult Create()
        {
            return View(new NewsItem { DatePosted = DateTime.Now, IsActive = true, Title = "", Message = "" });
        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsItem newsItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(newsItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(newsItem);
        }

        // GET: News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var newsItem = await _context.NewsItems.FindAsync(id);
            if (newsItem == null) return NotFound();

            return View(newsItem);
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NewsItem updatedItem)
        {
            if (id != updatedItem.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingItem = await _context.NewsItems.FindAsync(id);
                    if (existingItem == null) return NotFound();

                    // Alleen de velden bijwerken die mogen wijzigen
                    existingItem.Title = updatedItem.Title;
                    existingItem.Message = updatedItem.Message;
                    existingItem.DatePosted = updatedItem.DatePosted;
                    existingItem.IsActive = updatedItem.IsActive;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewsItemExists(updatedItem.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(updatedItem);
        }

        // GET: News/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var newsItem = await _context.NewsItems.FirstOrDefaultAsync(m => m.Id == id);
            if (newsItem == null) return NotFound();

            return View(newsItem);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var newsItem = await _context.NewsItems.FindAsync(id);
            if (newsItem != null)
            {
                _context.NewsItems.Remove(newsItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool NewsItemExists(int id) =>
            _context.NewsItems.Any(e => e.Id == id);
    }
}
