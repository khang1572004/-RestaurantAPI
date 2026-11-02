import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { ApiResponse } from '../../types/api';
import { Settings, Clock, Save, ShieldCheck, DollarSign } from 'lucide-react';

export const SettingsPage: React.FC = () => {
  const queryClient = useQueryClient();

  const [form, setForm] = useState({
    restaurantName: 'The Gourmet Palace',
    address: '123 Đường Lê Lợi, Quận 1, TP. Hồ Chí Minh',
    phoneNumber: '028 3822 9999',
    openingTime: '10:00',
    closingTime: '22:30',
    minAdvanceHours: 2,
    maxAdvanceDays: 30,
    depositRequired: true,
    depositPercent: 20,
  });

  const settingsQuery = useQuery({ queryKey: ['restaurant-settings'], queryFn: async () => (await apiClient.get<ApiResponse<any>>('/admin/restaurant/settings')).data.data });
  const saveInfo = useMutation({ mutationFn: () => apiClient.put('/admin/restaurant/settings', { name: form.restaurantName, address: form.address, phone: form.phoneNumber, email: 'contact@restaurant.com' }), onSuccess: () => queryClient.invalidateQueries({ queryKey: ['restaurant-settings'] }) });
  const saveHours = useMutation({ mutationFn: () => apiClient.put('/admin/restaurant/opening-hours', { openingHours: { mon: `${form.openingTime}-${form.closingTime}`, tue: `${form.openingTime}-${form.closingTime}`, wed: `${form.openingTime}-${form.closingTime}`, thu: `${form.openingTime}-${form.closingTime}`, fri: `${form.openingTime}-${form.closingTime}`, sat: `${form.openingTime}-${form.closingTime}`, sun: `${form.openingTime}-${form.closingTime}` } }) });
  const savePolicy = useMutation({ mutationFn: () => apiClient.put('/admin/restaurant/booking-policy', { minAdvanceHours: form.minAdvanceHours, cancellationHours: 4, requireDeposit: form.depositRequired, defaultDepositAmount: null }) });
  const handleSave = (e: React.FormEvent) => { e.preventDefault(); saveInfo.mutate(); saveHours.mutate(); savePolicy.mutate(); };

  return (
    <div className="space-y-6 max-w-4xl">
      <div>
        <h1 className="text-2xl font-bold text-slate-900 tracking-tight">
          Cài Đặt & Chính Sách Nhà Hàng
        </h1>
        <p className="text-slate-500 text-sm mt-0.5">
          Cấu hình khung giờ hoạt động, chính sách đặt cọc và quy định đặt bàn trước
        </p>
      </div>

      <form onSubmit={handleSave} className="bg-white rounded-3xl p-6 border border-slate-200/80 shadow-xs space-y-6">
        <div className="space-y-4">
          <h3 className="text-sm font-bold uppercase tracking-wider text-slate-400">
            Thông Tin Nhà Hàng
          </h3>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 text-xs">
            <div>
              <label className="block font-semibold text-slate-700 mb-1">Tên Nhà Hàng:</label>
              <input
                type="text"
                value={form.restaurantName}
                onChange={(e) => setForm({ ...form, restaurantName: e.target.value })}
                className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
              />
            </div>

            <div>
              <label className="block font-semibold text-slate-700 mb-1">Số Điện Thoại Liên Hệ:</label>
              <input
                type="text"
                value={form.phoneNumber}
                onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })}
                className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
              />
            </div>

            <div className="sm:col-span-2">
              <label className="block font-semibold text-slate-700 mb-1">Địa Chỉ Nhà Hàng:</label>
              <input
                type="text"
                value={form.address}
                onChange={(e) => setForm({ ...form, address: e.target.value })}
                className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
              />
            </div>
          </div>
        </div>

        <div className="pt-4 border-t border-slate-100 space-y-4">
          <h3 className="text-sm font-bold uppercase tracking-wider text-slate-400">
            Khung Giờ Hoạt Động
          </h3>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 text-xs">
            <div>
              <label className="block font-semibold text-slate-700 mb-1">Giờ Mở Cửa:</label>
              <input
                type="time"
                value={form.openingTime}
                onChange={(e) => setForm({ ...form, openingTime: e.target.value })}
                className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
              />
            </div>

            <div>
              <label className="block font-semibold text-slate-700 mb-1">Giờ Đóng Cửa:</label>
              <input
                type="time"
                value={form.closingTime}
                onChange={(e) => setForm({ ...form, closingTime: e.target.value })}
                className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
              />
            </div>
          </div>
        </div>

        <div className="pt-4 border-t border-slate-100 space-y-4">
          <h3 className="text-sm font-bold uppercase tracking-wider text-slate-400">
            Quy Định Đặt Bàn & Đặt Cọc
          </h3>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 text-xs">
            <div>
              <label className="block font-semibold text-slate-700 mb-1">Số Giờ Đặt Trước Tối Thiểu:</label>
              <div className="relative">
                <input
                  type="number"
                  min="1"
                  max="48"
                  value={form.minAdvanceHours}
                  onChange={(e) => setForm({ ...form, minAdvanceHours: parseInt(e.target.value) })}
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
                />
                <span className="absolute right-3 top-2 text-slate-400 text-xs">giờ</span>
              </div>
            </div>

            <div>
              <label className="block font-semibold text-slate-700 mb-1">Cho Phép Đặt Trước Tối Đa:</label>
              <div className="relative">
                <input
                  type="number"
                  min="1"
                  max="90"
                  value={form.maxAdvanceDays}
                  onChange={(e) => setForm({ ...form, maxAdvanceDays: parseInt(e.target.value) })}
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
                />
                <span className="absolute right-3 top-2 text-slate-400 text-xs">ngày</span>
              </div>
            </div>

            <div>
              <label className="block font-semibold text-slate-700 mb-1">% Đặt Cọc Trước:</label>
              <div className="relative">
                <input
                  type="number"
                  min="0"
                  max="100"
                  value={form.depositPercent}
                  onChange={(e) => setForm({ ...form, depositPercent: parseInt(e.target.value) })}
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
                />
                <span className="absolute right-3 top-2 text-slate-400 text-xs">%</span>
              </div>
            </div>
          </div>
        </div>

        <div className="pt-4 flex justify-end">
          <button
            type="submit"
            className="inline-flex items-center gap-2 px-6 py-2.5 bg-amber-500 hover:bg-amber-600 text-slate-950 font-bold rounded-xl text-xs shadow-md shadow-amber-500/20 cursor-pointer transition-colors"
          >
            <Save className="w-4 h-4" />
            <span>{saveInfo.isPending || saveHours.isPending || savePolicy.isPending ? 'Đang lưu...' : 'Lưu Thay Đổi Cấu Hình'}</span>
          </button>
        </div>
      </form>
    </div>
  );
};
