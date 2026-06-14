'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import type { ApplicationMapping } from '@/lib/types';

export default function MappingsPage() {
  const [mappings, setMappings] = useState<ApplicationMapping[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<ApplicationMapping | null>(null);
  const [form, setForm] = useState({
    applicationName: '',
    organization: '',
    project: '',
    repositoryName: '',
    buildDefinitionId: '',
    buildDefinitionName: '',
    releaseDefinitionId: '',
    releaseDefinitionName: '',
    productionEnvironmentName: '',
    uatEnvironmentName: '',
  });
  const [saving, setSaving] = useState(false);

  const fetchMappings = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await api.mappings.list();
      setMappings(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load mappings');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchMappings(); }, []);

  const openNew = () => {
    setEditing(null);
    setForm({
      applicationName: '', organization: '', project: '', repositoryName: '',
      buildDefinitionId: '', buildDefinitionName: '', releaseDefinitionId: '',
      releaseDefinitionName: '', productionEnvironmentName: '', uatEnvironmentName: '',
    });
    setShowForm(true);
  };

  const openEdit = (m: ApplicationMapping) => {
    setEditing(m);
    setForm({
      applicationName: m.applicationName,
      organization: m.organization || '',
      project: m.project || '',
      repositoryName: m.repositoryName || '',
      buildDefinitionId: m.buildDefinitionId?.toString() || '',
      buildDefinitionName: m.buildDefinitionName || '',
      releaseDefinitionId: m.releaseDefinitionId?.toString() || '',
      releaseDefinitionName: m.releaseDefinitionName || '',
      productionEnvironmentName: m.productionEnvironmentName || '',
      uatEnvironmentName: m.uatEnvironmentName || '',
    });
    setShowForm(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        applicationName: form.applicationName,
        organization: form.organization,
        project: form.project,
        repositoryName: form.repositoryName,
        buildDefinitionId: form.buildDefinitionId ? Number(form.buildDefinitionId) : null,
        buildDefinitionName: form.buildDefinitionName,
        releaseDefinitionId: form.releaseDefinitionId ? Number(form.releaseDefinitionId) : null,
        releaseDefinitionName: form.releaseDefinitionName,
        productionEnvironmentName: form.productionEnvironmentName,
        uatEnvironmentName: form.uatEnvironmentName,
      };

      if (editing) {
        await api.mappings.update(editing.id, payload);
      } else {
        await api.mappings.create(payload);
      }
      setShowForm(false);
      await fetchMappings();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Save failed');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-2xl font-bold text-white">Application Mappings</h1>
        <button
          onClick={openNew}
          className="px-4 py-2 bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-medium rounded-lg transition-colors"
        >
          + Add Mapping
        </button>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-900/30 border border-red-700 rounded-lg text-red-300 text-sm">
          {error}
        </div>
      )}

      {/* Form Modal */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
          <div className="bg-gray-900 border border-gray-700 rounded-xl p-6 w-full max-w-lg mx-4 max-h-[90vh] overflow-y-auto">
            <h2 className="text-lg font-semibold text-white mb-5">
              {editing ? 'Edit Mapping' : 'New Mapping'}
            </h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              <Field label="Application Name" value={form.applicationName} onChange={(v) => setForm({ ...form, applicationName: v })} required />
              <div className="grid grid-cols-2 gap-4">
                <Field label="Organization" value={form.organization} onChange={(v) => setForm({ ...form, organization: v })} />
                <Field label="Project" value={form.project} onChange={(v) => setForm({ ...form, project: v })} />
              </div>
              <Field label="Repository Name" value={form.repositoryName} onChange={(v) => setForm({ ...form, repositoryName: v })} />
              <div className="grid grid-cols-2 gap-4">
                <Field label="Build Definition ID" value={form.buildDefinitionId} onChange={(v) => setForm({ ...form, buildDefinitionId: v })} type="number" />
                <Field label="Build Definition Name" value={form.buildDefinitionName} onChange={(v) => setForm({ ...form, buildDefinitionName: v })} />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <Field label="Release Definition ID" value={form.releaseDefinitionId} onChange={(v) => setForm({ ...form, releaseDefinitionId: v })} type="number" />
                <Field label="Release Definition Name" value={form.releaseDefinitionName} onChange={(v) => setForm({ ...form, releaseDefinitionName: v })} />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <Field label="Production Env" value={form.productionEnvironmentName} onChange={(v) => setForm({ ...form, productionEnvironmentName: v })} />
                <Field label="UAT Env" value={form.uatEnvironmentName} onChange={(v) => setForm({ ...form, uatEnvironmentName: v })} />
              </div>
              <div className="flex gap-3 pt-2">
                <button
                  type="submit"
                  disabled={saving}
                  className="flex-1 px-4 py-2 bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors"
                >
                  {saving ? 'Saving…' : 'Save'}
                </button>
                <button
                  type="button"
                  onClick={() => setShowForm(false)}
                  className="px-4 py-2 bg-gray-800 hover:bg-gray-700 text-gray-300 text-sm font-medium rounded-lg transition-colors"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {loading ? (
        <div className="text-gray-400 text-sm">Loading mappings…</div>
      ) : mappings.length === 0 ? (
        <div className="text-center py-16">
          <p className="text-gray-500 text-lg mb-4">No application mappings configured</p>
          <button onClick={openNew} className="text-indigo-400 hover:text-indigo-300 text-sm font-medium">
            Add your first mapping →
          </button>
        </div>
      ) : (
        <div className="overflow-hidden rounded-xl border border-gray-800">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-900 text-gray-400 text-left">
                <th className="px-4 py-3 font-medium">Application</th>
                <th className="px-4 py-3 font-medium">Repository</th>
                <th className="px-4 py-3 font-medium">Build Definition</th>
                <th className="px-4 py-3 font-medium">Release Definition</th>
                <th className="px-4 py-3 font-medium">Environments</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-800">
              {mappings.map((m) => (
                <tr
                  key={m.id}
                  onClick={() => openEdit(m)}
                  className="hover:bg-gray-900/50 cursor-pointer transition-colors"
                >
                  <td className="px-4 py-3 font-medium text-gray-200">{m.applicationName}</td>
                  <td className="px-4 py-3 text-gray-500 text-xs font-mono">{m.repositoryName}</td>
                  <td className="px-4 py-3 text-gray-400 text-xs">
                    {m.buildDefinitionName || '—'}
                    {m.buildDefinitionId && (
                      <span className="text-gray-600 ml-1">(#{m.buildDefinitionId})</span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-gray-400 text-xs">
                    {m.releaseDefinitionName || '—'}
                    {m.releaseDefinitionId && (
                      <span className="text-gray-600 ml-1">(#{m.releaseDefinitionId})</span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-gray-500 text-xs">
                    {m.productionEnvironmentName || '—'} / {m.uatEnvironmentName || '—'}
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

function Field({
  label,
  value,
  onChange,
  type = 'text',
  required = false,
}: {
  label: string;
  value: string;
  onChange: (v: string) => void;
  type?: string;
  required?: boolean;
}) {
  return (
    <div>
      <label className="block text-xs font-medium text-gray-400 mb-1">{label}</label>
      <input
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        required={required}
        className="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-100 text-sm
                   placeholder-gray-600 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
      />
    </div>
  );
}
