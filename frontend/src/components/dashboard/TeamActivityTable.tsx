import { useMemo, useState } from 'react';
import { cn } from '@/utils/cn';
import type { TeamActivityMember } from '@/types/dashboard.types';

export type TeamSortKey = 'name' | 'completed' | 'assigned' | 'role';

export interface TeamActivityTableProps {
  members: TeamActivityMember[];
}

const roleTone: Record<string, string> = {
  Owner: 'bg-slate-900 text-white',
  Manager: 'bg-amber-100 text-amber-800',
  Member: 'bg-slate-100 text-slate-700',
};

export default function TeamActivityTable({ members }: TeamActivityTableProps) {
  const [sortKey, setSortKey] = useState<TeamSortKey>('completed');
  const [direction, setDirection] = useState<'asc' | 'desc'>('desc');

  const sortedMembers = useMemo(() => {
    const next = [...members];

    next.sort((a, b) => {
      let comparison = 0;

      switch (sortKey) {
        case 'name':
          comparison = a.fullName.localeCompare(b.fullName);
          break;
        case 'assigned':
          comparison = a.assignedTasks - b.assignedTasks;
          break;
        case 'role':
          comparison = a.role.localeCompare(b.role);
          break;
        case 'completed':
        default:
          comparison = a.completedTasks - b.completedTasks;
          break;
      }

      return direction === 'asc' ? comparison : -comparison;
    });

    return next;
  }, [members, sortKey, direction]);

  const handleSort = (key: TeamSortKey) => {
    if (sortKey === key) {
      setDirection((prev) => (prev === 'asc' ? 'desc' : 'asc'));
      return;
    }

    setSortKey(key);
    setDirection('desc');
  };

  const sortLabel = (key: TeamSortKey) =>
    `${sortKey === key ? (direction === 'asc' ? '▲' : '▼') : '↕'} ${
      key === 'name'
        ? 'Member'
        : key === 'completed'
        ? 'Completed'
        : key === 'assigned'
        ? 'Assigned'
        : 'Role'
    }`;

  return (
    <div className="rounded-2xl border border-slate-200/70 bg-white/80 p-6 shadow-sm backdrop-blur">
      <div className="flex flex-col gap-2 md:flex-row md:items-center md:justify-between">
        <div>
          <p className="text-xs uppercase tracking-[0.2em] text-slate-500">Team activity</p>
          <h3 className="mt-2 text-2xl font-semibold text-slate-900">Momentum leaderboard</h3>
          <p className="mt-2 text-sm text-slate-600">
            Sorted by completed tasks, with a spotlight on consistent delivery.
          </p>
        </div>
      </div>
      <div className="mt-6 overflow-hidden rounded-xl border border-slate-200">
        <table className="min-w-full divide-y divide-slate-200 text-sm">
          <thead className="bg-slate-100/70 text-xs uppercase tracking-widest text-slate-500">
            <tr>
              <th className="px-4 py-3 text-left">
                <button
                  type="button"
                  onClick={() => handleSort('name')}
                  className="font-semibold"
                >
                  {sortLabel('name')}
                </button>
              </th>
              <th className="px-4 py-3 text-left">
                <button
                  type="button"
                  onClick={() => handleSort('role')}
                  className="font-semibold"
                >
                  {sortLabel('role')}
                </button>
              </th>
              <th className="px-4 py-3 text-right">
                <button
                  type="button"
                  onClick={() => handleSort('assigned')}
                  className="font-semibold"
                >
                  {sortLabel('assigned')}
                </button>
              </th>
              <th className="px-4 py-3 text-right">
                <button
                  type="button"
                  onClick={() => handleSort('completed')}
                  className="font-semibold"
                >
                  {sortLabel('completed')}
                </button>
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-200 bg-white">
            {sortedMembers.map((member) => (
              <tr key={member.userId} className="hover:bg-slate-50">
                <td className="px-4 py-3">
                  <div className="font-semibold text-slate-900">{member.fullName}</div>
                  <div className="text-xs text-slate-500">{member.email}</div>
                </td>
                <td className="px-4 py-3">
                  <span
                    className={cn(
                      'inline-flex items-center rounded-full px-2.5 py-1 text-xs font-semibold',
                      roleTone[member.role] || 'bg-slate-100 text-slate-700'
                    )}
                  >
                    {member.role}
                  </span>
                </td>
                <td className="px-4 py-3 text-right font-semibold text-slate-700">
                  {member.assignedTasks}
                </td>
                <td className="px-4 py-3 text-right font-semibold text-emerald-700">
                  {member.completedTasks}
                </td>
              </tr>
            ))}
            {sortedMembers.length === 0 && (
              <tr>
                <td colSpan={4} className="px-4 py-6 text-center text-slate-500">
                  No team activity recorded yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
