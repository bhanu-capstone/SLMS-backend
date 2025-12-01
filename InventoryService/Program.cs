using Common.Logging;                          // 🔹 shared logging library (Serilog + middlewares)
using InventoryService.Models;
using InventoryService.Repositories;
using InventoryService.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;                                  // 🔹 for UseSerilogRequestLogging

var builder = WebApplication.CreateBuilder(args);

// =============================================================
// 0. LOGGING: SERILOG (centralized)
// =============================================================
// Uses configuration from appsettings.json -> "Serilog" section
// and enriches logs with ServiceName="InventoryService"
builder.AddSerilogLogging("InventoryService");

// =============================================================
// 1. DATABASE: SQL Server
// =============================================================
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// =============================================================
// 2. REPOSITORIES
// =============================================================
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILicenseRepository, LicenseRepository>();
builder.Services.AddScoped<IEntitlementRepository, EntitlementRepository>();
builder.Services.AddScoped<ISoftwareCatalogRepository, SoftwareCatalogRepository>();
builder.Services.AddScoped<IInstalledSoftwareRepository, InstalledSoftwareRepository>();
builder.Services.AddScoped<IInstallationHistoryRepository, InstallationHistoryRepository>();
builder.Services.AddScoped<IVendorContractRepository, VendorContractRepository>();

// =============================================================
// 3. SERVICES
// =============================================================
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();
builder.Services.AddScoped<IEntitlementService, EntitlementService>();
builder.Services.AddScoped<ISoftwareCatalogService, SoftwareCatalogService>();
builder.Services.AddScoped<IInstallationService, InstallationService>();
builder.Services.AddScoped<IInstallationHistoryService, InstallationHistoryService>();
builder.Services.AddScoped<IVendorContractService, VendorContractService>();

// =============================================================
// 4. CONTROLLERS + JSON SETTINGS
// =============================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Avoid reference loop issues in navigation properties
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// =============================================================
// 5. SWAGGER
// =============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =============================================================
// 6. CORS
// =============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// =============================================================
// 7. JWT AUTHENTICATION
// =============================================================
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Validate configuration early so app fails fast if misconfigured
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("Jwt:Key is not configured in appsettings.json");
if (string.IsNullOrEmpty(jwtIssuer))
    throw new InvalidOperationException("Jwt:Issuer is not configured in appsettings.json");
if (string.IsNullOrEmpty(jwtAudience))
    throw new InvalidOperationException("Jwt:Audience is not configured in appsettings.json");

if (jwtKey.Length < 32)
    Console.WriteLine("WARNING: Jwt:Key should be at least 32 characters for security");

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

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"Token validated for: {context.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
    });

// =============================================================
// 8. AUTHORIZATION POLICIES
// =============================================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorsOnly", policy =>
        policy.RequireRole("Admin", "Administrator"));

    options.AddPolicy("EmployeesOnly", policy =>
        policy.RequireClaim("EmployeeNumber"));

    options.AddPolicy("ReadOnly", policy =>
        policy.RequireClaim("Permission", "Read"));
});

// =============================================================
// BUILD APP
// =============================================================
var app = builder.Build();

// =============================================================
// 9. MIDDLEWARE PIPELINE (Order matters!)
// =============================================================

// Serilog HTTP request logging (status, time taken, etc.)
app.UseSerilogRequestLogging();

// Correlation ID -> adds X-Correlation-Id header and logs it
//app.UseCorrelationId();

// Global exception handler -> catches all unhandled exceptions
// and returns a consistent JSON error payload
app.UseGlobalExceptionHandling();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CRITICAL: Authentication BEFORE Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
