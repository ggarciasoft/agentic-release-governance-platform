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

```http
POST /api/releases/{releaseId}/rollback-candidates/discover
```

### Get Release Package

```http
GET /api/releases/{releaseId}/package
```

### Backend-Driven Analysis (optional / future)

For a server-side workflow (for example a future web UI that triggers collection on the
backend instead of through host agents), the API may also expose server-driven analysis
endpoints. These are **not required for the MCP-first MVP**.

```http
POST /api/releases/{releaseId}/analyze
POST /api/releases/{releaseId}/analyze/work-items
POST /api/releases/{releaseId}/analyze/pull-requests
POST /api/releases/{releaseId}/analyze/deployments
POST /api/releases/{releaseId}/analyze/rollback
GET  /api/releases/{releaseId}/analysis/status
```

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

### Save Document

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
