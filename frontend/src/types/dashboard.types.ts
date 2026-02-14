import type { TaskStatus } from './task.types';

export interface StatusCount {
  status: TaskStatus;
  count: number;
}

export interface TeamMemberMetrics {
  userId: string;
  fullName: string;
  email: string;
  role: string;
  assignedTasks: number;
  completedTasks: number;
}

export interface ProjectMetricsResponse {
  projectId: number;
  projectName: string;
  totalTasks: number;
  completedTasks: number;
  completionPercentage: number;
  statusCounts: StatusCount[];
  teamMembers: TeamMemberMetrics[];
}

export interface BurndownDay {
  date: string;
  completedTasks: number;
}

export interface BurndownResponse {
  projectId: number;
  projectName: string;
  startDate: string;
  endDate: string;
  totalCompleted: number;
  days: BurndownDay[];
}

export interface TeamActivityMember {
  userId: string;
  fullName: string;
  email: string;
  role: string;
  assignedTasks: number;
  completedTasks: number;
}

export interface TeamActivityResponse {
  projectId: number;
  projectName: string;
  totalCompletedTasks: number;
  members: TeamActivityMember[];
}
