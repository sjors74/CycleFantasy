using CycleManager.Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public ForgotPasswordModel(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public ForgotPasswordDto Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _clientFactory.CreateClient();
            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];
            var response = await client.PostAsJsonAsync($"{apiBaseUrl}/account/forgotpassword", new { Input.Email });

            if (response.IsSuccessStatusCode)
            {
                // Toon bevestiging
                return RedirectToPage("ForgotPasswordConfirmation");
            }

            ModelState.AddModelError(string.Empty, "Er is iets misgegaan.");
            return Page();
        }
    }
}
