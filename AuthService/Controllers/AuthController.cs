using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly HttpEmailClient _emailClient;
        public AuthController(IAuthService authService, HttpEmailClient emailClient)
        {
            _authService = authService;
            _emailClient = emailClient;
        }

        // ----------------------------------------------------
        // 1. REGISTER USER
        // ----------------------------------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var success = await _authService.RegisterAsync(request);

            if (!success)
                return BadRequest("Username or email already exists.");

            string body = $"Hello {request.Username}";

            var emailResponse = await _emailClient.SendEmailAsync(new()
            {
                To = request.Email,
                Subject = "Welcome to SLMs",
                Body = body
            });

            // ---------------------------------------------
            // Handle email response properly
            // ---------------------------------------------
            if (!emailResponse.IsSuccessStatusCode)
            {
                // Optional: read error content
                string? error = await emailResponse.Content.ReadAsStringAsync();

                return Ok(new
                {
                    Message = "User registered successfully, but email could not be sent.",
                    EmailSent = false,
                    Error = error
                });
            }

            return Ok(new
            {
                Message = "User registered successfully.",
                EmailSent = true
            });
        }



        // ----------------------------------------------------
        // 2. LOGIN
        // ----------------------------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);

            if (response == null)
                return Unauthorized("Invalid username or password.");

            return Ok(response);
        }

        // ----------------------------------------------------
        // 3. REFRESH TOKEN
        // ----------------------------------------------------
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var response = await _authService.RefreshTokenAsync(refreshToken);

            if (response == null)
                return Unauthorized("Invalid or expired refresh token.");

            return Ok(response);
        }

        
    }
}
