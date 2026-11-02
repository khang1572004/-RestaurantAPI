import { apiClient } from './apiClient';
import { ApiResponse, RevenueReport, BookingReport, BestSellingItem } from '../types/api';

export const reportService = {
  // Báo cáo doanh thu
  async getRevenueReport(fromDate: string, toDate: string, groupBy = 'day'): Promise<RevenueReport> {
    const response = await apiClient.get<ApiResponse<RevenueReport>>(
      `/admin/reports/revenue?fromDate=${fromDate}&toDate=${toDate}&groupBy=${groupBy}`
    );
    const data = response.data.data as RevenueReport & { chartData?: RevenueReport['chartData'] };
    return { ...data, dailyRevenue: data.dailyRevenue || data.chartData || [] };
  },

  // Báo cáo bookings
  async getBookingReport(fromDate: string, toDate: string): Promise<BookingReport> {
    const response = await apiClient.get<ApiResponse<BookingReport>>(
      `/admin/reports/bookings?fromDate=${fromDate}&toDate=${toDate}`
    );
    return response.data.data;
  },

  // Món ăn bán chạy nhất
  async getBestSellingReport(fromDate: string, toDate: string, topCount = 10): Promise<BestSellingItem[]> {
    const response = await apiClient.get<ApiResponse<BestSellingItem[]>>(
      `/admin/reports/best-selling?fromDate=${fromDate}&toDate=${toDate}&topCount=${topCount}`
    );
    const payload = response.data.data as unknown as { items?: BestSellingItem[] } | BestSellingItem[];
    return Array.isArray(payload) ? payload : payload.items || [];
  },
};
