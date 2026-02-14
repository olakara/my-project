import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Activity, CheckCircle2, ListChecks, Users } from 'lucide-react';
import { useProjectsStore } from '@/store/projectsStore';
import dashboardApi from '@/services/api/dashboardApi';
import type { BurndownDay, ProjectMetricsResponse } from '@/types/dashboard.types';
import { TaskStatus } from '@/types/task.types';
import MetricsCard from '@/components/dashboard/MetricsCard';
import BurndownChart from '@/components/dashboard/BurndownChart';
import TeamActivityTable from '@/components/dashboard/TeamActivityTable';

const statusLabels: Record<TaskStatus, string> = {
  [TaskStatus.ToDo]: 'To do',
  [TaskStatus.InProgress]: 'In progress',
  [TaskStatus.InReview]: 'In review',
  [TaskStatus.Done]: 'Done',
};

const toDateInputValue = (value: Date) => value.toISOString().slice(0, 10);

const buildDefaultRange = () => {
  const end = new Date();
  const start = new Date();
  start.setDate(end.getDate() - 29);

  return {
    startDate: toDateInputValue(start),
    endDate: toDateInputValue(end),
  };
};

export default function DashboardPage() {
  const { projectId } = useParams<{ projectId?: string }>();
  const navigate = useNavigate();
  const { projects, fetchProjects, isLoading: projectsLoading } = useProjectsStore();
  const [selectedProjectId, setSelectedProjectId] = useState<number | null>(null);
  const [range, setRange] = useState(buildDefaultRange);

  useEffect(() => {
    if (!projectId) {
      fetchProjects().catch(() => undefined);
    }
  }, [projectId, fetchProjects]);

  useEffect(() => {
    if (projectId) {
      const parsed = Number.parseInt(projectId, 10);
      if (!Number.isNaN(parsed)) {
        setSelectedProjectId(parsed);
      }
    }
  }, [projectId]);

  useEffect(() => {
    if (!projectId && !selectedProjectId && projects.length > 0) {
      setSelectedProjectId(projects[0].id);
    }
  }, [projectId, selectedProjectId, projects]);

  const activeProjectId = projectId
    ? Number.parseInt(projectId, 10)
    : selectedProjectId ?? undefined;

  const metricsQuery = useQuery({
    queryKey: ['dashboard-metrics', activeProjectId],
    queryFn: () => dashboardApi.getProjectMetrics(activeProjectId as number),
    enabled: Number.isFinite(activeProjectId),
  });

  const burndownQuery = useQuery({
    queryKey: ['dashboard-burndown', activeProjectId, range],
    queryFn: () => dashboardApi.getBurndown(activeProjectId as number, range),
    enabled: Number.isFinite(activeProjectId),
  });

  const teamQuery = useQuery({
    queryKey: ['dashboard-team', activeProjectId],
    queryFn: () => dashboardApi.getTeamActivity(activeProjectId as number),
    enabled: Number.isFinite(activeProjectId),
  });

  const statusCounts = useMemo(() => {
    if (!metricsQuery.data) {
      return [];
    }

    return metricsQuery.data.statusCounts.map((status) => ({
      ...status,
      label: statusLabels[status.status],
    }));
  }, [metricsQuery.data]);

  const burndownData = useMemo(() => {
    const days = burndownQuery.data?.days ?? [];
    return days.map((day: BurndownDay) => ({
      date: day.date,
      completedTasks: day.completedTasks,
    }));
  }, [burndownQuery.data]);

  const metrics = metricsQuery.data as ProjectMetricsResponse | undefined;
  const completion = metrics ? metrics.completionPercentage.toFixed(1) : '0.0';

  const handleProjectChange = (nextId: number) => {
    setSelectedProjectId(nextId);
    navigate(`/projects/${nextId}/dashboard`);
  };

  const isLoading = metricsQuery.isLoading || burndownQuery.isLoading || teamQuery.isLoading;
  const hasProject = Number.isFinite(activeProjectId);

  return (
    <div className="relative min-h-screen overflow-hidden bg-[#f5f1ea] text-slate-900">
      <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_top,_#e7f4f1,_transparent_55%)]" />
      <div className="pointer-events-none absolute -right-24 top-24 h-64 w-64 rounded-full bg-[#f6d6b6] opacity-60 blur-3xl" />
      <div className="pointer-events-none absolute -left-24 bottom-10 h-72 w-72 rounded-full bg-[#c7e8ef] opacity-70 blur-3xl" />

      <div className="relative mx-auto flex max-w-6xl flex-col gap-8 px-6 py-12">
        <header className="flex flex-col gap-6">
          <div className="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
            <div>
              <p className="text-xs uppercase tracking-[0.2em] text-slate-500">Dashboard</p>
              <h1 className="font-display text-4xl text-slate-900 md:text-5xl">
                {metrics?.projectName ?? 'Project pulse'}
              </h1>
              <p className="mt-2 max-w-2xl text-base text-slate-600">
                A quick read on delivery pace, status balance, and team momentum.
              </p>
            </div>
            {!projectId && (
              <div className="flex flex-col gap-2">
                <span className="text-xs uppercase tracking-[0.2em] text-slate-500">Project</span>
                <select
                  value={activeProjectId ?? ''}
                  onChange={(event) => handleProjectChange(Number(event.target.value))}
                  className="h-10 rounded-xl border border-slate-200 bg-white/90 px-3 text-sm"
                  disabled={projectsLoading || projects.length === 0}
                >
                  {projectsLoading && <option>Loading projects...</option>}
                  {!projectsLoading && projects.length === 0 && (
                    <option value="">No projects yet</option>
                  )}
                  {!projectsLoading &&
                    projects.map((project) => (
                      <option key={project.id} value={project.id}>
                        {project.name}
                      </option>
                    ))}
                </select>
              </div>
            )}
          </div>
        </header>

        {!hasProject && !projectsLoading && (
          <div className="rounded-2xl border border-slate-200 bg-white/90 p-8 text-center text-slate-600">
            Select a project to unlock metrics and charts.
          </div>
        )}

        {hasProject && (
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            <MetricsCard
              title="Total tasks"
              value={`${metrics?.totalTasks ?? 0}`}
              helper="Across the full project board"
              tone="slate"
              icon={<ListChecks size={20} />}
            />
            <MetricsCard
              title="Completed"
              value={`${metrics?.completedTasks ?? 0}`}
              subtitle={`(${completion}%)`}
              helper="Done tasks vs. total"
              tone="emerald"
              icon={<CheckCircle2 size={20} />}
            />
            <MetricsCard
              title="Active workload"
              value={`${(metrics?.totalTasks ?? 0) - (metrics?.completedTasks ?? 0)}`}
              helper="Tasks still in motion"
              tone="amber"
              icon={<Activity size={20} />}
            />
            <MetricsCard
              title="Team members"
              value={`${metrics?.teamMembers.length ?? 0}`}
              helper="Core contributors"
              tone="sky"
              icon={<Users size={20} />}
            />
          </div>
        )}

        {hasProject && statusCounts.length > 0 && (
          <section className="animate-rise-in rounded-2xl border border-slate-200/70 bg-white/80 p-6 shadow-sm backdrop-blur">
            <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
              <div>
                <p className="text-xs uppercase tracking-[0.2em] text-slate-500">Status mix</p>
                <h2 className="text-2xl font-semibold text-slate-900">Board distribution</h2>
              </div>
              <div className="flex flex-wrap gap-3">
                {statusCounts.map((status) => (
                  <div
                    key={status.status}
                    className="flex items-center gap-2 rounded-full bg-slate-100 px-3 py-1 text-sm font-semibold text-slate-700"
                  >
                    <span className="h-2 w-2 rounded-full bg-slate-400" />
                    {status.label}: {status.count}
                  </div>
                ))}
              </div>
            </div>
          </section>
        )}

        {hasProject && (
          <div className="grid gap-6 lg:grid-cols-[1.2fr_1fr]">
            <div className="animate-rise-in">
              {burndownQuery.isError && (
                <div className="rounded-2xl border border-rose-200 bg-rose-50 p-6 text-rose-700">
                  Failed to load burndown data.
                </div>
              )}
              {!burndownQuery.isError && (
                <BurndownChart
                  data={burndownData}
                  startDate={range.startDate}
                  endDate={range.endDate}
                  onRangeChange={setRange}
                  totalCompleted={burndownQuery.data?.totalCompleted}
                />
              )}
            </div>
            <div className="animate-rise-in">
              <TeamActivityTable members={teamQuery.data?.members ?? []} />
            </div>
          </div>
        )}

        {hasProject && isLoading && (
          <div className="rounded-2xl border border-slate-200 bg-white/80 p-6 text-slate-600">
            Loading dashboard data...
          </div>
        )}
      </div>
    </div>
  );
}
