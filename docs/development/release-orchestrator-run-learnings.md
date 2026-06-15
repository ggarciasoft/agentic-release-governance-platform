# Release Orchestrator Run Learnings (CR-3, 2026-06-15)

Notes from an end-to-end release orchestration run for **CR-3 → Production**. The workflow
eventually completed, but several friction points slowed the first attempt. This document
captures what failed, what worked, and what to fix or configure before the next run.

## Goal

Run the full release-orchestrator workflow:

1. Create release item
2. Collect work items, PRs, deployments, rollback candidates from Azure DevOps
3. Validate readiness
4. Generate and save the release document

## Outcome

The workflow **did complete** for release `39ef38fa-5d51-4cc9-94df-58460512d0a4`:

| Step | Result |
|---|---|
| Create release | OK — CR-3 Production Release |
| Work items | OK — WI #3 (Task, Done, tag CR-3) |
| Pull requests | OK — PR #4 (completed → main) |
| Deployments | OK — Release-5 Production attached |
| Rollback | OK — Release-4 Production (succeeded) |
| Validation | **Warning** — DEP003 (`notStarted` on Production) |
| Document | OK — v1 saved as `release-CR-3-Production.md` |

Validation status **Warning** (not Blocked) because Production deployment on Release-5 has
not started yet. That is expected ADO state, not missing data.

---

## What Struggled (and Why)

### 1. Azure DevOps auth from the agent shell

**Symptom:** Direct REST calls from PowerShell returned sign-in HTML (HTTP 203) or 404.

**Causes:**

- `ADO_MCP_AUTH_TOKEN` and `AZURE_DEVOPS_EXT_PAT` were **not set** in the shell environment.
- `dotnet user-secrets get` is **not a valid command** on this SDK version — only `list` works.
  A failed `get` left a garbage string that looked like a PAT but was not.
- Reading PAT from `%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json` still failed in the
  agent shell (sign-in page), even though the **running API** (`dotnet run`) loads the same
  user secrets successfully.

**Lesson:** For local runs, treat the **Release Assistant API** as the trusted ADO client when
user secrets are configured. Do not assume the agent shell can call ADO with the same creds
without explicitly loading them.

**Fix for MCP-first runs:**

```powershell
# Set before starting Cursor / MCP servers
$env:ADO_MCP_AUTH_TOKEN = "<your-pat>"
```

Also set this in `.mcp.json` env (never commit the value) or Cursor MCP settings. The
`azure-devops` MCP server reads **`ADO_MCP_AUTH_TOKEN` only** — not `AZURE_DEVOPS_EXT_PAT`.

### 2. No backend “analyze” endpoints in MVP

The API exposes **attach** endpoints (`/work-items`, `/pull-requests`, `/deployments`) that
expect **already-collected** ADO data. There are no implemented `POST /analyze/*` endpoints.

**Lesson:** The orchestrator must **pull from Azure DevOps first** (via `azure-devops` MCP or
`AzureDevOpsClient`), then **push into** release-governance via attach tools or REST.

Trying to drive the whole flow through attach endpoints alone, without an ADO collection step,
cannot work.

### 3. Delegating to the release-orchestrator subagent worked

The parent agent struggled with shell ADO auth. Delegating to the **release-orchestrator**
subagent completed the workflow using the running API + user-secrets-backed ADO access.

**Lesson:** When the parent agent lacks MCP tools or shell ADO auth, delegate early to
`release-orchestrator` rather than retrying raw REST calls.

### 4. `GET /api/releases/{id}` returns HTTP 500 (JSON cycle)

After data was attached, `GET /api/releases/{releaseId}` failed with:

```text
JsonException: A possible object cycle was detected ...
Path: $.Applications.Release.Applications.Release...
```

**Cause:** EF navigation properties — `ReleaseApplication` holds a `Release` reference, which
holds `Applications` again — create a serialization cycle when returning the full entity.

**Workaround:** Use these endpoints instead (they work):

- `GET /api/releases/{id}/package`
- `GET /api/releases/{id}/validation-results`
- `GET /api/releases/{id}/documents`
- `POST /api/releases/{id}/documents/generate`

