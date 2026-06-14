// ─────────────────────────────────────────────────────────────────────────────
// Types matching the Release Assistant API responses
// ─────────────────────────────────────────────────────────────────────────────

export type ReleaseStatus =
  | 'Created'
  | 'Analyzing'
  | 'AnalysisComplete'
  | 'ValidationComplete'
  | 'DocumentGenerated';

export type ReadinessStatus = 'Ready' | 'Warning' | 'Blocked' | 'Incomplete' | 'Unknown';
export type FindingSeverity = 'Info' | 'Warning' | 'Blocker' | 'Critical';

export interface ReleaseListItem {
  id: string;
  name: string;
  changeRequest: string;
  status: ReleaseStatus;
  createdAt: string;
}

export interface ReleaseApplication {
  id: string;
  applicationName: string;
  repositoryName: string;
  buildDefinitionId: number | null;
  releaseDefinitionId: number | null;
  productionEnvironmentName: string;
  uatEnvironmentName: string;
}

export interface ReleaseWorkItem {
  id: string;
  azureDevOpsWorkItemId: number;
  workItemType: string;
  title: string;
  state: string;
  assignedTo: string;
  tagsJson: string;
  areaPath: string;
  iterationPath: string;
  url: string;
}

export interface ReleasePullRequest {
  id: string;
  azureDevOpsPullRequestId: number;
  repositoryId: string;
  repositoryName: string;
  title: string;
  status: string;
  sourceBranch: string;
  targetBranch: string;
  createdBy: string;
  completedBy: string;
  createdAtFromAzureDevOps: string;
  completedAtFromAzureDevOps: string;
  url: string;
}

export interface ReleaseDeployment {
  id: string;
  applicationName: string;
  azureDevOpsReleaseId: number | null;
  azureDevOpsReleaseName: string;
  releaseDefinitionId: number | null;
  releaseDefinitionName: string;
  environmentName: string;
  environmentStatus: string;
  deploymentStatus: string;
  approvalStatus: string;
  deploymentUrl: string;
  isCurrentDeployment: boolean;
  startedAt: string | null;
  completedAt: string | null;
}

export interface ReleaseRollbackCandidate {
  id: string;
  applicationName: string;
  azureDevOpsReleaseId: number | null;
  azureDevOpsReleaseName: string;
  releaseDefinitionId: number | null;
  releaseDefinitionName: string;
  environmentName: string;
  deploymentStatus: string;
  rollbackUrl: string;
  completedAt: string | null;
}

export interface ReleaseValidationResult {
  id: string;
  ruleCode: string;
  severity: string;
  status: string;
  message: string;
  entityType: string;
  entityId: string | null;
}

export interface ReleaseDocument {
  id: string;
  version: number;
  format: string;
  generatedAt: string;
}

export interface ReleaseDetail {
  id: string;
  name: string;
  changeRequest: string;
  organization: string;
  project: string;
  targetEnvironment: string;
  status: ReleaseStatus;
  applications: ReleaseApplication[];
  workItems: ReleaseWorkItem[];
  pullRequests: ReleasePullRequest[];
  deployments: ReleaseDeployment[];
  rollbackCandidates: ReleaseRollbackCandidate[];
  validationResults: ReleaseValidationResult[];
  documents: ReleaseDocument[];
  createdAt: string;
  updatedAt: string;
}

export interface ReleasePackage {
  release: {
    id: string;
    name: string;
    changeRequest: string;
    organization: string;
    project: string;
    targetEnvironment: string;
    status: string;
  };
  applications: {
    applicationName: string;
    repositoryName: string;
    productionEnvironmentName: string;
  }[];
  workItems: {
    azureDevOpsWorkItemId: number;
    workItemType: string;
    title: string;
    state: string;
    assignedTo: string;
    url: string;
  }[];
  pullRequests: {
    azureDevOpsPullRequestId: number;
    repositoryName: string;
    title: string;
    status: string;
    targetBranch: string;
    url: string;
  }[];
  deployments: {
    applicationName: string;
    azureDevOpsReleaseName: string;
    environmentName: string;
    deploymentStatus: string;
    approvalStatus: string;
    deploymentUrl: string;
  }[];
  rollbackCandidates: {
    applicationName: string;
    azureDevOpsReleaseName: string;
    environmentName: string;
    deploymentStatus: string;
    rollbackUrl: string;
  }[];
  validations: {
    ruleCode: string;
    severity: string;
    message: string;
  }[];
}

export interface ApplicationMapping {
  id: string;
  applicationName: string;
  organization: string;
  project: string;
  repositoryId: string;
  repositoryName: string;
  buildDefinitionId: number | null;
  buildDefinitionName: string;
  releaseDefinitionId: number | null;
  releaseDefinitionName: string;
  productionEnvironmentName: string;
  uatEnvironmentName: string;
  isActive: boolean;
}

export interface ValidationSummary {
  status: ReadinessStatus;
  blockers: Finding[];
  warnings: Finding[];
  info: Finding[];
}

export interface Finding {
  code: string;
  message: string;
  severity: FindingSeverity;
  entityType?: string;
  entityId?: string;
}

export interface CreateReleaseRequest {
  releaseName: string;
  changeRequest: string;
  organization: string;
  project: string;
  targetEnvironment: string;
  applications: string[];
}
