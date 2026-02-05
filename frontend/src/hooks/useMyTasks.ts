import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import tasksApiClient from '../services/api/tasksApi';
import { Task, TaskFilter } from '../types/task.types';

export interface UseMyTasksOptions {
  initialFilters?: TaskFilter;
}

export const useMyTasks = ({ initialFilters }: UseMyTasksOptions = {}) => {
  const [filters, setFilters] = useState<TaskFilter>({
    page: 1,
    pageSize: 50,
    ...initialFilters,
  });

  const query = useQuery<Task[]>({
    queryKey: ['my-tasks', filters],
    queryFn: () => tasksApiClient.getMyTasks(filters),
    staleTime: 30000,
    retry: 1,
  });

  const tasks = useMemo(() => query.data ?? [], [query.data]);

  const updateFilters = (next: Partial<TaskFilter>) => {
    setFilters((current) => ({
      ...current,
      ...next,
    }));
  };

  const clearFilters = () => {
    setFilters({ page: 1, pageSize: filters.pageSize ?? 50 });
  };

  return {
    tasks,
    filters,
    isLoading: query.isLoading,
    error: query.error,
    refetch: query.refetch,
    setFilters: updateFilters,
    clearFilters,
  };
};

export default useMyTasks;
