import axios, { AxiosInstance } from 'axios';
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

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api/v1';

class TasksApiClient {
  private axiosInstance: AxiosInstance;

  constructor() {
    this.axiosInstance = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Add auth token from localStorage if available
    const token = localStorage.getItem('authToken');
    if (token) {
      this.axiosInstance.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    }
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

  // Set auth token for API requests
  setAuthToken(token: string): void {
    this.axiosInstance.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    localStorage.setItem('authToken', token);
  }

  // Remove auth token
  removeAuthToken(): void {
    delete this.axiosInstance.defaults.headers.common['Authorization'];
    localStorage.removeItem('authToken');
  }
}

export const tasksApiClient = new TasksApiClient();
export default tasksApiClient;
