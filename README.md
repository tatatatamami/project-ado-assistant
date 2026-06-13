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
- `PROJECT_ADO_ASSISTANT_FOUNDRY_API_KEY`

### Foundry Agent configuration

The chat API requires Azure AI Foundry project credentials. Set the following in `appsettings.Development.json` or as environment variables:

| Config Key | Environment Variable | Description |
|---|---|---|
| `ProjectAdoAssistant:Foundry:Endpoint` | `ProjectAdoAssistant__Foundry__Endpoint` | Azure AI Foundry project endpoint URL (recommended project endpoint) |
| `ProjectAdoAssistant:Foundry:AgentId` | `ProjectAdoAssistant__Foundry__AgentId` | Azure AI Foundry agent ID |

Authentication uses `DefaultAzureCredential`. For local development, run `az login` or set the `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, and `AZURE_CLIENT_SECRET` environment variables.
