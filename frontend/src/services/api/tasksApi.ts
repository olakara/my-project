import type { AxiosInstance } from 'axios';
import apiClient from './apiClient';
import {
  Task,
  TaskDetail,
  KanbanBoard,
  CreateTaskRequest,
  UpdateTaskRequest,
  UpdateTaskStatusRequest,
  TaskFilter,
  TaskStatus,
} from '../../types/task.types';

class TasksApiClient {
  private axiosInstance: AxiosInstance;

  constructor() {
    this.axiosInstance = apiClient;
  }

  // Create task
  async createTask(projectId: number, request: CreateTaskRequest): Promise<Task> {
    const response = await this.axiosInstance.post<Task>(
      `/projects/${projectId}/tasks`,
      request
    );
    return response.data;
  }

  // Get task by ID
  async getTask(taskId: number): Promise<TaskDetail> {
    const response = await this.axiosInstance.get<TaskDetail>(
      `/tasks/${taskId}`
    );
    return response.data;
  }

  // Get Kanban board for project
  async getKanbanBoard(
    projectId: number,
    filters?: TaskFilter
  ): Promise<KanbanBoard> {
    const params = new URLSearchParams();

    if (filters) {
      if (filters.page) params.append('page', filters.page.toString());
      if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
      if (filters.assigneeId) params.append('assigneeId', filters.assigneeId);
      if (filters.priority !== undefined) params.append('priority', filters.priority.toString());
      if (filters.dueDate) params.append('dueDate', filters.dueDate);
    }

    const queryString = params.toString();
    const url = `/projects/${projectId}/tasks${queryString ? `?${queryString}` : ''}`;

    const response = await this.axiosInstance.get<KanbanBoard>(url);
    return response.data;
  }

  // Get all tasks for a project
  async getProjectTasks(
    projectId: number,
    filters?: TaskFilter
  ): Promise<Task[]> {
    const params = new URLSearchParams();

    if (filters) {
      if (filters.page) params.append('page', filters.page.toString());
      if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
      if (filters.assigneeId) params.append('assigneeId', filters.assigneeId);
      if (filters.priority !== undefined) params.append('priority', filters.priority.toString());
      if (filters.dueDate) params.append('dueDate', filters.dueDate);
      if (filters.status) params.append('status', filters.status);
    }

    const queryString = params.toString();
    const url = `/projects/${projectId}/tasks${queryString ? `?${queryString}` : ''}`;

    const response = await this.axiosInstance.get<Task[]>(url);
    return response.data;
  }

  // Update task
  async updateTask(
    taskId: number,
    request: UpdateTaskRequest
  ): Promise<Task> {
    const response = await this.axiosInstance.put<Task>(
      `/tasks/${taskId}`,
      request
    );
    return response.data;
  }

  // Update task status (for drag-drop)
  async updateTaskStatus(
    taskId: number,
    request: UpdateTaskStatusRequest
  ): Promise<Task> {
    const response = await this.axiosInstance.patch<Task>(
      `/tasks/${taskId}/status`,
      request
    );
    return response.data;
  }

  // Update task status (simplified, for drag-drop optimistic updates)
  async updateTaskStatusSimple(
    taskId: number,
    newStatus: TaskStatus
  ): Promise<Task> {
    return this.updateTaskStatus(taskId, { newStatus });
  }

  // Delete task
  async deleteTask(taskId: number): Promise<void> {
    await this.axiosInstance.delete(`/tasks/${taskId}`);
  }

  // Add comment to task
  async addComment(
    taskId: number,
    content: string
  ): Promise<any> {
    const response = await this.axiosInstance.post(
      `/tasks/${taskId}/comments`,
      { content }
    );
    return response.data;
  }

  // Get task comments
  async getTaskComments(taskId: number): Promise<any[]> {
    const response = await this.axiosInstance.get<any[]>(
      `/tasks/${taskId}/comments`
    );
    return response.data;
  }

  // Assign task to user
  async assignTask(
    taskId: number,
    assigneeId: string
  ): Promise<Task> {
    const response = await this.axiosInstance.patch<Task>(
      `/tasks/${taskId}/assign`,
      { assigneeId }
    );
    return response.data;
  }

  // Get user's tasks (My Tasks)
  async getMyTasks(filters?: TaskFilter): Promise<Task[]> {
    const params = new URLSearchParams();

    if (filters) {
      if (filters.page) params.append('page', filters.page.toString());
      if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
      if (filters.priority !== undefined) params.append('priority', filters.priority.toString());
      if (filters.status) params.append('status', filters.status);
    }

    const queryString = params.toString();
    const url = `/tasks/my-tasks${queryString ? `?${queryString}` : ''}`;

    const response = await this.axiosInstance.get<Task[]>(url);
    return response.data;
  }

}

export const tasksApiClient = new TasksApiClient();
export default tasksApiClient;
