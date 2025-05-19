using Microsoft.AspNetCore.Identity;

namespace CycleManager.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(IdentityUser user);
    }
}
