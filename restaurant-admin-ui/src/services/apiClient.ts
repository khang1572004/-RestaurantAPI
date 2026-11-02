import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000,
});

let refreshPromise: Promise<string> | null = null;
const isRefreshRequest = (config?: InternalAxiosRequestConfig) => config?.url?.includes('/auth/refresh-token');

// Request interceptor to attach JWT Bearer token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('restaurant_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor to format errors and handle 401
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<{ message?: string; errors?: string[] }>) => {
    const original = error.config;
    if (error.response?.status === 401 && original && !isRefreshRequest(original) && !(original as InternalAxiosRequestConfig & { _retry?: boolean })._retry) {
      const refreshToken = localStorage.getItem('restaurant_refresh_token');
      if (refreshToken) {
        (original as InternalAxiosRequestConfig & { _retry?: boolean })._retry = true;
        try {
          refreshPromise ??= import('./authService').then(({ authService }) => authService.refreshToken(refreshToken)).then((data) => {
            localStorage.setItem('restaurant_token', data.token);
            localStorage.setItem('restaurant_refresh_token', data.refreshToken);
            localStorage.setItem('restaurant_user', JSON.stringify(data.user));
            return data.token;
          }).finally(() => { refreshPromise = null; });
          const token = await refreshPromise;
          original.headers.Authorization = `Bearer ${token}`;
          return apiClient(original);
        } catch {
          refreshPromise = null;
        }
      }
      // Clear token and redirect to login if session expired
      localStorage.removeItem('restaurant_token');
      localStorage.removeItem('restaurant_refresh_token');
      localStorage.removeItem('restaurant_user');
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    const errorMessage =
      error.response?.data?.message ||
      error.message ||
      'Có lỗi xảy ra khi kết nối máy chủ';
    return Promise.reject(new Error(errorMessage));
  }
);
