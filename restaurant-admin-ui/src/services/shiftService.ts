import { apiClient } from './apiClient';
import { ApiResponse, StaffShift } from '../types/api';

export const shiftService = {
  // Ca trực của nhân viên hiện tại
  async getMyShifts(fromDate?: string, toDate?: string): Promise<StaffShift[]> {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    const response = await apiClient.get<ApiResponse<StaffShift[]>>(`/staff/my-shifts?${params.toString()}`);
    return response.data.data;
  },

  // Tất cả ca trực (Admin)
  async getAllShifts(): Promise<StaffShift[]> {
    const response = await apiClient.get<ApiResponse<StaffShift[]>>('/admin/shifts');
    return response.data.data;
  },
};
