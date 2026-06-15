# API Specification

## 1. Base URL

```text
/api
```

## 2. Release Endpoints

### Create Release

```http
POST /api/releases
```

Request:

```json
{
  "name": "June 2026 Production Release",
  "changeRequest": "CR-12345",
  "organization": "my-org",
  "project": "my-project",
  "targetEnvironment": "Production",
  "applications": ["Payments API", "Admin Portal"]
}
```

Response:

```json
{
  "id": "rel_001",
  "name": "June 2026 Production Release",
  "status": "Created"
}
```

### Get Release

```http
GET /api/releases/{releaseId}
```

### List Releases

```http
GET /api/releases
```

### Update Release

```http
PUT /api/releases/{releaseId}
```

### Delete Release

```http
DELETE /api/releases/{releaseId}
```

For MVP, delete should be soft delete.

## 3. Release Data Endpoints

The MVP is **agent-driven**: agents (in any MCP host — Copilot, Cursor, or Claude) collect
Azure DevOps data through the `azure-devops` MCP server and then persist it through the
`release-governance` MCP tools.
These attach endpoints back the MCP `attach_*` tools one-to-one and are the primary path for
the MVP. They receive already-collected data; they do not call Azure DevOps themselves.

| Endpoint | Backing MCP tool |
|---|---|
| `POST /api/releases/{releaseId}/work-items` | `attach_work_items_to_release` |
| `POST /api/releases/{releaseId}/pull-requests` | `attach_pull_requests_to_release` |
| `POST /api/releases/{releaseId}/deployments` | `attach_deployments_to_release` |
| `POST /api/releases/{releaseId}/rollback-candidates/discover` | `find_rollback_candidates` |
| `GET /api/releases/{releaseId}/package` | `generate_release_package` |

### Attach Work Items

```http
POST /api/releases/{releaseId}/work-items
```

### Attach Pull Requests

```http
POST /api/releases/{releaseId}/pull-requests
```

### Attach Deployments

```http
POST /api/releases/{releaseId}/deployments
```

### Discover Rollback Candidates

Returns rollback candidates **already attached** to the release. This endpoint does **not**
query Azure DevOps. To discover rollback data from ADO, use `POST /analyze/rollback` or
collect via the `azure-devops` MCP server and attach with
`POST /api/releases/{releaseId}/rollback-candidates`.

```http
POST /api/releases/{releaseId}/rollback-candidates/discover
```

Response includes attached candidates and a note when none are present.

### Attach Rollback Candidates

```http
POST /api/releases/{releaseId}/rollback-candidates
```

### Get Release Package

```http
GET /api/releases/{releaseId}/package
```

### Backend-Driven Analysis

For server-side workflows (for example a web UI that triggers collection on the backend
instead of through host agents), the API exposes analysis endpoints that query Azure DevOps
via the configured PAT, attach results to the release, and return step summaries.

```http
POST /api/releases/{releaseId}/analyze
POST /api/releases/{releaseId}/analyze/work-items
POST /api/releases/{releaseId}/analyze/pull-requests
POST /api/releases/{releaseId}/analyze/deployments
POST /api/releases/{releaseId}/analyze/rollback
GET  /api/releases/{releaseId}/analysis/status
```

Each analyze step:

1. Reads release context (change request, organization, project, application mappings).
2. Queries Azure DevOps through `AzureDevOpsClient`.
3. Attaches collected data via the same attach logic used by MCP tools.

Work-item analysis searches `System.Tags` for the release `changeRequest` value. Deployment
and rollback analysis require application mappings with `releaseDefinitionId`.

Analysis status response:

```json
{
  "releaseId": "rel_001",
  "status": "CollectingDeployments",
  "progress": 65,
  "lastUpdatedAt": "2026-06-13T15:20:00Z"
}
```

## 4. Validation Endpoints

### Validate Release

```http
POST /api/releases/{releaseId}/validate
```

### Get Validation Results

```http
GET /api/releases/{releaseId}/validation-results
```

Response:

```json
{
  "releaseId": "rel_001",
  "status": "Warning",
  "blockers": [],
  "warnings": [],
  "info": []
}
```

## 5. Document Endpoints

### Generate Document

Generates document content from the release package (backs the document generation step).

```http
POST /api/releases/{releaseId}/documents/generate
```

Request:

```json
{
  "format": "markdown"
}
```

Response (`200 OK`):

- **Content-Type:** `text/markdown; charset=utf-8`
- **Content-Disposition:** `inline; filename="release-CR-12345-Production.md"`
- **Body:** the full generated Markdown document (not JSON-wrapped)


Persists agent-generated document content with versioning (backs the
`save_release_document` MCP tool).

```http
POST /api/releases/{releaseId}/documents
```

Request:

```json
{
  "format": "markdown",
  "content": "# Production Release Document..."
}
```

### Get Documents

```http
GET /api/releases/{releaseId}/documents
```

### Get Document

```http
GET /api/releases/{releaseId}/documents/{documentId}
```

### Download Document

```http
GET /api/releases/{releaseId}/documents/{documentId}/download
```

## 6. Application Mapping Endpoints

### Create Application Mapping

```http
POST /api/application-mappings
```

Request:

```json
{
  "applicationName": "Payments API",
  "repositoryName": "payments-api",
  "buildDefinitionId": 123,
  "releaseDefinitionId": 456,
  "productionEnvironmentName": "Production",
  "uatEnvironmentName": "UAT"
}
```

### List Application Mappings

```http
GET /api/application-mappings
```

### Get Application Mapping

```http
GET /api/application-mappings/{id}
```

### Update Application Mapping

```http
PUT /api/application-mappings/{id}
```

## 7. Settings Endpoints

### Get Project Settings

```http
GET /api/settings/projects/{projectId}
```

### Update Validation Rules

```http
PUT /api/settings/projects/{projectId}/validation-rules
```

## 8. Response Error Format

```json
{
  "error": {
    "code": "RELEASE_NOT_FOUND",
    "message": "Release was not found.",
    "details": []
  }
}
```
