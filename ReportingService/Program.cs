using Common.Logging;                                // 🔹 Shared logging & global exception handling
using ReportingService.Repositories.Inventory;
using ReportingService.Repositories.Compliance;
using ReportingService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =============================================================
// 0. LOGGING: SERILOG (CENTRALIZED)
// =============================================================
// Uses "Serilog" section from appsettings.json
// Enriches logs with ServiceName = "ReportingService"
builder.AddSerilogLogging("ReportingService");

// =============================================================
// 1. CONTROLLERS
// =============================================================
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // Keep property names as defined in DTOs (PascalCase)
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// =============================================================
// 2. SWAGGER
// =============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =============================================================
// 3. HTTP CLIENT FACTORY (INVENTORY + COMPLIANCE)
// =============================================================
builder.Services.AddHttpClient();

// =============================================================
// 4. INVENTORY REPOSITORIES
// =============================================================
builder.Services.AddScoped<ILicenseRepository, LicenseRepository>();
builder.Services.AddScoped<IEntitlementRepository, EntitlementRepository>();
builder.Services.AddScoped<IInstallationRepository, InstallationRepository>();

// =============================================================
// 5. COMPLIANCE REPOSITORY
// =============================================================
builder.Services.AddScoped<IComplianceReportRepository, ComplianceReportRepository>();

// =============================================================
// 6. BUSINESS SERVICES (ANALYTICS LAYER)
// =============================================================
builder.Services.AddScoped<IReportingService, ReportingService.Services.ReportingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITrendService, TrendService>();

// =============================================================
// 7. JWT AUTHENTICATION
// =============================================================
// Allowed roles for this service (report access)
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
    Console.WriteLine("WARNING: JWT is not fully configured for ReportingService. Running without authentication.");
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
// 8. AUTHORIZATION POLICIES
// =============================================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorsOnly", policy =>
        policy.RequireRole(allowedRoles));
});

// =============================================================
// 9. CORS POLICY
// =============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// =============================================================
// BUILD APP
// =============================================================
var app = builder.Build();

// =============================================================
// 10. MIDDLEWARE PIPELINE (ORDER MATTERS)
// =============================================================

// Serilog request logging (method, path, status, duration)
app.UseSerilogRequestLogging();

// Global exception handling from Common.Logging
// Returns consistent JSON error + logs full exception
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