**Fix needed:** Return DTOs from `GetById`, add `[JsonIgnore]` on back-references, or configure
`ReferenceHandler.IgnoreCycles` in `Program.cs`.

### 5. `find_rollback_candidates` is simplified in MVP

`ReleaseService.FindRollbackCandidatesAsync` does **not** call Azure DevOps. It only returns
rollback rows already attached to the release.

**Lesson:** Rollback data must be **discovered from ADO and attached** (or passed to
`attach_rollback_candidates`) during the collection phase. Do not expect
`POST .../rollback-candidates/discover` to query ADO by itself.

### 6. MCP / API port alignment

`.mcp.json` sets `RELEASE_ASSISTANT_API_BASE_URL`. The API in this session listened on
**5050** (from launch profile). If MCP points at **5000**, all release-governance tools fail
silently or with connection errors.

**Lesson:** Confirm the API port before starting MCP:

```text
Now listening on: http://localhost:5050
```

Match that in `.cursor/mcp.json` / `.mcp.json` env.

### 7. Organization vs project in ADO URLs

ADO URLs in this org sometimes use the **project GUID** (`fb9ee94c-...`) instead of the
display name (`release-document`). Both are valid. Do not treat a GUID in a URL as fabricated
data.

---

## Recommended Runbook (Next Time)

### Prerequisites

1. API running: `dotnet run --project src/ReleaseAssistant.Api`
2. User secrets configured:
   - `AzureDevOps:Organization` → `ggarciasoftOutlook`
   - `AzureDevOps:Project` → `release-document`
   - `AzureDevOps:Pat` → valid PAT with Boards, Code, Release read scopes
3. MCP env: `ADO_MCP_AUTH_TOKEN` set; `RELEASE_ASSISTANT_API_BASE_URL` matches API port
4. Work items tagged with the Change Request value (e.g. `CR-3`)

### Orchestration order

```text
create_release_item
  → search work items by tag (azure-devops)
  → attach_work_items_to_release
  → find PRs linked to work items (azure-devops)
  → attach_pull_requests_to_release
  → find release/deployment candidates for Production (azure-devops)
  → attach_deployments_to_release
  → discover prior successful Production release (azure-devops)
  → attach_rollback_candidates (if not auto-attached)
  → validate_release
  → generate_release_package
  → save_release_document
```

### Verification commands

```powershell
$id = "<release-guid>"

Invoke-RestMethod "http://localhost:5050/api/releases/$id/package"
Invoke-RestMethod "http://localhost:5050/api/releases/$id/validation-results" -Method Get
Invoke-RestMethod "http://localhost:5050/api/releases/$id/documents/generate" `
  -Method Post -Body '{"format":"markdown"}' -ContentType "application/json"
```

Avoid `GET /api/releases/$id` until the JSON cycle bug is fixed.

---

## Configuration Reference (This Environment)

| Setting | Value |
|---|---|
| ADO organization | `ggarciasoftOutlook` |
| ADO project | `release-document` |
| Application mapping | `release-document` → build def 3, release def 1, env Production |
| API base URL | `http://localhost:5050` |
| User secrets ID | `9ec72c85-78b8-4f63-a223-c3913bf5f6de` (ReleaseAssistant.Api) |

---

## Follow-Up Fixes (Backlog)

1. **Fix `GET /api/releases/{id}`** — break EF navigation cycle in JSON responses.
2. **Implement ADO-backed rollback discovery** — or document that attach is required.
3. **Align docs** — `docs/hosts/cursor.md` shows port 5000; actual launch profile may use 5050.
4. **Optional analyze endpoints** — for non-agent (UI-driven) workflows per API spec “future”.
5. **Set `ADO_MCP_AUTH_TOKEN` in dev** — so azure-devops MCP works without shell workarounds.

---

## Key Takeaway

This platform is **MCP-first and agent-driven**: Azure DevOps collection happens outside the
attach/validate/document pipeline. Auth must be wired in **two places** for a smooth run — user
secrets for the API, and `ADO_MCP_AUTH_TOKEN` for the azure-devops MCP server. When shell ADO
calls fail, delegate to release-orchestrator or call the local API endpoints that already hold
the credentials.
