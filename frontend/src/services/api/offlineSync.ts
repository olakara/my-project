/**
 * Offline Sync Service
 *
 * Handles queueing mutations when offline and syncing when connection is restored.
 * Uses localStorage to persist pending mutations across page reloads.
 */

import tasksApiClient from './tasksApi';

export type MutationType = 'createTask' | 'updateTask' | 'updateTaskStatus' | 'assignTask' | 'deleteTask' | 'addComment';

export interface QueuedMutation {
  id: string;
  type: MutationType;
  timestamp: number;
  projectId: number;
  payload: any;
  retryCount: number;
  maxRetries: number;
}

const STORAGE_KEY = 'task_mutations_queue';
const MAX_QUEUE_SIZE = 50;
const MAX_RETRIES = 3;

class OfflineSyncService {
  private queue: Map<string, QueuedMutation> = new Map();
  private listeners: Set<(queue: QueuedMutation[]) => void> = new Set();
  private processingRef = false;
  private isOnline = navigator.onLine;

  constructor() {
    // Load persisted queue from localStorage
    this.loadQueueFromStorage();

    // Listen for online/offline events
    window.addEventListener('online', () => this.handleOnline());
    window.addEventListener('offline', () => this.handleOffline());
  }

  /**
   * Queue a mutation to be executed when online
   */
  queueMutation(
    type: MutationType,
    projectId: number,
    payload: any
  ): QueuedMutation {
    // Check queue size limit
    if (this.queue.size >= MAX_QUEUE_SIZE) {
      throw new Error(
        `Offline mutation queue is full (max ${MAX_QUEUE_SIZE}). Please sync before adding more changes.`
      );
    }

    const mutation: QueuedMutation = {
      id: `${type}_${Date.now()}_${Math.random()}`,
      type,
      timestamp: Date.now(),
      projectId,
      payload,
      retryCount: 0,
      maxRetries: MAX_RETRIES,
    };

    this.queue.set(mutation.id, mutation);
    this.persistQueueToStorage();
    this.notifyListeners();

    return mutation;
  }

  /**
   * Get all queued mutations
   */
  getQueue(): QueuedMutation[] {
    return Array.from(this.queue.values());
  }

  /**
   * Subscribe to queue changes
   */
  onQueueChange(listener: (queue: QueuedMutation[]) => void): () => void {
    this.listeners.add(listener);
    return () => {
      this.listeners.delete(listener);
    };
  }

  /**
   * Process queued mutations when coming back online
   */
  private async processMutations(): Promise<void> {
    if (this.processingRef || !this.isOnline || this.queue.size === 0) {
      return;
    }

    this.processingRef = true;

    try {
      const mutations = Array.from(this.queue.values())
        .sort((a, b) => a.timestamp - b.timestamp);

      for (const mutation of mutations) {
        try {
          await this.executeMutation(mutation);
          this.queue.delete(mutation.id);
          this.persistQueueToStorage();
          this.notifyListeners();
        } catch (error) {
          mutation.retryCount++;

          if (mutation.retryCount >= mutation.maxRetries) {
            console.error(`Failed to sync mutation ${mutation.id} after ${MAX_RETRIES} retries:`, error);
            this.queue.delete(mutation.id);
          }

          this.persistQueueToStorage();
        }
      }
    } finally {
      this.processingRef = false;
    }
  }

  /**
   * Execute a queued mutation
   */
  private async executeMutation(mutation: QueuedMutation): Promise<any> {
    const { type, payload } = mutation;

    switch (type) {
      case 'createTask':
        return await tasksApiClient.createTask(mutation.projectId, payload);

      case 'updateTask':
        return await tasksApiClient.updateTask(payload.id, payload.data);

      case 'updateTaskStatus':
        return await tasksApiClient.updateTaskStatus(payload.id, { newStatus: payload.newStatus });

      case 'assignTask':
        return await tasksApiClient.assignTask(payload.id, payload.assigneeId);

      case 'deleteTask':
        return await tasksApiClient.deleteTask(payload.id);

      case 'addComment':
        return await tasksApiClient.addComment(payload.taskId, payload.content);

      default:
        throw new Error(`Unknown mutation type: ${type}`);
    }
  }

  /**
   * Clear a specific mutation from the queue
   */
  clearMutation(mutationId: string): void {
    this.queue.delete(mutationId);
    this.persistQueueToStorage();
    this.notifyListeners();
  }

  /**
   * Clear all mutations (e.g., on logout)
   */
  clearAll(): void {
    this.queue.clear();
    this.persistQueueToStorage();
    this.notifyListeners();
  }

  /**
   * Persist queue to localStorage
   */
  private persistQueueToStorage(): void {
    try {
      const queueArray = Array.from(this.queue.values());
      localStorage.setItem(STORAGE_KEY, JSON.stringify(queueArray));
    } catch (error) {
      console.error('Failed to persist queue to storage:', error);
    }
  }

  /**
   * Load queue from localStorage
   */
  private loadQueueFromStorage(): void {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        const queueArray = JSON.parse(stored) as QueuedMutation[];
        queueArray.forEach(mutation => {
          this.queue.set(mutation.id, mutation);
        });
        this.notifyListeners();
      }
    } catch (error) {
      console.error('Failed to load queue from storage:', error);
      localStorage.removeItem(STORAGE_KEY);
    }
  }

  /**
   * Handle coming online
   */
  private handleOnline(): void {
    console.log('Online - processing queued mutations');
    this.isOnline = true;
    this.processMutations();
  }

  /**
   * Handle going offline
   */
  private handleOffline(): void {
    console.log('Offline - mutations will be queued');
    this.isOnline = false;
  }

  /**
   * Notify all listeners of queue changes
   */
  private notifyListeners(): void {
    const queue = Array.from(this.queue.values());
    this.listeners.forEach(listener => listener(queue));
  }

  /**
   * Get current online status
   */
  isCurrentlyOnline(): boolean {
    return this.isOnline;
  }
}

// Create singleton instance
export const offlineSyncService = new OfflineSyncService();
