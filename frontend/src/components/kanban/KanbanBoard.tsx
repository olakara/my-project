import { KanbanBoard as KanbanBoardType, TaskFilter } from '../../types/task.types';
import KanbanColumn from './KanbanColumn';
import { useTasks } from '../../hooks/useTasks';

interface KanbanBoardProps {
  board: KanbanBoardType;
  projectId: number;
  onFiltersChange?: (filters: Partial<TaskFilter>) => void;
}

/**
 * Kanban Board Component
 * Displays tasks organized by status columns
 * NOTE: Drag-drop will be implemented when @hello-pangea/dnd is installed
 * For now, status updates can be done via task detail modal
 */
export default function KanbanBoard({ board }: KanbanBoardProps) {
  const { useUpdateTaskStatus } = useTasks();
  useUpdateTaskStatus(); // Ready for drag-drop integration

  return (
    <div className="flex gap-4 overflow-x-auto pb-4 flex-1">
      {board.columns.map((column) => (
        <div
          key={column.status}
          className="flex-shrink-0 w-80 rounded-lg border-2 border-gray-200 bg-white"
        >
          <KanbanColumn column={column} />
        </div>
      ))}
    </div>
  );
}
