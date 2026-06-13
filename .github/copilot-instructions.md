# Copilot Repository Instructions

This repository contains the AI Release Assistant for Azure DevOps.

## Product Purpose

Build a Copilot-callable, MCP-powered agentic release assistant that helps teams create release items, collect Azure DevOps work items, find linked pull requests, inspect release pipelines, validate readiness, identify rollback candidates, and generate release documents.

## Architecture Principles

- Use .NET 8 for backend services.
- Keep business logic in application services, not in controllers, prompts, or MCP wrappers.
- Keep MCP servers thin. MCP tools should delegate to application services.
- Use deterministic validation rules for release readiness.
- Use LLMs only for summarization, document drafting, and guided interaction.
- Never let the LLM invent work item IDs, PRs, pipeline links, rollback links, statuses, approvals, or dates.
- Treat Azure DevOps data as the source of truth.
- Use human-in-the-loop approval for any production-impacting action.

## Expected Projects

Recommended solution structure:

```text
src/
  ReleaseAssistant.Api/
  ReleaseAssistant.Application/
  ReleaseAssistant.Domain/
  ReleaseAssistant.Infrastructure/
  ReleaseAssistant.AzureDevOps/
  ReleaseAssistant.Agents/
  ReleaseAssistant.McpServer/
web/
  release-assistant-ui/
docs/
.github/
  agents/
  prompts/
  instructions/
```

## Coding Standards

- Prefer explicit types in public APIs.
- Use dependency injection.
- Use async APIs for I/O.
- Add cancellation tokens to async service methods.
- Return structured results with success, warnings, and errors.
- Avoid throwing for expected validation failures.
- Use strongly typed IDs where practical.
- Add unit tests for validation rules.
- Add integration tests for Azure DevOps connector interfaces using mocks or test doubles.

## Security Rules

- Do not commit secrets.
- Do not log PATs, OAuth tokens, connection strings, refresh tokens, or Authorization headers.
- Redact secrets as `[REDACTED]`.
- Use least privilege for MCP tools.
- Prefer read-only tools for Copilot agents by default.
- Any tool that creates or modifies data must clearly state what it changes.

## Release Workflow

The target workflow is:

1. Create release item.
2. Get user stories, bugs, features, and tasks by change request tag.
3. Get linked PRs.
4. Get build and release pipeline data.
5. Find current production deployment candidate.
6. Find latest successful production deployment as rollback candidate.
7. Validate readiness.
8. Generate structured release package.
9. Generate release document.
10. Save document and present warnings/blockers.

## Testing Expectations

When implementing features, include tests for:

- Work item readiness rules.
- PR readiness rules.
- Pipeline readiness rules.
- Rollback candidate selection.
- Missing data handling.
- MCP tool input validation.
- Document generation with missing fields.

## Documentation Expectations

Update the Markdown docs whenever changing:

- Agent behavior.
- MCP tool contracts.
- Validation rules.
- API endpoints.
- Database schema.
- Azure DevOps integration assumptions.
