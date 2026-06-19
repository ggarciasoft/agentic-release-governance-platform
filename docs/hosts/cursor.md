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
      "args": [
        "run",
        "--project",
        "src/ReleaseAssistant.McpServer/ReleaseAssistant.McpServer.csproj",
        "--no-launch-profile"
      ],
      "env": {}
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

Configure `AzureDevOps:Pat` (and organization/project) via **user secrets** on
`ReleaseAssistant.McpServer` so `collect_release_deployments` and
`collect_release_rollback_candidates` can query classic release pipelines. The MCP server
runs as a standalone process and does not call the REST API for release collection.

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

## Troubleshooting MCP in Cursor

### `azure-devops` stuck yellow / never green

Common causes:

1. **Too many tools** — do **not** use `-d all`. Load only the domains you need (for this
   project: `core`, `work`, `work-items`, `repositories`). Microsoft recommends limiting
   domains because Cursor can time out while loading 80+ tools.
2. **Slow `npx` cold start** — the first start downloads `@azure-devops/mcp` and can take
   30–90 seconds. Wait once, or pre-install: `npm install -g @azure-devops/mcp`.
3. **Misleading `[error]` log lines** — the server writes JSON startup logs to stderr. Cursor
   may label them `[error]` even when the message is `"level":"info"`. That alone is not a
   failure if the server eventually connects.
4. **Missing PAT** — `ADO_MCP_AUTH_TOKEN` must be set in the `env` block (or your shell).
   The server reads **only** `ADO_MCP_AUTH_TOKEN`, not `AZURE_DEVOPS_EXT_PAT`.
5. **Windows `npx` path** — if the server never starts, try `"command": "C:\\Program Files\\nodejs\\npx.cmd"` instead of `"npx"`.
6. **TLS / corporate proxy (`UNABLE_TO_VERIFY_LEAF_SIGNATURE`)** — npm cannot reach
   `registry.npmjs.org` when SSL is intercepted. Install globally with system CAs, then point
   MCP at the global binary (no `npx`):

   ```powershell
   $env:NODE_OPTIONS = "--use-system-ca"
   npm install -g @azure-devops/mcp
   ```

   In `.cursor/mcp.json`, use `"command": "%APPDATA%\\npm\\mcp-server-azuredevops.cmd"` and set
   `"NODE_OPTIONS": "--use-system-ca"` in `env`.

After editing `.cursor/mcp.json`, toggle the server off/on in **Cursor Settings → MCP**.

### `release-governance` stuck on "Waiting for initialize"

The server must use **stdio** transport (not HTTP) and log only to stderr. Use
`--no-launch-profile` in the `dotnet run` args. See
[`getting-started.md`](../development/getting-started.md) troubleshooting.
