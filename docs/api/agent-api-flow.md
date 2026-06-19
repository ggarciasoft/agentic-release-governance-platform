# Agent API Flow

## Purpose

This document shows how agents (in any MCP host — Copilot, Cursor, or Claude), MCP tools,
backend APIs, and Azure DevOps APIs work together. The flow is identical regardless of host;
only the agent definition format differs (see
[Agent Hosts Overview](../hosts/agent-hosts-overview.md)).

## Main Flow

```text
User in an MCP host (Copilot | Cursor | Claude)
  -> release-orchestrator (host-native agent definition)
  -> release-governance/create_release_item
  -> azure-devops/search work items
  -> release-governance/attach_work_items_to_release
  -> azure-devops/find linked PRs
  -> release-governance/attach_pull_requests_to_release
  -> release-governance/collect_release_deployments
  -> release-governance/collect_release_rollback_candidates
  -> release-governance/find_rollback_candidates
  -> release-governance/validate_release
  -> release-governance/generate_release_package
  -> release-document agent
  -> release-governance/save_release_document
```

## Backend Endpoints Supporting MCP

The MCP server can call internal API endpoints or application services directly.
These endpoints match the Release Data Endpoints in
[`api-specification.md`](api-specification.md):

```http
POST /api/releases
GET  /api/releases/{releaseId}
POST /api/releases/{releaseId}/work-items
POST /api/releases/{releaseId}/pull-requests
POST /api/releases/{releaseId}/deployments
POST /api/releases/{releaseId}/rollback-candidates/discover
POST /api/releases/{releaseId}/validate
GET  /api/releases/{releaseId}/package
POST /api/releases/{releaseId}/documents/generate
POST /api/releases/{releaseId}/documents
```

`POST /documents/generate` produces the document; the MCP `save_release_document` tool
persists agent-generated content through `POST /documents`.

## API/MCP Boundary

MCP tools are not the business layer.

Correct layering:

```text
MCP Tool
  -> Application Service
  -> Domain Rules
  -> Infrastructure / Azure DevOps / Database
```

Avoid:

```text
MCP Tool
  -> Direct SQL and duplicated business logic
```
