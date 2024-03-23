using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Net.Mime;
using System.Text.Json;

namespace HealthCheck.Server;

public class CustomHealthCheckOptions : HealthCheckOptions
{
    public CustomHealthCheckOptions() : base()
    {
        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;
            // Force 200 status code to avoid 503 being sent in case of Unhealthy check
            context.Response.StatusCode = StatusCodes.Status200OK;

            var result = JsonSerializer.Serialize(new
            {
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    responseTime = entry.Value.Duration.TotalMilliseconds,
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description
                }),
                totalStatus = report.Status,
                totalResponseTime = report.TotalDuration.TotalMilliseconds
            }, jsonSerializerOptions);

            await context.Response.WriteAsync(result);
        };
    }
}
