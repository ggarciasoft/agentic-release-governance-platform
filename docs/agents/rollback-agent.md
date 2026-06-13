# Rollback Agent

> Topology note: in the MVP there is **no separate rollback agent** in any host.
> Rollback discovery is performed by the [Pipeline Agent](pipeline-agent.md) (and the MVP
> [Azure DevOps Analysis Agent](azure-devops-analysis-agent.md)) via the
> `release-governance/find_rollback_candidates` tool. This document defines the rollback
> rules and output contract; it becomes a standalone agent only in the future split-agent
> topology described in [agents-overview.md](agents-overview.md).

## 1. Role

The Rollback Agent finds rollback candidates for each application in the release.

## 2. Responsibilities

- Find previous successful production deployment
- Confirm it belongs to the same application
- Confirm it belongs to the same environment
- Confirm deployment succeeded
- Return rollback link
- Identify missing rollback candidates

## 3. Required Inputs

```json
{
  "releaseId": "rel_001",
  "targetEnvironment": "Production",
  "deployments": [
    {
      "application": "Payments API",
      "currentReleaseId": "ado-release-1001"
    }
  ]
}
```

## 4. Rollback Candidate Rule

Default rule:

```text
Latest successful production deployment before the current production deployment candidate.
```

## 5. Required Fields

- Application name
- Previous release ID
- Previous release name
- Environment name
- Deployment status
- Completed date
- Rollback URL

## 6. Output

```json
{
  "success": true,
  "agent": "RollbackAgent",
  "releaseId": "rel_001",
  "data": {
    "rollbackCandidates": [
      {
        "application": "Payments API",
        "releaseName": "Payments API Release-20260601.3",
        "environmentName": "Production",
        "status": "Succeeded",
        "completedOn": "2026-06-01T19:22:00Z",
        "rollbackUrl": "https://dev.azure.com/..."
      }
    ]
  },
  "warnings": [],
  "errors": []
}
```

## 7. Rules

- Do not invent rollback links.
- Missing rollback candidate must be explicit.
- Do not recommend a rollback candidate from another application.
- Do not recommend a failed deployment as rollback.
