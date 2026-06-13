using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using ProjectAdoAssistant.Api.Configuration;
using ProjectAdoAssistant.Api.Models;
using ProjectAdoAssistant.Core.Dtos;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddOptions<ApiOptions>()
    .BindConfiguration(ApiOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("UnhandledException");

        logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path} with TraceId {TraceId}",
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(
            ApiResponse<object?>.Failure(
                "internal_server_error",
                "An unexpected error occurred.",
                context.TraceIdentifier));
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/health", (IOptions<ApiOptions> options, IWebHostEnvironment environment, ILoggerFactory loggerFactory) =>
{
    var apiOptions = options.Value;
    var logger = loggerFactory.CreateLogger("HealthEndpoint");

    logger.LogInformation(
        "Health check requested for {ServiceName} in {EnvironmentName}",
        apiOptions.ServiceName,
        environment.EnvironmentName);

    var response = ApiResponse<HealthCheckDto>.Ok(
        new HealthCheckDto(
            "ok",
            apiOptions.ServiceName,
            environment.EnvironmentName));

    return Results.Ok(response);
})
.WithName("GetHealth")
.WithOpenApi();

app.Run();
