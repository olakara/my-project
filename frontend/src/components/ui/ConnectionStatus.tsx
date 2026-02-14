import { useRealtime } from '../../hooks/useRealtime';

/**
 * Connection Status Indicator Component
 *
 * Displays real-time connection status with visual feedback:
 * - Green indicator: Connected
 * - Amber indicator: Connecting/Reconnecting
 * - Red indicator: Disconnected
 * - Red with error icon: Error state
 */
export default function ConnectionStatus() {
  const { status } = useRealtime({ autoConnect: false });

  // Don't show indicator when connected
  if (status === 'connected') {
    return null;
  }

  const getStatusConfig = () => {
    switch (status) {
      case 'connecting':
        return {
          bgColor: 'bg-blue-50',
          borderColor: 'border-blue-200',
          textColor: 'text-blue-700',
          dotColor: 'bg-blue-500',
          icon: '🔄',
          label: 'Connecting...',
          dotAnimation: 'animate-pulse',
        };

      case 'reconnecting':
        return {
          bgColor: 'bg-amber-50',
          borderColor: 'border-amber-200',
          textColor: 'text-amber-700',
          dotColor: 'bg-amber-500',
          icon: '🔄',
          label: 'Reconnecting...',
          dotAnimation: 'animate-pulse',
        };

      case 'disconnected':
        return {
          bgColor: 'bg-gray-50',
          borderColor: 'border-gray-200',
          textColor: 'text-gray-600',
          dotColor: 'bg-gray-400',
          icon: '⚠️',
          label: 'Offline - changes will sync when online',
          dotAnimation: '',
        };

      case 'error':
        return {
          bgColor: 'bg-red-50',
          borderColor: 'border-red-200',
          textColor: 'text-red-700',
          dotColor: 'bg-red-500',
          icon: '❌',
          label: 'Connection error - attempting to reconnect...',
          dotAnimation: 'animate-pulse',
        };

      default:
        return {
          bgColor: 'bg-gray-50',
          borderColor: 'border-gray-200',
          textColor: 'text-gray-600',
          dotColor: 'bg-gray-400',
          icon: '❓',
          label: 'Unknown status',
          dotAnimation: '',
        };
    }
  };

  const config = getStatusConfig();

  return (
    <div
      className={`
        fixed bottom-4 right-4 
        ${config.bgColor} 
        border ${config.borderColor}
        rounded-lg 
        px-4 py-3 
        shadow-md 
        flex items-center gap-2
        max-w-xs
        z-50
        transition-all duration-200
      `}
      role="status"
      aria-live="polite"
      aria-label={`Connection status: ${config.label}`}
    >
      {/* Status indicator dot */}
      <div
        className={`
          w-2.5 h-2.5 
          rounded-full 
          flex-shrink-0 
          ${config.dotColor} 
          ${config.dotAnimation}
        `}
        aria-hidden="true"
      />

      {/* Status text */}
      <span className={`text-sm font-medium ${config.textColor}`}>
        {config.label}
      </span>

      {/* Optional: Icon placeholder (using emoji for simplicity) */}
      <span className="ml-auto text-base" aria-hidden="true">
        {config.icon}
      </span>
    </div>
  );
}
