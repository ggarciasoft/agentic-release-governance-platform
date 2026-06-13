# Copilot Agent Strategy

## Goal

The system must expose a set of specialized AI agents that can be called from GitHub Copilot, especially from VS Code Copilot Chat Agent mode and, where supported, Copilot cloud agents.

The agents are responsible for coordinating the release workflow through MCP tools. They should not depend on a custom web UI to be useful. A release manager or developer should be able to open Copilot and call an agent such as:

```text
Use the Release Orchestrator agent to prepare the release for CR-12345 in the Production environment.
```

## Key Direction

The product should be designed as a Copilot-first agentic release system.

That means:

- Agents are defined as `.agent.md` files.
- Repository instructions are provided through `.github/copilot-instructions.md`.
- MCP servers expose the tools that agents can use.
- Existing MCP servers are used when they are good enough.
- A custom Release Governance MCP server is created for company-specific release rules and release package generation.
- Every production-impacting action remains human-reviewed.

## Copilot Integration Targets

### Primary Target: VS Code Copilot Custom Agents

Use workspace-level custom agents under:

```text
.github/agents/
```

Each agent is represented as:

```text
.github/agents/<agent-name>.agent.md
```

These agents appear in the Copilot Chat agent picker when the workspace is opened.

### Secondary Target: Copilot Cloud Agent

Where supported, the same agents can be adapted for GitHub Copilot cloud agent by including supported `target` and `mcp-servers` configuration in the `.agent.md` frontmatter.

Cloud agents should only be given read-only or low-risk tools unless the organization has approved stronger permissions.

## Agent Roles

The first implementation should include these Copilot-callable agents:

1. Release Orchestrator Agent
2. Work Item Agent
3. Pull Request Agent
4. Pipeline Agent
5. Validation Agent
6. Document Agent
7. Communication Agent

For the MVP, only these are required:

1. Release Orchestrator Agent
2. Azure DevOps Analysis Agent
3. Validation Agent
4. Document Agent

## MCP Server Strategy

Agents should use three categories of tools:

### 1. Existing Azure DevOps MCP Server

Used for raw Azure DevOps data retrieval when available:

- Work items
- Pull requests
- Repositories
- Builds
- Pipeline information
- Test plans
- Wikis

### 2. Custom Release Governance MCP Server

Used for release-specific workflow:

- Create release item
- Store release configuration
- Get application mapping
- Analyze release scope
- Validate release readiness
- Find rollback candidates
- Generate structured release package
- Save release document

### 3. Optional Document/Storage MCP Server

Used later for exporting documents to:

- Azure DevOps Wiki
- SharePoint
- Confluence
- GitHub repository
- Local workspace files

## Principle

Agents should be thin reasoning layers. MCP tools should expose reliable system capabilities. Business rules should live in the backend and custom MCP server, not inside prompts only.

## Example Copilot Usage

```text
@release-orchestrator Prepare the release document for CR-12345 in Production.
```

```text
@work-item-agent Get all user stories, bugs, and features tagged CR-12345 and validate their states.
```

```text
@pipeline-agent Find the queued production release and rollback release for Payments API.
```

```text
@document-agent Generate the official release document using release package rel_20260613_001.
```

## Human Review Rule

Agents may create release records and draft documents, but they must not approve, trigger, or complete production deployment unless the application explicitly supports it and the user has confirmed the action.
