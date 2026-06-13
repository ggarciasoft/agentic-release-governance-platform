# Updated Requirements: Copilot-Callable Agents with MCP Tools

## Requirement Summary

The system must provide AI agents that can be called from GitHub Copilot. These agents must use MCP tools, either existing or custom, to perform the release workflow.

## Required Capabilities

The agents must be able to:

1. Create a release item.
2. Get user stories, bugs, features, and tasks related to a release.
3. Get pull requests linked to those work items.
4. Get build and release pipeline information.
5. Find production deployment candidates.
6. Find rollback candidates.
7. Validate the release package.
8. Create the release document.
9. Save or return the generated release document.
10. Present blockers, warnings, and missing information.

## Primary Interaction

A user should be able to use Copilot like this:

```text
@release-orchestrator Create a release for CR-12345, analyze Azure DevOps, validate readiness, and generate the release document.
```

## MVP Acceptance Criteria

The MVP is complete when:

- The repository contains Copilot custom agent files.
- The repository contains Copilot instructions.
- The Release Governance MCP exposes the required tools.
- The agents can create a release item through MCP.
- The agents can retrieve or receive Azure DevOps work item data.
- The agents can retrieve or receive PR data.
- The agents can retrieve or receive pipeline/release data.
- The validation tool returns Ready, Warning, Blocked, Incomplete, or Unknown.
- The document tool generates Markdown release documentation.
- The final document clearly shows missing deployment or rollback data.

## Out of Scope for MVP

- Automatically approving production releases.
- Automatically triggering production deployments.
- Automatically modifying Azure DevOps work item states.
- Automatically sending release communications.
- Complex multi-organization support.
- Full web dashboard.
- PDF/Word export.

## Safety Requirement

Copilot agents must operate in a controlled way. Any action that changes production state must require explicit human confirmation and must be supported by permissions and audit logging.
