# Getting Started (Developer Setup)

This guide explains how to set up a local development environment and the order in which to
build and run the AI Release Assistant.

> Project status: the repository currently contains the **specification and Copilot agent
> definitions** (`docs/` and `.github/`). The backend solution, MCP server, and database
> described below are built by following the
> [Copilot-First MVP Roadmap](../roadmap/copilot-first-mvp-roadmap.md). The steps in this
> guide are the target developer workflow once each piece exists.

## 1. Prerequisites

- **.NET 8 SDK** (the documented backend target; align with `global.json` once added).
- **Node.js LTS** — only if/when the optional web UI is built. Not required for the
  Copilot-first MVP.
- **PostgreSQL** or **SQL Server** — local instance or container.
- **Git** and a **GitHub Copilot**-enabled editor (for example VS Code with Copilot Chat
  Agent mode).
- An **Azure DevOps** organization/project with a **Personal Access Token (PAT)** scoped for
  read access to Work Items, Code (PRs), Build, and Release.
- The **`azure-devops` MCP server** available to Copilot (see
  [MCP Configuration for Copilot](../copilot/mcp-configuration-for-copilot.md)).

## 2. Clone and Explore

```bash
git clone <repo-url>
cd agentic-release-governance-platform
```

Start with the [README](../../README.md) documentation map, then
[System Architecture](../architecture/system-architecture.md) and the
[Agents Overview](../agents/agents-overview.md).

## 3. Solution Layout

The backend follows the structure in
[System Architecture §3](../architecture/system-architecture.md):

```text
src/
  ReleaseAssistant.Api/
  ReleaseAssistant.Application/
  ReleaseAssistant.Domain/
  ReleaseAssistant.Infrastructure/
  ReleaseAssistant.AzureDevOps/
  ReleaseAssistant.Agents/
  ReleaseAssistant.Documents/
  ReleaseAssistant.McpServer/
  ReleaseAssistant.Tests/
```

## 4. Configuration

Configuration lives in `appsettings.json` / `appsettings.Development.json` and environment
variables. **Never commit secrets.** Locally, use `dotnet user-secrets`; in deployed
environments, use Azure Key Vault.

| Setting | Purpose | Example |
|---|---|---|
| `ConnectionStrings:Default` | Database connection | `Host=localhost;Database=releaseassistant;...` |
| `AzureDevOps:Organization` | Default ADO organization | `my-org` |
| `AzureDevOps:Project` | Default ADO project | `my-project` |
| `AzureDevOps:Pat` | PAT (user-secret/Key Vault only) | `***` |
| `AzureDevOps:BaseUrl` | ADO base URL | `https://dev.azure.com` |

Set the PAT as a user secret rather than in a file:

```bash
cd src/ReleaseAssistant.Api
dotnet user-secrets set "AzureDevOps:Pat" "<your-pat>"
```

## 5. Build and Run the Backend

```bash
dotnet restore
dotnet build
```

Apply database migrations (EF Core), then run the API:

```bash
dotnet ef database update --project src/ReleaseAssistant.Infrastructure --startup-project src/ReleaseAssistant.Api
dotnet run --project src/ReleaseAssistant.Api
```

The API exposes the routes in the [API Specification](../api/api-specification.md). Background
work (analysis, document generation) runs via Hangfire as described in
[System Architecture §6](../architecture/system-architecture.md).

## 6. Run the Release Governance MCP Server

The `release-governance` MCP server is a thin adapter over the application services. See
[Custom Release Governance MCP Server](../mcp/custom-release-governance-mcp.md) and the
[server spec](../mcp/release-governance-mcp-server-spec.md).

```bash
dotnet run --project src/ReleaseAssistant.McpServer
```

## 7. Wire Up Copilot

Register both MCP servers (`azure-devops` and `release-governance`) with your Copilot host
following [MCP Configuration for Copilot](../copilot/mcp-configuration-for-copilot.md). The
custom agents in `.github/agents/` are detected automatically by Copilot in the workspace.

Verify the setup with a prompt such as:

```text
@release-orchestrator Create a release for CR-12345 in Production, collect the related work
items, PRs, deployments, and rollback candidates, validate readiness, and generate the
release document.
```

## 8. Run Tests

See the [Testing Strategy](../testing/testing-strategy.md).

```bash
dotnet test
```

## 9. Recommended Build Order

Follow the [Copilot-First MVP Roadmap](../roadmap/copilot-first-mvp-roadmap.md):

1. Repository setup (Copilot instructions, agents, prompts) — already present.
2. Release Governance backend (entities and models).
3. Release Governance MCP server (10 canonical tools).
4. Azure DevOps data collection.
5. Validation and document generation.
6. Review and hardening (audit logging, secret redaction, permissions, tests).

## 10. Troubleshooting

- **Copilot does not see the agents** — ensure the workspace root contains `.github/agents/`
  and the editor has Copilot Agent mode enabled.
- **MCP tools fail** — confirm the `release-governance` MCP server is running and registered,
  and that tool names match [`mcp-tool-contracts.md`](../mcp/mcp-tool-contracts.md).
- **Azure DevOps 401/403** — verify the PAT scopes and that it is configured as a secret.
- **Empty work item results** — confirm the Change Request value is applied as a tag on the
  work items (`System.Tags`).
