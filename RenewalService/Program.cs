using Microsoft.EntityFrameworkCore;
using RenewalService.Data;
using RenewalService.Repositories;
using RenewalService.Services;
//using RenewalService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Program.cs

// 1. DB Context
builder.Services.AddDbContext<RenewalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Repositories & Managers
//builder.Services.AddScoped<IRenewalRepository, RenewalRepository>();
//builder.Services.AddScoped<RenewalManager>();

// 3. Register HTTP Client for Inventory Service

// Program.cs in RenewalService

builder.Services.AddScoped<RenewalRepository>();
builder.Services.AddScoped<RenewalManager>();

// For HttpClient, register the class directly
builder.Services.AddHttpClient<InventoryIntegrationService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7210/");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
