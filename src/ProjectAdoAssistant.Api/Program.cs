using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using ProjectAdoAssistant.Api.Configuration;
using ProjectAdoAssistant.Api.Models;
using ProjectAdoAssistant.Api.Services;
using ProjectAdoAssistant.Core.Dtos;
using ProjectAdoAssistant.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddOptions<ApiOptions>()
    .BindConfiguration(ApiOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<FoundryAgentOptions>()
    .BindConfiguration(FoundryAgentOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IFoundryAgentClient, FoundryAgentClient>();

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
            "Unhandled exception with TraceId {TraceId}",
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

app.MapPost("/api/chat", async (
    ChatRequestDto request,
    IFoundryAgentClient agentClient,
    ILoggerFactory loggerFactory,
    HttpContext context,
    CancellationToken cancellationToken) =>
{
    var logger = loggerFactory.CreateLogger("ChatEndpoint");

    if (string.IsNullOrWhiteSpace(request.UserMessage))
    {
        return Results.BadRequest(
            ApiResponse<ChatResponseDto>.Failure(
                "invalid_request",
                "UserMessage must not be empty.",
                context.TraceIdentifier));
    }

    logger.LogInformation(
        "Chat request received. ConversationId: {ConversationId}",
        (request.ConversationId ?? "(new)").Replace('\n', '_').Replace('\r', '_'));

    try
    {
        var result = await agentClient.SendMessageAsync(
            request.UserMessage,
            request.ConversationId,
            cancellationToken);

        return Results.Ok(ApiResponse<ChatResponseDto>.Ok(result));
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
    }
    catch (TimeoutException)
    {
        return Results.StatusCode(StatusCodes.Status504GatewayTimeout);
    }
    catch (InvalidOperationException ex)
    {
        logger.LogWarning(ex, "Chat request failed with operation error");
        return Results.BadRequest(ApiResponse<object?>.Failure(
            "agent_operation_failed",
            ex.Message,
            context.TraceIdentifier));
    }
})
.WithName("PostChat")
.WithOpenApi();

app.Run();
