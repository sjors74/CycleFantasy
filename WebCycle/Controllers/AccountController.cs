using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebCycleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        //private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ITokenService _tokenService;

        public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, IConfiguration configuration, ITokenService tokenService)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var user = new ApplicationUser
            { 
                UserName = model.Email, 
                Email = model.Email, 
                FirstName = "tbd", 
                LastName = "tbd" 
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest( new { Errors = errors});
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var frontEndUrl = _configuration["ClientSettings:FrontendBaseUrl"];
            var confirmationLink = $"{frontEndUrl}/Account/Bevestiging?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await _emailSender.SendEmailAsync(model.Email, "Bevestig je e-mail", 
                $"Klik op deze link om je account te bevestigen: <a href='{confirmationLink}'>link</a>");

            return Ok("Registratie succesvol! Bevestig je e-mail.");
        }

        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return BadRequest("Ongeldige bevestigingsgegevens.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("Gebruiker niet gevonden.");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                var frontendBaseUrl = _configuration["ClientSettings:FrontendBaseUrl"];
                return Redirect($"{frontendBaseUrl}/Account/Bevestiging");
            }

            return BadRequest("E-mail bevestiging mislukt.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized();

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized("Bevestig eerst je e-mailadres.");

            // Token genereren (JWT)
            var token = _tokenService.CreateToken(user); // jouw service

            return Ok(new LoginResponseDto { Token = token, UserId = user.Id });
        }
    }
}
