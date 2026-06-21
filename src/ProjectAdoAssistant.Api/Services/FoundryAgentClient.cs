extern alias AzureIdentity;

using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
using ProjectAdoAssistant.Api.Configuration;
using ProjectAdoAssistant.Core.Interfaces;

#pragma warning disable OPENAI001

namespace ProjectAdoAssistant.Api.Services;

public sealed class FoundryAgentClient : IFoundryAgentClient
{
    private readonly ProjectOpenAIClient _projectOpenAIClient;
    private readonly AgentReference _agentReference;
    private readonly int _pollingIntervalMs;
    private readonly int _timeoutMs;
    private readonly ILogger<FoundryAgentClient> _logger;

    public FoundryAgentClient(IOptions<FoundryAgentOptions> options, ILogger<FoundryAgentClient> logger)
    {
        var opts = options.Value;
        _pollingIntervalMs = opts.RunPollingIntervalMs;
        _timeoutMs = opts.RunTimeoutMs;
        _logger = logger;

        var projectClient = new AIProjectClient(new Uri(opts.Endpoint), new AzureIdentity::Azure.Identity.DefaultAzureCredential());
        _projectOpenAIClient = projectClient.GetProjectOpenAIClient();
        _agentReference = new AgentReference(opts.AgentId, null);
    }

    public async Task<(string Content, string ThreadId)> SendMessageAsync(
        string userMessage,
        string? threadId,
        CancellationToken cancellationToken = default)
    {
        var conversationId = string.IsNullOrWhiteSpace(threadId) ? null : threadId;
        var responseClient = _projectOpenAIClient.GetProjectResponsesClientForAgent(_agentReference, conversationId);

        var options = new CreateResponseOptions
        {
            InputItems = { ResponseItem.CreateUserMessageItem(userMessage) },
        };

        _logger.LogInformation(
            "Sending response request for agent {AgentId} with previous response {PreviousResponseId}",
            Sanitize(_agentReference.Name),
            Sanitize(threadId));

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_timeoutMs);

        var response = await responseClient.CreateResponseAsync(options, timeoutCts.Token);
        var result = response.Value;

        if (result.Status != OpenAI.Responses.ResponseStatus.Completed)
        {
            var errorMessage = result.Error?.Message ?? "Unknown error";
            _logger.LogWarning("Responses API request ended with status {Status}: {Error}", result.Status, errorMessage);
            throw new InvalidOperationException($"Agent response failed with status '{result.Status}': {errorMessage}");
        }

        var assistantContent = result.GetOutputText();
        return (assistantContent, result.Id);
    }

    private static string Sanitize(string? value) =>
        value?.Replace('\n', '_').Replace('\r', '_') ?? string.Empty;
}
