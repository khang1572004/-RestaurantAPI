import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';

export interface DepositPaymentResponse {
  paymentId: number;
  paymentUrl?: string;
  transactionId?: string;
  expiresAt?: string;
}

export interface BookingPayment {
  id: number;
  bookingId?: number;
  amount: number;
  paymentMethod: number;
  paymentType: string;
  status: string;
  transactionId?: string;
  paidAt?: string;
}

export const paymentService = {
  async createDeposit(bookingId: number, paymentMethod: number): Promise<DepositPaymentResponse> {
    const response = await apiClient.post<ApiResponse<DepositPaymentResponse>>('/payment/create-deposit', { bookingId, paymentMethod });
    return response.data.data;
  },
  async getBookingPayments(bookingId: number): Promise<BookingPayment[]> {
    const response = await apiClient.get<ApiResponse<BookingPayment[]>>(`/payment/booking/${bookingId}`);
    return response.data.data;
  },
};
