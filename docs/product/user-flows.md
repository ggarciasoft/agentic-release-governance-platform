# User Flows

## Flow 1 — Create Release

1. User opens the AI Release Assistant.
2. User clicks `Create Release`.
3. User enters release metadata:
   - Release name
   - Change Request tag
   - Azure DevOps organization
   - Azure DevOps project
   - Target environment
   - Applications
4. System creates release record.
5. System shows release detail page.

## Flow 2 — Analyze Release

1. User opens a release.
2. User clicks `Analyze Release`.
3. Release Orchestrator Agent starts workflow.
4. Work Item Agent searches Azure DevOps work items by tag.
5. Pull Request Agent finds linked pull requests.
6. Pipeline Agent finds deployment candidates.
7. Rollback Agent finds rollback candidates.
8. Validation Agent runs rules.
9. System displays readiness status.

## Flow 3 — Review Release Readiness

1. User opens the release analysis view.
2. System displays:
   - Release status
   - Work items found
   - Pull requests found
   - Applications found
   - Deployment links found
   - Rollback links found
3. User reviews blockers and warnings.
4. User fixes missing information or accepts known risks outside the system.
5. User re-runs analysis.

## Flow 4 — Generate Document

1. User opens analyzed release.
2. User clicks `Generate Document`.
3. Release Document Agent receives structured release package.
4. Agent generates Markdown release document.
5. System saves document version.
6. User previews document.
7. User copies or downloads document.

## Flow 5 — Missing Application Mapping

1. System finds a repository that is not mapped to an application.
2. Validation status becomes Warning or Incomplete.
3. System displays unmapped repository.
4. User creates application mapping.
5. User re-runs analysis.

## Flow 6 — Missing Rollback Candidate

1. System analyzes deployments.
2. Rollback Agent cannot find latest successful production deployment.
3. Validation Agent marks missing rollback candidate.
4. System displays warning or blocker based on configuration.
5. User reviews risk.

## Flow 7 — Blocked Release

1. System finds one or more blocking issues.
2. Readiness status becomes Blocked.
3. System displays blockers clearly.
4. Document can still be generated, but it must include the Blocked status and blockers.
5. System must not claim the release is ready.
