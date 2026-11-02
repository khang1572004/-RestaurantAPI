import { apiClient } from './apiClient';
import { ApiResponse, LoginResponse, User } from '../types/api';

export const authService = {
  async login(email: string, password: string): Promise<LoginResponse> {
    const response = await apiClient.post<ApiResponse<LoginResponse>>('/auth/login', {
      email,
      password,
    });
    return response.data.data;
  },

  async register(data: { email: string; password: string; fullName: string; phone?: string; role?: string }): Promise<User> {
    const response = await apiClient.post<ApiResponse<User>>('/auth/register', data);
    return response.data.data;
  },
  async refreshToken(refreshToken: string): Promise<LoginResponse> {
    const response = await apiClient.post<ApiResponse<LoginResponse>>('/auth/refresh-token', { refreshToken });
    return response.data.data;
  },
};
