# Host Guide: Cursor

Cursor is one of the supported [agent hosts](agent-hosts-overview.md). It connects to the
same `release-governance` and `azure-devops` MCP servers and implements the same
[agent roles](../agents/agents-overview.md) using Cursor **rules** and **MCP config**.

## Files

| Purpose | Location |
|---|---|
| MCP server config | `.cursor/mcp.json` |
| Always-applied repo rules | `.cursor/rules/release-assistant.mdc` |
| Per-role agent rules | `.cursor/rules/<role>.mdc` |

These files ship in this repository, so opening the project in Cursor wires everything up.

## MCP Registration

`.cursor/mcp.json` (project-scoped) registers both MCP servers:

```json
{
  "mcpServers": {
    "release-governance": {
      "command": "dotnet",
      "args": ["run", "--project", "src/ReleaseAssistant.McpServer/ReleaseAssistant.McpServer.csproj"],
      "env": {
        "RELEASE_ASSISTANT_API_BASE_URL": "http://localhost:5050"
      }
    },
    "azure-devops": {
      "command": "npx",
      "args": [
        "-y",
        "@azure-devops/mcp",
        "your-org",
        "-d",
        "core",
        "work",
        "work-items",
        "repositories",
        "--authentication",
        "envvar"
      ]
    }
  }
}
```

Replace `your-org` with your Azure DevOps organization name (positional argument — **not** an
env var). Set `ADO_MCP_AUTH_TOKEN` to your PAT in your shell or Cursor environment before
starting the server (`--authentication envvar`). Never commit secrets to `.cursor/mcp.json`.

After editing, enable both servers in **Cursor Settings → MCP** and confirm the tools appear.

Set `RELEASE_ASSISTANT_API_BASE_URL` to match the API launch profile port (`5050` in
`src/ReleaseAssistant.Api/Properties/launchSettings.json`).

## How Roles Map to Cursor

Cursor does not use `.agent.md` files. Instead, agent roles are expressed as **rules**
(`.cursor/rules/*.mdc`) with YAML frontmatter:

```mdc
---
description: Release Orchestrator — coordinates the end-to-end Azure DevOps release workflow.
alwaysApply: false
---

<role body: workflow, safety rules, output format — from docs/agents/*.md>
```

- `release-assistant.mdc` uses `alwaysApply: true` and carries the shared safety rules and
  project conventions (the equivalent of `copilot-instructions.md`).
- Each role rule uses `alwaysApply: false` with a `description`, so Cursor's Agent pulls it
  in when the task matches (the equivalent of selecting a custom agent).

## Invocation

In Cursor Agent, describe the task and reference the role. Cursor selects the matching rule:

```text
Act as the Release Orchestrator. Create a release for CR-12345 in Production, collect work
items, PRs, deployments, and rollback candidates, validate readiness, and generate the
release document.
```

```text
Act as the Validation agent and validate release rel_20260613_001.
```

You can also `@`-mention a rule file directly to force its inclusion.

## Notes

- Cursor rules are additive context, not hard tool restrictions. Keep the tool-access
  guidance from [`mcp-strategy.md`](../mcp/mcp-strategy.md) inside each rule body so the
  Agent respects least-privilege intent.
- The same safety rules apply: read-only against Azure DevOps in the MVP, no production
  actions without explicit human confirmation.
