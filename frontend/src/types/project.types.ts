// Project-related TypeScript types and interfaces

export enum ProjectRole {
  Owner = 'Owner',
  Manager = 'Manager',
  Member = 'Member',
}

export enum ProjectInvitationStatus {
  Pending = 'Pending',
  Accepted = 'Accepted',
  Declined = 'Declined',
}

export interface ProjectMember {
  userId: string;
  fullName: string;
  email: string;
  role: ProjectRole;
  joinedAt: string;
}

export interface ProjectOwner {
  userId: string;
  fullName: string;
  email: string;
}

export interface Project {
  id: number;
  name: string;
  description?: string;
  role: ProjectRole;
  memberCount: number;
  taskCount: number;
  createdAt: string;
  owner: ProjectOwner;
  members: ProjectMember[];
}

export interface ProjectSummary {
  id: number;
  name: string;
  description?: string;
  role: ProjectRole;
  memberCount: number;
  taskCount: number;
  createdAt: string;
}

export interface ProjectInvitation {
  id: number;
  email: string;
  role: ProjectRole;
  status: ProjectInvitationStatus;
  invitedBy: {
    userId: string;
    fullName: string;
    email: string;
  };
  createdAt: string;
  expiresAt: string;
}

export interface CreateProjectRequest {
  name: string;
  description?: string;
}

export interface CreateProjectResponse {
  id: number;
  name: string;
  description?: string;
  ownerId: string;
  isArchived: boolean;
  createdTimestamp: string;
  updatedTimestamp: string;
}

export interface UpdateProjectRequest {
  name?: string;
  description?: string;
}

export interface InviteMemberRequest {
  email: string;
  role: ProjectRole;
}

export interface InviteMemberResponse {
  id: number;
  email: string;
  role: string;
  status: string;
  invitedBy: {
    userId: string;
    fullName: string;
    email: string;
  };
  createdAt: string;
  expiresAt: string;
}

export interface AcceptInvitationResponse {
  projectId: number;
  projectName: string;
  role: string;
  message: string;
}

export interface ProjectsState {
  projects: ProjectSummary[];
  currentProject: Project | null;
  isLoading: boolean;
  error: string | null;
}
