using System.ComponentModel;

namespace CycleManager.Domain.Dto
{
    public class LoginDto
    {
        public required string Email { get; set; }
        [DisplayName("Wachtwoord")]
        public required string Password { get; set; }    
    }
}
