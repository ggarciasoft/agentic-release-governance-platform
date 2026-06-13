# Validation Rules

## 1. Purpose

Validation rules determine release readiness. They must be deterministic and configurable.

AI may explain validation results but must not override them.

## 2. Readiness Status

Final release readiness must be one of:

- Ready
- Warning
- Blocked
- Incomplete
- Unknown

## 3. Severity Levels

Each rule result must have one severity:

- Info
- Warning
- Blocker
- Critical

## 4. Work Item Rules

### WI001 — Work Item Must Be In Ready State

Default ready states:

```text
UATDone
Ready for Release
Approved for Production
Closed
Resolved
Done
```

If a work item is not in a ready state, classify based on state.

### WI002 — Warning States

Default warning states:

```text
UATReady
In Testing
QA Ready
Ready for UAT
```

### WI003 — Blocking States

Default blocking states:

```text
New
Active
In Progress
Rejected
Removed
Blocked
```

### WI004 — Critical Bugs

Critical bugs must be closed, resolved, or explicitly waived.

## 5. Pull Request Rules

### PR001 — Pull Request Must Be Completed

A PR included in a production release should be completed.

### PR002 — Target Branch Must Be Allowed

Default allowed branches:

```text
main
master
release/*
```

### PR003 — Abandoned PRs Are Invalid

Abandoned PRs should be warnings or blockers depending on configuration.

### PR004 — Active PRs Are Not Production Ready

Active PRs should be treated as blockers unless the organization allows pre-merge releases.

## 6. Application Rules

### APP001 — Application Must Be Mapped

Each application must have a valid mapping to repository and pipeline information.

### APP002 — Repository Must Be Mapped

Each repository discovered from PRs should map to an application.

## 7. Deployment Rules

### DEP001 — Production Deployment Candidate Must Exist

Each application must have a production deployment candidate.

### DEP002 — Deployment Link Must Exist

Each production deployment candidate must have a URL.

### DEP003 — Deployment Status Must Be Valid

Allowed candidate statuses:

```text
Queued
PendingApproval
Waiting
InProgress
Ready
```

These statuses are configurable.

## 8. Rollback Rules

### RB001 — Rollback Candidate Must Exist

Each application must have a rollback candidate.

### RB002 — Rollback Candidate Must Be Successful

Rollback candidate must have succeeded in production.

### RB003 — Rollback Candidate Must Match Application

Rollback candidate must belong to the same application.

### RB004 — Rollback Candidate Must Match Environment

Rollback candidate must belong to the same target production environment.

## 9. Approval Rules

### AP001 — Approval Status Must Be Known

If approvals are required for production, approval status should be retrieved.

### AP002 — Rejected Approval Blocks Release

Rejected production approval should block release.

### AP003 — Pending Approval Is Warning

Pending approval is usually a warning unless configured as blocker.

## 10. Final Status Calculation

Use this priority:

1. If required input is missing: Incomplete
2. If critical or blocker result exists: Blocked
3. If warning result exists: Warning
4. If no issues: Ready
5. If data is inconsistent or unavailable: Unknown
