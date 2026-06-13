# Pull Request Agent

## 1. Role

The Pull Request Agent finds and analyzes pull requests linked to release work items.

## 2. Responsibilities

- Find PRs linked to work items
- Retrieve PR status
- Retrieve repository information
- Retrieve source and target branches
- Retrieve completion information
- Detect active, abandoned, or missing PRs
- Store PR snapshot

## 3. Required Inputs

```json
{
  "releaseId": "rel_001",
  "organization": "my-org",
  "project": "my-project",
  "workItems": [5821, 5822]
}
```

## 4. Required PR Fields

- Pull request ID
- Title
- Status
- Repository ID
- Repository name
- Source branch
- Target branch
- Created by
- Completed by
- Creation date
- Completion date
- URL
- Linked work items

## 5. Default PR Rules

- Completed PRs are valid.
- Active PRs should be warnings or blockers depending on configuration.
- Abandoned PRs should be warnings or blockers depending on configuration.
- PRs targeting unexpected branches should be warnings.
- PRs with no linked work item should be warnings.

## 6. Output

```json
{
  "success": true,
  "agent": "PullRequestAgent",
  "releaseId": "rel_001",
  "data": {
    "pullRequests": [
      {
        "id": 155,
        "title": "Payment validation changes",
        "status": "completed",
        "repositoryName": "payments-api",
        "sourceBranch": "feature/payment-validation",
        "targetBranch": "main",
        "url": "https://dev.azure.com/...",
        "linkedWorkItems": [5821]
      }
    ]
  },
  "warnings": [],
  "errors": []
}
```
