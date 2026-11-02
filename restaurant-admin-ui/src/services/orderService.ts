import { apiClient } from './apiClient';
import { ApiResponse, Order, OrderItem } from '../types/api';

export interface OrderSummaryResponse {
  orderId: number;
  subtotal: number;
  discountAmount: number;
  voucherCode?: string;
  depositAmount: number;
  totalAmount: number;
  finalAmount: number;
}

export const orderService = {
  // Tạo order mới cho bàn (walk-in hoặc từ booking)
  async createOrder(data: { tableId?: number; bookingId?: number }): Promise<Order> {
    const response = await apiClient.post<ApiResponse<Order>>('/staff/orders/create', data);
    return response.data.data;
  },

  // Lấy chi tiết order
  async getOrderById(orderId: number): Promise<Order> {
    const response = await apiClient.get<ApiResponse<Order>>(`/staff/orders/${orderId}`);
    return response.data.data;
  },

  // Thêm món vào order
  async addOrderItem(orderId: number, data: { menuItemId: number; quantity: number }): Promise<OrderItem> {
    const response = await apiClient.post<ApiResponse<OrderItem>>(`/staff/orders/${orderId}/items`, data);
    return response.data.data;
  },

  // Cập nhật số lượng món
  async updateOrderItem(orderId: number, orderItemId: number, quantity: number): Promise<OrderItem> {
    const response = await apiClient.put<ApiResponse<OrderItem>>(`/staff/orders/${orderId}/items/${orderItemId}`, { quantity });
    return response.data.data;
  },

  // Xóa món khỏi order
  async removeOrderItem(orderId: number, orderItemId: number): Promise<boolean> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`/staff/orders/${orderId}/items/${orderItemId}`);
    return response.data.success;
  },

  // Áp dụng voucher
  async applyVoucher(orderId: number, voucherCode: string): Promise<Order> {
    const response = await apiClient.put<ApiResponse<Order>>(`/staff/orders/${orderId}/apply-voucher`, { voucherCode });
    return response.data.data;
  },

  // Xem tổng hóa đơn
  async getOrderSummary(orderId: number): Promise<OrderSummaryResponse> {
    const response = await apiClient.get<ApiResponse<OrderSummaryResponse>>(`/staff/orders/${orderId}/summary`);
    return response.data.data;
  },

  // Thanh toán
  async processPayment(orderId: number, paymentMethod: number): Promise<any> {
    // PaymentMethod enum in backend: 1: Cash, 2: BankTransfer, 3: VNPay, 4: MoMo
    const response = await apiClient.post<ApiResponse<any>>(`/staff/orders/${orderId}/pay`, { paymentMethod });
    return response.data.data;
  },

  // Hủy order
  async cancelOrder(orderId: number): Promise<boolean> {
    const response = await apiClient.post<ApiResponse<boolean>>(`/staff/orders/${orderId}/cancel`);
    return response.data.success;
  },
  async getHistory(params: { tableId?: number; bookingId?: number } = {}): Promise<Order[]> {
    const query = new URLSearchParams();
    if (params.tableId) query.set('tableId', String(params.tableId));
    if (params.bookingId) query.set('bookingId', String(params.bookingId));
    const response = await apiClient.get<ApiResponse<Order[]>>(`/staff/orders/history?${query}`);
    return response.data.data;
  },
};
