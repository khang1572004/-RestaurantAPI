import { apiClient } from './apiClient';
import type { ApiResponse, Review } from '../types/api';

export interface Staff { id: number; email: string; fullName: string; phone?: string; role: string; isActive: boolean; createdAt: string; lastLoginAt?: string; }
export interface Refund { id: number; bookingCode: string; customerName: string; refundAmount: number; reason: string; status: string; requestedAt: string; adminNote?: string; }

export const adminExtrasService = {
  async staff() { const res = await apiClient.get<ApiResponse<{ items: Staff[] }>>('/admin/staff?pageSize=100'); return res.data.data.items; },
  async createStaff(data: { email: string; password: string; fullName: string; phone?: string; role: number }) { const res = await apiClient.post<ApiResponse<Staff>>('/admin/staff', data); return res.data.data; },
  async updateStaff(id: number, data: Partial<Staff>) { await apiClient.put(`/admin/staff/${id}`, data); },
  async refunds() { const res = await apiClient.get<ApiResponse<Refund[]>>('/admin/Refund'); return res.data.data; },
  async processRefund(id: number, action: 'approve' | 'reject', adminNote: string) { await apiClient.put(`/admin/Refund/${id}/${action}`, { adminNote }); },
  async executeRefund(id: number) { await apiClient.post(`/admin/Refund/${id}/process`); },
  async reviews() { const res = await apiClient.get<ApiResponse<Review[]>>('/reviews/all'); return res.data.data; },
};
