using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using Domain.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class UserRepository : GenericRepository<ApplicationUser>, IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context)
        {
            _userManager = userManager;
        }

        public async Task<List<ApplicationUser>> GetConfirmedUsersAsync()
        {
            return await _userManager.Users
            .Where(u => u.EmailConfirmed)
            .ToListAsync();
        }
    }
}
