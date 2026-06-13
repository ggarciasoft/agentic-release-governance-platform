# Glossary

Shared vocabulary for the AI Release Assistant. Terms are grouped by area. Where a term has
a canonical name, alternative or deprecated names are noted.

## Core Domain

**Release / Release Item** — A governed record representing a planned production release. It
groups the applications, work items, pull requests, deployments, rollback candidates, and
validation results for one Change Request. Created via `create_release_item`.

**Change Request (CR)** — The business/ITSM identifier (for example `CR-12345`) that ties a
release to its scope. Work items are discovered by searching Azure DevOps for the CR value in
`System.Tags`. The CR tag is the primary input to a release analysis.

**Application** — A deployable unit (for example "Payments API"). Each application in a
release is expected to have an [application mapping](#configuration).

**Target Environment** — The environment a release is going to, typically `Production`. Used
to scope deployment and rollback discovery.

**Release Package** — The complete structured dataset for a release (release metadata,
applications, work items, pull requests, deployments, rollback candidates, validations) used
as the input for document generation. Produced by `generate_release_package`.

**Release Document** — The generated, versioned Markdown document that summarizes the release
for human review and approval. Persisted via `save_release_document`.

## Azure DevOps Concepts

**Work Item** — An Azure DevOps Boards item (User Story, Bug, Task, etc.). Relevant fields:
ID, type, title, state, assigned-to, tags, area/iteration path, URL, relations.

**Pull Request (PR)** — An Azure DevOps Repos pull request linked to release work items.
Relevant fields: PR ID, title, status (completed/active/abandoned), repository, source/target
branch, completion date, URL.

**Deployment Candidate** — The current production deployment for an application that the
release intends to promote (for example a release in `PendingApproval`). Discovered through
the application's mapped pipeline.

**Rollback Candidate** — The latest **successful** production deployment for an application
*before* the current deployment candidate, matching the same application and environment. Used
as the documented rollback target. Resolved by `find_rollback_candidates`.

**WIQL** — Work Item Query Language, used to search work items by CR tag.

**PAT** — Personal Access Token, the MVP authentication method for Azure DevOps. Production
uses Microsoft Entra ID.

## Readiness & Validation

**Readiness Status** — The final, deterministic outcome of validation for a release. One of:

- **Ready** — No issues found.
- **Warning** — Non-blocking issues exist; release may proceed with awareness.
- **Blocked** — A blocker or critical finding prevents release.
- **Incomplete** — Required input data is missing.
- **Unknown** — Data is inconsistent or unavailable.

**Finding Severity** — Severity assigned to each validation result: **Info**, **Warning**,
**Blocker**, or **Critical**.

**Validation Rule** — A deterministic, configurable check (for example `WI001`, `PR001`,
`DEP001`, `RB001`, `AP001`). AI may explain rule results but must never override them. Rule
codes are defined in [`security/validation-rules.md`](security/validation-rules.md).

**Waiver** — An explicit, recorded exception allowing an otherwise-failing condition (for
example a critical bug) to not block a release.

## Agents

**Agent** — A specialized AI role with a single responsibility and structured output. Defined
once as a host-neutral spec under `docs/agents/`, then expressed per host (`.github/agents/`,
`.cursor/rules/`, `.claude/agents/`).

**Agent Host** — An MCP-compatible client that runs agents (GitHub Copilot, Cursor, Claude,
or another MCP client). The host is interchangeable because the workflow is exposed through
MCP. See [Agent Hosts Overview](hosts/agent-hosts-overview.md).

**MCP-First** — The design principle that the release workflow is exposed through standard
MCP servers, so any MCP host can drive it without vendor lock-in.

**Release Orchestrator Agent** — Coordinates the end-to-end release workflow.

**Azure DevOps Analysis Agent** — MVP all-in-one collector for work items, PRs, deployments,
and rollback candidates. Splits into the Work Item, Pull Request, Pipeline, and Rollback
agents in the future topology.

**Validation Agent** — Runs deterministic readiness rules.

**Release Document Agent** — Generates the release document. (Deprecated names: "Document
Agent", "Document Generation Agent".)

**Communication Agent** — Drafts (never sends) release communications. Future/optional in MVP.

**Handoff** — The ordered passing of work from one agent to the next.

## MCP & Integration

**MCP (Model Context Protocol)** — The protocol that lets agents call tools safely and
consistently.

**`azure-devops` MCP server** — The existing/generic MCP server used to read raw Azure DevOps
data (work items, PRs, repos, builds).

**`release-governance` MCP server** — The custom MCP server for this project, exposing
release-specific tools. (Deprecated name: `release-governance-mcp`.) Implemented by the
`ReleaseAssistant.McpServer` project as a thin adapter over application services. Its 10
canonical tools are defined in
[`mcp/release-governance-mcp-server-spec.md`](mcp/release-governance-mcp-server-spec.md).

**Thin MCP** — The design principle that MCP tools delegate to application services rather
than containing business logic.

**Tool** — A capability exposed by an MCP server (for example `validate_release`). Referenced
from agents with a server-namespace prefix, for example `release-governance/validate_release`.

## Configuration

**Application Mapping** — Configuration linking an application to its repository, build/release
pipeline definitions, and production/UAT environment names. Required for deployment and
rollback discovery. Retrieved via `get_application_mapping`.

**Validation Rule Configuration** — Per-project configuration that enables/disables and tunes
validation rules (allowed states, branches, statuses, etc.).

## Platform & Operations

**Audit Log** — The record of release actions, agent runs, and tool calls (with secrets
redacted) for traceability.

**Snapshot** — Stored point-in-time copy of discovered Azure DevOps data (work items, PRs,
deployments) attached to a release, so a release reflects the data as collected.

**Human-in-the-loop** — The requirement that production-impacting decisions (approval,
deployment) require explicit human confirmation; agents never perform them in the MVP.

## Deprecated / Renamed Terms

| Deprecated | Canonical |
|---|---|
| `release-governance-mcp` | `release-governance` |
| Document Agent / Document Generation Agent | Release Document Agent |
| `create_release` (MCP tool) | `create_release_item` |
| `analyze_release_scope`, `find_current_deployments` (MCP tools) | Replaced by `attach_*` tools + `azure-devops` MCP collection |
