using Common.Logging;                  // 🔹 Shared logging & exception handling
using MailService.Data;
using MailService.Repositories;
using MailService.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// =============================================================
// 0. LOGGING: SERILOG (CENTRALIZED)
// =============================================================
// Uses "Serilog" section from appsettings.json
// Enriches logs with ServiceName = "MailService"
builder.AddSerilogLogging("MailService");

// =============================================================
// 1. CONTROLLERS
// =============================================================
builder.Services.AddControllers();

// =============================================================
// 2. SWAGGER
// =============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =============================================================
// 3. DATABASE (EF CORE)
// =============================================================
// Currently using InMemory DB for email logs / queue.
// Swap to UseSqlServer(...) later if you want persistence.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("EmailDb"));

// =============================================================
// 4. DEPENDENCY INJECTION (REPOSITORY + SERVICE)
// =============================================================
builder.Services.AddScoped<IEmailRepository, EmailRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();

// =============================================================
// BUILD APP
// =============================================================
var app = builder.Build();

// =============================================================
// 5. MIDDLEWARE PIPELINE (ORDER MATTERS)
// =============================================================

// Logs every HTTP request (method, path, status, duration)
app.UseSerilogRequestLogging();

// Global exception handler from Common.Logging
// Catches ANY unhandled exception and returns a standard JSON error
app.UseGlobalExceptionHandling();

// Swagger (you probably always want it here)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
