using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;

namespace CycleManager.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        
        public UserService(IUserRepository userRepo)
        {
            _userRepository = userRepo;
        }
        public async Task<IEnumerable<ApplicationUser>> GetAllUsers()
        {
            return await _userRepository.GetConfirmedUsersAsync();
        }
    }
}
