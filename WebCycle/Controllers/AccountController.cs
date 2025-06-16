using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebCycleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
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
                FirstName = model.FirstName,
                LastName = model.LastName 
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest( new { Errors = errors});
            }

            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var frontEndUrl = _configuration["ClientSettings:FrontendBaseUrl"];
                var confirmationLink = $"{frontEndUrl}/Account/Bevestiging?userId={user.Id}&token={Uri.EscapeDataString(token)}";
                var subject = "Bevestig je e-mail";
                var body = $@"
                    <p>Hoi,</p>
                    <p>Leuk dat je je hebt aangemeld bij mijn wielrenpool!</p>
                    <p>Om je account te activeren, hoef je alleen nog even je e-mailadres te bevestigen. Klik op de knop hieronder:</p>
                    <p><a href='{confirmationLink}' style='
                        background-color: #4CAF50;
                        color: white;
                        padding: 10px 20px;
                        text-decoration: none;
                        display: inline-block;
                        border-radius: 4px;
                        font-family: sans-serif;
                        font-size: 16px;
                        '>Bevestig mijn e-mailadres</a></p>
                    <p>De link is 24 uur geldig.</p>
                    <p>Was jij dit niet? Dan kun je deze mail gewoon negeren.</p>
                    <p>Groetjes,<br/>
                    Sjors</p>
                ";


                await _emailSender.SendEmailAsync(model.Email, subject, body);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Errors = new[] { "Fout bij verzenden van bevestigingsmail.", ex.Message } });
            }

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

        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Ok(); // Don't reveal user existence

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            var frontendBaseUrl = _configuration["ClientSettings:FrontendBaseUrl"];
            var resetLink = $"{frontendBaseUrl}/account/resetpassword?email={dto.Email}&token={encodedToken}";
            var subject = "Wachtwoord vergeten? Dat lossen we op";
            var body = $@"
                <p>Hey,</p>
                <p>Je hebt aangegeven dat je je wachtwoord bent vergeten. Geen probleem, dat gebeurt de besten!</p>
                <p>Klik op de knop hieronder om een nieuw wachtwoord in te stellen:</p>
                <p><a href='{resetLink}' style='
                    display: inline-block;
                    padding: 10px 20px;
                    background-color: #007bff;
                    color: white;
                    text-decoration: none;
                    border-radius: 5px;
                '>Stel nieuw wachtwoord in</a></p>
                <p>De link is 1 uur geldig en werkt maar één keer.</p>
                <p>Heb je dit niet zelf aangevraagd? Dan kun je deze mail gerust negeren.</p>
                <p>Groet,<br/>
                Sjors</p>
            ";

            await _emailSender.SendEmailAsync(dto.Email, subject, body);

            return Ok();
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Gebruiker niet gevonden");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.Password);
            if (result.Succeeded) return Ok();

            return BadRequest(result.Errors);
        }

    }
}
