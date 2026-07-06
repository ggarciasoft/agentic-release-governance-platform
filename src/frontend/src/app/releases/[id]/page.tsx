'use client';

import { useEffect, useState, useCallback } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { api } from '@/lib/api';
import type { ReleaseDetail, ValidationSummary } from '@/lib/types';

type Tab = 'overview' | 'work-items' | 'pull-requests' | 'deployments' | 'rollback' | 'validation' | 'documents';

const tabs: { key: Tab; label: string }[] = [
  { key: 'overview', label: 'Overview' },
  { key: 'work-items', label: 'Work Items' },
  { key: 'pull-requests', label: 'Pull Requests' },
  { key: 'deployments', label: 'Deployments' },
  { key: 'rollback', label: 'Rollback' },
  { key: 'validation', label: 'Validation' },
  { key: 'documents', label: 'Documents' },
];

const statusBadge = (status: string) => {
  const colors: Record<string, string> = {
    Ready: 'bg-emerald-600',
    Warning: 'bg-amber-600',
    Blocked: 'bg-red-600',
    Incomplete: 'bg-gray-600',
    Unknown: 'bg-gray-600',
    Created: 'bg-gray-600',
    Analyzing: 'bg-blue-600',
    AnalysisComplete: 'bg-cyan-600',
    ValidationComplete: 'bg-amber-600',
    DocumentGenerated: 'bg-emerald-600',
  };
  return (
    <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-medium text-white ${colors[status] || 'bg-gray-600'}`}>
      {status}
    </span>
  );
};

export default function ReleaseDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [release, setRelease] = useState<ReleaseDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<Tab>('overview');
  const [validating, setValidating] = useState(false);
  const [validationResult, setValidationResult] = useState<ValidationSummary | null>(null);
  const [generatingDoc, setGeneratingDoc] = useState(false);
  const [markdown, setMarkdown] = useState<string | null>(null);

  const fetchRelease = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await api.releases.getById(id);
      setRelease(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load release');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => { fetchRelease(); }, [fetchRelease]);

  const handleValidate = async () => {
    setValidating(true);
    try {
      const summary = await api.releases.validate(id);
      setValidationResult(summary);
      setActiveTab('validation');
      await fetchRelease();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Validation failed');
    } finally {
      setValidating(false);
    }
  };

  const handleGenerateDoc = async () => {
    setGeneratingDoc(true);
    try {
      const md = await api.releases.generateDocument(id);
      setMarkdown(md);
      setActiveTab('documents');
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to generate document');
    } finally {
      setGeneratingDoc(false);
    }
  };

  if (loading) {
    return <div className="text-gray-400 text-sm">Loading release…</div>;
  }

  if (error || !release) {
    return (
      <div className="p-8 text-center">
        <p className="text-red-400 mb-4">{error || 'Release not found'}</p>
        <Link href="/" className="text-indigo-400 hover:text-indigo-300 text-sm">← Back to Dashboard</Link>
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div className="flex items-start justify-between mb-6">
        <div>
          <Link href="/" className="text-gray-500 hover:text-gray-400 text-sm mb-2 inline-block">
            ← Dashboard
          </Link>
          <h1 className="text-2xl font-bold text-white">{release.name}</h1>
          <div className="flex items-center gap-3 mt-2 text-sm text-gray-400">
            <span className="font-mono text-xs">{release.changeRequest}</span>
            <span>·</span>
            <span>{release.organization}/{release.project}</span>
            <span>·</span>
            <span>{release.targetEnvironment}</span>
            <span>·</span>
            {statusBadge(release.status)}
          </div>
        </div>
        <div className="flex gap-2">
          <button
            onClick={handleValidate}
            disabled={validating}
            className="px-3 py-1.5 bg-amber-700 hover:bg-amber-600 disabled:opacity-50 text-white text-xs font-medium rounded-lg transition-colors"
          >
            {validating ? 'Validating…' : 'Validate'}
          </button>
          <button
            onClick={handleGenerateDoc}
            disabled={generatingDoc}
            className="px-3 py-1.5 bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white text-xs font-medium rounded-lg transition-colors"
          >
            {generatingDoc ? 'Generating…' : 'Generate Doc'}
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 border-b border-gray-800 mb-6 overflow-x-auto">
        {tabs.map((t) => (
          <button
            key={t.key}
            onClick={() => setActiveTab(t.key)}
            className={`px-4 py-2 text-sm font-medium whitespace-nowrap transition-colors border-b-2 -mb-px ${
              activeTab === t.key
                ? 'text-indigo-400 border-indigo-400'
                : 'text-gray-500 border-transparent hover:text-gray-300'
            }`}
          >
            {t.label}
            {t.key === 'work-items' && release.workItems.length > 0 && (
              <span className="ml-1.5 text-xs text-gray-500">({release.workItems.length})</span>
            )}
            {t.key === 'pull-requests' && release.pullRequests.length > 0 && (
              <span className="ml-1.5 text-xs text-gray-500">({release.pullRequests.length})</span>
            )}
            {t.key === 'deployments' && release.deployments.length > 0 && (
              <span className="ml-1.5 text-xs text-gray-500">({release.deployments.length})</span>
            )}
          </button>
        ))}
      </div>

      {/* Tab content */}
      <div className="min-h-[400px]">
        {activeTab === 'overview' && <OverviewTab release={release} />}
        {activeTab === 'work-items' && <WorkItemsTab items={release.workItems} />}
        {activeTab === 'pull-requests' && <PullRequestsTab prs={release.pullRequests} />}
        {activeTab === 'deployments' && <DeploymentsTab deployments={release.deployments} />}
        {activeTab === 'rollback' && <RollbackTab candidates={release.rollbackCandidates} />}
        {activeTab === 'validation' && (
          <ValidationTab
            results={release.validationResults}
            summary={validationResult}
          />
        )}
        {activeTab === 'documents' && (
          <DocumentsTab
            documents={release.documents}
            markdown={markdown}
            releaseId={id}
          />
        )}
      </div>
    </div>
  );
}

// ── Tab Components ────────────────────────────────────────────────────────────

function EmptyState({ message }: { message: string }) {
  return <p className="text-gray-600 text-sm py-8 text-center">{message}</p>;
}

function OverviewTab({ release }: { release: ReleaseDetail }) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      <Card title="Release Info">
        <dl className="space-y-2 text-sm">
          <KV label="ID" value={release.id} />
          <KV label="Organization" value={release.organization} />
          <KV label="Project" value={release.project} />
          <KV label="Target" value={release.targetEnvironment} />
          <KV label="Created" value={new Date(release.createdAt).toLocaleString()} />
          <KV label="Updated" value={new Date(release.updatedAt).toLocaleString()} />
        </dl>
      </Card>

      <Card title="Applications">
        {release.applications.length === 0 ? (
          <EmptyState message="No applications" />
        ) : (
          <ul className="space-y-2">
            {release.applications.map((app) => (
              <li key={app.id} className="text-sm bg-gray-900 rounded-lg p-3">
                <div className="font-medium text-gray-200">{app.applicationName}</div>
                <div className="text-gray-500 text-xs mt-1">
                  {app.repositoryName && <span>Repo: {app.repositoryName}</span>}
                  {app.productionEnvironmentName && (
                    <span className="ml-3">Env: {app.productionEnvironmentName}</span>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </Card>

      <Card title="Counts">
        <dl className="space-y-2 text-sm">
          <KV label="Work Items" value={String(release.workItems.length)} />
          <KV label="Pull Requests" value={String(release.pullRequests.length)} />
          <KV label="Deployments" value={String(release.deployments.length)} />
          <KV label="Rollback Candidates" value={String(release.rollbackCandidates.length)} />
          <KV label="Documents" value={String(release.documents.length)} />
        </dl>
      </Card>
    </div>
  );
}

function WorkItemsTab({ items }: { items: ReleaseDetail['workItems'] }) {
  if (items.length === 0) return <EmptyState message="No work items attached" />;
  return (
    <div className="overflow-hidden rounded-xl border border-gray-800">
      <table className="w-full text-sm">
        <thead>
          <tr className="bg-gray-900 text-gray-400 text-left">
            <th className="px-4 py-3 font-medium">ID</th>
            <th className="px-4 py-3 font-medium">Type</th>
            <th className="px-4 py-3 font-medium">Title</th>
            <th className="px-4 py-3 font-medium">State</th>
            <th className="px-4 py-3 font-medium">Assigned To</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-800">
          {items.map((wi) => (
            <tr key={wi.id} className="hover:bg-gray-900/50">
              <td className="px-4 py-3 font-mono text-xs text-indigo-400">
                {wi.url ? <a href={wi.url} target="_blank" rel="noreferrer" className="hover:underline">#{wi.azureDevOpsWorkItemId}</a> : `#${wi.azureDevOpsWorkItemId}`}
              </td>
              <td className="px-4 py-3 text-gray-400 text-xs">{wi.workItemType}</td>
              <td className="px-4 py-3 text-gray-200 max-w-md truncate">{wi.title}</td>
              <td className="px-4 py-3">{statusBadge(wi.state)}</td>
              <td className="px-4 py-3 text-gray-500 text-xs">{wi.assignedTo}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function PullRequestsTab({ prs }: { prs: ReleaseDetail['pullRequests'] }) {
  if (prs.length === 0) return <EmptyState message="No pull requests attached" />;
  return (
    <div className="overflow-hidden rounded-xl border border-gray-800">
      <table className="w-full text-sm">
        <thead>
          <tr className="bg-gray-900 text-gray-400 text-left">
            <th className="px-4 py-3 font-medium">PR #</th>
            <th className="px-4 py-3 font-medium">Title</th>
            <th className="px-4 py-3 font-medium">Repository</th>
            <th className="px-4 py-3 font-medium">Status</th>
            <th className="px-4 py-3 font-medium">Branch</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-800">
          {prs.map((pr) => (
            <tr key={pr.id} className="hover:bg-gray-900/50">
              <td className="px-4 py-3 font-mono text-xs text-indigo-400">
                {pr.url ? <a href={pr.url} target="_blank" rel="noreferrer" className="hover:underline">#{pr.azureDevOpsPullRequestId}</a> : `#${pr.azureDevOpsPullRequestId}`}
              </td>
              <td className="px-4 py-3 text-gray-200 max-w-md truncate">{pr.title}</td>
              <td className="px-4 py-3 text-gray-500 text-xs">{pr.repositoryName}</td>
              <td className="px-4 py-3">{statusBadge(pr.status)}</td>
              <td className="px-4 py-3 text-gray-500 text-xs font-mono">{pr.targetBranch}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function DeploymentsTab({ deployments }: { deployments: ReleaseDetail['deployments'] }) {
  const current = deployments.filter((d) => d.isCurrentDeployment);
  const history = deployments.filter((d) => !d.isCurrentDeployment);

  if (deployments.length === 0) return <EmptyState message="No deployments attached" />;

  return (
    <div className="space-y-8">
      {current.length > 0 && (
        <div>
          <h3 className="text-sm font-semibold text-gray-300 mb-3">Current Deployments</h3>
          <DeployTable deployments={current} />
        </div>
      )}
      {history.length > 0 && (
        <div>
          <h3 className="text-sm font-semibold text-gray-500 mb-3">Previous Deployments</h3>
          <DeployTable deployments={history} />
        </div>
      )}
    </div>
  );
}

function DeployTable({ deployments }: { deployments: ReleaseDetail['deployments'] }) {
  return (
    <div className="overflow-hidden rounded-xl border border-gray-800">
      <table className="w-full text-sm">
        <thead>
          <tr className="bg-gray-900 text-gray-400 text-left">
            <th className="px-4 py-3 font-medium">App</th>
            <th className="px-4 py-3 font-medium">Release</th>
            <th className="px-4 py-3 font-medium">Environment</th>
            <th className="px-4 py-3 font-medium">Status</th>
            <th className="px-4 py-3 font-medium">Approval</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-800">
          {deployments.map((d) => (
            <tr key={d.id} className="hover:bg-gray-900/50">
              <td className="px-4 py-3 font-medium text-gray-200">{d.applicationName}</td>
              <td className="px-4 py-3 text-gray-400 text-xs">
                {d.deploymentUrl ? (
                  <a href={d.deploymentUrl} target="_blank" rel="noreferrer" className="text-indigo-400 hover:underline">{d.azureDevOpsReleaseName}</a>
                ) : d.azureDevOpsReleaseName}
              </td>
              <td className="px-4 py-3 text-gray-500 text-xs">{d.environmentName}</td>
              <td className="px-4 py-3">{statusBadge(d.deploymentStatus)}</td>
              <td className="px-4 py-3 text-gray-500 text-xs">{d.approvalStatus || '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function RollbackTab({ candidates }: { candidates: ReleaseDetail['rollbackCandidates'] }) {
  if (candidates.length === 0) return <EmptyState message="No rollback candidates found" />;
  return (
    <div className="overflow-hidden rounded-xl border border-gray-800">
      <table className="w-full text-sm">
        <thead>
          <tr className="bg-gray-900 text-gray-400 text-left">
            <th className="px-4 py-3 font-medium">App</th>
            <th className="px-4 py-3 font-medium">Release</th>
            <th className="px-4 py-3 font-medium">Environment</th>
            <th className="px-4 py-3 font-medium">Status</th>
            <th className="px-4 py-3 font-medium">Completed</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-800">
          {candidates.map((c) => (
            <tr key={c.id} className="hover:bg-gray-900/50">
              <td className="px-4 py-3 font-medium text-gray-200">{c.applicationName}</td>
              <td className="px-4 py-3 text-gray-400 text-xs">
                {c.rollbackUrl ? (
                  <a href={c.rollbackUrl} target="_blank" rel="noreferrer" className="text-indigo-400 hover:underline">{c.azureDevOpsReleaseName}</a>
                ) : c.azureDevOpsReleaseName}
              </td>
              <td className="px-4 py-3 text-gray-500 text-xs">{c.environmentName}</td>
              <td className="px-4 py-3">{statusBadge(c.deploymentStatus)}</td>
              <td className="px-4 py-3 text-gray-500 text-xs">
                {c.completedAt ? new Date(c.completedAt).toLocaleString() : '—'}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function ValidationTab({
  results,
  summary,
}: {
  results: ReleaseDetail['validationResults'];
  summary: ValidationSummary | null;
}) {
  if (results.length === 0 && !summary) {
    return (
      <div className="text-center py-8">
        <p className="text-gray-600 text-sm mb-4">Not validated yet</p>
      </div>
    );
  }

  const displayResults = summary
    ? [
        ...summary.blockers.map((b) => ({ ...b, severity: 'Blocker' as const })),
        ...summary.warnings.map((w) => ({ ...w, severity: 'Warning' as const })),
        ...summary.info.map((i) => ({ ...i, severity: 'Info' as const })),
      ]
    : results.map((r) => ({
        code: r.ruleCode,
        message: r.message,
        severity: r.severity as 'Blocker' | 'Warning' | 'Info' | 'Critical',
      }));

  const severityIcon = (s: string) => {
    switch (s) {
      case 'Blocker':
      case 'Critical':
        return '🔴';
      case 'Warning':
        return '🟡';
      case 'Info':
        return '🔵';
      default:
        return '⚪';
    }
  };

  return (
    <div>
      {summary && (
        <div className={`mb-6 p-4 rounded-lg text-sm font-medium ${
          summary.status === 'Ready'
            ? 'bg-emerald-900/30 border border-emerald-700 text-emerald-300'
            : summary.status === 'Warning'
              ? 'bg-amber-900/30 border border-amber-700 text-amber-300'
              : 'bg-red-900/30 border border-red-700 text-red-300'
        }`}>
          Readiness: {summary.status}
          {summary.blockers.length > 0 && ` — ${summary.blockers.length} blocker(s)`}
          {summary.warnings.length > 0 && ` — ${summary.warnings.length} warning(s)`}
        </div>
      )}

      <div className="space-y-2">
        {displayResults.map((r, i) => (
          <div
            key={i}
            className="flex items-start gap-3 p-3 rounded-lg bg-gray-900/50 border border-gray-800 text-sm"
          >
            <span className="mt-0.5">{severityIcon(r.severity)}</span>
            <div>
              <span className="font-mono text-xs text-gray-500 mr-2">{r.code}</span>
              <span className="text-gray-200">{r.message}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function DocumentsTab({
  documents,
  markdown,
  releaseId,
}: {
  documents: ReleaseDetail['documents'];
  markdown: string | null;
  releaseId: string;
}) {
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);

  const handleSave = async () => {
    if (!markdown) return;
    setSaving(true);
    try {
      await api.releases.saveDocument(releaseId, 'markdown', markdown);
      setSaved(true);
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to save document');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-8">
      {/* Generated document preview */}
      {markdown && (
        <div>
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-sm font-semibold text-gray-300">Generated Document</h3>
            <button
              onClick={handleSave}
              disabled={saving || saved}
              className="px-3 py-1.5 bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white text-xs font-medium rounded-lg transition-colors"
            >
              {saved ? '✓ Saved' : saving ? 'Saving…' : 'Save Document'}
            </button>
          </div>
          <div className="p-5 bg-gray-900 border border-gray-800 rounded-lg max-h-[600px] overflow-y-auto prose prose-invert prose-sm max-w-none">
            <ReactMarkdown remarkPlugins={[remarkGfm]}>{markdown}</ReactMarkdown>
          </div>
        </div>
      )}

      {/* Saved documents */}
      {documents.length > 0 && (
        <div>
          <h3 className="text-sm font-semibold text-gray-300 mb-3">Saved Documents</h3>
          <div className="overflow-hidden rounded-xl border border-gray-800">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-900 text-gray-400 text-left">
                  <th className="px-4 py-3 font-medium">Version</th>
                  <th className="px-4 py-3 font-medium">Format</th>
                  <th className="px-4 py-3 font-medium">Generated</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-800">
                {documents.map((doc) => (
                  <tr key={doc.id} className="hover:bg-gray-900/50">
                    <td className="px-4 py-3 font-mono text-xs text-gray-200">v{doc.version}</td>
                    <td className="px-4 py-3 text-gray-500 text-xs">{doc.format}</td>
                    <td className="px-4 py-3 text-gray-500 text-xs">
                      {new Date(doc.generatedAt).toLocaleString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {!markdown && documents.length === 0 && (
        <EmptyState message="No documents yet. Click 'Generate Doc' to create one." />
      )}
    </div>
  );
}

// ── Shared helpers ────────────────────────────────────────────────────────────

function Card({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="bg-gray-900/50 border border-gray-800 rounded-xl p-5">
      <h3 className="text-sm font-semibold text-gray-300 mb-4">{title}</h3>
      {children}
    </div>
  );
}

function KV({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between">
      <dt className="text-gray-500">{label}</dt>
      <dd className="text-gray-200 font-mono text-xs truncate max-w-[60%]">{value}</dd>
    </div>
  );
}
