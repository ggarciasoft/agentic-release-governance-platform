# Security Model

## 1. Security Goals

The system must protect:

- Azure DevOps access
- Release data
- Deployment links
- Rollback links
- User identity
- Secrets and tokens
- Audit history

## 2. Authentication

Use Microsoft Entra ID for production.

MVP may use local authentication or organization SSO depending on company constraints.

## 3. Authorization

Recommended roles:

### Viewer

Can view releases and documents.

### Release Contributor

Can create releases and run analysis.

### Release Manager

Can generate documents and mark release documents as reviewed.

### Admin

Can configure application mappings and validation rules.

## 4. Secret Management

Store secrets in Azure Key Vault.

Never store or log:

- PAT tokens
- Access tokens
- Refresh tokens
- Client secrets
- Connection strings
- Private keys
- Raw authorization headers

## 5. Azure DevOps Permissions

MVP should use read-only permissions where possible.

Required read permissions:

- Work items read
- Code read
- Pull requests read
- Build read
- Release read
- Environment read

Avoid write permissions in MVP.

## 6. MCP Security

MCP tools must:

- Validate caller identity
- Validate caller permissions
- Log tool calls
- Redact secrets
- Return structured errors
- Use narrow tool scopes

## 7. Audit Logging

Log:

- Release creation
- Analysis start and completion
- Agent runs
- Tool calls
- Validation results
- Document generation
- Configuration changes

## 8. Data Retention

Keep release evidence according to organization policy.

Recommended minimum:

- Release metadata: 1 year
- Generated documents: 1 year
- Tool call logs: 90 days
- Error logs: 90 days

## 9. Production Safety

MVP must not:

- Trigger production deployments
- Approve production releases
- Modify work items
- Delete Azure DevOps resources
- Change pipeline configuration

## 10. Redaction Format

When secret-like values appear, redact them as:

```text
[REDACTED]
```
