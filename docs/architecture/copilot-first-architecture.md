# Copilot-First Architecture

## Overview

The AI Release Assistant is designed so users can operate the release workflow from GitHub Copilot without depending on a custom web interface.

The web application remains useful for dashboards, configuration, audit history, and document review, but the primary interaction model is:

```text
Developer or Release Manager
  -> GitHub Copilot Custom Agent
  -> MCP Tools
  -> Release Assistant Backend
  -> Azure DevOps
  -> Release Document
```

## Architecture Diagram

```text
┌────────────────────────────┐
│ User in VS Code / Copilot  │
└──────────────┬─────────────┘
               │
               ▼
┌────────────────────────────┐
│ Copilot Custom Agents      │
│ .github/agents/*.agent.md  │
└──────────────┬─────────────┘
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

Agents own:

- Understanding the user's intent
- Calling the correct tools
- Explaining results
- Coordinating next steps
- Producing human-friendly summaries

## MCP Responsibility

MCP servers expose capabilities to agents.

They should not contain all business logic. The custom Release Governance MCP should delegate to the backend application services.

## Why Copilot-First

This approach allows developers and release managers to stay inside their normal workflow:

- VS Code
- GitHub Copilot Chat
- Pull requests
- Markdown files
- Azure DevOps links

It also makes the product useful before the full UI is ready.
