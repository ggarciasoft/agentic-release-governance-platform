'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { api } from '@/lib/api';
import type { ReleaseListItem } from '@/lib/types';

const statusColors: Record<string, string> = {
  Created: 'bg-gray-600',
  Analyzing: 'bg-blue-600',
  AnalysisComplete: 'bg-cyan-600',
  ValidationComplete: 'bg-amber-600',
  DocumentGenerated: 'bg-emerald-600',
};

export default function DashboardPage() {
  const [releases, setReleases] = useState<ReleaseListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchReleases = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await api.releases.list();
      setReleases(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load releases');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchReleases(); }, []);

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Delete "${name}"?`)) return;
    try {
      await api.releases.delete(id);
      setReleases((prev) => prev.filter((r) => r.id !== id));
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Delete failed');
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-2xl font-bold text-white">Releases</h1>
        <Link
          href="/releases/new"
          className="px-4 py-2 bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-medium rounded-lg transition-colors"
        >
          + New Release
        </Link>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-900/30 border border-red-700 rounded-lg text-red-300 text-sm">
          {error}
          <button onClick={fetchReleases} className="ml-4 underline hover:text-red-200">
            Retry
          </button>
        </div>
      )}

      {loading ? (
        <div className="text-gray-400 text-sm">Loading releases…</div>
      ) : releases.length === 0 ? (
        <div className="text-center py-16">
          <p className="text-gray-500 text-lg mb-4">No releases yet</p>
          <Link
            href="/releases/new"
            className="text-indigo-400 hover:text-indigo-300 text-sm font-medium"
          >
            Create your first release →
          </Link>
        </div>
      ) : (
        <div className="overflow-hidden rounded-xl border border-gray-800">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-900 text-gray-400 text-left">
                <th className="px-4 py-3 font-medium">Name</th>
                <th className="px-4 py-3 font-medium">Change Request</th>
                <th className="px-4 py-3 font-medium">Status</th>
                <th className="px-4 py-3 font-medium">Created</th>
                <th className="px-4 py-3 font-medium w-20" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-800">
              {releases.map((r) => (
                <tr key={r.id} className="hover:bg-gray-900/50 transition-colors">
                  <td className="px-4 py-3">
                    <Link
                      href={`/releases/${r.id}`}
                      className="text-indigo-400 hover:text-indigo-300 font-medium"
                    >
                      {r.name}
                    </Link>
                  </td>
                  <td className="px-4 py-3 text-gray-300 font-mono text-xs">
                    {r.changeRequest}
                  </td>
                  <td className="px-4 py-3">
                    <span
                      className={`inline-block px-2 py-0.5 rounded-full text-xs font-medium text-white ${statusColors[r.status] || 'bg-gray-600'}`}
                    >
                      {r.status}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-500 text-xs">
                    {new Date(r.createdAt).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-3">
                    <button
                      onClick={() => handleDelete(r.id, r.name)}
                      className="text-gray-600 hover:text-red-400 text-xs transition-colors"
                      title="Delete"
                    >
                      ✕
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
