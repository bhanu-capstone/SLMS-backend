using AuthService.Models;
using AuthService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;

        public UserController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        // ----------------------------------------------------
        // 1. GET CURRENT USER INFO USING TOKEN
        // ----------------------------------------------------
        [HttpGet("me")]

        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst("id")?.Value;

            if (userId == null)
                return Unauthorized();

            var user = await _userRepo.GetByIdAsync(int.Parse(userId));

            if (user == null)
                return NotFound();

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            });
        }

        // ----------------------------------------------------
        // 2. GET IT ADMIN EMAILS
        // ----------------------------------------------------
        [HttpGet("it-emails")]
        [AllowAnonymous]
        public async Task<IActionResult> GetITAdminEmails()
        {
            var admins = await _userRepo.GetEmailsByRoleAsync("Admin");

            var dto = admins.Select(u => new
            {
                UserId = u.Id,
                Email = u.Email,
                Username = u.Username
            });

            return Ok(dto);
        }
    }
}