import { KanbanColumn as KanbanColumnType } from '../../types/task.types';
import TaskCard from '../tasks/TaskCard';

interface KanbanColumnProps {
  column: KanbanColumnType;
}

/**
 * Kanban Column Component
 * Displays a single column of tasks for a specific status
 */
export default function KanbanColumn({ column }: KanbanColumnProps) {
  return (
    <div className="flex flex-col h-full">
      {/* Column Header */}
      <div className="p-4 border-b border-gray-200 bg-gray-50">
        <h2 className="font-semibold text-gray-900">{column.statusLabel}</h2>
        <p className="text-sm text-gray-600 mt-1">{column.count} tasks</p>
      </div>

      {/* Tasks Container */}
      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        {column.tasks.length === 0 ? (
          <div className="flex items-center justify-center h-24 text-gray-400">
            <p className="text-sm">No tasks yet</p>
          </div>
        ) : (
          column.tasks.map((task) => (
            <TaskCard key={task.id} task={task} />
          ))
        )}
      </div>
    </div>
  );
}
