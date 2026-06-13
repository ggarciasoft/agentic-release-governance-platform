---
description: Prepare an Azure DevOps release using the Release Orchestrator agent.
mode: agent
---

# Prepare Release

Use the Release Orchestrator agent to prepare a release.

## Required Inputs

Ask for any missing required values:

- Change request or release tag
- Target environment
- Azure DevOps organization
- Azure DevOps project
- Applications, if not discoverable

## Steps

1. Create or locate the release item.
2. Find work items related to the change request.
3. Find linked pull requests.
4. Find release pipeline/deployment information.
5. Find rollback candidates.
6. Validate release readiness.
7. Generate the release document.

## Output

Return:

- Release status
- Release ID
- Blockers
- Warnings
- Missing information
- Link or content of the generated document
