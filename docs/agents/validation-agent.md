# Validation Agent

## 1. Role

The Validation Agent evaluates release readiness using deterministic rules.

It does not rely on subjective AI judgment for pass/fail decisions.

## 2. Responsibilities

- Validate work item states
- Validate PR statuses
- Validate branch rules
- Validate application mapping
- Validate production deployment links
- Validate rollback candidates
- Validate approval status
- Classify findings
- Determine final readiness status

## 3. Input

```json
{
  "releaseId": "rel_001",
  "workItems": [],
  "pullRequests": [],
  "applications": [],
  "deployments": [],
  "rollbackCandidates": []
}
```

## 4. Output

```json
{
  "success": true,
  "agent": "ValidationAgent",
  "releaseId": "rel_001",
  "status": "Warning",
  "blockers": [],
  "warnings": [
    {
      "code": "ROLLBACK_MISSING",
      "message": "Notification Service is missing a rollback candidate.",
      "entityType": "Application",
      "entityId": "Notification Service"
    }
  ],
  "info": []
}
```

## 5. Readiness Statuses

- Ready
- Warning
- Blocked
- Incomplete
- Unknown

## 6. Rule Severity

- Info
- Warning
- Blocker
- Critical

## 7. Rules

See `docs/security/validation-rules.md` for full rules.

## 8. AI Usage

AI may explain validation results in natural language, but it must not override deterministic rule outcomes.
