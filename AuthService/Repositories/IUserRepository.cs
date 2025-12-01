using AuthService.Models;
using AuthService.Models.Database;

namespace AuthService.Repositories
{
    public interface IUserRepository
    {
        Task<UserEntity?> GetByUsernameAsync(string username);
        Task<UserEntity?> GetByEmailAsync(string email);
        Task<UserEntity?> GetByIdAsync(int id);
        Task AddAsync(UserEntity user);
        Task UpdateAsync(UserEntity user);
        Task <IEnumerable<UserEmailDto>> GetEmailsByRoleAsync(string role);
    }
}
