using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    public class Top15Model : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }
        public void OnGet()
        {
        }
    }
}
