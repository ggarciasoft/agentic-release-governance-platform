# AI Coding Agent Build Prompt

Use this prompt with an AI coding agent to start building the system.

```text
You are a senior software architect and full-stack engineer.

Build the AI Release Assistant for Azure DevOps using the documentation in this repository.

Primary stack:
- Backend: .NET 8 Web API
- Frontend: React or Next.js
- Database: PostgreSQL or SQL Server
- Background jobs: Hangfire
- AI: Azure OpenAI or OpenAI
- Auth: Microsoft Entra ID-ready architecture
- Secrets: Azure Key Vault-ready architecture

First implement the MVP only.

MVP requirements:
1. Create release records.
2. Configure application mappings.
3. Search Azure DevOps work items by Change Request tag.
4. Store work item snapshots.
5. Find pull requests linked to work items.
6. Store PR snapshots.
7. Find production deployment candidates using application mapping.
8. Find rollback candidates using latest successful production deployment before current candidate.
9. Run deterministic validation rules.
10. Generate Markdown release document from structured release package.
11. Expose REST APIs documented in docs/api/api-specification.md.
12. Use the database model in docs/database/database-model.md.
13. Implement the agent contracts in docs/agents.
14. Implement the custom MCP server structure in docs/mcp.

Important constraints:
- Do not trigger production deployments.
- Do not approve releases.
- Do not modify Azure DevOps work items.
- Do not invent missing data.
- Keep AI usage limited to summarization and document generation.
- Validation must be deterministic.
- All tool calls and agent runs must be auditable.

Recommended first tasks:
1. Create solution structure.
2. Add domain entities.
3. Add EF Core DbContext.
4. Add release CRUD endpoints.
5. Add application mapping endpoints.
6. Add Azure DevOps client interfaces.
7. Add validation engine.
8. Add Markdown document generator.
9. Add basic React UI.
10. Add MCP server project with initial tool contracts.

Ask before making major architectural changes that conflict with the documentation.
```
