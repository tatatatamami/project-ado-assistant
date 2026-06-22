# project-ado-assistant

## Prerequisites

- .NET SDK 8.x

## Solution structure

- `ProjectAdoAssistant.sln`
- `src/ProjectAdoAssistant.Api` (ASP.NET Core Web API)
- `src/ProjectAdoAssistant.Web` (Blazor Server via `blazor` template with `Server` interactivity)
- `src/ProjectAdoAssistant.Core` (shared models / interfaces / DTOs)

## Build

```bash
dotnet restore ProjectAdoAssistant.sln
dotnet build ProjectAdoAssistant.sln
```

## Run locally

### API

```bash
dotnet run --project src/ProjectAdoAssistant.Api/ProjectAdoAssistant.Api.csproj
```

After startup, `GET /api/health` returns the API health response.

### Web

```bash
dotnet run --project src/ProjectAdoAssistant.Web/ProjectAdoAssistant.Web.csproj
```

## Environment variables (template)

Set these variables as needed for local development:

- `ProjectAdoAssistant__Api__ServiceName`
- `PROJECT_ADO_ASSISTANT_ENVIRONMENT`
- `PROJECT_ADO_ASSISTANT_LOG_LEVEL`
- `PROJECT_ADO_ASSISTANT_AZURE_DEVOPS_ORG_URL`
- `PROJECT_ADO_ASSISTANT_AZURE_DEVOPS_PAT`
- `PROJECT_ADO_ASSISTANT_FOUNDRY_ENDPOINT`

### Foundry Agent configuration

The chat API uses Azure AI Foundry Agent Service (Agent / Conversation / Response model) with Azure AI Projects 2.x SDKs.

- Agent: definition identified by `AgentName` + `AgentVersion`
- Conversation: chat-session context (`ConversationId`) reused across user messages
- Response: single execution result (`ResponseId`) per input

Set the following in `appsettings.Development.json` or as environment variables:

| Config Key | Environment Variable | Description |
|---|---|---|
| `ProjectAdoAssistant:Foundry:ProjectEndpoint` | `ProjectAdoAssistant__Foundry__ProjectEndpoint` | Azure AI Foundry project endpoint URL |
| `ProjectAdoAssistant:Foundry:AgentName` | `ProjectAdoAssistant__Foundry__AgentName` | Foundry agent name |
| `ProjectAdoAssistant:Foundry:AgentVersion` | `ProjectAdoAssistant__Foundry__AgentVersion` | Foundry agent version (set explicitly; do not rely on implicit latest) |
| `ProjectAdoAssistant:Foundry:RequestTimeoutMs` | `ProjectAdoAssistant__Foundry__RequestTimeoutMs` | Timeout in milliseconds for a single response request |

### Foundry SDK packages

The API project currently uses:

- `Azure.AI.Projects` `2.1.0-beta.3`
- `Azure.AI.Extensions.OpenAI` `2.1.0-beta.3`
- `Azure.Identity` `1.14.2`

`Azure.AI.Projects` and `Azure.AI.Extensions.OpenAI` are preview packages; check release notes before upgrading.
Always verify the latest package versions in `src/ProjectAdoAssistant.Api/ProjectAdoAssistant.Api.csproj`.

Authentication uses `DefaultAzureCredential`. For local development, run `az login` or set the `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, and `AZURE_CLIENT_SECRET` environment variables.
