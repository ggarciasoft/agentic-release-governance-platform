'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { api } from '@/lib/api';

export default function NewReleasePage() {
  const router = useRouter();
  const [form, setForm] = useState({
    releaseName: '',
    changeRequest: '',
    organization: '',
    project: '',
    targetEnvironment: 'Production',
    applicationsText: '',
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      const applications = form.applicationsText
        .split(',')
        .map((s) => s.trim())
        .filter(Boolean);

      const res = await api.releases.create({
        releaseName: form.releaseName,
        changeRequest: form.changeRequest,
        organization: form.organization,
        project: form.project,
        targetEnvironment: form.targetEnvironment,
        applications,
      });

      router.push(`/releases/${res.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create release');
      setSubmitting(false);
    }
  };

  const field = (label: string, name: keyof typeof form, type = 'text', placeholder = '') => (
    <div>
      <label className="block text-sm font-medium text-gray-300 mb-1">{label}</label>
      <input
        type={type}
        value={form[name]}
        onChange={(e) => setForm({ ...form, [name]: e.target.value })}
        placeholder={placeholder}
        required
        className="w-full px-3 py-2 bg-gray-900 border border-gray-700 rounded-lg text-gray-100 text-sm
                   placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
      />
    </div>
  );

  return (
    <div className="max-w-lg mx-auto">
      <h1 className="text-2xl font-bold text-white mb-8">New Release</h1>

      {error && (
        <div className="mb-6 p-4 bg-red-900/30 border border-red-700 rounded-lg text-red-300 text-sm">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-5">
        {field('Release Name', 'releaseName', 'text', 'October Prod Release')}
        {field('Change Request', 'changeRequest', 'text', 'CR-12345')}
        {field('Organization', 'organization', 'text', 'my-org')}
        {field('Project', 'project', 'text', 'my-project')}
        {field('Target Environment', 'targetEnvironment', 'text', 'Production')}

        <div>
          <label className="block text-sm font-medium text-gray-300 mb-1">
            Applications <span className="text-gray-500">(comma-separated)</span>
          </label>
          <textarea
            value={form.applicationsText}
            onChange={(e) => setForm({ ...form, applicationsText: e.target.value })}
            placeholder="payment-api, admin-portal, notification-service"
            rows={3}
            required
            className="w-full px-3 py-2 bg-gray-900 border border-gray-700 rounded-lg text-gray-100 text-sm
                       placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
          />
        </div>

        <div className="flex gap-3 pt-2">
          <button
            type="submit"
            disabled={submitting}
            className="flex-1 px-4 py-2.5 bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50
                       text-white text-sm font-medium rounded-lg transition-colors"
          >
            {submitting ? 'Creating…' : 'Create Release'}
          </button>
          <button
            type="button"
            onClick={() => router.back()}
            className="px-4 py-2.5 bg-gray-800 hover:bg-gray-700 text-gray-300 text-sm font-medium rounded-lg transition-colors"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}
