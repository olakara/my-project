// Task-related types for the frontend application

export enum TaskStatus {
  ToDo = 'ToDo',
  InProgress = 'InProgress',
  InReview = 'InReview',
  Done = 'Done',
}

export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
}

export interface User {
  id: string;
  firstName?: string;
  lastName?: string;
  email: string;
}

export interface Task {
  id: number;
  projectId: number;
  projectName: string;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  assignee?: User;
  createdBy: User;
  dueDate?: string; // ISO date string
  createdAt: string; // ISO timestamp
  updatedAt: string; // ISO timestamp
  commentCount: number;
  isOverdue: boolean;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  assigneeId?: string;
  priority?: TaskPriority;
  dueDate?: string; // ISO date string
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string;
  assigneeId?: string;
  priority?: TaskPriority;
  dueDate?: string;
}

export interface UpdateTaskStatusRequest {
  newStatus: TaskStatus;
}

export interface TaskCard {
  id: number;
  title: string;
  description?: string;
  priority: TaskPriority;
  assigneeName?: string;
  dueDate?: string;
  createdTimestamp: string;
  isOverdue: boolean;
}

export interface KanbanColumn {
  status: TaskStatus;
  statusLabel: string;
  tasks: TaskCard[];
  count: number;
}

export interface KanbanBoard {
  columns: KanbanColumn[];
  totalTasks: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
}

export interface TaskDetail extends Task {
  comments: Comment[];
  history: TaskHistory[];
}

export interface Comment {
  id: number;
  taskId: number;
  content: string;
  author: User;
  createdAt: string;
  updatedAt: string;
}

export interface TaskHistory {
  id: number;
  taskId: number;
  changeType: string;
  oldValue?: string;
  newValue?: string;
  changedBy: User;
  changedTimestamp: string;
}

export interface TaskFilter {
  assigneeId?: string;
  priority?: TaskPriority;
  dueDate?: string;
  status?: TaskStatus;
  page?: number;
  pageSize?: number;
}

export interface TasksState {
  tasks: Task[];
  selectedTask?: Task;
  taskDetail?: TaskDetail;
  kanbanBoard?: KanbanBoard;
  isLoading: boolean;
  error?: string;
  filters: TaskFilter;
}
