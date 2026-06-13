# Azure DevOps Integration

## 1. Purpose

The system integrates with Azure DevOps to collect release evidence.

## 2. Required Azure DevOps Areas

- Azure Boards
- Azure Repos
- Azure Pipelines
- Classic Releases, if used
- Environments and approvals
- Builds
- Pull requests

## 3. Authentication Strategy

### MVP

Use Personal Access Token for local development and internal proof of concept.

Requirements:

- Store PAT in user secrets locally
- Store PAT in Azure Key Vault in cloud
- Never log PAT
- Never expose PAT to the frontend

### Production

Prefer Microsoft Entra ID, service principals, managed identities, or secure OAuth flows depending on organization constraints.

## 4. Work Item Search

Search work items by Change Request tag.

Example WIQL concept:

```sql
SELECT [System.Id], [System.Title], [System.State], [System.WorkItemType]
FROM WorkItems
WHERE [System.TeamProject] = @project
AND [System.Tags] CONTAINS @changeRequest
```

Required fields:

- ID
- Work item type
- Title
- State
- Assigned To
- Tags
- Area Path
- Iteration Path
- URL
- Relations

## 5. Pull Request Discovery

Pull requests can be found by:

- Work item relations
- PR linked work items
- Repository PR search

Required fields:

- PR ID
- Title
- Status
- Repository
- Source branch
- Target branch
- Created by
- Completed by
- Completion date
- Merge commit
- URL
- Linked work items

## 6. Build Discovery

Required fields:

- Build ID
- Build number
- Definition name
- Status
- Result
- Source branch
- Source version
- Repository
- Queue time
- Finish time
- URL

## 7. Release and Deployment Discovery

Required fields:

- Release ID
- Release name
- Release definition
- Environment name
- Environment status
- Deployment status
- Approval status
- Created date
- Completed date
- URL

## 8. Rollback Candidate Logic

Default rule:

```text
Rollback candidate = latest successful production deployment for the same application and production environment before the current production candidate.
```

The rollback candidate must:

- Belong to the same application
- Belong to the same target environment
- Have completed successfully
- Be older than the current candidate
- Have a valid URL

## 9. Application Mapping

The system should not guess application ownership.

Required mapping:

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

## 10. Integration Error Handling

If Azure DevOps data cannot be retrieved:

- Mark affected section incomplete
- Store error details
- Show user-friendly message
- Do not fabricate data
