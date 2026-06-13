# Copilot-First MVP Roadmap

## Objective

Build the first version of the AI Release Assistant so it can be operated from GitHub Copilot using custom agents and MCP tools.

## Phase 1: Repository Setup

Deliverables:

- `.github/copilot-instructions.md`
- `.github/agents/*.agent.md`
- `.github/prompts/*.prompt.md`
- Documentation for Copilot and MCP usage

Acceptance criteria:

- Copilot can detect the custom agents in the workspace.
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

- Copilot agents can call the MCP tools.
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

- Copilot can generate a release document from a releaseId.
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
