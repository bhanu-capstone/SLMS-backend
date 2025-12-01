using Common.Logging;                            // 🔹 Shared logging & exception middleware
using ComplianceService.Models;
using ComplianceService.Repositories;
using ComplianceService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =============================================================
// 0. LOGGING: SERILOG (CENTRALIZED)
// =============================================================
// Reads Serilog config from appsettings.json ("Serilog" section)
// Enriches logs with ServiceName = "ComplianceService"
builder.AddSerilogLogging("ComplianceService");

// =============================================================
// 1. DATABASE: SQL Server
// =============================================================
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// =============================================================
// 2. HTTP CLIENTS (INVENTORY API CLIENT)
// =============================================================
builder.Services.AddHttpClient<InventoryApiClient>(client =>
{
    var url = builder.Configuration["ServiceUrls:InventoryService"];

    if (string.IsNullOrEmpty(url))
        throw new Exception("InventoryService URL is missing. Add it in appsettings.json under ServiceUrls!");

    client.BaseAddress = new Uri(url);
});

// =============================================================
// 3. DEPENDENCY INJECTION (REPOSITORIES + SERVICES)
// =============================================================
builder.Services.AddScoped<IComplianceEventRepository, ComplianceEventRepository>();
builder.Services.AddScoped<MatchingService>();
builder.Services.AddScoped<ComplianceEngineService>();

// =============================================================
// 4. CONTROLLERS
// =============================================================
builder.Services.AddControllers();

// =============================================================
// 5. SWAGGER
// =============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =============================================================
// 6. JWT AUTHENTICATION
// =============================================================
// Allowed roles for this service
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
    Console.WriteLine("WARNING: JWT is not fully configured for ComplianceService. Running without authentication.");
}
else
{
    if (jwtKey.Length < 32)
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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });
}

// =============================================================
// 7. AUTHORIZATION POLICIES
// =============================================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorsOnly", policy =>
        policy.RequireRole(allowedRoles));

    options.AddPolicy("EmployeesOnly", policy =>
        policy.RequireClaim("EmployeeNumber"));
});

// =============================================================
// BUILD APP
// =============================================================
var app = builder.Build();

// =============================================================
// 8. MIDDLEWARE PIPELINE (ORDER MATTERS)
// =============================================================

// Structured request logging (method, path, status, duration, etc.)
app.UseSerilogRequestLogging();

// Global exception handling (from Common.Logging)
// Catches ANY unhandled exception and returns a standard JSON response
app.UseGlobalExceptionHandling();

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
