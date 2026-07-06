# MCP Tool Contracts

This document is the authoritative input/output contract for the **`release-governance`**
MCP server tools. It must stay aligned with
[`release-governance-mcp-server-spec.md`](release-governance-mcp-server-spec.md)
and the tool list in the project `README.md`.

## Response Envelope

Every tool returns the same envelope. Payload data is always nested under `data`.

Success:

```json
{
  "success": true,
  "tool": "validate_release",
  "data": {},
  "warnings": [],
  "errors": []
}
```

Failure:

```json
{
  "success": false,
  "tool": "validate_release",
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

## Tool Index

| # | Tool | Type | Purpose |
|---|---|---|---|
| 1 | `create_release_item` | write | Create a release record |
| 2 | `get_release_item` | read | Retrieve a release record |
| 3 | `get_application_mapping` | read | Resolve application-to-repo/pipeline mapping |
| 4 | `attach_work_items_to_release` | write | Persist collected work items |
| 5 | `attach_pull_requests_to_release` | write | Persist collected pull requests |
| 6 | `attach_deployments_to_release` | write | Persist manually collected deployments |
| 6b | `collect_release_deployments` | read/write | Discover classic release deployments from Azure DevOps and attach |
| 7 | `find_rollback_candidates` | read | Return rollback candidates already attached to the release |
| 7b | `collect_release_rollback_candidates` | read/write | Discover classic release rollback candidates from Azure DevOps and attach |
| 7c | `attach_rollback_candidates_to_release` | write | Persist manually collected rollback candidates |
| 8 | `validate_release` | read | Run deterministic validation rules |
| 9 | `generate_release_package` | read | Build the structured release package |
| 10 | `save_release_document` | write | Store a generated release document |

The `attach_*` and `create_*`/`save_*` tools are the only tools that write data.
All writes are scoped to the Release Assistant database, never to Azure DevOps.

## 1. create_release_item

Creates a release item in the Release Assistant system.

### Input

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

### Output

```json
{
  "success": true,
  "tool": "create_release_item",
  "data": {
    "releaseId": "rel_20260613_001",
    "status": "Created"
  },
  "warnings": [],
  "errors": []
}
```

## 2. get_release_item

Returns the current release item.

### Input

```json
{
  "releaseId": "rel_20260613_001"
}
```

### Output

```json
{
  "success": true,
  "tool": "get_release_item",
  "data": {
    "releaseId": "rel_20260613_001",
    "changeRequest": "CR-12345",
    "targetEnvironment": "Production",
    "status": "Created"
  },
  "warnings": [],
  "errors": []
}
```

## 3. get_application_mapping

Returns application-to-repository and application-to-pipeline mapping.

### Input

```json
{
  "applicationName": "Payments API"
}
```

### Output

```json
{
  "success": true,
  "tool": "get_application_mapping",
  "data": {
    "applicationName": "Payments API",
    "repositoryName": "payments-api",
    "buildDefinitionId": 123,
    "releaseDefinitionId": 456,
    "productionEnvironmentName": "Production",
    "uatEnvironmentName": "UAT"
  },
  "warnings": [],
  "errors": []
}
```

## 4. attach_work_items_to_release

Stores work items collected by the Azure DevOps MCP server or Azure DevOps integration.

### Input

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

### Output

```json
{
  "success": true,
  "tool": "attach_work_items_to_release",
  "data": {
    "attachedCount": 1
  },
  "warnings": [],
  "errors": []
}
```

## 5. attach_pull_requests_to_release

Stores pull requests collected by the Azure DevOps MCP server or Azure DevOps integration.

`mergeCommitId` must be set to `lastMergeSourceCommit.commitId` from the Azure DevOps PR
response. It is used by `collect_release_deployments` to match the correct pipeline release
to the CR's code, instead of falling back to the most recent release by date.
**Always attach PRs before calling `collect_release_deployments`.**

### Input

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
      "url": "https://dev.azure.com/...",
      "mergeCommitId": "5b9ba1ac3eabcf544082d92cdb01f8db7d26e48d"
    }
  ]
}
```

### Output

```json
{
  "success": true,
  "tool": "attach_pull_requests_to_release",
  "data": {
    "attachedCount": 1
  },
  "warnings": [],
  "errors": []
}
```

## 6. attach_deployments_to_release

Stores deployment/release pipeline data.

### Input

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

### Output

```json
{
  "success": true,
  "tool": "attach_deployments_to_release",
  "data": {
    "attachedCount": 1
  },
  "warnings": [],
  "errors": []
}
```

