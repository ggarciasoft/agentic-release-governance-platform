# Host Guide: Claude

Claude is one of the supported [agent hosts](agent-hosts-overview.md). Both **Claude Code**
(terminal/IDE) and **Claude Desktop** connect to the same `release-governance` and
`azure-devops` MCP servers and implement the same
[agent roles](../agents/agents-overview.md).

## Claude Code

### Files

| Purpose | Location |
|---|---|
| Project memory / instructions | `CLAUDE.md` |
| Project MCP server config | `.mcp.json` |
| Subagents (per-role) | `.claude/agents/<role>.md` |

These files ship in this repository, so running Claude Code in the project root wires
everything up.

### MCP Registration

`.mcp.json` (project-scoped, shared via source control) registers both servers:

```json
{
  "mcpServers": {
    "release-governance": {
      "command": "dotnet",
      "args": ["run", "--project", "src/ReleaseAssistant.McpServer/ReleaseAssistant.McpServer.csproj"],
      "env": {
        "RELEASE_ASSISTANT_API_BASE_URL": "http://localhost:5000"
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

Approve the project MCP servers when Claude Code prompts. Replace `your-org` with your
Azure DevOps organization name (positional CLI argument). Set `ADO_MCP_AUTH_TOKEN` to your PAT
in your shell (`export ADO_MCP_AUTH_TOKEN=...`) before launching; never commit it.

### Subagent Format

Each role is a Claude Code subagent in `.claude/agents/`, with YAML frontmatter:

```md
---
name: release-orchestrator
description: Coordinates the Azure DevOps release workflow from change request to document. Use proactively when a user asks to prepare or build a release.
tools: mcp__release-governance, mcp__azure-devops
---

<role body: workflow, safety rules, output format — from docs/agents/*.md>
```

- `name` and `description` let Claude delegate to the subagent automatically.
- `tools` restricts the subagent to specific MCP servers (least privilege). Omit `tools` to
  inherit all available tools.
- The body is the portable role definition from [`docs/agents/`](../agents/agents-overview.md).

`CLAUDE.md` holds the shared safety rules and project conventions (the equivalent of
`copilot-instructions.md`).

### Invocation

```text
Use the release-orchestrator subagent to create a release for CR-12345 in Production,
collect work items, PRs, deployments, and rollback candidates, validate readiness, and
generate the release document.
```

Claude can also auto-delegate based on each subagent's `description`.

## Claude Desktop

Claude Desktop registers MCP servers in `claude_desktop_config.json`:

- macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`
- Windows: `%APPDATA%\Claude\claude_desktop_config.json`

```json
{
  "mcpServers": {
    "release-governance": {
      "command": "dotnet",
      "args": ["run", "--project", "/abs/path/to/src/ReleaseAssistant.McpServer/ReleaseAssistant.McpServer.csproj"],
      "env": { "RELEASE_ASSISTANT_API_BASE_URL": "http://localhost:5000" }
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

Claude Desktop does not support file-based subagents. Paste the relevant role definition
from [`docs/agents/`](../agents/agents-overview.md) into the conversation (or a Project's
custom instructions) and the MCP tools will be available.

## Notes

- The same safety rules apply: read-only against Azure DevOps in the MVP, no production
  actions without explicit human confirmation.
- Use absolute paths in `claude_desktop_config.json`; `.mcp.json` in Claude Code resolves
  paths relative to the project root.
