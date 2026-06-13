---
description: Generate the official Markdown release document.
mode: agent
---

# Generate Release Document

Use the Release Document Agent.

## Input

- releaseId

## Rules

- Use only release package data.
- Do not invent missing values.
- Mark missing links as `Missing`.
- Include blockers and warnings.
- Save the document if the user requests it.

## Required Sections

```markdown
# Production Release Document

## Release Summary
## Change Request
## Applications Included
## Work Items Included
## Pull Requests Included
## Deployment Information
## Rollback Plan
## Validation Summary
## Blockers
## Warnings
## Missing Information
## Post-Deployment Checklist
## Approval Notes
```
