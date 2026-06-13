# Communication Agent

## 1. Role

The Communication Agent drafts release-related messages for Teams, Slack, email, or other communication channels.

This agent is optional for MVP.

## 2. Responsibilities

- Draft release review message
- Draft production deployment message
- Draft blocked release message
- Draft rollback readiness message
- Draft post-deployment completion message

## 3. Required Input

```json
{
  "release": {},
  "status": "Warning",
  "applications": [],
  "blockers": [],
  "warnings": [],
  "deploymentLinks": [],
  "rollbackLinks": []
}
```

## 4. Rules

- Do not say the release is complete unless deployment data confirms completion.
- Do not hide blockers or warnings.
- Keep messages concise and clear.
- Include next action.

## 5. Example Output

```text
Production release CR-12345 is ready for review.

Status: Warning
Applications: Payments API, Admin Portal

Warnings:
- Admin Portal is pending approval.
- Notification Service rollback link is missing.

Please review the generated release document before approval.
```