## 6b. collect_release_deployments

Discovers current deployment candidates from Azure DevOps **classic release pipelines**
(`vsrm.dev.azure.com`) for each application on the release that has a `releaseDefinitionId`
mapping, then attaches them to the release.

Requires `AzureDevOps:Pat` (and organization/project) configured for the MCP server process.

### Input

```json
{
  "releaseId": "rel_20260613_001"
}
```

### Output

```json
{
  "success": true,
  "tool": "collect_release_deployments",
  "data": {
    "attachedCount": 1,
    "step": "deployments",
    "deployments": [
      {
        "applicationName": "Payments API",
        "releaseName": "Payments API Release-20260613.1",
        "environmentName": "Production",
        "status": "succeeded",
        "url": "https://dev.azure.com/...",
        "isCurrentDeployment": true
      }
    ]
  },
  "warnings": [],
  "errors": []
}
```

## 7. find_rollback_candidates

Returns rollback candidates **already attached** to the release. This tool does **not** query
Azure DevOps. Call `collect_release_rollback_candidates` first (or attach manually with
`attach_rollback_candidates_to_release`), then call this tool to read the attached rows.

The intended rollback candidate is the latest successful production deployment before the
current production deployment candidate.

### Input

```json
{
  "releaseId": "rel_20260613_001"
}
```

### Output

```json
{
  "success": true,
  "tool": "find_rollback_candidates",
  "data": {
    "rollbackCandidates": [
      {
        "applicationName": "Payments API",
        "releaseName": "Payments API Release-20260601.3",
        "environmentName": "Production",
        "status": "Succeeded",
        "url": "https://dev.azure.com/..."
      }
    ]
  },
  "warnings": [],
  "errors": []
}
```

## 7b. collect_release_rollback_candidates

Discovers the prior successful classic release pipeline deployment for each mapped application
(excluding the current deployment candidate when known) and attaches one rollback candidate
per application.

Requires `AzureDevOps:Pat` (and organization/project) configured for the MCP server process.

### Input

```json
{
  "releaseId": "rel_20260613_001"
}
```

### Output

```json
{
  "success": true,
  "tool": "collect_release_rollback_candidates",
  "data": {
    "attachedCount": 1,
    "step": "rollback",
    "rollbackCandidates": [
      {
        "applicationName": "Payments API",
        "releaseName": "Payments API Release-20260601.3",
        "environmentName": "Production",
        "status": "succeeded",
        "url": "https://dev.azure.com/..."
      }
    ]
  },
  "warnings": [],
  "errors": []
}
```

## 8. validate_release

Runs deterministic validation rules. Rule codes are defined in
[`../security/validation-rules.md`](../security/validation-rules.md).

### Input

```json
{
  "releaseId": "rel_20260613_001"
}
```

### Output

```json
{
  "success": true,
  "tool": "validate_release",
  "data": {
    "status": "Warning",
    "blockers": [],
    "warnings": [
      {
        "code": "WORK_ITEM_NOT_UAT_DONE",
        "message": "Work item 12346 is UATReady but not UATDone."
      }
    ],
    "info": []
  },
  "warnings": [],
  "errors": []
}
```

## 9. generate_release_package

Returns the structured release package used by the Release Document Agent.

### Input

```json
{
  "releaseId": "rel_20260613_001"
}
```

### Output

```json
{
  "success": true,
  "tool": "generate_release_package",
  "data": {
    "releasePackage": {
      "release": {},
      "applications": [],
      "workItems": [],
      "pullRequests": [],
      "deployments": [],
      "rollbackCandidates": [],
      "validations": []
    }
  },
  "warnings": [],
  "errors": []
}
```

## 10. save_release_document

Stores a generated release document with versioning.

### Input

```json
{
  "releaseId": "rel_20260613_001",
  "format": "markdown",
  "content": "# Production Release Document..."
}
```

### Output

```json
{
  "success": true,
  "tool": "save_release_document",
  "data": {
    "documentId": "doc_001",
    "version": 1,
    "status": "Saved"
  },
  "warnings": [],
  "errors": []
}
```

## Common Error Codes

| Code | Meaning |
|---|---|
| `VALIDATION_ERROR` | A required input was missing or malformed |
| `RELEASE_NOT_FOUND` | The supplied `releaseId` does not exist |
| `MAPPING_NOT_FOUND` | No application mapping was found |
| `PERMISSION_DENIED` | The caller is not authorized for this tool |
| `UPSTREAM_ERROR` | Azure DevOps or another dependency failed |
