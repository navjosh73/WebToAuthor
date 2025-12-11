using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
// Explicitly configure console logging so app.Logger messages appear in the terminal.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Ensure the app binds to known ports in case environment or tooling doesn't set URLs.
builder.WebHost.UseUrls("http://localhost:5000");

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

app.Use(async (ctx, next) =>
{
    Console.WriteLine($"REQUEST: {ctx.Request.Method} {ctx.Request.Path}");
    try
    {
        await next();
        Console.WriteLine($"RESPONSE: {ctx.Response.StatusCode} for {ctx.Request.Path}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"EXCEPTION: {ex.GetType().Name} - {ex.Message}");
        throw;
    }
});



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

app.MapGet("/authorise", async (string? token)=>
{
    // Simple token validation (hardcoded for demonstration)
    
    if (string.IsNullOrWhiteSpace(token) || token != "valid-token-123")
    {
        app.Logger.LogWarning("Authorisation failed for token: {Token}", token);
        return Results.Unauthorized();
    }

    app.Logger.LogInformation("Authorisation succeeded for token: {Token}", token);
    return Results.Text("Authorisation successful");
});

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

app.Run("http://0.0.0.0:8080");

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
