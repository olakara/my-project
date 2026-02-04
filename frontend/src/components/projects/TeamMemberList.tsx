import React, { useState } from 'react';
import { Button } from '@/components/ui/button';
import { ProjectRole, type ProjectMember } from '@/types/project.types';
import { useProjectsStore } from '@/store/projectsStore';

interface TeamMemberListProps {
  members: ProjectMember[];
  projectId: number;
  canManage: boolean;
}

export const TeamMemberList: React.FC<TeamMemberListProps> = ({
  members,
  projectId,
  canManage,
}) => {
  const { removeMember, isLoading } = useProjectsStore();
  const [removingUserId, setRemovingUserId] = useState<string | null>(null);

  const handleRemoveMember = async (userId: string) => {
    if (!window.confirm('Are you sure you want to remove this member from the project?')) {
      return;
    }

    setRemovingUserId(userId);
    try {
      await removeMember(projectId, userId);
    } catch (error) {
      console.error('Failed to remove member:', error);
    } finally {
      setRemovingUserId(null);
    }
  };

  const getRoleBadgeColor = (role: ProjectRole | string) => {
    switch (role) {
      case ProjectRole.Owner:
      case 'Owner':
        return 'bg-purple-100 text-purple-800';
      case ProjectRole.Manager:
      case 'Manager':
        return 'bg-blue-100 text-blue-800';
      case ProjectRole.Member:
      case 'Member':
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getInitials = (fullName: string): string => {
    const names = fullName.split(' ');
    if (names.length >= 2) {
      return `${names[0].charAt(0)}${names[1].charAt(0)}`.toUpperCase();
    }
    return fullName.charAt(0).toUpperCase();
  };

  if (members.length === 0) {
    return (
      <div className="text-center py-8 text-gray-500">
        <p>No team members yet</p>
      </div>
    );
  }

  // Sort members: Owner first, then Manager, then Member
  const sortedMembers = [...members].sort((a, b) => {
    const roleOrder = { Owner: 0, Manager: 1, Member: 2 };
    const aRole = a.role as keyof typeof roleOrder;
    const bRole = b.role as keyof typeof roleOrder;
    return roleOrder[aRole] - roleOrder[bRole];
  });

  return (
    <div className="space-y-4">
      {sortedMembers.map((member) => (
        <div
          key={member.userId}
          className="flex items-center justify-between p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
        >
          <div className="flex items-center space-x-4">
            {/* Avatar */}
            <div className="h-12 w-12 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center">
              <span className="text-white font-semibold text-sm">
                {getInitials(member.fullName)}
              </span>
            </div>

            {/* Member Info */}
            <div>
              <div className="flex items-center space-x-2">
                <h3 className="text-sm font-medium text-gray-900">{member.fullName}</h3>
                <span
                  className={`px-2 py-1 text-xs font-semibold rounded-full ${getRoleBadgeColor(
                    member.role
                  )}`}
                >
                  {member.role}
                </span>
              </div>
              <p className="text-sm text-gray-500">{member.email}</p>
              <p className="text-xs text-gray-400 mt-1">
                Joined {new Date(member.joinedAt).toLocaleDateString()}
              </p>
            </div>
          </div>

          {/* Actions */}
          {canManage && member.role !== 'Owner' && (
            <Button
              onClick={() => handleRemoveMember(member.userId)}
              disabled={isLoading || removingUserId === member.userId}
              className="bg-red-100 hover:bg-red-200 text-red-700 text-sm"
            >
              {removingUserId === member.userId ? (
                <span className="flex items-center">
                  <svg
                    className="animate-spin -ml-1 mr-2 h-4 w-4"
                    fill="none"
                    viewBox="0 0 24 24"
                  >
                    <circle
                      className="opacity-25"
                      cx="12"
                      cy="12"
                      r="10"
                      stroke="currentColor"
                      strokeWidth="4"
                    />
                    <path
                      className="opacity-75"
                      fill="currentColor"
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                    />
                  </svg>
                  Removing...
                </span>
              ) : (
                'Remove'
              )}
            </Button>
          )}
        </div>
      ))}

      {/* Member Count Summary */}
      <div className="pt-4 border-t border-gray-200">
        <p className="text-sm text-gray-600">
          Total members: <span className="font-semibold">{members.length}</span>
          {' • '}
          Owners: <span className="font-semibold">
            {members.filter((m) => m.role === 'Owner').length}
          </span>
          {' • '}
          Managers: <span className="font-semibold">
            {members.filter((m) => m.role === 'Manager').length}
          </span>
          {' • '}
          Members: <span className="font-semibold">
            {members.filter((m) => m.role === 'Member').length}
          </span>
        </p>
      </div>
    </div>
  );
};

export default TeamMemberList;
