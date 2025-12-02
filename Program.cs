using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
// Explicitly configure console logging so app.Logger messages appear in the terminal.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Ensure the app binds to known ports in case environment or tooling doesn't set URLs.
builder.WebHost.UseUrls("https://localhost:5001", "http://localhost:5000");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Add HTTP logging to capture request/response details in the terminal during development.
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPropertiesAndHeaders
                          | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestBody
                          | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders
                          | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseBody;
    // Limits for body logging (adjust as needed)
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable HTTP logging middleware early in the pipeline so requests/responses are logged.
app.UseHttpLogging();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    app.Logger.LogInformation("In weatherforecast WebToAuthorise");
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
