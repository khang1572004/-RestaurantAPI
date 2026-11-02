import { apiClient } from './apiClient';
import type { ApiResponse, Booking, Review } from '../types/api';

export interface AvailableTable { id: number; tableNumber: string; capacity: number; zoneName?: string; tableTypeName?: string; depositAmount?: number; }

export const customerService = {
  async availability(data: { bookingDate: string; bookingTime: string; numGuests: number; serviceDurationMinutes: number }) {
    const params = new URLSearchParams({ ...data, numGuests: String(data.numGuests), serviceDurationMinutes: String(data.serviceDurationMinutes) });
    const res = await apiClient.get<ApiResponse<AvailableTable[]>>(`/Booking/availability?${params}`);
    return res.data.data;
  },
  async createBooking(data: { tableId: number; bookingDate: string; bookingTime: string; numGuests: number; serviceDurationMinutes: number; notes?: string }) {
    const res = await apiClient.post<ApiResponse<Booking>>('/Booking/create', data);
    return res.data.data;
  },
  async myBookings() {
    const res = await apiClient.get<ApiResponse<Booking[]>>('/Booking/my-bookings');
    return res.data.data;
  },
  async cancelBooking(id: number, reason: string) { await apiClient.post(`/Booking/${id}/cancel`, { reason }); },
  async createReview(data: { bookingId: number; rating: number; comment?: string }) {
    const res = await apiClient.post<ApiResponse<Review>>('/reviews', data);
    return res.data.data;
  },
};
