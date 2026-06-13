# Agents Overview

## 1. Purpose

The system uses specialized agents to perform release analysis in a controlled and auditable way.

Each agent has a single responsibility and returns structured output.

## 2. MVP Agents

For the first version, implement these four agents (each has a spec under `docs/agents/`
and a Copilot agent file under `.github/agents/`):

1. [Release Orchestrator Agent](release-orchestrator-agent.md)
2. [Azure DevOps Analysis Agent](azure-devops-analysis-agent.md) — all-in-one collector
3. [Validation Agent](validation-agent.md)
4. [Release Document Agent](document-agent.md)

The MVP **Azure DevOps Analysis Agent** combines the work of the Work Item, Pull Request,
Pipeline, and Rollback agents so the MVP can ship with a smaller set.

## 3. Full Agent Set

For the mature version, the all-in-one collector is split into specialized agents:

1. [Release Orchestrator Agent](release-orchestrator-agent.md)
2. [Work Item Agent](work-item-agent.md)
3. [Pull Request Agent](pull-request-agent.md)
4. [Pipeline Agent](pipeline-agent.md)
5. [Rollback Agent](rollback-agent.md)
6. [Validation Agent](validation-agent.md)
7. [Release Document Agent](document-agent.md)
8. [Communication Agent](communication-agent.md)
9. Monitoring Agent — *future, not yet specified*
10. Compliance Agent — *future, not yet specified*

The Monitoring and Compliance agents are planned future capabilities. They do not yet have
agent specs or Copilot agent files; their intended scope is described in
[`../roadmap/future-roadmap.md`](../roadmap/future-roadmap.md) (post-deployment monitoring
and compliance/audit reporting).

## 4. Agent Responsibilities

| Agent | Responsibility | Status |
|---|---|---|
| Release Orchestrator Agent | Coordinates the full workflow | MVP |
| Azure DevOps Analysis Agent | Collects work items, PRs, deployments, and rollback candidates | MVP |
| Work Item Agent | Finds and analyzes work items | Future split |
| Pull Request Agent | Finds and analyzes linked PRs | Future split |
| Pipeline Agent | Finds deployments, pipeline status, and rollback candidates | Future split |
| Rollback Agent | Finds rollback candidates (folded into Pipeline Agent in the MVP) | Future split |
| Validation Agent | Runs deterministic readiness rules | MVP |
| Release Document Agent | Generates release documents | MVP |
| Communication Agent | Drafts release communications | Future |
| Monitoring Agent | Post-deployment monitoring | Future (unspecified) |
| Compliance Agent | Compliance and audit reporting | Future (unspecified) |

## 5. Agent Output Standard

Every agent should return structured data:

```json
{
  "success": true,
  "agent": "WorkItemAgent",
  "releaseId": "rel_001",
  "status": "Completed",
  "summary": "18 work items found.",
  "data": {},
  "warnings": [],
  "errors": []
}
```

## 6. Agent Rules

Agents must:

- Use tools for factual data
- Return structured output
- Mark missing data explicitly
- Preserve traceability
- Avoid assumptions
- Never invent Azure DevOps data

Agents must not:

- Approve production releases
- Trigger production deployments in MVP
- Modify work items in MVP
- Hide warnings or blockers
- Generate fake deployment or rollback links
