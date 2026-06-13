# Custom Release Governance MCP Server

## 1. Name

Recommended service name:

```text
ReleaseAssistant.McpServer
```

MCP server name (canonical):

```text
release-governance
```

## 2. Purpose

The Release Governance MCP exposes release-specific tools to agents.

It should not replace the Azure DevOps MCP Server. It should complement it with business-specific release logic.

## 3. Responsibilities

- Create and retrieve release records
- Retrieve application mappings
- Execute release analysis workflows
- Find current deployment candidates
- Find rollback candidates
- Run validation rules
- Generate release package JSON
- Save generated documents
- Return structured data to agents

## 4. Internal Dependencies

The MCP server should call internal services:

- ReleaseService
- ApplicationMappingService
- AzureDevOpsAnalysisService
- DeploymentDiscoveryService
- RollbackDiscoveryService
- ReleaseValidationService
- ReleasePackageService
- DocumentService

## 5. Recommended Project Structure

```text
src/ReleaseAssistant.McpServer/
  Tools/
    CreateReleaseItemTool.cs
    GetReleaseItemTool.cs
    GetApplicationMappingTool.cs
    AttachWorkItemsToReleaseTool.cs
    AttachPullRequestsToReleaseTool.cs
    AttachDeploymentsToReleaseTool.cs
    FindRollbackCandidatesTool.cs
    ValidateReleaseTool.cs
    GenerateReleasePackageTool.cs
    SaveReleaseDocumentTool.cs
  Security/
  Contracts/
  Program.cs
```

The tool set above matches the canonical 10 tools in
[`release-governance-mcp-server-spec.md`](release-governance-mcp-server-spec.md) and
[`mcp-tool-contracts.md`](mcp-tool-contracts.md).

## 6. Tool Response Standard

Every tool must return:

```json
{
  "success": true,
  "tool": "validate_release",
  "data": {},
  "warnings": [],
  "errors": []
}
```

## 7. Error Standard

```json
{
  "success": false,
  "tool": "validate_release",
  "data": null,
  "warnings": [],
  "errors": [
    {
      "code": "RELEASE_NOT_FOUND",
      "message": "Release rel_001 was not found."
    }
  ]
}
```

## 8. Security Requirements

- Validate user permissions before executing tools.
- Log tool calls.
- Do not return secrets.
- Do not allow production write actions in MVP.
- Use least privilege for Azure DevOps access.
- Use Key Vault for secrets.
- Use Entra ID authentication for production.

## 9. Audit Requirements

Every MCP tool call should log:

- Tool name
- User identity
- Release ID
- Input parameters, with secrets redacted
- Result status
- Timestamp
- Duration
- Error code, if any
