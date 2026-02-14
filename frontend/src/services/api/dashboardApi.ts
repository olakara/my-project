import apiClient from './apiClient';
import type {
  ProjectMetricsResponse,
  BurndownResponse,
  TeamActivityResponse,
} from '@/types/dashboard.types';

export interface BurndownRange {
  startDate?: string;
  endDate?: string;
}

export const dashboardApi = {
  async getProjectMetrics(projectId: number): Promise<ProjectMetricsResponse> {
    const response = await apiClient.get<ProjectMetricsResponse>(
      `/projects/${projectId}/metrics`
    );
    return response.data;
  },

  async getBurndown(projectId: number, range?: BurndownRange): Promise<BurndownResponse> {
    const params = new URLSearchParams();

    if (range?.startDate) {
      params.append('startDate', range.startDate);
    }

    if (range?.endDate) {
      params.append('endDate', range.endDate);
    }

    const query = params.toString();
    const url = `/projects/${projectId}/burndown${query ? `?${query}` : ''}`;
    const response = await apiClient.get<BurndownResponse>(url);
    return response.data;
  },

  async getTeamActivity(projectId: number): Promise<TeamActivityResponse> {
    const response = await apiClient.get<TeamActivityResponse>(
      `/projects/${projectId}/team-activity`
    );
    return response.data;
  },
};

export default dashboardApi;
