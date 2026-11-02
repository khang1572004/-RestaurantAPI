import React from 'react';
import { NavLink } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import {
  LayoutGrid,
  ShoppingBag,
  CalendarCheck,
  BarChart3,
  BookOpen,
  Settings,
  Ticket,
  Clock,
  Layers,
  Users,
} from 'lucide-react';

interface NavItem {
  label: string;
  path: string;
  icon: React.ElementType;
  adminOnly?: boolean;
}

const navItems: NavItem[] = [
  { label: 'Sơ đồ bàn', path: '/tables', icon: LayoutGrid },
  { label: 'POS Gọi món', path: '/pos', icon: ShoppingBag },
  { label: 'Quản lý Đặt bàn', path: '/bookings', icon: CalendarCheck },
  { label: 'Ca trực nhân viên', path: '/shifts', icon: Clock },
  // Admin only items
  { label: 'Báo cáo Doanh thu', path: '/admin/reports', icon: BarChart3, adminOnly: true },
  { label: 'Quản lý Thực đơn', path: '/admin/menu', icon: BookOpen, adminOnly: true },
  { label: 'Quản lý Bàn & Khu vực', path: '/admin/tables-management', icon: Layers, adminOnly: true },
  { label: 'Mã Giảm Giá (Vouchers)', path: '/admin/vouchers', icon: Ticket, adminOnly: true },
  { label: 'Cài đặt Nhà hàng', path: '/admin/settings', icon: Settings, adminOnly: true },
  { label: 'Nhân viên', path: '/admin/staff', icon: Users, adminOnly: true },
  { label: 'Hoàn tiền', path: '/admin/refunds', icon: Ticket, adminOnly: true },
  { label: 'Đánh giá khách hàng', path: '/admin/reviews', icon: BookOpen, adminOnly: true },
];

export const Sidebar: React.FC = () => {
  const { user } = useAuthStore();
  const isAdmin = user?.role === 'Admin';

  const visibleItems = navItems.filter((item) => !item.adminOnly || isAdmin);

  return (
    <aside className="w-64 bg-slate-900 text-slate-300 flex flex-col flex-shrink-0 min-h-[calc(100vh-4rem)] border-r border-slate-800">
      <div className="p-4 flex-1 space-y-1.5">
        <div className="px-3 py-2 text-[11px] font-bold uppercase tracking-wider text-slate-400">
          Chức năng nghiệp vụ
        </div>

        {visibleItems.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-semibold transition-all cursor-pointer ${
                  isActive
                    ? 'bg-amber-500 text-white shadow-md shadow-amber-500/20'
                    : 'text-slate-400 hover:text-white hover:bg-slate-800/80'
                }`
              }
            >
              <Icon className="w-5 h-5 flex-shrink-0" />
              <span>{item.label}</span>
              {item.adminOnly && (
                <span className="ml-auto text-[10px] font-extrabold uppercase px-1.5 py-0.5 rounded bg-slate-800 text-amber-400 border border-amber-500/30">
                  Admin
                </span>
              )}
            </NavLink>
          );
        })}
      </div>

      {/* Footer info */}
      <div className="p-4 border-t border-slate-800/60 bg-slate-950/40 text-xs text-slate-500">
        <div className="flex items-center justify-between">
          <span>Phiên bản POS v2.0</span>
          <span className="inline-block w-2 h-2 rounded-full bg-emerald-500"></span>
        </div>
        <p className="text-[11px] text-slate-400 mt-1">Sẵn sàng phục vụ bàn</p>
      </div>
    </aside>
  );
};
