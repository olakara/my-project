import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { useTasks } from '../../hooks/useTasks';
import KanbanBoard from '../../components/kanban/KanbanBoard';
import { TaskFilter, TaskPriority } from '../../types/task.types';

/**
 * Kanban Page Component
 * Displays tasks for a project in a Kanban board format with filtering and drag-drop
 */
export default function KanbanPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const [filters, setFilters] = useState<TaskFilter>({
    page: 1,
    pageSize: 50,
  });
  const [selectedPriority, setSelectedPriority] = useState<TaskPriority | undefined>();

  const { useKanbanBoard } = useTasks();
  const { data: kanbanBoard, isLoading, error } = useKanbanBoard(
    parseInt(projectId || '0'),
    filters
  );

  const handleFilterChange = (newFilters: Partial<TaskFilter>) => {
    setFilters((prev) => ({ ...prev, ...newFilters, page: 1 })); // Reset to page 1
  };

  const handlePriorityFilter = (priority?: TaskPriority) => {
    setSelectedPriority(priority);
    handleFilterChange({ priority });
  };

  const handlePageChange = (page: number) => {
    handleFilterChange({ page });
  };

  if (!projectId) {
    return <div className="p-4">Invalid project ID</div>;
  }

  return (
    <div className="flex flex-col gap-4 p-4 h-full bg-gray-50">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">Kanban Board</h1>
        <div className="flex gap-2">
          <button
            className={`px-4 py-2 rounded-lg text-sm font-medium transition ${
              selectedPriority === TaskPriority.High
                ? 'bg-red-500 text-white'
                : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
            }`}
            onClick={() => handlePriorityFilter(
              selectedPriority === TaskPriority.High ? undefined : TaskPriority.High
            )}
          >
            High Priority
          </button>
        </div>
      </div>

      {/* Error Message */}
      {error && (
        <div className="p-4 bg-red-100 border border-red-400 text-red-700 rounded">
          {error instanceof Error ? error.message : 'Failed to load Kanban board'}
        </div>
      )}

      {/* Loading State */}
      {isLoading && (
        <div className="flex items-center justify-center h-96">
          <div className="text-lg text-gray-500">Loading tasks...</div>
        </div>
      )}

      {/* Kanban Board */}
      {!isLoading && kanbanBoard && (
        <>
          <KanbanBoard
            board={kanbanBoard}
            projectId={parseInt(projectId)}
            onFiltersChange={handleFilterChange}
          />

          {/* Pagination Controls */}
          <div className="flex items-center justify-between bg-white p-4 rounded-lg border border-gray-200">
            <div className="text-sm text-gray-600">
              Page {kanbanBoard.currentPage} of {kanbanBoard.totalPages} 
              ({kanbanBoard.totalTasks} total tasks)
            </div>
            <div className="flex gap-2">
              <button
                disabled={kanbanBoard.currentPage === 1}
                onClick={() => handlePageChange(kanbanBoard.currentPage - 1)}
                className="px-3 py-1 rounded border border-gray-300 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
              >
                Previous
              </button>
              <button
                disabled={kanbanBoard.currentPage >= kanbanBoard.totalPages}
                onClick={() => handlePageChange(kanbanBoard.currentPage + 1)}
                className="px-3 py-1 rounded border border-gray-300 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
              >
                Next
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
