extern alias AzureIdentity;

using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
using ProjectAdoAssistant.Api.Configuration;
using ProjectAdoAssistant.Core.Dtos;
using ProjectAdoAssistant.Core.Interfaces;

#pragma warning disable OPENAI001

namespace ProjectAdoAssistant.Api.Services;

public sealed class FoundryAgentClient : IFoundryAgentClient
{
    private readonly ProjectOpenAIClient _projectOpenAIClient;
    private readonly ProjectConversationsClient _projectConversationsClient;
    private readonly AgentReference _agentReference;
    private readonly int _requestTimeoutMs;
    private readonly ILogger<FoundryAgentClient> _logger;

    public FoundryAgentClient(IOptions<FoundryAgentOptions> options, ILogger<FoundryAgentClient> logger)
    {
        var opts = options.Value;
        _requestTimeoutMs = opts.RequestTimeoutMs;
        _logger = logger;

        var projectClient = new AIProjectClient(new Uri(opts.ProjectEndpoint), new AzureIdentity::Azure.Identity.DefaultAzureCredential());
        _projectOpenAIClient = projectClient.GetProjectOpenAIClient();
        _projectConversationsClient = _projectOpenAIClient.GetProjectConversationsClient();
        _agentReference = new AgentReference(name: opts.AgentName, version: opts.AgentVersion);
    }

    public async Task<ChatResponseDto> SendMessageAsync(
        string userMessage,
        string? conversationId,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_requestTimeoutMs);

        var resolvedConversationId = string.IsNullOrWhiteSpace(conversationId)
            ? await CreateConversationAsync(timeoutCts.Token)
            : conversationId;
        var responseClient = _projectOpenAIClient.GetProjectResponsesClientForAgent(_agentReference, resolvedConversationId);

        var options = new CreateResponseOptions
        {
            InputItems = { ResponseItem.CreateUserMessageItem(userMessage) },
        };

        _logger.LogInformation(
            "Sending response request for agent {AgentName} conversation {ConversationId}",
            Sanitize(_agentReference.Name),
            Sanitize(resolvedConversationId));

        var response = await responseClient.CreateResponseAsync(options, timeoutCts.Token);
        var result = response.Value;

        if (result.Status != OpenAI.Responses.ResponseStatus.Completed)
        {
            var errorMessage = result.Error?.Message ?? "Unknown error";
            _logger.LogWarning("Responses API request ended with status {Status}: {Error}", result.Status, errorMessage);
            throw new InvalidOperationException($"Agent response failed with status '{result.Status}': {errorMessage}");
        }

        var assistantContent = result.GetOutputText();
        if (string.IsNullOrWhiteSpace(assistantContent))
        {
            throw new InvalidOperationException("Agent response did not include output text.");
        }

        if (string.IsNullOrWhiteSpace(result.Id))
        {
            throw new InvalidOperationException("Agent response did not include response ID.");
        }

        return new ChatResponseDto(assistantContent, resolvedConversationId, result.Id);
    }

    private async Task<string> CreateConversationAsync(CancellationToken cancellationToken)
    {
        var conversation = await _projectConversationsClient.CreateProjectConversationAsync(cancellationToken: cancellationToken);
        var conversationId = conversation.Value.Id;
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            throw new InvalidOperationException("Conversation ID was not returned by Foundry.");
        }

        return conversationId;
    }

    private static string Sanitize(string? value) =>
        value?.Replace('\n', '_').Replace('\r', '_') ?? string.Empty;
}
