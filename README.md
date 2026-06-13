# AI Release Assistant for Azure DevOps

The AI Release Assistant is a Copilot-callable, MCP-powered agentic release system for Azure DevOps teams.

It helps release managers and developers create a release item, collect user stories and bugs, find linked pull requests, inspect release pipelines, identify rollback candidates, validate release readiness, and generate official release documentation.

## Updated Direction

The system is now designed as a **Copilot-first multi-agent project**.

Users should be able to call specialized AI agents from GitHub Copilot, especially in VS Code Copilot Chat Agent mode.

Example:

```text
@release-orchestrator Create a release for CR-12345, get the related user stories, PRs, production release pipelines, validate readiness, and create the release document.
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
GitHub Copilot
  -> Custom Agents (.github/agents/*.agent.md)
  -> MCP Tools
      -> Existing Azure DevOps MCP Server
      -> Custom Release Governance MCP Server
  -> Release Assistant Backend
  -> Azure DevOps + Database + Document Store
```

## Recommended Stack

```text
Backend: .NET 8 Web API
MCP Server: .NET MCP server wrapping application services
Frontend: React or Next.js, optional for MVP
Database: PostgreSQL or SQL Server
Background Jobs: Hangfire or Azure Functions
AI: GitHub Copilot custom agents + optional Azure OpenAI/OpenAI for document generation
DevOps Integration: Azure DevOps REST API and/or Azure DevOps MCP Server
```

## Documentation Map

### Product

- `docs/product/product-requirements.md`
- `docs/product/mvp-scope.md`
- `docs/product/user-flows.md`
- `docs/product/updated-requirements-copilot-agents.md`

### Architecture

- `docs/architecture/system-architecture.md`
- `docs/architecture/agentic-architecture.md`
- `docs/architecture/copilot-first-architecture.md`
- `docs/architecture/azure-devops-integration.md`

### Copilot

- `docs/copilot/copilot-agent-strategy.md`
- `docs/copilot/copilot-agent-files.md`
- `docs/copilot/mcp-configuration-for-copilot.md`
- `.github/copilot-instructions.md`
- `.github/agents/*.agent.md`
- `.github/prompts/*.prompt.md`
- `.github/instructions/release-assistant.instructions.md`

### Agents

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
- `docs/api/copilot-agent-api-flow.md`
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

- `docs/roadmap/copilot-first-mvp-roadmap.md` (canonical MVP plan)
- `docs/roadmap/mvp-roadmap.md` (legacy, UI-first — superseded)
- `docs/roadmap/future-roadmap.md`

## MVP Agents

The first implementation should include:

```text
release-orchestrator.agent.md
azure-devops-analysis-agent.agent.md
validation-agent.agent.md
release-document-agent.agent.md
```

Then split the analysis agent into:

```text
work-item-agent.agent.md
pull-request-agent.agent.md
pipeline-agent.agent.md
communication-agent.agent.md
```

## Required Custom MCP

Create a custom MCP server named:

```text
release-governance
```

Required tools:

```text
create_release_item
get_release_item
get_application_mapping
attach_work_items_to_release
attach_pull_requests_to_release
attach_deployments_to_release
find_rollback_candidates
validate_release
generate_release_package
save_release_document
```

## Safety Rules

- Agents must not invent Azure DevOps data.
- Agents must not approve production releases.
- Agents must not trigger production deployments in the MVP.
- Agents must not modify work item states in the MVP.
- Missing data must be explicit.
- Validation must be deterministic.
- All production-impacting actions require human confirmation and audit logging.

## First Build Recommendation

Start with the Copilot-first MVP:

1. Add Copilot instructions and agent files.
2. Build the Release Governance backend.
3. Build the Release Governance MCP server.
4. Integrate Azure DevOps work item search.
5. Integrate PR discovery.
6. Integrate pipeline and rollback discovery.
7. Add validation rules.
8. Add Markdown release document generation.
