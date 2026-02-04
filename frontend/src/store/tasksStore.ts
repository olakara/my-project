import { create } from 'zustand';
import { Task, TaskFilter, TasksState, TaskStatus } from '../types/task.types';
import tasksApiClient from '../services/api/tasksApi';

interface TasksStore extends TasksState {
  // Actions
  fetchKanbanBoard: (projectId: number, filters?: TaskFilter) => Promise<void>;
  fetchProjectTasks: (projectId: number, filters?: TaskFilter) => Promise<void>;
  fetchTask: (taskId: number) => Promise<void>;
  createTask: (projectId: number, title: string, description?: string) => Promise<Task>;
  updateTask: (taskId: number, updates: Partial<Task>) => Promise<Task>;
  updateTaskStatus: (taskId: number, newStatus: TaskStatus) => Promise<Task>;
  assignTask: (taskId: number, assigneeId: string) => Promise<Task>;
  deleteTask: (taskId: number) => Promise<void>;
  addComment: (taskId: number, content: string) => Promise<void>;
  
  // Filter management
  setFilter: (filter: Partial<TaskFilter>) => void;
  clearFilters: () => void;
  
  // UI state
  setLoading: (loading: boolean) => void;
  setError: (error?: string) => void;
  clearError: () => void;
}

const initialFilters: TaskFilter = {
  pageSize: 50,
  page: 1,
};

const initialState: TasksState = {
  tasks: [],
  isLoading: false,
  filters: initialFilters,
};

export const useTasksStore = create<TasksStore>((set, get) => ({
  ...initialState,

  fetchKanbanBoard: async (projectId: number, filters?: TaskFilter) => {
    set({ isLoading: true, error: undefined });
    try {
      const kanbanBoard = await tasksApiClient.getKanbanBoard(projectId, filters);
      set({ kanbanBoard, isLoading: false });
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to fetch Kanban board';
      set({ error: errorMessage, isLoading: false });
    }
  },

  fetchProjectTasks: async (projectId: number, filters?: TaskFilter) => {
    set({ isLoading: true, error: undefined });
    try {
      const tasks = await tasksApiClient.getProjectTasks(projectId, filters);
      set({ tasks, isLoading: false });
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to fetch tasks';
      set({ error: errorMessage, isLoading: false });
    }
  },

  fetchTask: async (taskId: number) => {
    set({ isLoading: true, error: undefined });
    try {
      const taskDetail = await tasksApiClient.getTask(taskId);
      set({ taskDetail, selectedTask: taskDetail as Task, isLoading: false });
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to fetch task';
      set({ error: errorMessage, isLoading: false });
    }
  },

  createTask: async (projectId: number, title: string, description?: string) => {
    try {
      const newTask = await tasksApiClient.createTask(projectId, {
        title,
        description,
      });
      const state = get();
      set({ tasks: [newTask, ...state.tasks] });
      return newTask;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to create task';
      set({ error: errorMessage });
      throw error;
    }
  },

  updateTask: async (taskId: number, updates: Partial<Task>) => {
    try {
      const updatedTask = await tasksApiClient.updateTask(taskId, {
        title: updates.title,
        description: updates.description,
        priority: updates.priority,
        dueDate: updates.dueDate,
      });
      const state = get();
      set({
        tasks: state.tasks.map(t => t.id === taskId ? updatedTask : t),
        selectedTask: state.selectedTask?.id === taskId ? updatedTask : state.selectedTask,
      });
      return updatedTask;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to update task';
      set({ error: errorMessage });
      throw error;
    }
  },

  updateTaskStatus: async (taskId: number, newStatus: TaskStatus) => {
    try {
      const updatedTask = await tasksApiClient.updateTaskStatus(taskId, { newStatus });
      const state = get();
      set({
        tasks: state.tasks.map(t => t.id === taskId ? updatedTask : t),
        selectedTask: state.selectedTask?.id === taskId ? updatedTask : state.selectedTask,
      });
      return updatedTask;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to update task status';
      set({ error: errorMessage });
      throw error;
    }
  },

  assignTask: async (taskId: number, assigneeId: string) => {
    try {
      const updatedTask = await tasksApiClient.assignTask(taskId, assigneeId);
      const state = get();
      set({
        tasks: state.tasks.map(t => t.id === taskId ? updatedTask : t),
        selectedTask: state.selectedTask?.id === taskId ? updatedTask : state.selectedTask,
      });
      return updatedTask;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to assign task';
      set({ error: errorMessage });
      throw error;
    }
  },

  deleteTask: async (taskId: number) => {
    try {
      await tasksApiClient.deleteTask(taskId);
      const state = get();
      set({ tasks: state.tasks.filter(t => t.id !== taskId) });
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to delete task';
      set({ error: errorMessage });
      throw error;
    }
  },

  addComment: async (taskId: number, content: string) => {
    try {
      await tasksApiClient.addComment(taskId, content);
      // Optionally fetch task again to get updated comments
      await get().fetchTask(taskId);
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to add comment';
      set({ error: errorMessage });
      throw error;
    }
  },

  setFilter: (filter: Partial<TaskFilter>) => {
    const state = get();
    set({ filters: { ...state.filters, ...filter } });
  },

  clearFilters: () => {
    set({ filters: initialFilters });
  },

  setLoading: (loading: boolean) => {
    set({ isLoading: loading });
  },

  setError: (error?: string) => {
    set({ error });
  },

  clearError: () => {
    set({ error: undefined });
  },
}));
