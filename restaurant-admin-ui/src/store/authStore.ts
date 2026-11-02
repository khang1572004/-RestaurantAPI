import { create } from 'zustand';
import { User, StaffShift } from '../types/api';

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  activeShift: StaffShift | null;
  login: (token: string, user: User, refreshToken?: string) => void;
  logout: () => void;
  setActiveShift: (shift: StaffShift | null) => void;
  isAdmin: () => boolean;
  isStaff: () => boolean;
}

// Initial state from localStorage
const storedToken = localStorage.getItem('restaurant_token');
let storedUser: User | null = null;
try {
  const userJson = localStorage.getItem('restaurant_user');
  if (userJson) {
    storedUser = JSON.parse(userJson);
  }
} catch {
  storedUser = null;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: storedUser,
  token: storedToken,
  isAuthenticated: !!storedToken && !!storedUser,
  activeShift: null,

  login: (token: string, user: User, refreshToken?: string) => {
    localStorage.setItem('restaurant_token', token);
    if (refreshToken) localStorage.setItem('restaurant_refresh_token', refreshToken);
    localStorage.setItem('restaurant_user', JSON.stringify(user));
    set({ token, user, isAuthenticated: true });
  },

  logout: () => {
    localStorage.removeItem('restaurant_token');
    localStorage.removeItem('restaurant_refresh_token');
    localStorage.removeItem('restaurant_user');
    set({ token: null, user: null, isAuthenticated: false, activeShift: null });
  },

  setActiveShift: (shift: StaffShift | null) => {
    set({ activeShift: shift });
  },

  isAdmin: () => get().user?.role === 'Admin',
  isStaff: () => get().user?.role === 'Staff' || get().user?.role === 'Admin',
}));
