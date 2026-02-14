import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../../store/authStore';

export type ConnectionStatus = 'connecting' | 'connected' | 'disconnected' | 'reconnecting' | 'error';

export interface SignalREvent {
  type: string;
  data: any;
}

export interface SignalREventHandler {
  (data: any): void;
}

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private url: string;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 10;
  private reconnectDelay = 3000; // 3 seconds
  private statusListeners: Set<(status: ConnectionStatus) => void> = new Set();
  private eventListeners: Map<string, Set<SignalREventHandler>> = new Map();
  private currentStatus: ConnectionStatus = 'disconnected';

  constructor(baseUrl: string = '') {
    const protocol = window.location.protocol === 'https:' ? 'wss' : 'ws';
    const host = window.location.host;
    this.url = baseUrl || `${protocol}://${host}/hubs/taskmanagement`;
  }

  /**
   * Initialize the SignalR connection with JWT authentication
   */
  async connect(): Promise<void> {
    if (this.connection) {
      if (
        this.connection.state === signalR.HubConnectionState.Connecting ||
        this.connection.state === signalR.HubConnectionState.Connected
      ) {
        return;
      }
    }

    this.setStatus('connecting');

    try {
      const token = useAuthStore.getState().accessToken;

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this.url, {
          accessTokenFactory: () => token || '',
          transport: signalR.HttpTransportType.WebSockets,
          skipNegotiation: true,
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .withHubProtocol(new signalR.JsonHubProtocol())
        .build();

      // Set up connection event handlers
      this.setupConnectionHandlers();

      // Register all event listeners
      this.registerEventListeners();

      await this.connection.start();
      this.reconnectAttempts = 0;
      this.setStatus('connected');
    } catch (error) {
      console.error('SignalR connection error:', error);
      this.setStatus('error');
      this.scheduleReconnect();
    }
  }

  /**
   * Disconnect from the hub
   */
  async disconnect(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch (error) {
        console.error('Error disconnecting from SignalR:', error);
      }
    }
    this.connection = null;
    this.setStatus('disconnected');
  }

  /**
   * Set up connection event handlers
   */
  private setupConnectionHandlers(): void {
    if (!this.connection) return;

    this.connection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
      this.setStatus('reconnecting');
    });

    this.connection.onreconnected(() => {
      console.log('SignalR reconnected');
      this.reconnectAttempts = 0;
      this.setStatus('connected');
      this.emitEvent('reconnected', {});
    });

    this.connection.onclose(() => {
      console.log('SignalR connection closed');
      this.setStatus('disconnected');
    });
  }

  /**
   * Register event listeners that were previously subscribed
   */
  private registerEventListeners(): void {
    if (!this.connection) return;

    this.eventListeners.forEach((handlers, eventType) => {
      this.connection?.on(eventType, (data: any) => {
        handlers.forEach(handler => handler(data));
      });
    });
  }

  /**
   * Subscribe to an event
   */
  onEvent(eventType: string, handler: SignalREventHandler): () => void {
    if (!this.eventListeners.has(eventType)) {
      this.eventListeners.set(eventType, new Set());
    }

    this.eventListeners.get(eventType)!.add(handler);

    // If already connected, register the listener immediately
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      this.connection.on(eventType, handler);
    }

    // Return unsubscribe function
    return () => {
      const handlers = this.eventListeners.get(eventType);
      if (handlers) {
        handlers.delete(handler);
      }
      if (this.connection) {
        this.connection.off(eventType, handler);
      }
    };
  }

  /**
   * Emit an event from the client to the server
   */
  async emitEvent(methodName: string, ...args: any[]): Promise<void> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      console.warn(`Cannot emit event "${methodName}" - not connected`);
      return;
    }

    try {
      await this.connection.invoke(methodName, ...args);
    } catch (error) {
      console.error(`Error invoking method "${methodName}":`, error);
      throw error;
    }
  }

  /**
   * Join a project room to receive real-time updates
   */
  async joinProject(projectId: number): Promise<void> {
    await this.emitEvent('JoinProject', projectId);
  }

  /**
   * Leave a project room
   */
  async leaveProject(projectId: number): Promise<void> {
    await this.emitEvent('LeaveProject', projectId);
  }

  /**
   * Send typing indicator to other users
   */
  async sendTypingIndicator(taskId: number, isTyping: boolean): Promise<void> {
    await this.emitEvent('SendTypingIndicator', taskId, isTyping);
  }

  /**
   * Get current connection status
   */
  getStatus(): ConnectionStatus {
    return this.currentStatus;
  }

  /**
   * Subscribe to connection status changes
   */
  onStatusChange(listener: (status: ConnectionStatus) => void): () => void {
    this.statusListeners.add(listener);

    // Return unsubscribe function
    return () => {
      this.statusListeners.delete(listener);
    };
  }

  /**
   * Set connection status and notify listeners
   */
  private setStatus(status: ConnectionStatus): void {
    if (this.currentStatus === status) return;

    this.currentStatus = status;
    this.statusListeners.forEach(listener => listener(status));
  }

  /**
   * Schedule a reconnection attempt
   */
  private scheduleReconnect(): void {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.error('Max reconnection attempts reached');
      this.setStatus('disconnected');
      return;
    }

    this.reconnectAttempts++;
    const delay = this.reconnectDelay * Math.pow(1.5, this.reconnectAttempts - 1);

    setTimeout(() => {
      console.log(
        `Reconnection attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts}`
      );
      this.connect().catch(error => {
        console.error('Reconnection failed:', error);
      });
    }, delay);
  }

  /**
   * Check if connected
   */
  isConnected(): boolean {
    return (
      this.connection !== null && this.connection.state === signalR.HubConnectionState.Connected
    );
  }

  /**
   * Clear all event listeners and disconnect
   */
  dispose(): void {
    this.statusListeners.clear();
    this.eventListeners.clear();
    this.disconnect();
  }
}

// Create singleton instance
export const signalRService = new SignalRService();
