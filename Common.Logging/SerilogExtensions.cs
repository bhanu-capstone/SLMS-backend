using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Common.Logging
{
    public static class SerilogExtensions
    {
        public static void AddSerilogLogging(this WebApplicationBuilder builder, string serviceName)
        {
            // Build Serilog logger from configuration
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", serviceName)
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
