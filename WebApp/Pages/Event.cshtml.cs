using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    public class EventModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public EventModel(IConfiguration configuration)
        {
            _configuration = configuration;
            if (!int.TryParse(_configuration["ClientSettings:MaxRating"], out var maxRating))
            {
                maxRating = 0;
            }
            MaxRating = maxRating;
        }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        public int MaxRating { get; }

        public Task<IActionResult> OnGetAsync()
        {
            return Task.FromResult<IActionResult>(Page());
        }
    }
}
