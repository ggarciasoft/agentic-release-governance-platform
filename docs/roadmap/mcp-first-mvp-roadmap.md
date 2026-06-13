# MCP-First MVP Roadmap

## Objective

Build the first version of the AI Release Assistant so it can be operated from any
MCP-compatible host (GitHub Copilot, Cursor, or Claude) using host-native agent definitions
and shared MCP tools.

## Phase 1: Repository Setup

Deliverables:

- Host instructions: `.github/copilot-instructions.md`, `CLAUDE.md`,
  `.cursor/rules/release-assistant.mdc`
- Host agent definitions: `.github/agents/*.agent.md`, `.cursor/rules/*.mdc`,
  `.claude/agents/*.md`
- MCP config per host: VS Code `mcp.json`, `.cursor/mcp.json`, `.mcp.json`
- Host setup guides under `docs/hosts/`

Acceptance criteria:

- At least one host can detect the agents in the workspace; parity exists for all three.
- Agents have clear role instructions and tool constraints.

## Phase 2: Release Governance Backend

Deliverables:

- Release entity
- Application mapping entity
- Work item attachment model
- PR attachment model
- Deployment attachment model
- Validation result model
- Release document model

Acceptance criteria:

- Backend can create a release.
- Backend can store release artifacts collected by agents.

## Phase 3: Release Governance MCP Server

Deliverables:

- `create_release_item`
- `get_release_item`
- `get_application_mapping`
- `attach_work_items_to_release`
- `attach_pull_requests_to_release`
- `attach_deployments_to_release`
- `find_rollback_candidates`
- `validate_release`
- `generate_release_package`
- `save_release_document`

Acceptance criteria:

- Agents in any host can call the MCP tools.
- Tools return structured success/warning/error payloads.

## Phase 4: Azure DevOps Data Collection

Deliverables:

- Work item discovery by tag/CR
- Linked PR discovery
- Pipeline/release discovery
- Deployment candidate discovery

Acceptance criteria:

- A release package can include real Azure DevOps data.

## Phase 5: Validation and Document Generation

Deliverables:

- Deterministic validation rules
- Markdown document generator
- Document versioning

Acceptance criteria:

- An agent can generate a release document from a releaseId.
- Missing deployment/rollback data is clearly shown.

## Phase 6: Review and Hardening

Deliverables:

- Audit logging
- Secret redaction
- Permission model
- Tests
- Sample release package

Acceptance criteria:

- The system is safe for internal pilot usage.
