# AI Release Assistant for Azure DevOps

The AI Release Assistant is an **MCP-first, agent-host-agnostic** agentic release system for
Azure DevOps teams.

It helps release managers and developers create a release item, collect user stories and bugs,
find linked pull requests, inspect release pipelines, identify rollback candidates, validate
release readiness, and generate official release documentation.

## Direction

The system is designed as an **MCP-first multi-agent project**. The release workflow is exposed
through standard [Model Context Protocol](https://modelcontextprotocol.io) servers, so the same
agents run on any MCP-compatible host:

- **GitHub Copilot** (VS Code Copilot Chat Agent mode)
- **Cursor** (Agent + rules)
- **Claude** (Claude Code subagents, Claude Desktop)

You pick the host your team already uses; the backend, MCP tools, and agent roles are identical.
See [Agent Hosts Overview](docs/hosts/agent-hosts-overview.md).

Example (syntax varies by host):

```text
Use the release-orchestrator to create a release for CR-12345, get the related user stories,
PRs, and production release pipelines, validate readiness, and create the release document.
```

## Core Capabilities

- Create release items.
- Search Azure DevOps work items by change request or tag.
- Find linked pull requests.
- Find release/build pipeline information.
- Find current production deployment candidates.
- Find rollback candidates.
- Validate release readiness using deterministic rules.
- Generate release documents.
- Save release documents with versioning.
- Present blockers, warnings, and missing information.

## Main Architecture

```text
Any MCP host (Copilot | Cursor | Claude)
  -> Host-native agent definitions
       (.github/agents/*.agent.md | .cursor/rules/*.mdc | .claude/agents/*.md)
  -> MCP Tools
      -> Existing Azure DevOps MCP Server
      -> Custom Release Governance MCP Server
  -> Release Assistant Backend
  -> Azure DevOps + Database + Document Store
```

See [MCP-First Architecture](docs/architecture/mcp-first-architecture.md).

## Stack

```text
Backend:           .NET 9 Web API (ASP.NET Core)
MCP Server:        .NET 9 MCP server (ModelContextProtocol.AspNetCore)
Database:          SQLite (local dev) · PostgreSQL or SQL Server (production)
ORM:               Entity Framework Core
AI Host:           GitHub Copilot · Cursor · Claude (any MCP-compatible client)
DevOps:            Azure DevOps REST API + azure-devops MCP server
Frontend:          Optional — React or Next.js (not required for the MCP-first MVP)
```

## Documentation Map

### Product

- `docs/product/product-requirements.md`
- `docs/product/mvp-scope.md`
- `docs/product/user-flows.md`
- `docs/product/agent-host-requirements.md`

### Architecture

- `docs/architecture/system-architecture.md`
- `docs/architecture/agentic-architecture.md`
- `docs/architecture/mcp-first-architecture.md`
- `docs/architecture/azure-devops-integration.md`

### Agent Hosts

- `docs/hosts/agent-hosts-overview.md` (start here)
- `docs/hosts/github-copilot.md` — `.github/agents/*.agent.md`, `.github/copilot-instructions.md`
- `docs/hosts/cursor.md` — `.cursor/mcp.json`, `.cursor/rules/*.mdc`
- `docs/hosts/claude.md` — `.mcp.json`, `CLAUDE.md`, `.claude/agents/*.md`

### Agents (host-neutral role specs)

- `docs/agents/agents-overview.md`
- `docs/agents/release-orchestrator-agent.md`
- `docs/agents/azure-devops-analysis-agent.md`
- `docs/agents/work-item-agent.md`
- `docs/agents/pull-request-agent.md`
- `docs/agents/pipeline-agent.md`
- `docs/agents/rollback-agent.md`
- `docs/agents/validation-agent.md`
- `docs/agents/document-agent.md`
- `docs/agents/communication-agent.md`

### MCP

- `docs/mcp/mcp-strategy.md`
- `docs/mcp/custom-release-governance-mcp.md`
- `docs/mcp/mcp-tool-contracts.md`
- `docs/mcp/release-governance-mcp-server-spec.md`

### API and Data

- `docs/api/api-specification.md`
- `docs/api/agent-api-flow.md`
- `docs/database/database-model.md`

### Security and Rules

- `docs/security/security-model.md`
- `docs/security/validation-rules.md`

### Prompts and Instructions

- `docs/prompts/system-instructions.md`
- `docs/prompts/agent-prompts.md`
- `docs/prompts/ai-coding-agent-build-prompt.md`

### Development and Testing

- `docs/development/getting-started.md`
- `docs/testing/testing-strategy.md`

### Reference

- `docs/glossary.md`

### Roadmap

- `docs/roadmap/mcp-first-mvp-roadmap.md` (canonical MVP plan)
- `docs/roadmap/mvp-roadmap.md` (legacy, UI-first — superseded)
- `docs/roadmap/future-roadmap.md`

## Agent Definitions per Host

The same role set is defined for each host. The MVP needs the first four; the rest refine the
all-in-one analysis agent into specialists.

| Role | Copilot | Cursor | Claude |
|---|---|---|---|
| release-orchestrator | `.github/agents/release-orchestrator.agent.md` | `.cursor/rules/release-orchestrator.mdc` | `.claude/agents/release-orchestrator.md` |
| azure-devops-analysis-agent | ✓ | ✓ | ✓ |
| validation-agent | ✓ | ✓ | ✓ |
| release-document-agent | ✓ | ✓ | ✓ |
| work-item-agent | ✓ | ✓ | ✓ |
| pull-request-agent | ✓ | ✓ | ✓ |
| pipeline-agent | ✓ | ✓ | ✓ |
| communication-agent | ✓ | ✓ | ✓ |

## Custom MCP Server

The `release-governance` MCP server lives in `src/ReleaseAssistant.McpServer` and exposes
10 canonical tools:

```text
create_release_item          get_release_item
get_application_mapping      attach_work_items_to_release
attach_pull_requests_to_release  attach_deployments_to_release
find_rollback_candidates     validate_release
generate_release_package     save_release_document
```

See [`docs/mcp/mcp-tool-contracts.md`](docs/mcp/mcp-tool-contracts.md) for full input/output
contracts and [`docs/mcp/release-governance-mcp-server-spec.md`](docs/mcp/release-governance-mcp-server-spec.md)
for design rules.

## Safety Rules

- Agents must not invent Azure DevOps data.
- Agents must not approve production releases.
- Agents must not trigger production deployments in the MVP.
- Agents must not modify work item states in the MVP.
- Missing data must be explicit.
- Validation must be deterministic.
- All production-impacting actions require human confirmation and audit logging.

## Getting Started

The backend, MCP server, and agent definitions for all three hosts are already in the
repository. To run the system locally:

```bash
# 1. Build
dotnet restore && dotnet build

# 2. Run the backend API (http://localhost:5000)
dotnet run --project src/ReleaseAssistant.Api

# 3. In a second terminal, run the MCP server
dotnet run --project src/ReleaseAssistant.McpServer

# 4. Run tests
dotnet test
```

Then connect your agent host following the matching guide:

- **GitHub Copilot** → [`docs/hosts/github-copilot.md`](docs/hosts/github-copilot.md)
- **Cursor** → [`docs/hosts/cursor.md`](docs/hosts/cursor.md)
- **Claude** → [`docs/hosts/claude.md`](docs/hosts/claude.md)

For full configuration steps (secrets, Azure DevOps PAT, EF migrations) see
[`docs/development/getting-started.md`](docs/development/getting-started.md).
Full phase-by-phase build plan: [MCP-First MVP Roadmap](docs/roadmap/mcp-first-mvp-roadmap.md).
