# Release Document Agent

> Canonical name: **Release Document Agent**. The Copilot agent file is
> [`.github/agents/release-document-agent.agent.md`](../../.github/agents/release-document-agent.agent.md)
> and the structured output `agent` value is `ReleaseDocumentAgent`. Earlier drafts called
> this the "Document Agent" / "Document Generation Agent"; those names are deprecated.

## 1. Role

The Release Document Agent generates release documents from structured release data.

## 2. Responsibilities

- Generate Markdown release document
- Summarize release changes
- Include work items
- Include PRs
- Include applications
- Include deployment links
- Include rollback links
- Include validation summary
- Include blockers and warnings
- Include post-deployment checklist
- Avoid hallucination

## 3. Required Input

The Release Document Agent must receive a structured release package.

```json
{
  "release": {},
  "applications": [],
  "workItems": [],
  "pullRequests": [],
  "deployments": [],
  "rollbackCandidates": [],
  "validations": []
}
```

## 4. Document Structure

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
## Risks
## Post-Deployment Checklist
## Approval Notes
## Appendix
```

## 5. Rules

- Use only provided data.
- Do not invent missing links.
- Mark missing data as Missing.
- Include validation status.
- Include all blockers.
- Include all warnings.
- Use professional release-management language.

## 6. Output

```json
{
  "success": true,
  "agent": "ReleaseDocumentAgent",
  "releaseId": "rel_001",
  "format": "markdown",
  "content": "# Production Release Document...",
  "warnings": [],
  "errors": []
}
```
