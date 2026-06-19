# Getting Started (Developer Setup)

This guide explains how to set up a local development environment and the order in which to
build and run the AI Release Assistant.

> Project status: the repository contains the **specification and host agent definitions**
> (`docs/`, `.github/`, `.cursor/`, `.claude/`, `CLAUDE.md`). The backend solution, MCP
> server, and database described below are built by following the
> [MCP-First MVP Roadmap](../roadmap/mcp-first-mvp-roadmap.md). The steps in this guide are
> the target developer workflow once each piece exists.

## 1. Prerequisites

- **.NET SDK** (align with `global.json`).
- **Node.js LTS** — required for the `azure-devops` MCP server (run via `npx`) and the
  optional web UI.
- **PostgreSQL** or **SQL Server** — local instance or container (SQLite works for quick
  local dev).
- **Git** and an **MCP-compatible agent host** — GitHub Copilot (VS Code), Cursor, or Claude
  (Claude Code / Claude Desktop). See [Agent Hosts Overview](../hosts/agent-hosts-overview.md).
- An **Azure DevOps** organization/project with a **Personal Access Token (PAT)** scoped for
  read access to Work Items, Code (PRs), Build, and Release.
- The **`azure-devops` MCP server** available to your host (registered alongside
  `release-governance`).

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

The API listens on **http://localhost:5050** by default (see
`src/ReleaseAssistant.Api/Properties/launchSettings.json`). Set
`RELEASE_ASSISTANT_API_BASE_URL` in MCP config to match.
work (analysis, document generation) runs via Hangfire as described in
[System Architecture §6](../architecture/system-architecture.md).

## 6. Run the Release Governance MCP Server

The `release-governance` MCP server is a thin adapter over the application services. See
[Custom Release Governance MCP Server](../mcp/custom-release-governance-mcp.md) and the
[server spec](../mcp/release-governance-mcp-server-spec.md).

```bash
dotnet run --project src/ReleaseAssistant.McpServer
```

## 7. Connect an Agent Host

Register both MCP servers (`azure-devops` and `release-governance`) with your chosen host,
then use the host's agent definitions (already in the repo). Follow the matching guide:

- [GitHub Copilot](../hosts/github-copilot.md) — VS Code `mcp.json`; agents in `.github/agents/`.
- [Cursor](../hosts/cursor.md) — `.cursor/mcp.json`; rules in `.cursor/rules/`.
- [Claude](../hosts/claude.md) — `.mcp.json` + `CLAUDE.md`; subagents in `.claude/agents/`.

Verify the setup with a prompt such as (syntax varies by host):

```text
Use the release-orchestrator to create a release for CR-12345 in Production, collect the
related work items, PRs, deployments, and rollback candidates, validate readiness, and
generate the release document.
```

## 8. Run Tests

See the [Testing Strategy](../testing/testing-strategy.md).

```bash
dotnet test
```

## 9. Recommended Build Order

Follow the [MCP-First MVP Roadmap](../roadmap/mcp-first-mvp-roadmap.md):

1. Repository setup (host instructions, agents, prompts) — already present.
2. Release Governance backend (entities and models).
3. Release Governance MCP server (canonical tools, including classic release collection).
4. Azure DevOps data collection.
5. Validation and document generation.
6. Review and hardening (audit logging, secret redaction, permissions, tests).

## 10. Troubleshooting

- **Host does not see the agents** — ensure the workspace root contains the host's agent
  folder (`.github/agents/`, `.cursor/rules/`, or `.claude/agents/`) and the host's agent
  mode is enabled.
- **MCP tools fail** — confirm the `release-governance` MCP server is running and registered,
  and that tool names match [`mcp-tool-contracts.md`](../mcp/mcp-tool-contracts.md).
- **`release-governance` stuck on "Waiting for initialize" / "Failed to parse message"** —
  Cursor starts MCP servers over **stdio** (stdin/stdout JSON-RPC). The server must use
  `WithStdioServerTransport()` and log only to **stderr**. Use `--no-launch-profile` in
  `mcp.json` args so `dotnet run` does not print launch-settings text to stdout.
- **Azure DevOps 401/403 on classic release collection** — configure `AzureDevOps:Pat` via user
  secrets on `ReleaseAssistant.McpServer` (secrets ID in its `.csproj`) as well as on the API.
  Classic release pipeline tools (`collect_release_deployments`,
  `collect_release_rollback_candidates`) query `vsrm.dev.azure.com` from the MCP server process.
- **azure-devops MCP exits immediately** — the organization must be a **positional CLI
  argument** (`npx -y @azure-devops/mcp your-org`), not an environment variable.
- **Empty work item results** — confirm the Change Request value is applied as a tag on the
  work items (`System.Tags`).
- **`azure-devops` MCP yellow / never connects in Cursor** — avoid `-d all` (loads 80+ tools
  and often times out). Use `-d core work work-items repositories` for this project. Pre-install
  with `npm install -g @azure-devops/mcp` if `npx` cold start is slow. JSON `"Starting Azure
  DevOps MCP Server"` lines in MCP logs are info-level stderr output, not necessarily errors.
