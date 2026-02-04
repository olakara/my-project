import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useProjectsStore } from '@/store/projectsStore';
import projectsApi from '@/services/api/projectsApi';
import type {
  CreateProjectRequest,
  UpdateProjectRequest,
  ProjectRole,
} from '@/types/project.types';

/**
 * Custom hook for projects with React Query caching
 * Provides projects data and mutation functions with automatic cache updates
 */
export const useProjects = () => {
  const queryClient = useQueryClient();
  const {
    projects,
    currentProject,
    isLoading: storeLoading,
    error,
    clearError,
  } = useProjectsStore();

  // Fetch all projects with React Query caching
  const {
    data: projectsList,
    isLoading: queryLoading,
    refetch: refetchProjects,
  } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectsApi.getProjects(),
    staleTime: 5 * 60 * 1000, // Consider data fresh for 5 minutes
    gcTime: 30 * 60 * 1000, // Keep in cache for 30 minutes
  });

  // Fetch single project
  const fetchProject = (projectId: number) => {
    return useQuery({
      queryKey: ['project', projectId],
      queryFn: () => projectsApi.getProject(projectId),
      enabled: !!projectId,
      staleTime: 2 * 60 * 1000, // Consider data fresh for 2 minutes
    });
  };

  // Create project mutation
  const createProjectMutation = useMutation({
    mutationFn: (data: CreateProjectRequest) => projectsApi.createProject(data),
    onSuccess: () => {
      // Invalidate and refetch projects list
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });

  // Update project mutation
  const updateProjectMutation = useMutation({
    mutationFn: ({ projectId, data }: { projectId: number; data: UpdateProjectRequest }) =>
      projectsApi.updateProject(projectId, data),
    onSuccess: (_, variables) => {
      // Invalidate specific project and projects list
      queryClient.invalidateQueries({ queryKey: ['project', variables.projectId] });
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });

  // Delete project mutation
  const deleteProjectMutation = useMutation({
    mutationFn: (projectId: number) => projectsApi.deleteProject(projectId),
    onSuccess: () => {
      // Invalidate projects list
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });

  // Invite member mutation
  const inviteMemberMutation = useMutation({
    mutationFn: ({
      projectId,
      email,
      role,
    }: {
      projectId: number;
      email: string;
      role: ProjectRole;
    }) => projectsApi.inviteMember(projectId, { email, role }),
    onSuccess: (_, variables) => {
      // Invalidate project to refresh members list
      queryClient.invalidateQueries({ queryKey: ['project', variables.projectId] });
    },
  });

  // Remove member mutation
  const removeMemberMutation = useMutation({
    mutationFn: ({ projectId, userId }: { projectId: number; userId: string }) =>
      projectsApi.removeMember(projectId, userId),
    onSuccess: (_, variables) => {
      // Invalidate project to refresh members list
      queryClient.invalidateQueries({ queryKey: ['project', variables.projectId] });
    },
  });

  // Accept invitation mutation
  const acceptInvitationMutation = useMutation({
    mutationFn: (invitationId: number) => projectsApi.acceptInvitation(invitationId),
    onSuccess: () => {
      // Invalidate projects list to include newly joined project
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });

  return {
    // State from store (for compatibility)
    projects: projectsList || projects,
    currentProject,
    isLoading: queryLoading || storeLoading,
    error,

    // Query functions
    fetchProject,
    refetchProjects,

    // Mutations
    createProject: createProjectMutation.mutateAsync,
    updateProject: (projectId: number, data: UpdateProjectRequest) =>
      updateProjectMutation.mutateAsync({ projectId, data }),
    deleteProject: deleteProjectMutation.mutateAsync,
    inviteMember: (projectId: number, email: string, role: ProjectRole) =>
      inviteMemberMutation.mutateAsync({ projectId, email, role }),
    removeMember: (projectId: number, userId: string) =>
      removeMemberMutation.mutateAsync({ projectId, userId }),
    acceptInvitation: acceptInvitationMutation.mutateAsync,

    // Mutation states
    isCreating: createProjectMutation.isPending,
    isUpdating: updateProjectMutation.isPending,
    isDeleting: deleteProjectMutation.isPending,
    isInviting: inviteMemberMutation.isPending,
    isRemoving: removeMemberMutation.isPending,
    isAccepting: acceptInvitationMutation.isPending,

    // Utility
    clearError,
  };
};

export default useProjects;
