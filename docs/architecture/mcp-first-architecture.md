# MCP-First Architecture

## Overview

The AI Release Assistant is designed so users can operate the release workflow from **any
MCP-compatible agent host** — GitHub Copilot, Cursor, Claude, or another MCP client —
without depending on a custom web interface.

The web application remains useful for dashboards, configuration, audit history, and document
review, but the primary interaction model is:

```text
Developer or Release Manager
  -> Agent Host (Copilot | Cursor | Claude)
  -> MCP Tools
  -> Release Assistant Backend
  -> Azure DevOps
  -> Release Document
```

Because the workflow is exposed through standard MCP servers, the host is interchangeable.
See [Agent Hosts Overview](../hosts/agent-hosts-overview.md) for per-host setup.

## Architecture Diagram

```text
┌────────────────────────────┐
│ User in any MCP host       │
│ (Copilot | Cursor | Claude)│
└──────────────┬─────────────┘
               │
               ▼
┌────────────────────────────────────────────┐
│ Host-native agent definitions              │
│ .agent.md | .cursor/rules/*.mdc |          │
│ .claude/agents/*.md                        │
└──────────────┬─────────────────────────────┘
               │
        MCP Tool Calls
               │
 ┌─────────────┴────────────────────────────┐
 │                                          │
 ▼                                          ▼
┌────────────────────────────┐   ┌────────────────────────────┐
│ Azure DevOps MCP Server    │   │ Release Governance MCP     │
│ Raw DevOps data            │   │ Company release workflow   │
└──────────────┬─────────────┘   └──────────────┬─────────────┘
               │                                │
               ▼                                ▼
┌────────────────────────────┐   ┌────────────────────────────┐
│ Azure DevOps REST APIs     │   │ Release Assistant API      │
└────────────────────────────┘   └──────────────┬─────────────┘
                                                │
                                                ▼
                                  ┌────────────────────────────┐
                                  │ Database + Document Store  │
                                  └────────────────────────────┘
```

## Backend Responsibility

The backend owns:

- Release records
- Application mappings
- Validation rules
- Rollback selection logic
- Release package persistence
- Document versioning
- Audit logs

## Agent Responsibility

Agents (in any host) own:

- Understanding the user's intent
- Calling the correct tools
- Explaining results
- Coordinating next steps
- Producing human-friendly summaries

## MCP Responsibility

MCP servers expose capabilities to agents.

They should not contain all business logic. The custom Release Governance MCP should delegate
to the backend application services.

## Why MCP-First

This approach lets developers and release managers stay inside their normal workflow,
whatever host they already use:

- Their editor or terminal (VS Code, Cursor, Claude Code)
- Their AI chat (Copilot Chat, Cursor Agent, Claude)
- Pull requests
- Markdown files
- Azure DevOps links

Building on the open MCP standard means:

- One backend and one set of MCP tools serve every host.
- Agent role definitions are written once (see [`docs/agents/`](../agents/agents-overview.md))
  and adapted to each host's file format.
- The product is useful before the full UI is ready.
- The team is not locked into a single AI vendor.
