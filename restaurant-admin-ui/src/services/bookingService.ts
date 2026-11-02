import { apiClient } from './apiClient';
import { ApiResponse, Booking, BookingFilter, PaginatedBookings } from '../types/api';

export const bookingService = {
  // Đặt bàn hôm nay
  async getTodayBookings(): Promise<Booking[]> {
    const response = await apiClient.get<ApiResponse<Booking[]>>('/staff/bookings/today');
    return response.data.data;
  },

  // Đặt bàn theo ngày
  async getBookingsByDate(date: string): Promise<Booking[]> {
    const response = await apiClient.get<ApiResponse<Booking[]>>(`/staff/bookings/date?date=${date}`);
    return response.data.data;
  },

  // Check-in thông thường
  async checkIn(bookingCode: string): Promise<Booking> {
    const response = await apiClient.post<ApiResponse<Booking>>('/staff/bookings/check-in', { bookingCode });
    return response.data.data;
  },

  // Late check-in (30-60 phút sau giờ đặt)
  async lateCheckIn(bookingCode: string): Promise<Booking> {
    const response = await apiClient.post<ApiResponse<Booking>>('/staff/bookings/late-check-in', { bookingCode, acknowledgeDepositForfeit: true });
    return response.data.data;
  },

  // Tất cả booking (Admin phân trang)
  async getAllAdminBookings(filter?: BookingFilter): Promise<PaginatedBookings> {
    const params = new URLSearchParams();
    if (filter?.date) { params.append('fromDate', filter.date); params.append('toDate', filter.date); }
    if (filter?.status) params.append('status', filter.status);
    if (filter?.search) params.append('searchTerm', filter.search);
    if (filter?.pageNumber) params.append('pageNumber', filter.pageNumber.toString());
    if (filter?.pageSize) params.append('pageSize', filter.pageSize.toString());

    const response = await apiClient.get<ApiResponse<PaginatedBookings>>(`/admin/bookings?${params.toString()}`);
    return response.data.data;
  },

  // Duyệt đặt bàn (Admin)
  async approveBooking(id: number): Promise<void> {
    await apiClient.put<ApiResponse<any>>(`/admin/bookings/${id}/approve`);
  },

  // Từ chối đặt bàn (Admin)
  async rejectBooking(id: number, reason: string): Promise<void> {
    await apiClient.put<ApiResponse<any>>(`/admin/bookings/${id}/reject`, { reason });
  },

  // Hủy đặt bàn (Admin)
  async cancelBooking(id: number, reason: string): Promise<void> {
    await apiClient.post<ApiResponse<any>>(`/admin/bookings/${id}/cancel`, { reason });
  },
};
