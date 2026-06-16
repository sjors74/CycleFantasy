using CycleManager.Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Get a list of all applicationusers in an event
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ApplicationUser>> GetAllUsers();
    }
}
