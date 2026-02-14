import { create } from 'zustand';
import { Task, TaskFilter, TasksState, TaskStatus } from '../types/task.types';
import tasksApiClient from '../services/api/tasksApi';
import { signalRService } from '../services/signalr/signalrService';

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
  
  // Real-time sync
  initializeRealtimeListeners: (projectId: number) => void;
  cleanupRealtimeListeners: () => void;
  handleTaskCreated: (task: Task) => void;
  handleTaskUpdated: (task: Task) => void;
  handleTaskStatusChanged: (taskId: number, newStatus: TaskStatus) => void;
  handleCommentAdded: (taskId: number, comment: any) => void;
  
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

  // Real-time sync handlers
  initializeRealtimeListeners: (projectId: number) => {
    // Listen for new tasks
    signalRService.onEvent('TaskCreated', (task: Task) => {
      if (task.projectId === projectId) {
        get().handleTaskCreated(task);
      }
    });

    // Listen for task updates
    signalRService.onEvent('TaskUpdated', (task: Task) => {
      if (task.projectId === projectId) {
        get().handleTaskUpdated(task);
      }
    });

    // Listen for task status changes
    signalRService.onEvent('TaskStatusChanged', (data: { taskId: number; newStatus: TaskStatus }) => {
      get().handleTaskStatusChanged(data.taskId, data.newStatus);
    });

    // Listen for comments
    signalRService.onEvent('CommentAdded', (data: { taskId: number; comment: any }) => {
      get().handleCommentAdded(data.taskId, data.comment);
    });

    // Listen for task deletions
    signalRService.onEvent('TaskDeleted', (data: { taskId: number }) => {
      const state = get();
      set({
        tasks: state.tasks.filter(t => t.id !== data.taskId),
        selectedTask: state.selectedTask?.id === data.taskId ? undefined : state.selectedTask,
      });
    });
  },

  cleanupRealtimeListeners: () => {
    // Unsubscribe all real-time listeners
    signalRService.onEvent('TaskCreated', () => {}); // No-op to cleanup
  },

  handleTaskCreated: (task: Task) => {
    const state = get();
    set({ tasks: [task, ...state.tasks] });
  },

  handleTaskUpdated: (task: Task) => {
    const state = get();
    set({
      tasks: state.tasks.map(t => t.id === task.id ? task : t),
      selectedTask: state.selectedTask?.id === task.id ? task : state.selectedTask,
    });
  },

  handleTaskStatusChanged: (taskId: number, newStatus: TaskStatus) => {
    const state = get();
    const updatedTask = state.tasks.find(t => t.id === taskId);
    if (updatedTask) {
      const task = { ...updatedTask, status: newStatus };
      set({
        tasks: state.tasks.map(t => t.id === taskId ? task : t),
        selectedTask: state.selectedTask?.id === taskId ? task : state.selectedTask,
      });
    }
  },

  handleCommentAdded: (taskId: number, _comment: any) => {
    const state = get();
    const task = state.tasks.find(t => t.id === taskId);
    if (task) {
      const updatedTask = { ...task, commentCount: (task.commentCount || 0) + 1 };
      set({
        tasks: state.tasks.map(t => t.id === taskId ? updatedTask : t),
        selectedTask: state.selectedTask?.id === taskId ? updatedTask : state.selectedTask,
      });
    }
  },
}));
