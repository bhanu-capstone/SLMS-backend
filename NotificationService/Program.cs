using Common.Logging;         //  Shared Serilog + global exception handling
using Microsoft.EntityFrameworkCore;
using NotificationService.Clients;
using NotificationService.Data;
using NotificationService.Repositories;
using NotificationService.BackgroundWorkers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

// =============================================================

// 0. LOGGING: SERILOG (CENTRALIZED)

// =============================================================

// Uses "Serilog" section from appsettings.json

// Enriches logs with ServiceName = "NotificationService"

builder.AddSerilogLogging("NotificationService");

// =============================================================

// 1. DATABASE

// =============================================================

builder.Services.AddDbContext<NotificationDbContext>(opts =>

    opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// =============================================================

// 2. REPOSITORIES

// =============================================================

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// =============================================================

// 3. HTTP CLIENTS TO OTHER SERVICES

// =============================================================

builder.Services.AddHttpClient<ComplianceApiClient>(c =>

{

    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ComplianceService"]);

});

builder.Services.AddHttpClient<AuthAdminClient>(c =>

{

    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:AuthService"]);

});

builder.Services.AddHttpClient<MailServiceClient>(c =>

{

    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:MailService"]);

});

// =============================================================

// 4. BACKGROUND WORKERS

// =============================================================

builder.Services.AddHostedService<ComplianceNotificationWorker>();

builder.Services.AddHostedService<NotificationDispatcherWorker>();

// =============================================================

// 5. CONTROLLERS + SWAGGER

// =============================================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

// =============================================================

// BUILD APP

// =============================================================

var app = builder.Build();

// =============================================================

// 6. AUTO-CREATE DB (DEV / SIMPLE USE)

// =============================================================

using (var scope = app.Services.CreateScope())

{

    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

    db.Database.EnsureCreated();

}

// =============================================================

// 7. MIDDLEWARE PIPELINE (ORDER MATTERS)

// =============================================================

// Logs each HTTP request (method, path, status, duration, etc.)

app.UseSerilogRequestLogging();

// Centralized exception handling from Common.Logging

// Any unhandled exception (controllers or workers invoking HTTP) will be logged

// and API responses get a consistent JSON error shape.

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())

{

    app.UseSwagger();

    app.UseSwaggerUI();

}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

