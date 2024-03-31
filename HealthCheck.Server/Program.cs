global using HealthCheck.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks()
    .AddCheck("ICMP_01", new ICMPHealthCheck("www.ryadel.com", 100))
    .AddCheck("ICMP_02", new ICMPHealthCheck("www.google.com", 100))
    .AddCheck("ICMP_03", new ICMPHealthCheck($"www.{Guid.NewGuid():N}.com", 100));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
    options.AddPolicy(name: "AngularPolicy",
        cfg =>
        {
            string allowedCorsOrigins = builder.Configuration["AllowedCORS"]
                ?? throw new InvalidOperationException("Missing necessary configuration: AllowedCORS");
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
            cfg.WithOrigins(allowedCorsOrigins);
        }));

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("AngularPolicy");
app.UseHealthChecks(new PathString("/api/health"), new CustomHealthCheckOptions());
app.MapControllers();
app.MapMethods("/api/heartbeat", ["HEAD"], () => Results.Ok());
app.MapFallbackToFile("/index.html");
app.Run();
