using AuthService.Models;
using AuthService.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _db;

        public UserRepository(AuthDbContext db)
        {
            _db = db;
        }

        public Task<UserEntity?> GetByUsernameAsync(string username)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public Task<UserEntity?> GetByEmailAsync(string email)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<UserEntity?> GetByIdAsync(int id)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task AddAsync(UserEntity user)
        {
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserEntity user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserEmailDto>> GetEmailsByRoleAsync(string role)
        {
            return await _db.Users
                .Where(u => u.Role == role)
                .Select(u => new UserEmailDto
                {
                    Id = u.Id,
                    Email = u.Email
                })
                .ToListAsync();
        }

    }
}
