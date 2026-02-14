import type { ReactNode } from 'react';
import { cn } from '@/utils/cn';

export type MetricsTone = 'sky' | 'amber' | 'emerald' | 'slate';

const toneStyles: Record<MetricsTone, { ring: string; glow: string; icon: string }> = {
  sky: {
    ring: 'border-sky-200/70',
    glow: 'bg-sky-200/60',
    icon: 'bg-sky-100 text-sky-700',
  },
  amber: {
    ring: 'border-amber-200/70',
    glow: 'bg-amber-200/60',
    icon: 'bg-amber-100 text-amber-700',
  },
  emerald: {
    ring: 'border-emerald-200/70',
    glow: 'bg-emerald-200/60',
    icon: 'bg-emerald-100 text-emerald-700',
  },
  slate: {
    ring: 'border-slate-200/70',
    glow: 'bg-slate-200/60',
    icon: 'bg-slate-100 text-slate-700',
  },
};

export interface MetricsCardProps {
  title: string;
  value: string;
  subtitle?: string;
  helper?: string;
  tone?: MetricsTone;
  icon?: ReactNode;
}

export default function MetricsCard({
  title,
  value,
  subtitle,
  helper,
  tone = 'slate',
  icon,
}: MetricsCardProps) {
  const styles = toneStyles[tone];

  return (
    <div
      className={cn(
        'relative overflow-hidden rounded-2xl border bg-white/80 p-5 shadow-sm backdrop-blur',
        styles.ring
      )}
    >
      <div
        className={cn(
          'absolute -right-10 -top-10 h-24 w-24 rounded-full blur-2xl',
          styles.glow
        )}
        aria-hidden="true"
      />
      <div className="relative flex items-start justify-between gap-4">
        <div>
          <p className="text-xs uppercase tracking-[0.2em] text-slate-500">{title}</p>
          <div className="mt-3 flex items-end gap-2">
            <p className="text-3xl font-semibold text-slate-900">{value}</p>
            {subtitle && <p className="text-sm text-slate-600">{subtitle}</p>}
          </div>
          {helper && <p className="mt-3 text-xs text-slate-500">{helper}</p>}
        </div>
        {icon && (
          <div className={cn('flex h-11 w-11 items-center justify-center rounded-xl', styles.icon)}>
            {icon}
          </div>
        )}
      </div>
    </div>
  );
}
