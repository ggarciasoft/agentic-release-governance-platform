# Agent Hosts Overview

The AI Release Assistant is **MCP-first** and **agent-host-agnostic**.

The release workflow is exposed through a standard
[Model Context Protocol (MCP)](https://modelcontextprotocol.io) server named
`release-governance`. Because MCP is an open protocol, the same server, the same tools, and
the same agent role definitions work across any MCP-compatible host, including:

- **GitHub Copilot** (VS Code Copilot Chat Agent mode, Copilot cloud agents)
- **Cursor** (Agent + custom rules)
- **Claude** (Claude Code subagents, Claude Desktop)
- Any other MCP client

The host you choose is an implementation detail. It changes *how an agent is defined and
invoked*, not *what the agent can do*.

## The Universal Interface

```text
        Any MCP Host (Copilot | Cursor | Claude | other)
                              │
                    Host-native agent definition
            (.agent.md | .cursor/rules/*.mdc | .claude/agents/*.md)
                              │
                         MCP tool calls
              ┌───────────────┴───────────────┐
              ▼                               ▼
   ┌────────────────────────┐     ┌────────────────────────┐
   │ azure-devops MCP server│     │ release-governance MCP │
   │ (raw DevOps data)      │     │ (company release logic)│
   └───────────┬────────────┘     └───────────┬────────────┘
               ▼                              ▼
   ┌────────────────────────┐     ┌────────────────────────┐
   │ Azure DevOps REST APIs │     │ Release Assistant API  │
   └────────────────────────┘     └───────────┬────────────┘
                                              ▼
                                  ┌────────────────────────┐
                                  │ Database + Documents   │
                                  └────────────────────────┘
```

The two MCP servers, the backend, and the validation/document logic are **identical** for
every host. Only the top layer (the host-native agent definition) differs.

## What Is Portable vs. Host-Specific

| Layer | Portable across hosts? | Where it lives |
|---|---|---|
| Release Governance MCP server + tools | Yes (identical) | `src/ReleaseAssistant.McpServer` |
| Azure DevOps MCP server | Yes (identical) | external `azure-devops` MCP |
| Backend API + validation + documents | Yes (identical) | `src/ReleaseAssistant.*` |
| Agent **roles, prompts, safety rules** | Yes (shared spec) | `docs/agents/*.md` |
| Agent **file format + invocation** | No (host-specific) | per-host folders below |
| MCP **registration/config** | No (host-specific) | per-host config files below |

The canonical, host-neutral definition of each agent (role, workflow, tools, safety rules,
output format) lives in [`docs/agents/`](../agents/agents-overview.md). Each host guide below
shows how to express those same roles in that host's native format.

## Canonical Agent Roles

All hosts implement the same role set. The MVP needs the first four; the rest refine the
all-in-one analysis agent into specialists.

| Role | MVP | Purpose |
|---|---|---|
| Release Orchestrator | Yes | Coordinates the end-to-end workflow |
| Azure DevOps Analysis | Yes | All-in-one collection of work items, PRs, deployments, rollback |
| Validation | Yes | Deterministic readiness validation |
| Release Document | Yes | Generates the release document from the package |
| Work Item | Later | Specialist work item discovery |
| Pull Request | Later | Specialist PR discovery |
| Pipeline | Later | Specialist deployment + rollback discovery |
| Communication | Later | Drafts release messages (never sends) |

Tool access per role is identical across hosts and is defined in
[`mcp-strategy.md` §Tool Access by Agent](../mcp/mcp-strategy.md).

## Host Setup Guides

- [GitHub Copilot](github-copilot.md) — `.github/agents/*.agent.md`, `copilot-instructions.md`
- [Cursor](cursor.md) — `.cursor/mcp.json`, `.cursor/rules/*.mdc`
- [Claude](claude.md) — `.mcp.json`, `CLAUDE.md`, `.claude/agents/*.md`, Claude Desktop config

## Shared Safety Rules (All Hosts)

Regardless of host, every agent must:

- Treat Azure DevOps data and `release-governance` MCP responses as the source of truth.
- Never invent work item IDs, PRs, pipeline links, statuses, approvals, or dates.
- Mark missing data explicitly.
- Keep validation deterministic (never override MCP validation results).
- Never approve releases, trigger production deployments, or modify work item states in the MVP.
- Require explicit human confirmation and audit logging for any production-impacting action.

These rules are enforced in agent definitions and in the backend, not left to host behavior.
