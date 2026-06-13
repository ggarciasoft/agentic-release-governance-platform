# CLAUDE.md

Project memory and instructions for Claude Code working in this repository.

This is an **MCP-first, agent-host-agnostic** release assistant for Azure DevOps. The same
`release-governance` and `azure-devops` MCP servers power agents in Claude, Cursor, and GitHub
Copilot. See `docs/hosts/agent-hosts-overview.md`.

## Subagents

Role-specific subagents live in `.claude/agents/`. Claude can delegate to them automatically
based on each subagent's `description`, or you can invoke them explicitly (for example,
"use the release-orchestrator subagent ..."). The four MVP roles are release-orchestrator,
azure-devops-analysis-agent, validation-agent, and release-document-agent.

## MCP Tools

The `release-governance` MCP server exposes 10 canonical tools (see
`docs/mcp/mcp-tool-contracts.md`):
`create_release_item`, `get_release_item`, `get_application_mapping`,
`attach_work_items_to_release`, `attach_pull_requests_to_release`,
`attach_deployments_to_release`, `find_rollback_candidates`, `validate_release`,
`generate_release_package`, `save_release_document`.

Use the `azure-devops` MCP server (read-only) for raw work items, PRs, and pipeline data.

## Hard Safety Rules

- Treat Azure DevOps data and `release-governance` MCP responses as the source of truth.
- Never invent work item IDs, PRs, pipeline links, rollback links, statuses, approvals, or dates.
- Mark missing data explicitly as `Missing`; never hide it.
- Never override or soften MCP validation results.
- Never approve releases, trigger production deployments, or modify Azure DevOps work item
  states in the MVP.
- Any production-impacting action requires explicit human confirmation and audit logging.

## Architecture Principles

- Keep business logic in application services, not in controllers, prompts, or MCP wrappers.
- Keep MCP servers thin: MCP tools delegate to application services.
- Use deterministic validation rules for release readiness.
- Use the model only for summarization, document drafting, and guided interaction.

## Coding Standards

- Use dependency injection and async APIs with cancellation tokens for I/O.
- Return structured results with success, warnings, and errors; avoid throwing for expected
  validation failures.
- Do not commit secrets. Never log PATs, tokens, connection strings, or Authorization
  headers; redact as `[REDACTED]`.
- Add unit tests for validation rules and document generation with missing fields.
