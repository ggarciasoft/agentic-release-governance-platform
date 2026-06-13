# Agentic Architecture

## 1. Purpose

The agentic architecture divides the release workflow into specialized agents. Each agent has a clear responsibility and uses tools exposed through MCP servers or internal services.

## 2. Agents vs MCPs

Agents are workers that reason and coordinate steps.

MCP servers are tool providers.

Do not create one MCP per agent. Create shared MCP servers that multiple agents can use.

## 3. Recommended Agents

### MVP Agents

1. Release Orchestrator Agent
2. Azure DevOps Analysis Agent
3. Validation Agent
4. Release Document Agent

### Future Agents

1. Work Item Agent
2. Pull Request Agent
3. Pipeline Agent
4. Rollback Agent
5. Communication Agent
6. Monitoring Agent
7. Compliance Agent

## 4. Agent Workflow

```text
Release Orchestrator Agent
  ↓
Work Item Agent
  ↓
Pull Request Agent
  ↓
Pipeline Agent
  ↓
Rollback Agent
  ↓
Validation Agent
  ↓
Document Agent
  ↓
Communication Agent
```

## 5. State Machine

Each release analysis should move through states:

```text
Created
CollectingWorkItems
CollectingPullRequests
CollectingDeployments
FindingRollbackCandidates
Validating
GeneratingDocument
ReadyForReview
Completed
Failed
```

## 6. Tool Usage Rules

Agents should:

- Use tools for factual data
- Store tool results
- Return structured output
- Mark missing data explicitly
- Avoid guessing

Agents should not:

- Generate fake IDs
- Generate fake URLs
- Claim deployment readiness without validation
- Execute high-risk actions without human confirmation

## 7. Human-in-the-Loop

The system must involve a human when:

- A release is blocked
- Critical data is missing
- Multiple deployments match
- A production action is requested
- A validation rule needs to be waived

## 8. Agent Memory

Agents should not rely on long-term memory for release facts. Release facts should come from:

- Database snapshots
- Azure DevOps data
- User input
- Validated tool output

## 9. Agent Output Contract

Every agent should return:

```json
{
  "success": true,
  "releaseId": "rel_001",
  "status": "Completed",
  "summary": "Analysis completed.",
  "data": {},
  "warnings": [],
  "errors": []
}
```

## 10. Failure Handling

If an agent fails:

1. Store the error.
2. Mark affected release sections as incomplete.
3. Continue if safe.
4. Stop if core release data is unavailable.
