import { useMemo, useState } from 'react';
import { useMyTasks } from '../../hooks/useMyTasks';
import { TaskPriority, TaskStatus } from '../../types/task.types';

const statusOrder: TaskStatus[] = [
  TaskStatus.ToDo,
  TaskStatus.InProgress,
  TaskStatus.InReview,
  TaskStatus.Done,
];

const priorityOptions: Array<TaskPriority | 'All'> = [
  'All',
  TaskPriority.High,
  TaskPriority.Medium,
  TaskPriority.Low,
];

const statusOptions: Array<TaskStatus | 'All'> = ['All', ...statusOrder];

export default function MyTasksPage() {
  const [statusFilter, setStatusFilter] = useState<TaskStatus | 'All'>('All');
  const [priorityFilter, setPriorityFilter] = useState<TaskPriority | 'All'>('All');

  const { tasks, isLoading, error, setFilters, clearFilters } = useMyTasks();

  const groupedTasks = useMemo(() => {
    const groups: Record<TaskStatus, Task[]> = {
      [TaskStatus.ToDo]: [],
      [TaskStatus.InProgress]: [],
      [TaskStatus.InReview]: [],
      [TaskStatus.Done]: [],
    };

    tasks.forEach((task) => {
      groups[task.status].push(task);
    });

    return groups;
  }, [tasks]);

  const overdueCount = useMemo(() => tasks.filter((task) => task.isOverdue).length, [tasks]);

  const handleStatusChange = (value: TaskStatus | 'All') => {
    setStatusFilter(value);
    setFilters({ status: value === 'All' ? undefined : value, page: 1 });
  };

  const handlePriorityChange = (value: TaskPriority | 'All') => {
    setPriorityFilter(value);
    setFilters({ priority: value === 'All' ? undefined : value, page: 1 });
  };

  const handleClearFilters = () => {
    setStatusFilter('All');
    setPriorityFilter('All');
    clearFilters();
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">My Tasks</h1>
            <p className="text-gray-600 mt-1">
              Tasks assigned to you across all projects.
            </p>
          </div>
          <div className="flex flex-wrap gap-3">
            <div className="flex items-center gap-2">
              <label className="text-sm text-gray-600">Status</label>
              <select
                value={statusFilter}
                onChange={(event) => handleStatusChange(event.target.value as TaskStatus | 'All')}
                className="rounded-md border border-gray-300 bg-white px-3 py-2 text-sm"
              >
                {statusOptions.map((status) => (
                  <option key={status} value={status}>
                    {status}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex items-center gap-2">
              <label className="text-sm text-gray-600">Priority</label>
              <select
                value={priorityFilter}
                onChange={(event) => handlePriorityChange(event.target.value as TaskPriority | 'All')}
                className="rounded-md border border-gray-300 bg-white px-3 py-2 text-sm"
              >
                {priorityOptions.map((priority) => (
                  <option key={priority} value={priority}>
                    {priority}
                  </option>
                ))}
              </select>
            </div>
            <button
              type="button"
              onClick={handleClearFilters}
              className="rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-gray-700 hover:bg-gray-100"
            >
              Clear filters
            </button>
          </div>
        </div>

        {/* Summary */}
        <div className="grid gap-4 mt-6 sm:grid-cols-2 lg:grid-cols-3">
          <div className="rounded-lg bg-white border border-gray-200 p-4">
            <p className="text-sm text-gray-500">Total tasks</p>
            <p className="text-2xl font-semibold text-gray-900">{tasks.length}</p>
          </div>
          <div className="rounded-lg bg-white border border-gray-200 p-4">
            <p className="text-sm text-gray-500">Overdue</p>
            <p className="text-2xl font-semibold text-red-600">{overdueCount}</p>
          </div>
          <div className="rounded-lg bg-white border border-gray-200 p-4">
            <p className="text-sm text-gray-500">In progress</p>
            <p className="text-2xl font-semibold text-blue-600">
              {groupedTasks[TaskStatus.InProgress].length}
            </p>
          </div>
        </div>

        {/* Error */}
        {error && (
          <div className="mt-6 rounded-md bg-red-50 p-4 text-red-700">
            {error instanceof Error ? error.message : 'Failed to load tasks'}
          </div>
        )}

        {/* Loading */}
        {isLoading && (
          <div className="mt-8 text-center text-gray-500">Loading tasks...</div>
        )}

        {/* Task Groups */}
        {!isLoading && tasks.length === 0 && !error && (
          <div className="mt-12 text-center text-gray-500">
            No tasks assigned to you yet.
          </div>
        )}

        {!isLoading && tasks.length > 0 && (
          <div className="mt-8 grid gap-6 lg:grid-cols-4">
            {statusOrder.map((status) => (
              <div key={status} className="bg-white border border-gray-200 rounded-lg p-4">
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-sm font-semibold text-gray-700">{status}</h2>
                  <span className="text-xs text-gray-500">
                    {groupedTasks[status].length}
                  </span>
                </div>
                <div className="space-y-3">
                  {groupedTasks[status].length === 0 ? (
                    <p className="text-xs text-gray-400">No tasks</p>
                  ) : (
                    groupedTasks[status].map((task) => (
                      <div
                        key={task.id}
                        className="rounded-md border border-gray-200 bg-gray-50 p-3"
                      >
                        <p className="text-sm font-medium text-gray-900 truncate">
                          {task.title}
                        </p>
                        <p className="text-xs text-gray-500 truncate">{task.projectName}</p>
                        <div className="mt-2 flex items-center justify-between text-xs">
                          <span
                            className={`rounded-full px-2 py-1 font-medium ${
                              task.priority === TaskPriority.High
                                ? 'bg-red-100 text-red-700'
                                : task.priority === TaskPriority.Medium
                                ? 'bg-yellow-100 text-yellow-700'
                                : 'bg-green-100 text-green-700'
                            }`}
                          >
                            {task.priority}
                          </span>
                          {task.dueDate && (
                            <span className={task.isOverdue ? 'text-red-600' : 'text-gray-500'}>
                              {new Date(task.dueDate).toLocaleDateString()}
                            </span>
                          )}
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
