using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public ResetPasswordModel(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string Email { get; set; }
            public string Token { get; set; }

            [Required(ErrorMessage = "Wachtwoord is verplicht")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Wachtwoord moet minstens 6 tekens zijn.")]
            [DataType(DataType.Password)]
            [Display(Name ="Wachtwoord")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Bevestig je wachtwoord")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Wachtwoorden komen niet overeen")]
            [Display(Name ="Bevestig wachtwoord")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string email, string token)
        {
            Input = new InputModel { Email = email, Token = token };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _clientFactory.CreateClient();
            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];
            var response = await client.PostAsJsonAsync($"{apiBaseUrl}/api/account/resetpassword", Input);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("ResetPasswordConfirmation");
            }

            ModelState.AddModelError(string.Empty, "Wachtwoord reset mislukt.");
            return Page();
        }
    }
}
