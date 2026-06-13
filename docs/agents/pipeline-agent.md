# Pipeline Agent

## 1. Role

The Pipeline Agent finds build, release, and deployment pipeline information for applications included in the release.

## 2. Responsibilities

- Resolve application mapping
- Find build pipelines
- Find release pipelines
- Find production deployment candidate
- Find deployment status
- Find approval status if available
- Store deployment snapshot

## 3. Required Inputs

```json
{
  "releaseId": "rel_001",
  "targetEnvironment": "Production",
  "applications": ["Payments API", "Admin Portal"]
}
```

## 4. Required Application Mapping

```json
{
  "applicationName": "Payments API",
  "repositoryName": "payments-api",
  "buildDefinitionId": 123,
  "releaseDefinitionId": 456,
  "productionEnvironmentName": "Production"
}
```

## 5. Deployment Candidate Rule

The current deployment candidate is the release or pipeline run where the production environment is:

- Queued
- Pending approval
- Waiting
- In progress
- Ready for deployment

The exact statuses must be configurable per organization.

## 6. Output

```json
{
  "success": true,
  "agent": "PipelineAgent",
  "releaseId": "rel_001",
  "data": {
    "deployments": [
      {
        "application": "Payments API",
        "releaseName": "Payments API Release-20260613.1",
        "environmentName": "Production",
        "status": "PendingApproval",
        "deploymentUrl": "https://dev.azure.com/..."
      }
    ]
  },
  "warnings": [],
  "errors": []
}
```

## 7. Rules

- Do not guess application-to-pipeline mappings.
- If an application is not mapped, mark it as unmapped.
- If production deployment candidate is missing, return missing deployment warning.
