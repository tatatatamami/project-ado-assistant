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

### Web

```bash
dotnet run --project src/ProjectAdoAssistant.Web/ProjectAdoAssistant.Web.csproj
```

## Environment variables (template)

Set these variables as needed for local development:

- `PROJECT_ADO_ASSISTANT_ENVIRONMENT`
- `PROJECT_ADO_ASSISTANT_LOG_LEVEL`
- `PROJECT_ADO_ASSISTANT_AZURE_DEVOPS_ORG_URL`
- `PROJECT_ADO_ASSISTANT_AZURE_DEVOPS_PAT`
- `PROJECT_ADO_ASSISTANT_FOUNDRY_ENDPOINT`
- `PROJECT_ADO_ASSISTANT_FOUNDRY_API_KEY`
