using Microsoft.EntityFrameworkCore;
using NotificationService.Clients;
using NotificationService.Data;
using NotificationService.Repositories;
using NotificationService.BackgroundWorkers;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// DB
builder.Services.AddDbContext<NotificationDbContext>(opts =>
    opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// Repos
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// HTTP clients
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


// workers
builder.Services.AddHostedService<ComplianceNotificationWorker>();

builder.Services.AddHostedService<NotificationDispatcherWorker>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-create DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
