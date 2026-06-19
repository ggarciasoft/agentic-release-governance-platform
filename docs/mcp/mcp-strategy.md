# MCP Strategy

## 1. Purpose

The system should use MCP servers to let agents access tools safely and consistently.

MCP servers should expose capabilities. Agents should use those capabilities to complete release tasks.

## 2. Recommended MCPs

### Existing Azure DevOps MCP Server

Use for generic Azure DevOps operations:

- Work items
- Pull requests
- Repositories
- Builds
- Test plans
- Wikis
- Pipeline context, if available

### Custom Release Governance MCP Server

Create this MCP for your company-specific release workflow:

- Release records
- Application mappings
- Validation rules
- Rollback logic
- Release package generation
- Document storage

### Optional Azure MCP Server

Use later for Azure infrastructure checks:

- App Service status
- Azure Container Apps status
- Application Insights
- Key Vault
- Storage
- Resource groups

### Optional Document MCP

Use later for publishing to:

- SharePoint
- Confluence
- Azure DevOps Wiki
- Filesystem

## 3. Why Create a Custom MCP?

Azure DevOps MCP can retrieve raw Azure DevOps data.

Your system needs release-specific business decisions:

- Is this release ready?
- Which deployment is the current production candidate?
- Which deployment is the rollback candidate?
- Which work items block the release?
- Which applications are missing release data?
- What is the official release package?

Those rules belong in your system, exposed through your custom MCP.

## 4. MCP Design Principle

Keep the custom MCP thin.

The MCP should expose tools that call application services. It should not contain all business logic directly.

```text
Agent
  ↓
Custom MCP Tool
  ↓
Application Service
  ↓
Domain Logic / Database / Azure DevOps Client
```

## 5. Security Principles

MCP tools should be:

- Permission-aware
- Audited
- Validated
- Narrow in scope
- Mostly read-only in MVP
- Protected from secret exposure

## 6. MVP MCP Tools

Implement the canonical release-governance tools (see
[`release-governance-mcp-server-spec.md`](release-governance-mcp-server-spec.md)):

- create_release_item
- get_release_item
- get_application_mapping
- attach_work_items_to_release
- attach_pull_requests_to_release
- attach_deployments_to_release
- collect_release_deployments
- find_rollback_candidates
- collect_release_rollback_candidates
- attach_rollback_candidates_to_release
- validate_release
- generate_release_package
- save_release_document

## 7. Avoid in MVP

Do not expose tools for:

- Approving production releases
- Triggering production deployments
- Deleting Azure DevOps data
- Modifying work items
- Changing pipeline configuration

These can be added later with strict permissions.

---

# Agent-Host MCP Update

The MCP strategy must support agents in any MCP-compatible host (GitHub Copilot, Cursor,
Claude, or another MCP client). The same servers and tools serve every host; see
[Agent Hosts Overview](../hosts/agent-hosts-overview.md).

## Required MCP Servers (All Hosts)

1. `azure-devops` MCP server for raw Azure DevOps data.
2. `release-governance` MCP server for company-specific release workflow.

## Tool Access by Agent

| Agent | MCP Tools |
|---|---|
| Release Orchestrator | `release-governance/*`, `azure-devops/*` |
| Work Item Agent | `azure-devops/*`, `release-governance/attach_work_items_to_release` |
| Pull Request Agent | `azure-devops/*`, `release-governance/attach_pull_requests_to_release` |
| Pipeline Agent | `release-governance/get_application_mapping`, `release-governance/collect_release_deployments`, `release-governance/collect_release_rollback_candidates`, `release-governance/find_rollback_candidates` |
| Validation Agent | `release-governance/validate_release`, `release-governance/generate_release_package` |
| Release Document Agent | `release-governance/generate_release_package`, `release-governance/save_release_document` |
| Communication Agent | `release-governance/generate_release_package` |

## Required Design

The custom MCP server should be implemented as a thin adapter over the backend application services.
