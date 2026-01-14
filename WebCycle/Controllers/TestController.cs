using Domain.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCycle.Services;

namespace WebCycle.Controllers
{
    [ApiController]
    [Route("test")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public TestController(ApplicationDbContext db) => _db = db;

        [HttpGet("dump-events")]
        public IActionResult DumpEvents()
        {
            return Ok(_db.Events.ToList());
        }

        [HttpGet("seed-ready")]
        public async Task<IActionResult> SeedReady()
        {
            await SeedData.EnsureSeedAsync(_db);
            return Ok();
        }

        [HttpGet("deelnemers")]
        public async Task<IActionResult> GetDeelnemers()
        {
            var data = await _db.GameCompetitorsEvent
                .ToListAsync();

            return Ok(data);
        }
    }
}
