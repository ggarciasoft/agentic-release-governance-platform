# System Architecture

## 1. Overview

AI Release Assistant is composed of:

- Web UI
- Backend API
- Database
- Background job processor
- Agent orchestration layer
- Azure DevOps integration layer
- MCP integration layer
- Document generation module
- Security and audit module

## 2. High-Level Architecture

```text
User
  ↓
React / Next.js UI
  ↓
.NET 8 API
  ↓
Application Services
  ↓
┌────────────────────┬──────────────────────┬────────────────────┐
│ Agent Orchestrator │ Azure DevOps Client   │ Validation Engine  │
└────────────────────┴──────────────────────┴────────────────────┘
  ↓
┌────────────────────┬──────────────────────┬────────────────────┐
│ Azure DevOps MCP   │ Release Governance MCP│ Database           │
└────────────────────┴──────────────────────┴────────────────────┘
  ↓
Azure DevOps / Azure OpenAI / Storage / Monitoring
```

## 3. Backend Projects

Recommended .NET solution structure:

```text
src/
  ReleaseAssistant.Api/
  ReleaseAssistant.Application/
  ReleaseAssistant.Domain/
  ReleaseAssistant.Infrastructure/
  ReleaseAssistant.AzureDevOps/
  ReleaseAssistant.Agents/
  ReleaseAssistant.Documents/
  ReleaseAssistant.McpServer/
  ReleaseAssistant.Tests/
```

## 4. Frontend Structure

Recommended React or Next.js structure:

```text
web/
  app/
    releases/
    applications/
    settings/
  components/
  features/
    releases/
    analysis/
    documents/
    applications/
  lib/
  hooks/
  services/
```

## 5. Main Backend Modules

### Release Management Module

Responsible for release records, release status, and release lifecycle.

### Application Configuration Module

Responsible for application-to-repository and application-to-pipeline mapping.

### Azure DevOps Integration Module

Responsible for retrieving work items, PRs, builds, releases, deployments, and approvals.

### Agent Module

Responsible for coordinating specialized agents and tool calls.

### Validation Module

Responsible for deterministic release readiness checks.

### Document Module

Responsible for Markdown generation, document versioning, and exports.

### MCP Server Module

Responsible for exposing release-governance tools to agents.

## 6. Background Jobs

Use background jobs for long-running operations:

- Analyze release
- Refresh Azure DevOps data
- Generate document
- Export document
- Re-run validation

Recommended implementation:

- Hangfire for MVP
- Azure Functions for cloud-native later version

## 7. Data Flow

1. User creates release.
2. Backend stores release metadata.
3. User starts analysis.
4. Background job starts agent workflow.
5. Agents retrieve data from Azure DevOps through MCP or API client.
6. System stores snapshots of discovered data.
7. Validation engine evaluates readiness.
8. Document agent generates release document from structured data.
9. User reviews result.

## 8. AI Boundaries

The AI should not be responsible for final truth. The system should retrieve factual data and run deterministic validation first.

AI can:

- Summarize work items
- Explain risks
- Draft release notes
- Generate document text
- Draft communication messages

AI must not:

- Invent missing release links
- Override validation rules
- Approve releases
- Trigger deployments
- Hide blockers
