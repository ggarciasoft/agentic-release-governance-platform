// ─────────────────────────────────────────────────────────────────────────────
// API client — mirrors the Backend API surface
// ─────────────────────────────────────────────────────────────────────────────

import type {
  ReleaseListItem,
  ReleaseDetail,
  ReleasePackage,
  ValidationSummary,
  ApplicationMapping,
  CreateReleaseRequest,
} from './types';

const BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5050';

async function apiFetch<T>(
  path: string,
  options?: RequestInit,
): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
  });

  if (!res.ok) {
    const text = await res.text();
    let message = text;
    try {
      const json = JSON.parse(text);
      message = json.title || json.detail || text;
    } catch {
      // use raw text
    }
    throw new Error(`${res.status}: ${message}`);
  }

  if (res.status === 204 || res.headers.get('content-length') === '0') {
    return undefined as unknown as T;
  }

  const text = await res.text();
  if (!text) return undefined as unknown as T;
  return JSON.parse(text) as T;
}

// ── Releases ─────────────────────────────────────────────────────────────────

export const api = {
  releases: {
    list: () =>
      apiFetch<ReleaseListItem[]>('/api/releases'),

    getById: (id: string) =>
      apiFetch<ReleaseDetail>(`/api/releases/${id}`),

    create: (req: CreateReleaseRequest) =>
      apiFetch<{ id: string; name: string; status: string }>('/api/releases', {
        method: 'POST',
        body: JSON.stringify(req),
      }),

    delete: (id: string) =>
      apiFetch<void>(`/api/releases/${id}`, { method: 'DELETE' }),

    attachWorkItems: (releaseId: string, workItems: { releaseId: string; workItems: unknown[] }) =>
      apiFetch<{ attachedCount: number }>(`/api/releases/${releaseId}/work-items`, {
        method: 'POST',
        body: JSON.stringify(workItems),
      }),

    attachPullRequests: (releaseId: string, prs: { releaseId: string; pullRequests: unknown[] }) =>
      apiFetch<{ attachedCount: number }>(`/api/releases/${releaseId}/pull-requests`, {
        method: 'POST',
        body: JSON.stringify(prs),
      }),

    ensureApplications: (releaseId: string, applications: string[]) =>
      apiFetch<{ addedCount: number }>(`/api/releases/${releaseId}/applications`, {
        method: 'POST',
        body: JSON.stringify(applications),
      }),

    attachDeployments: (releaseId: string, deployments: { releaseId: string; deployments: unknown[] }) =>
      apiFetch<{ attachedCount: number }>(`/api/releases/${releaseId}/deployments`, {
        method: 'POST',
        body: JSON.stringify(deployments),
      }),

    discoverRollback: (releaseId: string) =>
      apiFetch<unknown[]>(`/api/releases/${releaseId}/rollback-candidates/discover`, {
        method: 'POST',
      }),

    validate: (releaseId: string) =>
      apiFetch<ValidationSummary>(`/api/releases/${releaseId}/validate`, {
        method: 'POST',
      }),

    getValidationResults: (releaseId: string) =>
      apiFetch<{
        releaseId: string;
        results: { ruleCode: string; severity: string; status: string; message: string }[];
      }>(`/api/releases/${releaseId}/validation-results`),

    getPackage: (releaseId: string) =>
      apiFetch<ReleasePackage>(`/api/releases/${releaseId}/package`),

    generateDocument: async (releaseId: string): Promise<string> => {
      const res = await fetch(`${BASE_URL}/api/releases/${releaseId}/documents/generate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ format: 'markdown' }),
      });
      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || `Failed to generate document: ${res.status}`);
      }
      return res.text();
    },

    saveDocument: (releaseId: string, format: string, content: string) =>
      apiFetch<{ documentId: string; version: number; status: string }>(
        `/api/releases/${releaseId}/documents`,
        { method: 'POST', body: JSON.stringify({ format, content }) },
      ),

    getDocuments: (releaseId: string) =>
      apiFetch<{ id: string; version: number; format: string; generatedAt: string }[]>(
        `/api/releases/${releaseId}/documents`,
      ),
  },

  // ── Application Mappings ────────────────────────────────────────────────────

  mappings: {
    list: () =>
      apiFetch<ApplicationMapping[]>('/api/application-mappings'),

    getById: (id: string) =>
      apiFetch<ApplicationMapping>(`/api/application-mappings/${id}`),

    create: (mapping: Partial<ApplicationMapping>) =>
      apiFetch<ApplicationMapping>('/api/application-mappings', {
        method: 'POST',
        body: JSON.stringify(mapping),
      }),

    update: (id: string, mapping: Partial<ApplicationMapping>) =>
      apiFetch<ApplicationMapping>(`/api/application-mappings/${id}`, {
        method: 'PUT',
        body: JSON.stringify(mapping),
      }),
  },
};
