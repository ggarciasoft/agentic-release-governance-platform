# Azure DevOps Analysis Agent

> This is the **MVP all-in-one** Azure DevOps collection agent. It combines the work of the
> future [Work Item Agent](work-item-agent.md), [Pull Request Agent](pull-request-agent.md),
> [Pipeline Agent](pipeline-agent.md), and [Rollback Agent](rollback-agent.md) into a single
> agent so the MVP can ship with a smaller agent set. It is expressed per host as
> [`.github/agents/azure-devops-analysis-agent.agent.md`](../../.github/agents/azure-devops-analysis-agent.agent.md),
> [`.cursor/rules/azure-devops-analysis-agent.mdc`](../../.cursor/rules/azure-devops-analysis-agent.mdc),
> and [`.claude/agents/azure-devops-analysis-agent.md`](../../.claude/agents/azure-devops-analysis-agent.md).
> When the system moves to the split-agent topology, this agent is replaced by the four
> specialized agents.

## 1. Role

The Azure DevOps Analysis Agent collects all Azure DevOps data needed to assemble a release
package: work items, linked pull requests, deployment candidates, and rollback candidates.

It retrieves factual data only. It does not decide release readiness (that is the
[Validation Agent](validation-agent.md)) and it does not generate documents (that is the
[Release Document Agent](document-agent.md)).

## 2. Responsibilities

- Search work items by Change Request tag and attach them to the release.
- Find pull requests linked to those work items and attach them.
- Resolve application mappings for each application in the release.
- Find current production deployment candidates and attach them.
- Find rollback candidates for each application.
- Return a structured summary of everything collected.
- Mark missing data explicitly.

## 3. Required Inputs

```json
{
  "releaseId": "rel_001",
  "organization": "my-org",
  "project": "my-project",
  "changeRequest": "CR-12345",
  "targetEnvironment": "Production"
}
```

## 4. Tools

- `azure-devops/*` — read Azure DevOps work items, PRs, repositories, builds, and releases.
- `release-governance/get_application_mapping` — resolve application-to-pipeline mapping.
- `release-governance/attach_work_items_to_release`
- `release-governance/attach_pull_requests_to_release`
- `release-governance/attach_deployments_to_release`
- `release-governance/find_rollback_candidates`

Tool contracts are defined in
[`../mcp/mcp-tool-contracts.md`](../mcp/mcp-tool-contracts.md).

## 5. Workflow

1. Search work items where `System.Tags` contains the Change Request value.
2. Attach the work items to the release item.
3. Find pull requests linked to those work items.
4. Attach the pull requests to the release item.
5. Resolve the application mapping for each application.
6. Find the current production deployment candidate per application.
7. Attach the deployment data.
8. Find rollback candidates per application.
9. Return a structured summary.

## 6. Output

```json
{
  "success": true,
  "agent": "AzureDevOpsAnalysisAgent",
  "releaseId": "rel_001",
  "status": "Completed",
  "summary": "18 work items, 12 pull requests, 3 deployments, 3 rollback candidates collected.",
  "data": {
    "workItemCount": 18,
    "pullRequestCount": 12,
    "deploymentCount": 3,
    "rollbackCandidateCount": 3,
    "applicationsWithoutMapping": [],
    "applicationsWithoutDeployment": [],
    "applicationsWithoutRollback": ["Notification Service"]
  },
  "warnings": [
    {
      "code": "ROLLBACK_MISSING",
      "message": "Notification Service has no rollback candidate."
    }
  ],
  "errors": []
}
```

## 7. Rules

- Read-only against Azure DevOps. Never approve or trigger deployments.
- Never modify work item state.
- Do not invent work items, PRs, deployment links, or rollback links.
- If a data set is empty, return an empty result and an explicit warning.
- Do not decide final release readiness.
- Preserve traceability: every attached item must carry its Azure DevOps URL.
