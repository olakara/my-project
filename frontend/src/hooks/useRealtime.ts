import { useEffect, useRef, useCallback, useState } from 'react';
import { signalRService, ConnectionStatus, SignalREventHandler } from '../services/signalr/signalrService';

interface UseRealtimeOptions {
  autoConnect?: boolean;
  onConnected?: () => void;
  onDisconnected?: () => void;
  onError?: (error: Error) => void;
}

/**
 * Hook for managing real-time event subscriptions via SignalR
 *
 * @example
 * const { status, subscribe, joinProject, leaveProject } = useRealtime();
 *
 * useEffect(() => {
 *   if (status === 'connected') {
 *     joinProject(projectId);
 *     const unsubscribe = subscribe('TaskCreated', (task) => {
 *       console.log('New task:', task);
 *     });
 *     return unsubscribe;
 *   }
 * }, [status, projectId]);
 */
export const useRealtime = (options: UseRealtimeOptions = {}) => {
  const { autoConnect = true, onConnected, onDisconnected, onError } = options;
  const [status, setStatus] = useState<ConnectionStatus>('disconnected');
  const subscriptionsRef = useRef<Array<() => void>>([]);
  const statusUnsubscribeRef = useRef<(() => void) | null>(null);

  /**
   * Subscribe to a real-time event
   */
  const subscribe = useCallback(
    (eventType: string, handler: SignalREventHandler): (() => void) => {
      const unsubscribe = signalRService.onEvent(eventType, handler);
      subscriptionsRef.current.push(unsubscribe);
      return unsubscribe;
    },
    []
  );

  /**
   * Join a project room to receive updates
   */
  const joinProject = useCallback(async (projectId: number): Promise<void> => {
    try {
      await signalRService.joinProject(projectId);
    } catch (error) {
      console.error('Error joining project:', error);
      onError?.(error instanceof Error ? error : new Error(String(error)));
    }
  }, [onError]);

  /**
   * Leave a project room
   */
  const leaveProject = useCallback(async (projectId: number): Promise<void> => {
    try {
      await signalRService.leaveProject(projectId);
    } catch (error) {
      console.error('Error leaving project:', error);
      onError?.(error instanceof Error ? error : new Error(String(error)));
    }
  }, [onError]);

  /**
   * Send typing indicator
   */
  const sendTypingIndicator = useCallback(
    async (taskId: number, isTyping: boolean): Promise<void> => {
      try {
        await signalRService.sendTypingIndicator(taskId, isTyping);
      } catch (error) {
        console.error('Error sending typing indicator:', error);
        onError?.(error instanceof Error ? error : new Error(String(error)));
      }
    },
    [onError]
  );

  /**
   * Manually connect
   */
  const connect = useCallback(async (): Promise<void> => {
    try {
      await signalRService.connect();
    } catch (error) {
      console.error('Error connecting:', error);
      onError?.(error instanceof Error ? error : new Error(String(error)));
    }
  }, [onError]);

  /**
   * Manually disconnect
   */
  const disconnect = useCallback(async (): Promise<void> => {
    try {
      await signalRService.disconnect();
    } catch (error) {
      console.error('Error disconnecting:', error);
      onError?.(error instanceof Error ? error : new Error(String(error)));
    }
  }, [onError]);

  // Initialize connection and handle lifecycle
  useEffect(() => {
    if (autoConnect) {
      signalRService.connect().catch(error => {
        console.error('Failed to auto-connect:', error);
        onError?.(error instanceof Error ? error : new Error(String(error)));
      });
    }

    // Subscribe to status changes
    statusUnsubscribeRef.current = signalRService.onStatusChange((newStatus: ConnectionStatus) => {
      setStatus(newStatus);

      if (newStatus === 'connected') {
        onConnected?.();
      } else if (newStatus === 'disconnected') {
        onDisconnected?.();
      }
    });

    return () => {
      // Clean up all event subscriptions
      subscriptionsRef.current.forEach(unsubscribe => unsubscribe());
      subscriptionsRef.current = [];

      // Unsubscribe from status changes
      statusUnsubscribeRef.current?.();
    };
  }, [autoConnect, onConnected, onDisconnected, onError]);

  return {
    status,
    subscribe,
    joinProject,
    leaveProject,
    sendTypingIndicator,
    connect,
    disconnect,
    isConnected: signalRService.isConnected(),
  };
};
