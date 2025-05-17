using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Account
{
    [Authorize(AuthenticationSchemes = "MyCookieAuth")]
    public class ProfielModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
