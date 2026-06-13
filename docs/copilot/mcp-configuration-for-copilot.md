# MCP Configuration for Copilot

## Purpose

This document describes how Copilot agents should connect to MCP tools for the AI Release Assistant.

## Required MCP Servers

### 1. Azure DevOps MCP Server

Used for raw Azure DevOps context:

- Work items
- Pull requests
- Repositories
- Builds
- Releases/pipelines if supported
- Wikis/test plans if needed

### 2. Release Governance MCP Server

Custom MCP server created by this project.

Used for release-specific workflow:

- Create release item
- Get release item
- Get application mapping
- Analyze release scope
- Find deployment candidates
- Find rollback candidates
- Validate release
- Generate release package
- Save release document

## Example VS Code MCP Configuration

The actual file location depends on the host. For VS Code, MCP servers may be configured in workspace or user configuration.

Example conceptual configuration:

```json
{
  "servers": {
    "release-governance": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "src/ReleaseAssistant.McpServer/ReleaseAssistant.McpServer.csproj"
      ],
      "env": {
        "RELEASE_ASSISTANT_API_BASE_URL": "http://localhost:5000",
        "AZURE_DEVOPS_ORGANIZATION": "${input:azureDevOpsOrganization}",
        "AZURE_DEVOPS_PROJECT": "${input:azureDevOpsProject}"
      }
    }
  }
}
```

## Example Agent-Level MCP Configuration

For Copilot environments that support MCP configuration inside custom agent frontmatter, an agent can declare MCP servers like this:

```yaml
---
name: release-orchestrator
description: Coordinates release analysis and document generation.
tools: ['release-governance/*', 'azure-devops/*']
mcp-servers:
  release-governance:
    type: local
    command: dotnet
    args:
      - run
      - --project
      - src/ReleaseAssistant.McpServer/ReleaseAssistant.McpServer.csproj
    tools: ['*']
    env:
      RELEASE_ASSISTANT_API_BASE_URL: ${{ variables.RELEASE_ASSISTANT_API_BASE_URL }}
      AZURE_DEVOPS_ORGANIZATION: ${{ variables.AZURE_DEVOPS_ORGANIZATION }}
      AZURE_DEVOPS_PROJECT: ${{ variables.AZURE_DEVOPS_PROJECT }}
---
```

## Secret Handling

Do not hardcode:

- Azure DevOps PATs
- OAuth client secrets
- API keys
- Database connection strings
- Storage keys

Use organization or repository-level agent secrets/variables where supported.

## Local Development Recommendation

For the first build:

- Run Release Governance MCP locally through stdio.
- Use a PAT only in local environment variables.
- Keep Azure DevOps access read-only where possible.
- Generate documents locally as Markdown.

For production:

- Use service principal or managed identity where supported.
- Use Key Vault or approved secret store.
- Enable audit logging for every tool invocation.
