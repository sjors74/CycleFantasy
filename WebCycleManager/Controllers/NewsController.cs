using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebCycleManager.Controllers
{
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;
        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }

        // GET: News
        public async Task<IActionResult> Index()
        {
            var newsItems = await _newsService.GetAllActiveNewsItems(); 
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
                await _newsService.CreateAsync(newsItem);
                return RedirectToAction(nameof(Index));
            }
            return View(newsItem);
        }

        // GET: News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var newsItem = await _newsService.GetByIdAsync((int)id);
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
                    var existingItem = await _newsService.GetByIdAsync((int)id);
                    if (existingItem == null) return NotFound();

                    // Alleen de velden bijwerken die mogen wijzigen
                    existingItem.Title = updatedItem.Title;
                    existingItem.Message = updatedItem.Message;
                    existingItem.DatePosted = updatedItem.DatePosted;
                    existingItem.IsActive = updatedItem.IsActive;

                    await _newsService.UpdateAsync(existingItem);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NewsItemExists(updatedItem.Id)) return NotFound();
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

            var newsItem = await _newsService.GetByIdAsync((int)id);
            if (newsItem == null) return NotFound();

            return View(newsItem);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var newsItem = await _newsService.GetByIdAsync(id);
            if (newsItem != null)
            {
                await _newsService.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> NewsItemExists(int id)
        {
            return await _newsService.ExistsAsync(id);
        }
    }
}
