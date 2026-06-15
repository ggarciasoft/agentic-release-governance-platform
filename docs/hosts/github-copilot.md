# Host Guide: GitHub Copilot

GitHub Copilot is one of the supported [agent hosts](agent-hosts-overview.md). This guide
shows how to express the canonical [agent roles](../agents/agents-overview.md) as Copilot
custom agents and how to register the MCP servers.

> Copilot is **not** required. The same release workflow runs on [Cursor](cursor.md) and
> [Claude](claude.md) using the same MCP servers. Pick the host your team already uses.

## Files

| Purpose | Location |
|---|---|
| Repository instructions | `.github/copilot-instructions.md` |
| Build/coding instructions | `.github/instructions/release-assistant.instructions.md` |
| Custom agents | `.github/agents/<agent-name>.agent.md` |
| Reusable prompts | `.github/prompts/<name>.prompt.md` |

These files are detected automatically when the workspace is opened in a Copilot-enabled
editor (for example VS Code with Copilot Chat Agent mode).

## Agent File Format

Each agent is a Markdown file with YAML frontmatter:

```yaml
---
name: release-orchestrator
description: Coordinates the Azure DevOps release workflow from change request to document.
argument-hint: "CR-12345 Production"
tools: ['release-governance/*', 'azure-devops/*', 'agent']
agents: ['work-item-agent', 'pull-request-agent', 'pipeline-agent', 'validation-agent', 'release-document-agent']
handoffs:
  - label: Validate Release
    agent: validation-agent
    prompt: Validate the release package and report blockers, warnings, and missing information.
    send: false
---

# Release Orchestrator Agent

<body: role, workflow, safety rules, output format — copied from docs/agents/*.md>
```

The body of each `.agent.md` is the portable role definition from
[`docs/agents/`](../agents/agents-overview.md). Only the frontmatter is Copilot-specific.

## MCP Registration

Copilot reads MCP servers from VS Code workspace/user configuration (for example
`.vscode/mcp.json`) or from agent frontmatter. Register both servers:

```json
{
  "inputs": [
    {
      "id": "ado_org",
      "type": "promptString",
      "description": "Azure DevOps organization name (e.g. 'contoso')"
    }
  ],
  "servers": {
    "release-governance": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "src/ReleaseAssistant.McpServer/ReleaseAssistant.McpServer.csproj"],
      "env": {
        "RELEASE_ASSISTANT_API_BASE_URL": "http://localhost:5050",
        "AZURE_DEVOPS_ORGANIZATION": "${input:azureDevOpsOrganization}",
        "AZURE_DEVOPS_PROJECT": "${input:azureDevOpsProject}"
      }
    },
    "azure-devops": {
      "type": "stdio",
      "command": "npx",
      "args": [
        "-y",
        "@azure-devops/mcp",
        "${input:ado_org}",
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

For Copilot environments that support MCP config inside agent frontmatter, an agent can
declare an `mcp-servers:` block instead. Use organization/repository agent secrets for any
PAT or credential — never hardcode secrets.

## Invocation

```text
@release-orchestrator Create a release for CR-12345 in Production, collect work items, PRs,
deployments, and rollback candidates, validate readiness, and generate the release document.
```

```text
@validation-agent Validate release rel_20260613_001 and report blockers.
```

```text
@release-document-agent Generate the official release document for rel_20260613_001.
```

## Cloud Agents

Where supported, the same agents can be adapted for Copilot cloud agents by adding `target`
and `mcp-servers` configuration to the frontmatter. Give cloud agents read-only or low-risk
tools unless the organization has approved stronger permissions.

## Secret Handling

Do not hardcode PATs, OAuth secrets, API keys, connection strings, or storage keys. For the
`azure-devops` MCP server, set `ADO_MCP_AUTH_TOKEN` via environment variable or repository
secrets (`--authentication envvar`). Locally, use `export ADO_MCP_AUTH_TOKEN=...` or
`dotnet user-secrets` for the backend API. See [Security Model](../security/security-model.md).
