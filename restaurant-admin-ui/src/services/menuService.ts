import { apiClient } from './apiClient';
import { ApiResponse, Category, MenuItem } from '../types/api';

export const menuService = {
  // Lấy danh mục
  async getCategories(): Promise<Category[]> {
    const response = await apiClient.get<ApiResponse<Category[]>>('/menu/categories');
    return response.data.data;
  },

  // Lấy món ăn theo danh mục hoặc tất cả
  async getMenuItems(categoryId?: number): Promise<MenuItem[]> {
    const url = categoryId ? `/menu/items?categoryId=${categoryId}` : '/menu/items';
    const response = await apiClient.get<ApiResponse<MenuItem[]>>(url);
    return response.data.data;
  },

  // Tìm kiếm món
  async searchMenuItems(query: string): Promise<MenuItem[]> {
    const response = await apiClient.get<ApiResponse<MenuItem[]>>(`/menu/items/search?q=${encodeURIComponent(query)}`);
    return response.data.data;
  },

  // Quản lý món ăn (Admin)
  async createMenuItem(data: Partial<MenuItem>): Promise<MenuItem> {
    const response = await apiClient.post<ApiResponse<MenuItem>>('/admin/menu-items', data);
    return response.data.data;
  },

  async updateMenuItem(id: number, data: Partial<MenuItem>): Promise<MenuItem> {
    const response = await apiClient.put<ApiResponse<MenuItem>>(`/admin/menu-items/${id}`, data);
    return response.data.data;
  },

  async toggleAvailability(id: number, isAvailable: boolean): Promise<void> {
    await apiClient.patch<ApiResponse<any>>(`/admin/menu-items/${id}/availability`, { isAvailable });
  },

  async deleteMenuItem(id: number): Promise<boolean> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`/admin/menu-items/${id}`);
    return response.data.success;
  },

  // Quản lý danh mục (Admin)
  async createCategory(data: { name: string; description?: string; displayOrder?: number }): Promise<Category> {
    const response = await apiClient.post<ApiResponse<Category>>('/admin/categories', data);
    return response.data.data;
  },

  async updateCategory(id: number, data: Partial<Category>): Promise<Category> {
    const response = await apiClient.put<ApiResponse<Category>>(`/admin/categories/${id}`, data);
    return response.data.data;
  },

  async deleteCategory(id: number): Promise<boolean> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`/admin/categories/${id}`);
    return response.data.success;
  },
};
