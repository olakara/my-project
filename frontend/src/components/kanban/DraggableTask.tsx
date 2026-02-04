import { TaskCard as TaskCardType } from '../../types/task.types';

interface DraggableTaskProps {
  task: TaskCardType;
  onTaskClick?: (task: TaskCardType) => void;
}

/**
 * Draggable Task Card Component
 * Represents a single task in the Kanban board
 * NOTE: Drag functionality will be added when @hello-pangea/dnd is installed
 */
export default function DraggableTask({ task, onTaskClick }: DraggableTaskProps) {
  return (
    <div
      className="p-3 bg-white border border-gray-200 rounded-lg shadow-sm hover:shadow-md transition cursor-grab active:cursor-grabbing"
      onClick={() => onTaskClick?.(task)}
    >
      {/* Task Title */}
      <h3 className="font-medium text-sm text-gray-900 truncate">{task.title}</h3>

      {/* Task Details */}
      <div className="mt-2 space-y-1 text-xs text-gray-600">
        {/* Priority Badge */}
        <div>
          <span
            className={`inline-block px-2 py-1 rounded text-white font-medium ${
              task.priority === 'High'
                ? 'bg-red-500'
                : task.priority === 'Medium'
                  ? 'bg-yellow-500'
                  : 'bg-green-500'
            }`}
          >
            {task.priority}
          </span>
        </div>

        {/* Assignee */}
        {task.assigneeName && (
          <div className="text-gray-600">
            Assigned to: <span className="font-medium">{task.assigneeName}</span>
          </div>
        )}

        {/* Due Date */}
        {task.dueDate && (
          <div className={task.isOverdue ? 'text-red-600 font-medium' : ''}>
            Due: {new Date(task.dueDate).toLocaleDateString()}
            {task.isOverdue && ' (Overdue)'}
          </div>
        )}
      </div>
    </div>
  );
}
