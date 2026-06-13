# Database Model

## 1. Release

Stores the main release record.

```text
Id
Name
ChangeRequest
Organization
Project
TargetEnvironment
Status
CreatedBy
CreatedAt
UpdatedAt
DeletedAt
```

## 2. ReleaseApplication

Stores applications included in the release.

```text
Id
ReleaseId
ApplicationName
RepositoryName
BuildDefinitionId
ReleaseDefinitionId
ProductionEnvironmentName
UatEnvironmentName
CreatedAt
UpdatedAt
```

## 3. ApplicationMapping

Stores reusable application-to-pipeline mapping.

```text
Id
ApplicationName
Organization
Project
RepositoryId
RepositoryName
BuildDefinitionId
BuildDefinitionName
ReleaseDefinitionId
ReleaseDefinitionName
ProductionEnvironmentName
UatEnvironmentName
IsActive
CreatedAt
UpdatedAt
```

## 4. ReleaseWorkItem

Stores snapshot of work items included in the release.

```text
Id
ReleaseId
AzureDevOpsWorkItemId
WorkItemType
Title
State
AssignedTo
TagsJson
AreaPath
IterationPath
Url
RawJson
CreatedAt
UpdatedAt
```

## 5. ReleasePullRequest

Stores snapshot of pull requests included in the release.

```text
Id
ReleaseId
AzureDevOpsPullRequestId
RepositoryId
RepositoryName
Title
Status
SourceBranch
TargetBranch
CreatedBy
CompletedBy
CreatedAtFromAzureDevOps
CompletedAtFromAzureDevOps
Url
RawJson
CreatedAt
UpdatedAt
```

## 6. ReleasePullRequestWorkItem

Join table between PRs and work items.

```text
Id
ReleasePullRequestId
ReleaseWorkItemId
AzureDevOpsWorkItemId
AzureDevOpsPullRequestId
```

## 7. ReleaseDeployment

Stores deployment candidate data.

```text
Id
ReleaseId
ApplicationName
AzureDevOpsReleaseId
AzureDevOpsReleaseName
ReleaseDefinitionId
ReleaseDefinitionName
EnvironmentName
EnvironmentStatus
DeploymentStatus
ApprovalStatus
DeploymentUrl
IsCurrentDeployment
StartedAt
CompletedAt
RawJson
CreatedAt
UpdatedAt
```

## 8. ReleaseRollbackCandidate

Stores rollback candidate data.

```text
Id
ReleaseId
ApplicationName
AzureDevOpsReleaseId
AzureDevOpsReleaseName
ReleaseDefinitionId
ReleaseDefinitionName
EnvironmentName
DeploymentStatus
RollbackUrl
CompletedAt
RawJson
CreatedAt
UpdatedAt
```

## 9. ReleaseValidationResult

Stores validation results.

```text
Id
ReleaseId
RuleCode
Severity
Status
Message
EntityType
EntityId
CreatedAt
```

## 10. ReleaseDocument

Stores generated release documents.

```text
Id
ReleaseId
Format
Content
Version
GeneratedBy
GeneratedAt
CreatedAt
UpdatedAt
```

## 11. AgentRun

Stores agent workflow execution.

```text
Id
ReleaseId
AgentName
Status
StartedAt
CompletedAt
InputJson
OutputJson
ErrorJson
CreatedAt
```

## 12. ToolCallLog

Stores tool calls for auditability.

```text
Id
ReleaseId
AgentRunId
ToolName
InputJson
OutputJson
Status
ErrorCode
StartedAt
CompletedAt
DurationMs
CreatedBy
```

## 13. ValidationRuleConfiguration

Stores configurable validation rules.

```text
Id
Organization
Project
RuleCode
IsEnabled
Severity
ConfigurationJson
CreatedAt
UpdatedAt
```

## 14. Index Recommendations

Create indexes on:

```text
Release.ChangeRequest
Release.Organization + Release.Project
ReleaseWorkItem.AzureDevOpsWorkItemId
ReleasePullRequest.AzureDevOpsPullRequestId
ReleaseDeployment.ApplicationName
ReleaseValidationResult.ReleaseId
ApplicationMapping.ApplicationName
ApplicationMapping.RepositoryName
```
