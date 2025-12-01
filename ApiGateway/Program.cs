using Common.Logging;                          // 🔹 Shared Serilog + exception middleware
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =============================================================
// 0. LOGGING: SERILOG (CENTRALIZED)
// =============================================================
// Uses "Serilog" section from appsettings.json
// Enriches logs with ServiceName = "ApiGateway"
builder.AddSerilogLogging("ApiGateway");

// =============================================================
// 1. Load ocelot.json
// =============================================================
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// =============================================================
// 2. Add CORS (Allow All)
// =============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// =============================================================
// 3. Swagger for Gateway UI
// =============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =============================================================
// 4. JWT Authentication
// NOTE: Scheme name "JwtBearer" must match what is used in ocelot.json
// =============================================================
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

bool jwtConfigured =
    !string.IsNullOrEmpty(jwtKey) &&
    !string.IsNullOrEmpty(jwtIssuer) &&
    !string.IsNullOrEmpty(jwtAudience);

if (!jwtConfigured)
{
    Console.WriteLine("WARNING: JWT is not fully configured for ApiGateway. Check Jwt:Key/Issuer/Audience in appsettings.json.");
}

builder.Services.AddAuthentication("JwtBearer")
    .AddJwtBearer("JwtBearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey ?? string.Empty))
        };
    });

// Add authorization services
builder.Services.AddAuthorization();

// =============================================================
// 5. Add Ocelot
// =============================================================
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// =============================================================
// 6. MIDDLEWARE PIPELINE (ORDER MATTERS)
// =============================================================

// Serilog request logging (method, path, status, duration)
app.UseSerilogRequestLogging();

// Global exception handling from Common.Logging
// Any unhandled exception in gateway returns consistent JSON error
app.UseGlobalExceptionHandling();

// CORS
app.UseCors("AllowAll");

// Authentication & Authorization (must be before Swagger & Ocelot)
app.UseAuthentication();
app.UseAuthorization();

// Swagger for gateway (optional, usually for debugging)
app.UseSwagger();
app.UseSwaggerUI();

// =============================================================
// 7. Run Ocelot Middleware
// =============================================================
await app.UseOcelot();

app.Run();
