import { useState } from 'react';
import { useTasks } from '../../hooks/useTasks';
import { TaskStatus } from '../../types/task.types';
import TaskForm from './TaskForm';

interface TaskDetailProps {
  taskId: number;
  onClose: () => void;
}

/**
 * Task Detail Modal Component
 * Displays full task information including comments, history, and edit options
 */
export default function TaskDetail({ taskId, onClose }: TaskDetailProps) {
  const [isEditing, setIsEditing] = useState(false);
  const { useTask, useUpdateTaskStatus } = useTasks();
  const { data: task, isLoading, error } = useTask(taskId);
  const { mutate: updateStatus } = useUpdateTaskStatus();

  const handleStatusChange = (newStatus: TaskStatus) => {
    updateStatus({ taskId, newStatus });
  };

  const handleCloseModal = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <div
      className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      onClick={handleCloseModal}
    >
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-96 overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <h2 className="text-2xl font-bold text-gray-900">Task Details</h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 text-2xl"
          >
            ✕
          </button>
        </div>

        {/* Loading State */}
        {isLoading && (
          <div className="p-6 text-center">
            <p className="text-gray-500">Loading task...</p>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="p-6 bg-red-50 border border-red-200 rounded-lg text-red-700">
            {error instanceof Error ? error.message : 'Failed to load task'}
          </div>
        )}

        {/* Content */}
        {!isLoading && task && (
          <div className="p-6 space-y-6">
            {/* Task Title and Status */}
            <div>
              <h1 className="text-xl font-bold text-gray-900 mb-4">{task.title}</h1>
              
              <div className="flex items-center gap-4">
                <label className="text-sm font-medium text-gray-700">Status:</label>
                <select
                  value={task.status}
                  onChange={(e) => handleStatusChange(e.target.value as TaskStatus)}
                  className="px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  {Object.values(TaskStatus).map((status) => (
                    <option key={status} value={status}>
                      {status}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            {/* Description */}
            {task.description && (
              <div>
                <h3 className="text-sm font-medium text-gray-700 mb-2">Description</h3>
                <p className="text-sm text-gray-600 bg-gray-50 p-3 rounded">{task.description}</p>
              </div>
            )}

            {/* Task Metadata */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-xs font-medium text-gray-500 uppercase">Priority</p>
                <p className="text-sm text-gray-900 mt-1">{task.priority}</p>
              </div>
              <div>
                <p className="text-xs font-medium text-gray-500 uppercase">Assignee</p>
                <p className="text-sm text-gray-900 mt-1">
                  {task.assignee?.firstName && task.assignee?.lastName
                    ? `${task.assignee.firstName} ${task.assignee.lastName}`
                    : task.assignee?.email || 'Unassigned'}
                </p>
              </div>
              <div>
                <p className="text-xs font-medium text-gray-500 uppercase">Due Date</p>
                <p className="text-sm text-gray-900 mt-1">
                  {task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'No due date'}
                </p>
              </div>
              <div>
                <p className="text-xs font-medium text-gray-500 uppercase">Created</p>
                <p className="text-sm text-gray-900 mt-1">
                  {new Date(task.createdAt).toLocaleDateString()}
                </p>
              </div>
            </div>

            {/* Comments Section */}
            <div>
              <h3 className="text-sm font-medium text-gray-700 mb-3">
                Comments ({task.commentCount})
              </h3>
              <div className="bg-gray-50 rounded p-3 min-h-20">
                <p className="text-sm text-gray-600">Comments feature coming soon...</p>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex gap-3 pt-4 border-t border-gray-200">
              <button
                onClick={() => setIsEditing(!isEditing)}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition font-medium"
              >
                {isEditing ? 'Cancel' : 'Edit'}
              </button>
              <button
                onClick={onClose}
                className="flex-1 px-4 py-2 bg-gray-200 text-gray-900 rounded-lg hover:bg-gray-300 transition font-medium"
              >
                Close
              </button>
            </div>

            {/* Edit Form */}
            {isEditing && <TaskForm taskId={taskId} onClose={() => setIsEditing(false)} />}
          </div>
        )}
      </div>
    </div>
  );
}
