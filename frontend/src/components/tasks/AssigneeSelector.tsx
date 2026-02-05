import { useMemo } from 'react';
import { ProjectMember, ProjectRole } from '../../types/project.types';

interface AssigneeSelectorProps {
  members: ProjectMember[];
  value?: string | null;
  onChange: (assigneeId: string | null) => void;
  disabled?: boolean;
  label?: string;
  allowUnassigned?: boolean;
}

const roleOrder: Record<ProjectRole, number> = {
  [ProjectRole.Owner]: 0,
  [ProjectRole.Manager]: 1,
  [ProjectRole.Member]: 2,
};

export default function AssigneeSelector({
  members,
  value,
  onChange,
  disabled = false,
  label = 'Assignee',
  allowUnassigned = true,
}: AssigneeSelectorProps) {
  const sortedMembers = useMemo(() => {
    return [...members].sort((a, b) => {
      return roleOrder[a.role] - roleOrder[b.role];
    });
  }, [members]);

  const handleChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const selected = event.target.value;
    onChange(selected ? selected : null);
  };

  return (
    <div className="space-y-1">
      <label className="text-sm font-medium text-gray-700">{label}</label>
      <select
        value={value ?? ''}
        onChange={handleChange}
        disabled={disabled}
        className="w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {allowUnassigned && <option value="">Unassigned</option>}
        {sortedMembers.map((member) => (
          <option key={member.userId} value={member.userId}>
            {member.fullName} ({member.role})
          </option>
        ))}
      </select>
    </div>
  );
}
