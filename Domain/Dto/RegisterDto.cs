using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CycleManager.Domain.Dto
{
    public class RegisterDto
    {
        [DisplayName("Voornaam")]
        public string FirstName { get; set; }
        [DisplayName("Achternaam")]
        public string LastName { get; set; }
        public string Email { get;set; }
        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Wachtwoord moet minstens 6 tekens zijn.")]
        [DataType(DataType.Password)]
        [DisplayName("Wachtwoord")]
        public string Password { get;set; }

        [Required(ErrorMessage = "Bevestig je wachtwoord")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Wachtwoorden komen niet overeen")]
        [DisplayName("Bevestig wachtwoord")]
        public string ConfirmPassword { get; set; }
    }
}
