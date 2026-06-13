# Testing Strategy

## 1. Goals

The AI Release Assistant makes release-readiness decisions, so testing focuses on
**determinism, traceability, and safety**:

- Validation rules produce the same result for the same input, every time.
- Agents and MCP tools never invent Azure DevOps data.
- Production-impacting actions are never performed automatically.
- Missing data is always surfaced explicitly.

## 2. Test Levels

### Unit Tests

Fast, isolated tests for business logic. Highest priority:

- **Validation engine** — every rule in
  [`../security/validation-rules.md`](../security/validation-rules.md) (`WI001`–`WI004`,
  `PR001`–`PR004`, `APP001`–`APP002`, `DEP001`–`DEP003`, `RB001`–`RB004`, `AP001`–`AP003`),
  including the final status-calculation priority.
- **Rollback candidate selection** — latest successful production deployment before the
  current candidate, matching application and environment.
- **Application mapping resolution**.
- **Release package assembly**.
- **Markdown document generation** — section presence and "Missing" handling.

### Integration Tests

Test components together with real infrastructure where practical:

- **Repository / EF Core** against a real database (Testcontainers or a disposable local DB).
- **API endpoints** ([`../api/api-specification.md`](../api/api-specification.md)) — release
  CRUD, attach endpoints, validate, package, document save.
- **Azure DevOps client** against recorded/mocked HTTP responses (no live calls in CI).

### MCP Contract Tests

For each of the 10 `release-governance` tools
([`../mcp/mcp-tool-contracts.md`](../mcp/mcp-tool-contracts.md)):

- Valid input returns the documented `data` shape inside the standard envelope.
- Invalid/missing input returns `success: false` with the correct error code.
- Write tools (`create_release_item`, `attach_*`, `save_release_document`) persist correctly.
- Read tools never mutate state.

### Agent Behavior Tests

Agents are probabilistic, so assert on **structure and safety**, not exact prose:

- Output conforms to the agent output envelope
  ([`../agents/agents-overview.md`](../agents/agents-overview.md)).
- No fabricated work items, PRs, deployment links, or rollback links appear without source
  data (verify every item carries an Azure DevOps URL from input).
- Empty data sets yield empty results plus an explicit warning.
- Agents never emit approval/deployment actions.

### End-to-End (Pilot) Test

One scripted run of the full end-to-end flow from a Change Request to a generated release
document, used as the pilot acceptance check
([MCP-First MVP Roadmap](../roadmap/mcp-first-mvp-roadmap.md) Phase 6).

## 3. Determinism Tests

Validation must be reproducible. For a fixed release package input, `validate_release` must
return identical status, blockers, and warnings across repeated runs. Snapshot/golden-file
tests are recommended for both validation results and generated documents.

## 4. Test Data and Fixtures

- Maintain a **sample release package** fixture covering Ready, Warning, Blocked, and
  Incomplete scenarios.
- Maintain recorded Azure DevOps responses (work items, PRs, deployments) for the ADO client.
- Cover the "missing data" cases explicitly: no work items, unmapped application, no
  deployment candidate, no rollback candidate.

## 5. Coverage Priorities

| Area | Priority | Rationale |
|---|---|---|
| Validation engine | Critical | Decides readiness; must be deterministic |
| Rollback discovery | Critical | Safety-relevant, easy to get subtly wrong |
| MCP tool contracts | High | Stable interface for agents across all hosts |
| Application mapping | High | Gates deployment/rollback discovery |
| Document generation | Medium | Output quality; "Missing" handling |
| Agent prose quality | Low | Asserted structurally, not by exact text |

## 6. CI Expectations

- Run `dotnet build` and `dotnet test` on every pull request.
- No live Azure DevOps calls in CI; use recorded responses.
- Fail the build on validation/document snapshot drift unless the snapshot is updated
  intentionally.
- Run secret scanning to enforce the "never log/store secrets" rule from
  [`../security/security-model.md`](../security/security-model.md).

## 7. Out of Scope for the MVP

- Load/performance testing.
- UI end-to-end tests (no UI in the MCP-first MVP).
- Testing production deployment/approval actions (not implemented in the MVP).
