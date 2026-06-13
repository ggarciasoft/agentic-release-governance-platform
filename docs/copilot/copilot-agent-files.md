# Copilot Custom Agent Files

## Purpose

This project includes workspace-level Copilot custom agents in:

```text
.github/agents/
```

Each file defines a role, tool access, and behavior for a specialized agent.

## File Naming

Use lowercase kebab-case:

```text
release-orchestrator.agent.md
work-item-agent.agent.md
pull-request-agent.agent.md
pipeline-agent.agent.md
validation-agent.agent.md
release-document-agent.agent.md
communication-agent.agent.md
```

## Agent File Structure

Each agent file should contain:

```yaml
---
name: release-orchestrator
description: Coordinates the release workflow from CR/tag to release document.
tools: ['release-governance/*', 'azure-devops/*']
---
```

Then the Markdown body defines:

- Role
- Goals
- Available tools
- Workflow
- Safety rules
- Output format

## Tool Naming Convention

Use MCP server namespace prefixes:

```text
azure-devops/search_work_items
azure-devops/get_pull_requests
release-governance/create_release_item
release-governance/validate_release
release-governance/generate_release_package
```

The canonical `release-governance` tool names are defined in
[`../mcp/release-governance-mcp-server-spec.md`](../mcp/release-governance-mcp-server-spec.md)
and [`../mcp/mcp-tool-contracts.md`](../mcp/mcp-tool-contracts.md). Keep agent files aligned
with those names.

## Handoff Strategy

Agents should hand off work in this order:

```text
Release Orchestrator
  -> Work Item Agent
  -> Pull Request Agent
  -> Pipeline Agent (also discovers rollback candidates)
  -> Validation Agent
  -> Release Document Agent
  -> Communication Agent
```

The orchestrator may also call subagents directly if the Copilot host supports subagent invocation.

In the current `.github/agents/` set, rollback discovery is owned by the **Pipeline Agent**
(it calls `release-governance/find_rollback_candidates`), so there is no separate rollback
agent file. The standalone [Rollback Agent](../agents/rollback-agent.md) spec describes the
same rules for the future split-agent topology.

## Tool Access Strategy

Give each agent only the tools it needs.

- Work Item Agent: work item read tools only.
- Pull Request Agent: PR/repository read tools only.
- Pipeline Agent: pipeline/release read tools only.
- Validation Agent: release-governance validation tools.
- Release Document Agent: release package and document tools.
- Communication Agent: draft-only tools.

Avoid giving every agent every tool.
