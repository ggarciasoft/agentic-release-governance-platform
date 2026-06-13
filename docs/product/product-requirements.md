# Product Requirements — AI Release Assistant

## 1. Purpose

AI Release Assistant helps software teams using Azure DevOps prepare release documentation and validate release readiness with less manual effort and fewer mistakes.

The system is designed for teams that use Change Request tickets, Azure DevOps work items, pull requests, build pipelines, release pipelines, and production approval workflows.

## 2. Problem Statement

Release managers often need to manually collect information from multiple places before a production release:

- Work items included in the release
- Pull requests included in the release
- Applications affected
- Deployment pipeline links
- Rollback pipeline links
- Approval status
- UAT status
- Production readiness
- Risk notes
- Post-deployment checks

This process is slow, repetitive, and error-prone.

## 3. Target Users

### Release Manager

Responsible for preparing and reviewing production release documentation.

### Developer

Responsible for linking PRs to work items and tagging work items with the Change Request value.

### QA Engineer

Responsible for confirming that stories, bugs, and features are UAT ready or UAT done.

### DevOps Engineer

Responsible for production pipeline readiness, deployment links, and rollback availability.

### Engineering Manager

Responsible for release governance, compliance, and operational risk review.

## 4. Main Use Case

A release manager creates a release in the app and enters:

- Release name
- Change Request number
- Azure DevOps organization
- Azure DevOps project
- Target environment
- Applications included

The system then collects Azure DevOps data, validates readiness, and generates a release document.

## 5. Core Features

### Release Creation

Users can create a release record with a Change Request value.

### Work Item Discovery

The system searches Azure DevOps work items by tag or Change Request identifier.

### Pull Request Discovery

The system finds pull requests linked to the discovered work items.

### Application Mapping

The system maps repositories and pipelines to internal application names.

### Deployment Discovery

The system finds current production deployment candidates.

### Rollback Discovery

The system finds the latest successful production deployment for each application.

### Validation

The system validates release readiness using deterministic rules.

### Document Generation

The system generates a Markdown release document using structured data.

### Agentic Workflow

Agents coordinate release analysis, validation, and document generation.

## 6. Readiness Statuses

The system must return one of the following release readiness statuses:

- Ready
- Warning
- Blocked
- Incomplete
- Unknown

## 7. Non-Goals for MVP

The MVP should not:

- Trigger production deployments
- Approve releases automatically
- Modify Azure DevOps work items
- Delete release data
- Auto-detect every application mapping without configuration
- Replace human approval

## 8. Success Criteria

The MVP is successful when:

- A release manager can enter a Change Request tag.
- The system finds related work items.
- The system finds linked PRs.
- The system shows affected applications.
- The system finds deployment and rollback links.
- The system validates readiness.
- The system generates a usable Markdown release document.

---

# Copilot-First Requirement Update

The product must support calling specialized AI agents from GitHub Copilot. These agents should use MCP tools to execute the release workflow.

## New Primary User Experience

A release manager or developer can use Copilot to request:

```text
Create a release item for CR-12345, collect the related Azure DevOps work items, find linked PRs and production release pipelines, validate readiness, and create the release document.
```

## Required Agent Capabilities

- Create release item.
- Get user stories, bugs, features, and tasks.
- Get linked PRs.
- Get release pipeline/deployment data.
- Validate release readiness.
- Generate release document.

## Required Technical Capability

The project must include:

- Copilot custom agents under `.github/agents`.
- Repository-wide Copilot instructions under `.github/copilot-instructions.md`.
- A custom Release Governance MCP server.
- MCP tool contracts for release workflow operations.
