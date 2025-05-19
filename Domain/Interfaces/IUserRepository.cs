using CycleManager.Domain.Models;

namespace CycleManager.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<List<ApplicationUser>> GetConfirmedUsersAsync();
    }
}
