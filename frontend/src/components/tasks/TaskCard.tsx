import { useState } from 'react';
import { TaskCard as TaskCardType, TaskPriority } from '../../types/task.types';
import TaskDetail from './TaskDetail';

interface TaskCardProps {
  task: TaskCardType;
  onClick?: () => void;
}

/**
 * Task Card Component
 * Displays task summary with title, assignee, priority, and due date
 */
export default function TaskCard({ task }: TaskCardProps) {
  const [showDetail, setShowDetail] = useState(false);

  const handleOpenDetail = () => {
    setShowDetail(true);
  };

  const handleCloseDetail = () => {
    setShowDetail(false);
  };

  const getPriorityColor = (priority: TaskPriority) => {
    switch (priority) {
      case TaskPriority.High:
        return 'bg-red-100 text-red-800 border-red-300';
      case TaskPriority.Medium:
        return 'bg-yellow-100 text-yellow-800 border-yellow-300';
      case TaskPriority.Low:
        return 'bg-green-100 text-green-800 border-green-300';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-300';
    }
  };

  const getOverdueColor = (isOverdue: boolean) => {
    return isOverdue ? 'text-red-600 font-semibold' : 'text-gray-600';
  };

  return (
    <>
      <div
        className="p-3 bg-white border border-gray-200 rounded-lg hover:shadow-md transition cursor-pointer group"
        onClick={handleOpenDetail}
      >
        {/* Task Title */}
        <h3 className="font-medium text-sm text-gray-900 group-hover:text-blue-600 truncate">
          {task.title}
        </h3>

        {/* Priority Badge */}
        <div className="mt-2">
          <span
            className={`inline-block px-2 py-1 rounded-full text-xs font-medium border ${getPriorityColor(
              task.priority
            )}`}
          >
            {task.priority}
          </span>
        </div>

        {/* Assignee and Due Date */}
        <div className="mt-2 space-y-1 text-xs">
          {/* Assignee */}
          {task.assigneeName && (
            <div className="text-gray-600 truncate">
              👤 {task.assigneeName}
            </div>
          )}

          {/* Due Date */}
          {task.dueDate && (
            <div className={getOverdueColor(task.isOverdue)}>
              📅 {new Date(task.dueDate).toLocaleDateString()}
              {task.isOverdue && ' ⚠️'}
            </div>
          )}
        </div>
      </div>

      {/* Task Detail Modal */}
      {showDetail && (
        <TaskDetail taskId={task.id} onClose={handleCloseDetail} />
      )}
    </>
  );
}
