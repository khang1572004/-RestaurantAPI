import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';

interface ProtectedRouteProps {
  requiredRole?: 'Admin' | 'Staff';
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ requiredRole }) => {
  const { isAuthenticated, user } = useAuthStore();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (requiredRole === 'Admin' && user?.role !== 'Admin') {
    // If Admin required but user is Staff, redirect to tables
    return <Navigate to="/tables" replace />;
  }

  return <Outlet />;
};
