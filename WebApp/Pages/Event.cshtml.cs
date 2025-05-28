using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    public class EventModel : PageModel
    {
        public EventModel()
        {
        }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }


        public Task<IActionResult> OnGetAsync()
        {
            return Task.FromResult<IActionResult>(Page());
        }
    }
}
