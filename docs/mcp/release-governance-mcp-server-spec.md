# Release Governance MCP Server Specification

## Purpose

The Release Governance MCP Server exposes company-specific release workflow capabilities to agents in any MCP-compatible host (GitHub Copilot, Cursor, Claude, or another MCP client).

It should be the main custom MCP server for this project.

## Server Name

```text
release-governance
```

## Design Rules

- Keep the MCP layer thin.
- Delegate business logic to application services.
- Validate every input.
- Return structured JSON responses.
- Never expose secrets.
- Log every tool invocation.
- Prefer read-only operations unless the tool name clearly creates or saves data.

## Required Tools

The examples below show the meaningful payload fields for each tool. The exact wire
format wraps every payload in the standard envelope (see **Response Standard** below);
[`mcp-tool-contracts.md`](mcp-tool-contracts.md) is the authoritative per-tool contract.

### create_release_item

Creates a release item in the Release Assistant system.

Input:

```json
{
  "releaseName": "June 2026 Production Release",
  "changeRequest": "CR-12345",
  "organization": "my-org",
  "project": "my-project",
  "targetEnvironment": "Production",
  "applications": ["Payments API", "Admin Portal"]
}
```

Output:

```json
{
  "success": true,
  "releaseId": "rel_20260613_001",
  "status": "Created",
  "warnings": [],
  "errors": []
}
```

### get_release_item

Returns the current release item.

Input:

```json
{
  "releaseId": "rel_20260613_001"
}
```

Output:

```json
{
  "success": true,
  "release": {
    "releaseId": "rel_20260613_001",
    "changeRequest": "CR-12345",
    "targetEnvironment": "Production",
    "status": "Created"
  }
}
```

### get_application_mapping

Returns application-to-repo and application-to-pipeline mapping.

Input:

```json
{
  "applicationName": "Payments API"
}
```

Output:

```json
{
  "success": true,
  "mapping": {
    "applicationName": "Payments API",
    "repositoryName": "payments-api",
    "buildDefinitionName": "Payments API Build",
    "releaseDefinitionName": "Payments API Release",
    "productionEnvironmentName": "Production",
    "uatEnvironmentName": "UAT"
  }
}
```

### attach_work_items_to_release

Stores work items collected by Azure DevOps MCP or Azure DevOps integration.

Input:

```json
{
  "releaseId": "rel_20260613_001",
  "workItems": [
    {
      "id": 12345,
      "type": "User Story",
      "title": "Add payment validation",
      "state": "UATDone",
      "url": "https://dev.azure.com/..."
    }
  ]
}
```

Output:

```json
{
  "success": true,
  "attachedCount": 1,
  "warnings": [],
  "errors": []
}
```

### attach_pull_requests_to_release

Stores pull requests collected by Azure DevOps MCP or Azure DevOps integration.

Input:

```json
{
  "releaseId": "rel_20260613_001",
  "pullRequests": [
    {
      "pullRequestId": 55,
      "repositoryName": "payments-api",
      "title": "Payment validation changes",
      "status": "completed",
      "targetBranch": "main",
      "url": "https://dev.azure.com/..."
    }
  ]
}
```

Output:

```json
{
  "success": true,
  "attachedCount": 1,
  "warnings": [],
  "errors": []
}
```

### attach_deployments_to_release

Stores deployment/release pipeline data.

Input:

```json
{
  "releaseId": "rel_20260613_001",
  "deployments": [
    {
      "applicationName": "Payments API",
      "releaseName": "Payments API Release-20260613.1",
      "environmentName": "Production",
      "status": "PendingApproval",
      "url": "https://dev.azure.com/..."
    }
  ]
}
```

Output:

```json
{
  "success": true,
  "attachedCount": 1,
  "warnings": [],
  "errors": []
}
```

### find_rollback_candidates

Finds the rollback candidate for each application.

Input:

```json
{
  "releaseId": "rel_20260613_001"
}
```

Output:

```json
{
  "success": true,
  "rollbackCandidates": [
    {
      "applicationName": "Payments API",
      "releaseName": "Payments API Release-20260601.3",
      "environmentName": "Production",
      "status": "Succeeded",
      "url": "https://dev.azure.com/..."
    }
  ],
  "warnings": []
}
```

### validate_release

Runs deterministic validation rules.

Input:

```json
{
  "releaseId": "rel_20260613_001"
}
```

Output:

```json
{
  "success": true,
  "status": "Warning",
  "blockers": [],
  "warnings": [
    {
      "code": "WORK_ITEM_NOT_UAT_DONE",
      "message": "Work item 12346 is UATReady but not UATDone."
    }
  ],
  "errors": []
}
```

### generate_release_package

Returns the structured release package used by the document agent.

Input:

```json
{
  "releaseId": "rel_20260613_001"
}
```

Output:

```json
{
  "success": true,
  "releasePackage": {
    "release": {},
    "applications": [],
    "workItems": [],
    "pullRequests": [],
    "deployments": [],
    "rollbackCandidates": [],
    "validations": []
  }
}
```

### save_release_document

Saves the generated release document.

Input:

```json
{
  "releaseId": "rel_20260613_001",
  "format": "markdown",
  "content": "# Production Release Document..."
}
```

Output:

```json
{
  "success": true,
  "documentId": "doc_001",
  "version": 1,
  "status": "Saved"
}
```

## Response Standard

All MCP tools return the same envelope, with payload data nested under `data` and the
invoked tool name echoed back in `tool`:

```json
{
  "success": true,
  "tool": "create_release_item",
  "data": {},
  "warnings": [],
  "errors": []
}
```

For failures:

```json
{
  "success": false,
  "tool": "create_release_item",
  "data": null,
  "warnings": [],
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "releaseId is required."
    }
  ]
}
```

See [`mcp-tool-contracts.md`](mcp-tool-contracts.md) for the full input/output contract of
each tool and the common error-code catalog.
