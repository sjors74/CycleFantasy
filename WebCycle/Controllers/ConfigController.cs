using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("config")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
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
