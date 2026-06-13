# MVP Roadmap (Legacy — UI-First)

> **Superseded.** This UI-first, agents-last roadmap is **not** the current plan. The
> canonical MVP plan is the [Copilot-First MVP Roadmap](copilot-first-mvp-roadmap.md), which
> builds the Copilot agents and MCP server first and treats the web UI as optional/future
> (see [`../product/updated-requirements-copilot-agents.md`](../product/updated-requirements-copilot-agents.md)).
>
> This document is retained as a reference for the alternative server-driven, UI-led approach
> that may inform the future web dashboard. Where the two roadmaps conflict (build order,
> Phase 1 web UI, agents in the final phase), the Copilot-first roadmap wins.

## Phase 1 — Foundation

### Goals

Create the basic application structure.

### Tasks

- Create .NET solution
- Create React or Next.js app
- Create database project setup
- Add release entity
- Add application mapping entity
- Add initial migrations
- Add release CRUD API
- Add application mapping CRUD API
- Add basic UI shell

### Deliverable

User can create a release and configure application mappings.

## Phase 2 — Azure DevOps Work Item Integration

### Tasks

- Add Azure DevOps authentication configuration
- Add work item search by tag
- Add work item detail retrieval
- Store work item snapshots
- Display work items in UI

### Deliverable

User can analyze a release and see related work items.

## Phase 3 — Pull Request Discovery

### Tasks

- Retrieve PRs linked to work items
- Store PR snapshots
- Display PRs in UI
- Add basic PR validation

### Deliverable

User can see PRs included in the release.

## Phase 4 — Deployment and Rollback Discovery

### Tasks

- Use application mapping to resolve pipelines
- Find current production deployment candidate
- Find latest successful production deployment as rollback
- Store deployment snapshots
- Display deployment and rollback links

### Deliverable

User can see deploy and rollback links per application.

## Phase 5 — Validation Engine

### Tasks

- Implement validation rules
- Add rule configuration
- Store validation results
- Calculate final readiness status
- Display blockers and warnings

### Deliverable

User can see Ready, Warning, Blocked, Incomplete, or Unknown status.

## Phase 6 — Document Generation

### Tasks

- Build release package JSON
- Generate Markdown document
- Save document version
- Preview document in UI
- Download Markdown

### Deliverable

User can generate a release document.

## Phase 7 — Agent and MCP Layer

### Tasks

- Implement Release Orchestrator Agent
- Implement Azure DevOps Analysis Agent
- Implement Validation Agent
- Implement Document Agent
- Implement custom MCP server tools
- Log agent runs and tool calls

### Deliverable

The system behaves like an agentic release assistant.
