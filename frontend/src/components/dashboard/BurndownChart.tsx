import {
  Area,
  AreaChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { Input } from '@/components/ui/input';

export interface BurndownPoint {
  date: string;
  completedTasks: number;
}

export interface BurndownChartProps {
  data: BurndownPoint[];
  startDate: string;
  endDate: string;
  onRangeChange: (range: { startDate: string; endDate: string }) => void;
  totalCompleted?: number;
}

const formatLabel = (value: string) => {
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return value;
  }
  return parsed.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
};

export default function BurndownChart({
  data,
  startDate,
  endDate,
  onRangeChange,
  totalCompleted,
}: BurndownChartProps) {
  return (
    <div className="rounded-2xl border border-slate-200/70 bg-white/80 p-6 shadow-sm backdrop-blur">
      <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
        <div>
          <p className="text-xs uppercase tracking-[0.2em] text-slate-500">Burndown</p>
          <h3 className="mt-2 text-2xl font-semibold text-slate-900">Completion tempo</h3>
          <p className="mt-2 text-sm text-slate-600">
            {typeof totalCompleted === 'number'
              ? `${totalCompleted} tasks completed in this window.`
              : 'Track completed tasks per day.'}
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <div className="flex items-center gap-2">
            <span className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">
              From
            </span>
            <Input
              type="date"
              value={startDate}
              onChange={(event) =>
                onRangeChange({ startDate: event.target.value, endDate })
              }
              className="h-9 w-[145px] bg-white"
            />
          </div>
          <div className="flex items-center gap-2">
            <span className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">
              To
            </span>
            <Input
              type="date"
              value={endDate}
              onChange={(event) =>
                onRangeChange({ startDate, endDate: event.target.value })
              }
              className="h-9 w-[145px] bg-white"
            />
          </div>
        </div>
      </div>
      <div className="mt-6 h-64 w-full">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={data} margin={{ left: 12, right: 12, top: 8, bottom: 0 }}>
            <defs>
              <linearGradient id="burndownFill" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="#0f766e" stopOpacity={0.35} />
                <stop offset="100%" stopColor="#0f766e" stopOpacity={0.05} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="4 4" stroke="#E2E8F0" />
            <XAxis
              dataKey="date"
              tickFormatter={formatLabel}
              tick={{ fill: '#64748B', fontSize: 12 }}
              axisLine={false}
              tickLine={false}
            />
            <YAxis
              tick={{ fill: '#64748B', fontSize: 12 }}
              axisLine={false}
              tickLine={false}
              allowDecimals={false}
            />
            <Tooltip
              formatter={(value: number) => [`${value}`, 'Completed']}
              labelFormatter={(label: string) => formatLabel(label)}
              contentStyle={{
                borderRadius: '12px',
                borderColor: '#E2E8F0',
                fontSize: '12px',
              }}
            />
            <Area
              type="monotone"
              dataKey="completedTasks"
              stroke="#0f766e"
              strokeWidth={2}
              fill="url(#burndownFill)"
            />
          </AreaChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
