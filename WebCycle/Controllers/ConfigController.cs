using CycleManager.Domain.Interfaces;
using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("config")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISeasonYearService _seasonYearService;

        public ConfigController(IConfiguration configuration, ISeasonYearService seasonYearService)
        {
            _configuration = configuration;
            _seasonYearService = seasonYearService;
        }

        [HttpGet]
        public IActionResult GetClientSettings()
        {
            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];
            return Ok(new {  apiBaseUrl });
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}
