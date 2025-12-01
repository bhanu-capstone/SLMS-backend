using AuthService.Models;
using AuthService.Models.Database;
using AuthService.Repositories;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthService(
            IUserRepository userRepo,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IRefreshTokenService refreshTokenService)
        {
            _userRepo = userRepo;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
        }

        // ---------------------------------------------------
        // 1. REGISTER USER
        // ---------------------------------------------------
        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            // 1. Validate role (RBAC enforcement)
            var allowedRoles = new[] { "Admin", "Finance", "Auditor", "ReadOnly" };

            if (!allowedRoles.Contains(request.Role))
                return false;

            // 2. Check existing username
            var existingUser = await _userRepo.GetByUsernameAsync(request.Username);
            if (existingUser != null)
                return false;

            // 3. Check existing email
            var existingEmail = await _userRepo.GetByEmailAsync(request.Email);
            if (existingEmail != null)
                return false;

            // 4. Create new user
            var user = new UserEntity
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = request.Role
            };

            await _userRepo.AddAsync(user);
            return true;
        }

        // ---------------------------------------------------
        // 2. LOGIN
        // ---------------------------------------------------
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetByUsernameAsync(request.Username);
            if (user == null) return null;

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
                return null;

            var token = _tokenService.GenerateToken(user);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user);

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                Expiration = DateTime.UtcNow.AddHours(1)
            };
        }

        // ---------------------------------------------------
        // 3. REFRESH TOKEN
        // ---------------------------------------------------
        public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken);
            if (storedToken == null) return null;

            var user = await _userRepo.GetByIdAsync(storedToken.UserId);
            if (user == null) return null;

            var newAccessToken = _tokenService.GenerateToken(user);
            var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user);

            storedToken.IsRevoked = true;
            await _refreshTokenService.RevokeRefreshTokenAsync(storedToken);

            return new AuthResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                Expiration = DateTime.UtcNow.AddHours(1)
            };
        }

        
    }
}
