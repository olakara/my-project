import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import projectsApi from '@/services/api/projectsApi';
import type {
  Project,
  ProjectSummary,
  CreateProjectRequest,
  UpdateProjectRequest,
  ProjectRole,
} from '@/types/project.types';

interface ProjectsStore {
  // State
  projects: ProjectSummary[];
  currentProject: Project | null;
  isLoading: boolean;
  error: string | null;

  // Actions
  fetchProjects: () => Promise<void>;
  fetchProject: (projectId: number) => Promise<void>;
  createProject: (data: CreateProjectRequest) => Promise<Project | null>;
  updateProject: (projectId: number, data: UpdateProjectRequest) => Promise<void>;
  deleteProject: (projectId: number) => Promise<void>;
  inviteMember: (projectId: number, email: string, role: ProjectRole) => Promise<void>;
  removeMember: (projectId: number, userId: string) => Promise<void>;
  acceptInvitation: (invitationId: number) => Promise<void>;
  setCurrentProject: (project: Project | null) => void;
  clearError: () => void;
  reset: () => void;
}

const initialState = {
  projects: [],
  currentProject: null,
  isLoading: false,
  error: null,
};

export const useProjectsStore = create<ProjectsStore>()(
  persist(
    (set, get) => ({
      // Initial state
      ...initialState,

      // Fetch all projects for current user
      fetchProjects: async () => {
        set({ isLoading: true, error: null });
        try {
          const projects = await projectsApi.getProjects();
          set({ projects, isLoading: false });
        } catch (error: any) {
          const errorMessage = error.response?.data?.error || 'Failed to fetch projects';
          set({ error: errorMessage, isLoading: false });
          throw error;
        }
      },

      // Fetch single project details
      fetchProject: async (projectId: number) => {
        set({ isLoading: true, error: null });
        try {
          const project = await projectsApi.getProject(projectId);
          set({ currentProject: project, isLoading: false });
        } catch (error: any) {
          const errorMessage = error.response?.data?.error || 'Failed to fetch project';
          set({ error: errorMessage, isLoading: false, currentProject: null });
          throw error;
        }
      },

      // Create new project
      createProject: async (data: CreateProjectRequest) => {
        set({ isLoading: true, error: null });
        try {
          const response = await projectsApi.createProject(data);
          
          // Refresh projects list
          await get().fetchProjects();
          
          // Fetch the full project details
          await get().fetchProject(response.id);
          
          set({ isLoading: false });
          return get().currentProject;
        } catch (error: any) {
          const errorMessage = error.response?.data?.error || 'Failed to create project';
          set({ error: errorMessage, isLoading: false });
          throw error;
        }
      },

      // Update existing project
      updateProject: async (projectId: number, data: UpdateProjectRequest) => {
        set({ isLoading: true, error: null });
        try {
          const updatedProject = await projectsApi.updateProject(projectId, data);
          
          // Update current project if it's the one being updated
          if (get().currentProject?.id === projectId) {
            set({ currentProject: updatedProject });
          }
          
          // Refresh projects list
          await get().fetchProjects();
          
          set({ isLoading: false });
        } catch (error: any) {
          const errorMessage = error.response?.data?.error || 'Failed to update project';
          set({ error: errorMessage, isLoading: false });
          throw error;
        }
      },

      // Delete project
      deleteProject: async (projectId: number) => {
        set({ isLoading: true, error: null });
        try {
          await projectsApi.deleteProject(projectId);
          
          // Remove from projects list
          set((state) => ({
            projects: state.projects.filter((p) => p.id !== projectId),
            currentProject: state.currentProject?.id === projectId ? null : state.currentProject,
            isLoading: false,
          }));
        } catch (error: any) {
          const errorMessage = error.response?.data?.error || 'Failed to delete project';
          set({ error: errorMessage, isLoading: false });
          throw error;
        }
      },

      // Invite member to project
      inviteMember: async (projectId: number, email: string, role: ProjectRole) => {
        set({ isLoading: true, error: null });
        try {
          await projectsApi.inviteMember(projectId, { email, role });
          
          // Refresh current project to show updated members (if invitation accepted immediately)
          if (get().currentProject?.id === projectId) {
            await get().fetchProject(projectId);
          }
          
          set({ isLoading: false });
        } catch (error: any) {
          const errorMessage = error.response?.data?.error || 'Failed to invite member';
          set({ error: errorMessage, isLoading: false });
          throw error;
        }
      },

      // Remove member from project
      removeMember: async (projectId: number, userId: string) => {
        set({ isLoading: true, error: null });
        try {
          await projectsApi.removeMember(projectId, userId);
          
          // Refresh current project to show updated members
          if (get().currentProject?.id === projectId) {
            await get().fetchProject(projectId);
          }
          
          set({ isLoading: false });
        } catch (error: any) {
          const errorMessage = error.response?.data?.error || 'Failed to remove member';
          set({ error: errorMessage, isLoading: false });
          throw error;
        }
      },

      // Accept invitation
      acceptInvitation: async (invitationId: number) => {
        set({ isLoading: true, error: null });
        try {
          const response = await projectsApi.acceptInvitation(invitationId);
          
          // Refresh projects list to include newly joined project
          await get().fetchProjects();
          
          // Optionally fetch the project details
          await get().fetchProject(response.projectId);
          
          set({ isLoading: false });
        } catch (error: any) {
          const errorMessage = error.response?.data?.error || 'Failed to accept invitation';
          set({ error: errorMessage, isLoading: false });
          throw error;
        }
      },

      // Set current project
      setCurrentProject: (project: Project | null) => {
        set({ currentProject: project });
      },

      // Clear error
      clearError: () => {
        set({ error: null });
      },

      // Reset store
      reset: () => {
        set(initialState);
      },
    }),
    {
      name: 'projects-storage',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        // Only persist projects list, not current project
        projects: state.projects,
      }),
    }
  )
);

export default useProjectsStore;
