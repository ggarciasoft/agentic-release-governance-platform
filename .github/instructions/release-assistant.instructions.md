---
applyTo: "**/*"
---

# Release Assistant Coding Instructions

When building this project, follow these instructions:

- Implement Copilot agents as `.agent.md` files under `.github/agents`.
- Implement the custom MCP server as a thin adapter over application services.
- Keep validation deterministic.
- Treat Azure DevOps as source of truth.
- Do not hardcode Azure DevOps organizations, projects, PATs, or pipeline names.
- Keep app-to-repo/pipeline mapping configurable.
- Every MCP tool must validate input and return structured errors.
- Every release document must be generated from a structured release package.
- Never edit release document Markdown files directly; use `save_release_document` or the API
  document endpoints to persist generated content.
- Missing data must be explicit, not hidden.
- Production-impacting actions require human confirmation and audit logs.
