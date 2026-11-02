import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Layout } from './components/common/Layout';
import { ProtectedRoute } from './components/common/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { TablesPage } from './pages/TablesPage';
import { POSPage } from './pages/POSPage';
import { BookingsPage } from './pages/BookingsPage';
import { ShiftsPage } from './pages/ShiftsPage';
import { ReportsPage } from './pages/admin/ReportsPage';
import { MenuManagementPage } from './pages/admin/MenuManagementPage';
import { TablesManagementPage } from './pages/admin/TablesManagementPage';
import { VouchersPage } from './pages/admin/VouchersPage';
import { SettingsPage } from './pages/admin/SettingsPage';
import { CustomerLayout } from './components/customer/CustomerLayout';
import { HomePage } from './pages/customer/HomePage';
import { BookTablePage } from './pages/customer/BookTablePage';
import { MyBookingsPage } from './pages/customer/MyBookingsPage';
import { MenuPage } from './pages/customer/MenuPage';
import { PaymentResultPage } from './pages/customer/PaymentResultPage';
import { StaffManagementPage } from './pages/admin/StaffManagementPage';
import { RefundsPage } from './pages/admin/RefundsPage';
import { ReviewsPage } from './pages/admin/ReviewsPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 5000,
    },
  },
});

export const App: React.FC = () => {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          {/* Customer routes */}
          <Route element={<CustomerLayout />}>
            <Route path="/" element={<HomePage />} />
            <Route path="/menu" element={<MenuPage />} />
            <Route path="/payment-result" element={<PaymentResultPage />} />
            <Route path="/book-table" element={<BookTablePage />} />
            <Route element={<ProtectedRoute />}>
              <Route path="/my-bookings" element={<MyBookingsPage />} />
            </Route>
          </Route>

          {/* Public Route */}
          <Route path="/login" element={<LoginPage />} />

          {/* Protected Routes for Staff & Admin */}
          <Route element={<ProtectedRoute />}>
            <Route element={<Layout />}>
              <Route path="/tables" element={<TablesPage />} />
              <Route path="/pos" element={<POSPage />} />
              <Route path="/bookings" element={<BookingsPage />} />
              <Route path="/shifts" element={<ShiftsPage />} />

              {/* Admin-only Routes */}
              <Route element={<ProtectedRoute requiredRole="Admin" />}>
                <Route path="/admin/reports" element={<ReportsPage />} />
                <Route path="/admin/menu" element={<MenuManagementPage />} />
                <Route path="/admin/tables-management" element={<TablesManagementPage />} />
                <Route path="/admin/vouchers" element={<VouchersPage />} />
                <Route path="/admin/settings" element={<SettingsPage />} />
                <Route path="/admin/staff" element={<StaffManagementPage />} />
                <Route path="/admin/refunds" element={<RefundsPage />} />
                <Route path="/admin/reviews" element={<ReviewsPage />} />
              </Route>
            </Route>
          </Route>

          {/* Catch-all */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
};
