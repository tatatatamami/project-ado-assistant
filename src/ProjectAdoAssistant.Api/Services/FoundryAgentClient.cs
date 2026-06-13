using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Options;
using ProjectAdoAssistant.Api.Configuration;
using ProjectAdoAssistant.Core.Interfaces;

namespace ProjectAdoAssistant.Api.Services;

public sealed class FoundryAgentClient : IFoundryAgentClient
{
    private readonly AgentsClient _agentsClient;
    private readonly string _agentId;
    private readonly int _pollingIntervalMs;
    private readonly int _timeoutMs;
    private readonly ILogger<FoundryAgentClient> _logger;

    public FoundryAgentClient(IOptions<FoundryAgentOptions> options, ILogger<FoundryAgentClient> logger)
    {
        var opts = options.Value;
        _agentId = opts.AgentId;
        _pollingIntervalMs = opts.RunPollingIntervalMs;
        _timeoutMs = opts.RunTimeoutMs;
        _logger = logger;

        var connectionString = $"{opts.Endpoint};{opts.SubscriptionId};{opts.ResourceGroupName};{opts.ProjectName}";
        var projectClient = new AIProjectClient(connectionString, new DefaultAzureCredential());
        _agentsClient = projectClient.GetAgentsClient();
    }

    public async Task<(string Content, string ThreadId)> SendMessageAsync(
        string userMessage,
        string? threadId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(threadId))
        {
            var thread = (await _agentsClient.CreateThreadAsync(cancellationToken: cancellationToken)).Value;
            threadId = thread.Id;
            _logger.LogInformation("Created new agent thread {ThreadId}", Sanitize(threadId));
        }

        await _agentsClient.CreateMessageAsync(
            threadId,
            MessageRole.User,
            userMessage,
            cancellationToken: cancellationToken);

        var run = (await _agentsClient.CreateRunAsync(
            threadId,
            _agentId,
            cancellationToken: cancellationToken)).Value;

        _logger.LogInformation(
            "Started agent run {RunId} on thread {ThreadId}",
            Sanitize(run.Id),
            Sanitize(threadId));

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_timeoutMs);

        while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress)
        {
            await Task.Delay(_pollingIntervalMs, timeoutCts.Token);
            run = (await _agentsClient.GetRunAsync(threadId, run.Id, timeoutCts.Token)).Value;
            _logger.LogDebug("Agent run {RunId} status: {Status}", Sanitize(run.Id), run.Status);
        }

        if (run.Status != RunStatus.Completed)
        {
            var errorMessage = run.LastError?.Message ?? "Unknown error";
            _logger.LogWarning(
                "Agent run {RunId} ended with status {Status}: {Error}",
                Sanitize(run.Id), run.Status, errorMessage);
            throw new InvalidOperationException(
                $"Agent run failed with status '{run.Status}': {errorMessage}");
        }

        var messages = (await _agentsClient.GetMessagesAsync(
            threadId,
            runId: run.Id,
            order: ListSortOrder.Descending,
            cancellationToken: cancellationToken)).Value;

        var assistantContent = messages.Data
            .FirstOrDefault(m => m.Role == MessageRole.Agent)
            ?.ContentItems
            .OfType<MessageTextContent>()
            .FirstOrDefault()
            ?.Text ?? string.Empty;

        return (assistantContent, threadId);
    }

    private static string Sanitize(string? value) =>
        value?.Replace('\n', '_').Replace('\r', '_') ?? string.Empty;
}
