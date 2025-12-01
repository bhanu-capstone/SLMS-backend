using Common.Logging;                                  // 🔹 Shared logging & exception handling
using AuthService.Models.Database;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =============================================================
// 0. LOGGING: SERILOG (CENTRALIZED)
// =============================================================
builder.AddSerilogLogging("AuthService");

// =============================================================
// 1. ADD CONTROLLERS
// =============================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// =============================================================
// 2. SWAGGER
// =============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =============================================================
// 3. DATABASE - SQL Server
// =============================================================
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// =============================================================
// 4. HTTP CLIENT FOR MAIL SERVICE
// =============================================================
builder.Services.AddHttpClient<HttpEmailClient>(client =>
{
    var url = builder.Configuration["ServiceUrls:MailService"];
    if (string.IsNullOrEmpty(url))
        throw new Exception("MailService URL missing from ServiceUrls configuration.");

    client.BaseAddress = new Uri(url);
});

// =============================================================
// 5. REGISTER REPOSITORIES
// =============================================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// =============================================================
// 6. REGISTER AUTHENTICATION SERVICES
// =============================================================
builder.Services.AddScoped<IAuthService, AuthService.Services.AuthService>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

// =============================================================
// 7. JWT AUTHENTICATION SETUP
// =============================================================
var allowedRoles = new[] { "Admin", "Finance", "Auditor", "ReadOnly" };

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

bool jwtConfigured =
    !string.IsNullOrEmpty(jwtKey) &&
    !string.IsNullOrEmpty(jwtIssuer) &&
    !string.IsNullOrEmpty(jwtAudience);

if (!jwtConfigured)
{
    Console.WriteLine("WARNING: JWT is not fully configured for AuthService.");
}
else
{
    if (jwtKey!.Length < 32)
        Console.WriteLine("WARNING: Jwt:Key should be at least 32 characters for security.");

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });
}

// =============================================================
// 8. AUTHORIZATION POLICIES
// =============================================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorsOnly", policy =>
        policy.RequireRole(allowedRoles));

    options.AddPolicy("EmployeesOnly", policy =>
        policy.RequireClaim("EmployeeNumber"));
});

// =============================================================
// 9. CORS (Allow All for Dev)
// =============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// =============================================================
// 10. BUILD APP
// =============================================================
var app = builder.Build();

// =============================================================
// 11. APPLY MIGRATIONS (Dev)
// =============================================================
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
//    db.Database.Migrate();
//}

// =============================================================
// 12. MIDDLEWARE PIPELINE
// =============================================================
app.UseSerilogRequestLogging();
app.UseGlobalExceptionHandling();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

if (jwtConfigured)
{
    app.UseAuthentication();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
