import { useEffect } from 'react';
import { KanbanBoard as KanbanBoardType, TaskFilter } from '../../types/task.types';
import KanbanColumn from './KanbanColumn';
import { useTasks } from '../../hooks/useTasks';
import { useRealtime } from '../../hooks/useRealtime';
import { useTasksStore } from '../../store/tasksStore';

interface KanbanBoardProps {
  board: KanbanBoardType;
  projectId: number;
  onFiltersChange?: (filters: Partial<TaskFilter>) => void;
}

/**
 * Kanban Board Component
 * Displays tasks organized by status columns with real-time collaboration
 * Supports drag-drop between columns and real-time updates from other users
 */
export default function KanbanBoard({ board, projectId }: KanbanBoardProps) {
  const { useUpdateTaskStatus } = useTasks();
  const { status: realtimeStatus, joinProject, leaveProject } = useRealtime();
  const initializeRealtimeListeners = useTasksStore(s => s.initializeRealtimeListeners);

  useUpdateTaskStatus(); // Ready for drag-drop integration

  // Setup real-time listeners and join project
  useEffect(() => {
    if (realtimeStatus === 'connected') {
      // Join project to receive real-time updates
      joinProject(projectId).catch(error => {
        console.error('Failed to join project:', error);
      });

      // Initialize real-time event listeners
      initializeRealtimeListeners(projectId);

      // Cleanup on unmount
      return () => {
        leaveProject(projectId).catch(error => {
          console.error('Failed to leave project:', error);
        });
      };
    }
  }, [realtimeStatus, projectId, joinProject, leaveProject, initializeRealtimeListeners]);

  return (
    <div className="flex gap-4 overflow-x-auto pb-4 flex-1">
      {realtimeStatus !== 'connected' && (
        <div className="absolute top-4 right-4 text-amber-600 text-sm">
          {realtimeStatus === 'disconnected' && '⚠️ Offline - changes will sync when connected'}
          {realtimeStatus === 'connecting' && '🔄 Connecting...'}
          {realtimeStatus === 'reconnecting' && '🔄 Reconnecting...'}
          {realtimeStatus === 'error' && '❌ Connection error - retrying...'}
        </div>
      )}
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
