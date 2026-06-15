# Requirements: MCP Agents Across Hosts

## Requirement Summary

The system must provide AI agents that can be called from any MCP-compatible host
(GitHub Copilot, Cursor, Claude, or another MCP client). These agents must use MCP tools,
either existing or custom, to perform the release workflow. No single AI vendor is required.

## Required Capabilities

The agents must be able to:

1. Create a release item.
2. Get user stories, bugs, features, and tasks related to a release.
3. Get pull requests linked to those work items.
4. Get build and release pipeline information.
5. Find production deployment candidates.
6. Find rollback candidates.
7. Validate the release package.
8. Create the release document from the structured release package.
9. Save or return the generated release document via MCP or API tools — never by editing
   Markdown files directly in the workspace.
10. Present blockers, warnings, and missing information.

## Primary Interaction

A user should be able to invoke the orchestrator from their host of choice, for example:

```text
Use the release-orchestrator to create a release for CR-12345, analyze Azure DevOps,
validate readiness, and generate the release document.
```

The exact invocation syntax depends on the host (see
[Agent Hosts Overview](../hosts/agent-hosts-overview.md)), but the behavior is identical.

## MVP Acceptance Criteria

The MVP is complete when:

- The repository contains agent definitions for at least one host, with parity available for
  Copilot (`.github/agents`), Cursor (`.cursor/rules`), and Claude (`.claude/agents`).
- The repository contains host instructions (`copilot-instructions.md`, `CLAUDE.md`,
  `.cursor/rules/release-assistant.mdc`).
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

Agents must operate in a controlled way regardless of host. Any action that changes
production state must require explicit human confirmation and must be supported by
permissions and audit logging.
