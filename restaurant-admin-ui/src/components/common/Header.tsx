import React, { useState, useEffect } from 'react';
import { useAuthStore } from '../../store/authStore';
import { 
  UtensilsCrossed, 
  LogOut, 
  User as UserIcon, 
  ShieldCheck, 
  Clock, 
  CheckCircle2,
  Bell
} from 'lucide-react';

export const Header: React.FC = () => {
  const { user, logout, activeShift } = useAuthStore();
  const [time, setTime] = useState<string>('');

  useEffect(() => {
    const updateTime = () => {
      const now = new Date();
      setTime(
        now.toLocaleTimeString('vi-VN', {
          hour: '2-digit',
          minute: '2-digit',
          second: '2-digit',
          hour12: false,
        })
      );
    };
    updateTime();
    const interval = setInterval(updateTime, 1000);
    return () => clearInterval(interval);
  }, []);

  return (
    <header className="h-16 bg-white border-b border-slate-200 px-6 flex items-center justify-between sticky top-0 z-30 shadow-xs">
      {/* Left: Mobile toggle / branding */}
      <div className="flex items-center gap-4">
        <div className="flex items-center gap-2.5">
          <div className="w-10 h-10 rounded-xl bg-amber-500 text-white flex items-center justify-center shadow-md shadow-amber-500/20">
            <UtensilsCrossed className="w-5 h-5" />
          </div>
          <div>
            <h1 className="font-bold text-slate-900 text-base leading-tight tracking-tight">
              Restaurant Master
            </h1>
            <p className="text-xs text-slate-500 font-medium">
              Hệ thống Quản Trị & Điểm Bán POS
            </p>
          </div>
        </div>
      </div>

      {/* Middle: Shift status & Live Clock */}
      <div className="hidden md:flex items-center gap-6">
        {/* Live Clock */}
        <div className="flex items-center gap-2 px-3 py-1.5 bg-slate-100 rounded-lg text-slate-700 text-xs font-semibold">
          <Clock className="w-4 h-4 text-amber-600" />
          <span>{time}</span>
        </div>

        {/* Shift Badge */}
        <div className="flex items-center gap-2 px-3 py-1.5 bg-emerald-50 border border-emerald-200 rounded-lg text-emerald-800 text-xs font-medium">
          <CheckCircle2 className="w-4 h-4 text-emerald-600" />
          <span>
            Ca trực: <strong className="font-semibold">{activeShift?.shiftName || 'Ca Sáng (Đang trực)'}</strong>
          </span>
        </div>
      </div>

      {/* Right: User Profile & Actions */}
      <div className="flex items-center gap-3">
        {/* Notifications Icon (decorative) */}
        <button 
          title="Thông báo"
          className="p-2 text-slate-500 hover:text-slate-800 hover:bg-slate-100 rounded-lg transition-colors cursor-pointer relative"
        >
          <Bell className="w-5 h-5" />
          <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-rose-500 rounded-full"></span>
        </button>

        <div className="h-6 w-px bg-slate-200"></div>

        {/* User Badge */}
        <div className="flex items-center gap-2.5">
          <div className="w-9 h-9 rounded-full bg-slate-800 text-amber-400 flex items-center justify-center font-bold text-sm">
            {user?.fullName?.charAt(0).toUpperCase() || 'U'}
          </div>
          <div className="hidden sm:block text-left">
            <div className="text-xs font-bold text-slate-800 flex items-center gap-1.5">
              <span>{user?.fullName || 'Nhân viên'}</span>
              {user?.role === 'Admin' ? (
                <span className="inline-flex items-center gap-0.5 px-1.5 py-0.5 rounded text-[10px] font-bold bg-amber-100 text-amber-800">
                  <ShieldCheck className="w-3 h-3" /> Admin
                </span>
              ) : (
                <span className="inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-bold bg-blue-100 text-blue-800">
                  <UserIcon className="w-3 h-3" /> Staff
                </span>
              )}
            </div>
            <p className="text-[11px] text-slate-400 truncate max-w-[130px]">{user?.email}</p>
          </div>
        </div>

        {/* Logout Button */}
        <button
          onClick={logout}
          title="Đăng xuất"
          className="p-2 text-slate-500 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors cursor-pointer ml-1"
        >
          <LogOut className="w-5 h-5" />
        </button>
      </div>
    </header>
  );
};
