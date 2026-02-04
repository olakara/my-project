import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import tasksApiClient from '../services/api/tasksApi';
import { Task, TaskDetail, KanbanBoard, TaskFilter, TaskStatus } from '../types/task.types';

/**
 * Custom hook for managing tasks with React Query
 * Provides queries and mutations for task operations with caching and real-time sync support
 */
export const useTasks = () => {
  const queryClient = useQueryClient();

  // Query: Get Kanban board
  const useKanbanBoard = (projectId: number, filters?: TaskFilter) => {
    return useQuery<KanbanBoard>({
      queryKey: ['kanban-board', projectId, filters],
      queryFn: () => tasksApiClient.getKanbanBoard(projectId, filters),
      staleTime: 30000, // 30 seconds
      retry: 1,
    });
  };

  // Query: Get project tasks
  const useProjectTasks = (projectId: number, filters?: TaskFilter) => {
    return useQuery<Task[]>({
      queryKey: ['project-tasks', projectId, filters],
      queryFn: () => tasksApiClient.getProjectTasks(projectId, filters),
      staleTime: 30000,
      retry: 1,
    });
  };

  // Query: Get task details
  const useTask = (taskId: number) => {
    return useQuery<TaskDetail>({
      queryKey: ['task', taskId],
      queryFn: () => tasksApiClient.getTask(taskId),
      staleTime: 30000,
      retry: 1,
    });
  };

  // Query: Get my tasks
  const useMyTasks = (filters?: TaskFilter) => {
    return useQuery<Task[]>({
      queryKey: ['my-tasks', filters],
      queryFn: () => tasksApiClient.getMyTasks(filters),
      staleTime: 30000,
      retry: 1,
    });
  };

  // Mutation: Create task
  const useCreateTask = (projectId: number) => {
    return useMutation({
      mutationFn: (data: { title: string; description?: string }) =>
        tasksApiClient.createTask(projectId, data),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['kanban-board', projectId] });
        queryClient.invalidateQueries({ queryKey: ['project-tasks', projectId] });
      },
    });
  };

  // Mutation: Update task
  const useUpdateTask = (taskId: number) => {
    return useMutation({
      mutationFn: (data: any) => tasksApiClient.updateTask(taskId, data),
      onSuccess: (data) => {
        queryClient.setQueryData(['task', taskId], data);
        queryClient.invalidateQueries({ queryKey: ['project-tasks'] });
        queryClient.invalidateQueries({ queryKey: ['kanban-board'] });
      },
    });
  };

  // Mutation: Update task status (for drag-drop)
  const useUpdateTaskStatus = () => {
    return useMutation({
      mutationFn: (data: { taskId: number; newStatus: TaskStatus }) =>
        tasksApiClient.updateTaskStatus(data.taskId, { newStatus: data.newStatus }),
      onMutate: async (data) => {
        // Optimistically update cache
        const previousData = queryClient.getQueryData(['task', data.taskId]);
        if (previousData) {
          queryClient.setQueryData(['task', data.taskId], {
            ...previousData,
            status: data.newStatus,
          });
        }
        return { previousData };
      },
      onError: (_error, data, context) => {
        // Revert optimistic update on error
        if (context?.previousData) {
          queryClient.setQueryData(['task', data.taskId], context.previousData);
        }
      },
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['project-tasks'] });
        queryClient.invalidateQueries({ queryKey: ['kanban-board'] });
      },
    });
  };

  // Mutation: Assign task
  const useAssignTask = () => {
    return useMutation({
      mutationFn: (data: { taskId: number; assigneeId: string }) =>
        tasksApiClient.assignTask(data.taskId, data.assigneeId),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['project-tasks'] });
        queryClient.invalidateQueries({ queryKey: ['kanban-board'] });
        queryClient.invalidateQueries({ queryKey: ['my-tasks'] });
      },
    });
  };

  // Mutation: Delete task
  const useDeleteTask = () => {
    return useMutation({
      mutationFn: (taskId: number) => tasksApiClient.deleteTask(taskId),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['project-tasks'] });
        queryClient.invalidateQueries({ queryKey: ['kanban-board'] });
      },
    });
  };

  // Mutation: Add comment
  const useAddComment = (taskId: number) => {
    return useMutation({
      mutationFn: (content: string) => tasksApiClient.addComment(taskId, content),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['task', taskId] });
      },
    });
  };

  return {
    useKanbanBoard,
    useProjectTasks,
    useTask,
    useMyTasks,
    useCreateTask,
    useUpdateTask,
    useUpdateTaskStatus,
    useAssignTask,
    useDeleteTask,
    useAddComment,
  };
};
