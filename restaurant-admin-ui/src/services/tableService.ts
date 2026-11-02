import { apiClient } from './apiClient';
import { ApiResponse, TableStatus, Table, Zone, TableType } from '../types/api';

export const tableService = {
  // Lấy trạng thái tất cả các bàn hiện tại
  async getTableStatus(): Promise<TableStatus[]> {
    const response = await apiClient.get<ApiResponse<TableStatus[]>>('/staff/tables/status');
    return response.data.data;
  },

  // Danh sách bàn chi tiết (Admin)
  async getTables(): Promise<Table[]> {
    const response = await apiClient.get<ApiResponse<Table[]>>('/admin/tables');
    return response.data.data;
  },

  // Danh sách khu vực (Zones)
  async getZones(): Promise<Zone[]> {
    const response = await apiClient.get<ApiResponse<Zone[]>>('/admin/zones');
    return response.data.data;
  },

  // Danh sách loại bàn (Table Types)
  async getTableTypes(): Promise<TableType[]> {
    const response = await apiClient.get<ApiResponse<TableType[]>>('/admin/table-types');
    return response.data.data;
  },

  // Thêm bàn mới
  async createTable(data: { tableNumber: string; zoneId: number; tableTypeId: number; notes?: string }): Promise<Table> {
    const response = await apiClient.post<ApiResponse<Table>>('/admin/tables', data);
    return response.data.data;
  },

  // Cập nhật bàn
  async updateTable(id: number, data: Partial<Table>): Promise<Table> {
    const response = await apiClient.put<ApiResponse<Table>>(`/admin/tables/${id}`, data);
    return response.data.data;
  },

  // Xóa bàn
  async deleteTable(id: number): Promise<boolean> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`/admin/tables/${id}`);
    return response.data.success;
  },

  // Thêm khu vực
  async createZone(data: { name: string; description?: string }): Promise<Zone> {
    const response = await apiClient.post<ApiResponse<Zone>>('/admin/zones', data);
    return response.data.data;
  },
};
