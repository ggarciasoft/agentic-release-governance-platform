# MVP Scope

## MVP Name

AI Release Assistant MVP

## MVP Objective

Build a read-only Azure DevOps release analysis and documentation tool that can generate a release document from a Change Request tag.

## MVP Input

A user provides:

- Azure DevOps organization
- Azure DevOps project
- Release name
- Change Request tag
- Target environment
- Applications included

Example:

```json
{
  "releaseName": "June 2026 Production Release",
  "changeRequest": "CR-12345",
  "organization": "my-company",
  "project": "main-project",
  "targetEnvironment": "Production",
  "applications": ["Payments API", "Admin Portal"]
}
```

## MVP Output

The system produces:

- Release readiness status
- Work items included
- Pull requests included
- Applications included
- Deployment links
- Rollback links
- Warnings
- Blockers
- Missing information
- Markdown release document

## MVP Features

### 1. Release Management

- Create release
- View release
- Update release metadata
- Start release analysis
- View analysis status

### 2. Application Configuration

- Configure application name
- Configure repository name
- Configure build pipeline name or ID
- Configure release pipeline name or ID
- Configure production environment name
- Configure UAT environment name

### 3. Azure DevOps Work Item Search

- Search work items by Change Request tag
- Retrieve work item type, title, state, assigned user, tags, and URL
- Store snapshot in database

### 4. Pull Request Discovery

- Find pull requests linked to discovered work items
- Retrieve PR title, status, source branch, target branch, repository, completion date, and URL
- Store snapshot in database

### 5. Deployment Discovery

- Find current production deployment candidate per application
- Identify status: queued, pending approval, in progress, succeeded, failed, canceled, not found
- Store deployment link

### 6. Rollback Discovery

- Find latest successful production deployment before the current candidate
- Store rollback release name and link

### 7. Validation

- Validate work item states
- Validate PR status
- Validate deployment link exists
- Validate rollback link exists
- Validate application mapping exists

### 8. Document Generation

- Generate Markdown document
- Save document version
- Allow user to copy or download Markdown

## MVP Agent Set

Start with four agents:

1. Release Orchestrator Agent
2. Azure DevOps Analysis Agent
3. Validation Agent
4. Release Document Agent

Later versions can split Azure DevOps Analysis Agent into Work Item, PR, Pipeline, and Rollback agents.

## MVP MCP Set

Use:

- Azure DevOps MCP Server, if available and approved by the organization
- Custom Release Governance MCP Server

The MVP may also call Azure DevOps REST APIs directly from the backend if the MCP tooling does not cover all required endpoints.

## MVP Exclusions

Do not include in MVP:

- Production deployment execution
- Automatic approval
- Azure DevOps work item updates
- Slack or Teams posting
- PDF and Word export
- SharePoint or Confluence publishing
- Full monitoring integration
