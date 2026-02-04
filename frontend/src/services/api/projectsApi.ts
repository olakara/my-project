import apiClient from './apiClient';
import type {
  Project,
  ProjectSummary,
  CreateProjectRequest,
  CreateProjectResponse,
  UpdateProjectRequest,
  InviteMemberRequest,
  InviteMemberResponse,
  AcceptInvitationResponse,
} from '@/types/project.types';

/**
 * Projects API client
 * Handles project CRUD operations, invitations, and member management
 */

export const projectsApi = {
  /**
   * Get all projects for the current user
   * @returns List of projects where user is a member
   */
  async getProjects(): Promise<ProjectSummary[]> {
    const response = await apiClient.get<ProjectSummary[]>('/projects');
    return response.data;
  },

  /**
   * Get project details by ID
   * @param projectId - Project identifier
   * @returns Full project details including members
   */
  async getProject(projectId: number): Promise<Project> {
    const response = await apiClient.get<Project>(`/projects/${projectId}`);
    return response.data;
  },

  /**
   * Create a new project
   * @param data - Project creation details (name, description)
   * @returns Created project details
   */
  async createProject(data: CreateProjectRequest): Promise<CreateProjectResponse> {
    const response = await apiClient.post<CreateProjectResponse>('/projects', data);
    return response.data;
  },

  /**
   * Update an existing project
   * @param projectId - Project identifier
   * @param data - Updated project details (name, description)
   * @returns Updated project details
   */
  async updateProject(projectId: number, data: UpdateProjectRequest): Promise<Project> {
    const response = await apiClient.put<Project>(`/projects/${projectId}`, data);
    return response.data;
  },

  /**
   * Delete a project (archive)
   * @param projectId - Project identifier
   */
  async deleteProject(projectId: number): Promise<void> {
    await apiClient.delete(`/projects/${projectId}`);
  },

  /**
   * Invite a member to the project
   * @param projectId - Project identifier
   * @param data - Invitation details (email, role)
   * @returns Invitation details
   */
  async inviteMember(projectId: number, data: InviteMemberRequest): Promise<InviteMemberResponse> {
    const response = await apiClient.post<InviteMemberResponse>(
      `/projects/${projectId}/invitations`,
      data
    );
    return response.data;
  },

  /**
   * Accept a project invitation
   * @param invitationId - Invitation identifier
   * @returns Acceptance confirmation with project details
   */
  async acceptInvitation(invitationId: number): Promise<AcceptInvitationResponse> {
    const response = await apiClient.post<AcceptInvitationResponse>(
      `/invitations/${invitationId}/accept`
    );
    return response.data;
  },

  /**
   * Remove a member from the project
   * @param projectId - Project identifier
   * @param userId - User identifier to remove
   */
  async removeMember(projectId: number, userId: string): Promise<void> {
    await apiClient.delete(`/projects/${projectId}/members/${userId}`);
  },

  /**
   * Get pending invitations for the current user
   * @returns List of pending invitations
   */
  async getPendingInvitations(): Promise<InviteMemberResponse[]> {
    const response = await apiClient.get<InviteMemberResponse[]>('/invitations/pending');
    return response.data;
  },
};

export default projectsApi;
