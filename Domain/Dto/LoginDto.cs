using System.ComponentModel;

namespace CycleManager.Domain.Dto
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        [DisplayName("Wachtwoord")]
        public string Password { get; set; } = string.Empty;    
    }
}
