import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useProjectsStore } from '@/store/projectsStore';
import { Button } from '@/components/ui/button';
import TeamMemberList from '@/components/projects/TeamMemberList';
import { ProjectForm } from '@/components/projects/ProjectForm';
import { ProjectRole, UpdateProjectRequest } from '@/types/project.types';

export const ProjectDetailPage: React.FC = () => {
  const { projectId } = useParams<{ projectId: string }>();
  const navigate = useNavigate();
  const { currentProject, fetchProject, updateProject, isLoading, error, clearError } =
    useProjectsStore();
  const [showInviteDialog, setShowInviteDialog] = useState(false);
  const [showSettings, setShowSettings] = useState(false);

  useEffect(() => {
    if (projectId) {
      fetchProject(parseInt(projectId, 10));
    }
  }, [projectId, fetchProject]);

  if (isLoading || !currentProject) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading project...</p>
        </div>
      </div>
    );
  }

  const canManageProject =
    currentProject.role === ProjectRole.Owner || currentProject.role === ProjectRole.Manager;

  const handleOpenSettings = () => {
    clearError();
    setShowSettings(true);
  };

  const handleCloseSettings = () => {
    clearError();
    setShowSettings(false);
  };

  const handleSubmitSettings = async (data: UpdateProjectRequest) => {
    if (!currentProject) {
      return;
    }

    await updateProject(currentProject.id, data);
    setShowSettings(false);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Back Button */}
        <Button
          onClick={() => navigate('/projects')}
          className="mb-4 bg-gray-200 hover:bg-gray-300 text-gray-700"
        >
          ← Back to Projects
        </Button>

        {/* Error Message */}
        {error && (
          <div className="mb-6 rounded-md bg-red-50 p-4">
            <div className="flex">
              <div className="ml-3">
                <h3 className="text-sm font-medium text-red-800">{error}</h3>
              </div>
            </div>
          </div>
        )}

        {/* Project Header */}
        <div className="bg-white shadow rounded-lg p-6 mb-6">
          <div className="flex justify-between items-start">
            <div className="flex-1">
              <div className="flex items-center space-x-3 mb-2">
                <h1 className="text-3xl font-bold text-gray-900">{currentProject.name}</h1>
                <span
                  className={`px-3 py-1 text-sm font-semibold rounded-full ${
                    currentProject.role === 'Owner'
                      ? 'bg-purple-100 text-purple-800'
                      : currentProject.role === 'Manager'
                      ? 'bg-blue-100 text-blue-800'
                      : 'bg-gray-100 text-gray-800'
                  }`}
                >
                  {currentProject.role}
                </span>
              </div>

              {currentProject.description && (
                <p className="text-gray-600 mb-4">{currentProject.description}</p>
              )}

              <div className="flex items-center space-x-6 text-sm text-gray-500">
                <span className="flex items-center">
                  <svg
                    className="h-5 w-5 mr-2"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"
                    />
                  </svg>
                  {currentProject.memberCount} members
                </span>
                <span className="flex items-center">
                  <svg
                    className="h-5 w-5 mr-2"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                    />
                  </svg>
                  {currentProject.taskCount} tasks
                </span>
                <span className="flex items-center">
                  <svg
                    className="h-5 w-5 mr-2"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                    />
                  </svg>
                  Created {new Date(currentProject.createdAt).toLocaleDateString()}
                </span>
              </div>
            </div>

            <div className="flex space-x-3">
              {canManageProject && (
                <>
                  <Button
                    onClick={() => setShowInviteDialog(true)}
                    className="bg-blue-600 hover:bg-blue-700"
                  >
                    + Invite Member
                  </Button>
                  <Button
                    onClick={handleOpenSettings}
                    className="bg-gray-200 hover:bg-gray-300 text-gray-700"
                  >
                    Settings
                  </Button>
                </>
              )}
              <Button
                onClick={() => navigate(`/projects/${currentProject.id}/board`)}
                className="bg-green-600 hover:bg-green-700"
              >
                Open Board
              </Button>
            </div>
          </div>
        </div>

        {/* Project Owner */}
        <div className="bg-white shadow rounded-lg p-6 mb-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Project Owner</h2>
          <div className="flex items-center">
            <div className="h-10 w-10 rounded-full bg-purple-100 flex items-center justify-center">
              <span className="text-purple-600 font-semibold">
                {currentProject.owner.fullName.charAt(0).toUpperCase()}
              </span>
            </div>
            <div className="ml-3">
              <p className="text-sm font-medium text-gray-900">
                {currentProject.owner.fullName}
              </p>
              <p className="text-sm text-gray-500">{currentProject.owner.email}</p>
            </div>
          </div>
        </div>

        {/* Team Members */}
        <div className="bg-white shadow rounded-lg p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Team Members</h2>
          <TeamMemberList
            members={currentProject.members}
            projectId={currentProject.id}
            canManage={canManageProject}
          />
        </div>

        {/* Invite Dialog - Placeholder */}
        {showInviteDialog && (
          <div className="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full">
              <h2 className="text-xl font-bold mb-4">Invite Team Member</h2>
              <p className="text-gray-600 mb-4">
                Invite dialog will be implemented with form component
              </p>
              <Button onClick={() => setShowInviteDialog(false)}>Close</Button>
            </div>
          </div>
        )}

        {/* Settings Dialog */}
        {showSettings && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-gray-900/50 px-4">
            <div
              role="dialog"
              aria-modal="true"
              aria-labelledby="project-settings-title"
              className="w-full max-w-lg rounded-lg bg-white p-6 shadow-lg"
            >
              <div className="flex items-start justify-between">
                <div>
                  <h2 id="project-settings-title" className="text-xl font-bold text-gray-900">
                    Project Settings
                  </h2>
                  <p className="mt-1 text-sm text-gray-600">
                    Update the project name or description.
                  </p>
                </div>
                <Button
                  type="button"
                  onClick={handleCloseSettings}
                  className="bg-gray-200 hover:bg-gray-300 text-gray-700"
                >
                  Close
                </Button>
              </div>

              {error && (
                <div className="mt-4 rounded-md bg-red-50 p-3 text-sm text-red-700">
                  {error}
                </div>
              )}

              <div className="mt-6">
                <ProjectForm
                  initialData={{
                    name: currentProject.name,
                    description: currentProject.description,
                  }}
                  onSubmit={handleSubmitSettings}
                  onCancel={handleCloseSettings}
                  isLoading={isLoading}
                  mode="edit"
                />
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ProjectDetailPage;
