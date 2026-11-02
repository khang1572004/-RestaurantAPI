import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { ApiResponse, Voucher } from '../../types/api';
import { Ticket, Plus, Tag, Calendar, Percent, DollarSign, X } from 'lucide-react';

export const VouchersPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState({
    code: '',
    discountType: 'Percentage',
    discountValue: 10,
    minOrderAmount: 100000,
    startDate: new Date().toISOString().split('T')[0],
    endDate: new Date(Date.now() + 30 * 86400000).toISOString().split('T')[0],
    usageLimit: 100,
  });

  const { data: vouchers = [], isLoading, isError } = useQuery<Voucher[]>({
    queryKey: ['vouchers'],
    queryFn: async () => {
      const res = await apiClient.get<ApiResponse<Voucher[]>>('/admin/vouchers');
      const payload = res.data.data as unknown as { items?: Voucher[] } | Voucher[];
      return Array.isArray(payload) ? payload : payload.items || [];
    },
  });

  const createVoucherMutation = useMutation({
    mutationFn: async (data: any) => {
      const res = await apiClient.post<ApiResponse<Voucher>>('/admin/vouchers', data);
      return res.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vouchers'] });
      setModalOpen(false);
      alert('Tạo mã voucher thành công!');
    },
    onError: (error: any) => {
      alert(error?.response?.data?.message || error?.message || 'Không thể tạo voucher');
    },
  });

  const toggleVoucherMutation = useMutation({
    mutationFn: async (id: number) => {
      const voucher = vouchers.find((item) => item.id === id);
      await apiClient.put<ApiResponse<any>>(`/admin/vouchers/${id}/toggle`, { isActive: !voucher?.isActive });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vouchers'] });
    },
  });

  const formatVND = (val?: number) => {
    if (!val) return '0 đ';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 tracking-tight">
            Chương Trình Khuyến Mãi & Voucher
          </h1>
          <p className="text-slate-500 text-sm mt-0.5">
            Quản lý mã giảm giá áp dụng khi khách thanh toán hóa đơn
          </p>
        </div>

        <button
          onClick={() => setModalOpen(true)}
          className="inline-flex items-center gap-2 px-4 py-2 bg-slate-900 hover:bg-amber-600 text-white rounded-xl text-xs font-bold shadow-xs cursor-pointer transition-colors"
        >
          <Plus className="w-4 h-4 text-amber-400" />
          <span>Tạo Mã Mới</span>
        </button>
      </div>

      {isError && <div className="rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-700">Không thể tải danh sách voucher. Vui lòng thử lại.</div>}
      {!isLoading && !isError && vouchers.length === 0 && <div className="rounded-xl bg-white p-8 text-center text-sm text-slate-500">Chưa có voucher nào.</div>}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {vouchers.map((v) => (
          <div
            key={v.id}
            className="bg-white rounded-2xl p-5 border border-slate-200/80 shadow-xs flex flex-col justify-between"
          >
            <div>
              <div className="flex items-center justify-between">
                <span className="px-3 py-1 bg-amber-50 text-amber-800 font-mono font-extrabold text-sm rounded-lg border border-amber-200 flex items-center gap-1.5">
                  <Ticket className="w-4 h-4 text-amber-600" />
                  {v.code}
                </span>

                <button
                  onClick={() => toggleVoucherMutation.mutate(v.id)}
                  className={`px-2.5 py-0.5 rounded-full text-[11px] font-bold cursor-pointer transition-colors ${
                    v.isActive
                      ? 'bg-emerald-100 text-emerald-800'
                      : 'bg-slate-100 text-slate-500'
                  }`}
                >
                  {v.isActive ? 'Đang kích hoạt' : 'Tạm dừng'}
                </button>
              </div>

              <div className="my-3 space-y-1.5 text-xs text-slate-600">
                <div className="flex justify-between">
                  <span>Mức giảm:</span>
                  <span className="font-bold text-slate-900 text-sm">
                    {v.discountType === 'Percentage'
                      ? `${v.discountValue}%`
                      : formatVND(v.discountValue)}
                  </span>
                </div>

                <div className="flex justify-between">
                  <span>Đơn tối thiểu:</span>
                    <span className="font-medium text-slate-800">{formatVND(v.minOrderAmount ?? v.minOrderValue)}</span>
                </div>

                <div className="flex justify-between">
                  <span>Lượt đã dùng:</span>
                  <span className="font-medium text-slate-800">
                    {v.usedCount} / {v.usageLimit ?? v.totalUsageLimit ?? '∞'}
                  </span>
                </div>

                <div className="flex justify-between pt-1 border-t border-slate-100 text-[11px] text-slate-400">
                  <span>Hạn dùng:</span>
                  <span>{v.endDate?.split('T')[0]}</span>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>

      {modalOpen && (
        <div className="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl max-w-sm w-full p-6 shadow-2xl border border-slate-200">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <h3 className="text-base font-bold text-slate-900">Tạo Mã Khuyến Mãi Mới</h3>
              <button
                onClick={() => setModalOpen(false)}
                className="p-1 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 cursor-pointer"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="my-4 space-y-3 text-xs">
              <div>
                <label className="block font-semibold text-slate-700 mb-1">Mã Voucher:</label>
                <input
                  type="text"
                  value={form.code}
                  onChange={(e) => setForm({ ...form, code: e.target.value.toUpperCase() })}
                  placeholder="GIAM20, VIP50K..."
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500 font-mono uppercase"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Loại Giảm:</label>
                  <select
                    value={form.discountType}
                    onChange={(e) => setForm({ ...form, discountType: e.target.value })}
                    className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500 bg-white"
                  >
                    <option value="Percentage">Theo %</option>
                    <option value="FixedAmount">Số Tiền (VND)</option>
                  </select>
                </div>

                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Giá Trị Giảm:</label>
                  <input
                    type="number"
                    value={form.discountValue}
                    onChange={(e) => setForm({ ...form, discountValue: parseFloat(e.target.value) })}
                    className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
                  />
                </div>
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Đơn Tối Thiểu:</label>
                <input
                  type="number"
                  step="10000"
                  value={form.minOrderAmount}
                  onChange={(e) => setForm({ ...form, minOrderAmount: parseFloat(e.target.value) })}
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Từ Ngày:</label>
                  <input
                    type="date"
                    value={form.startDate}
                    onChange={(e) => setForm({ ...form, startDate: e.target.value })}
                    className="w-full px-2.5 py-2 border border-slate-200 rounded-xl text-xs"
                  />
                </div>
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Đến Ngày:</label>
                  <input
                    type="date"
                    value={form.endDate}
                    onChange={(e) => setForm({ ...form, endDate: e.target.value })}
                    className="w-full px-2.5 py-2 border border-slate-200 rounded-xl text-xs"
                  />
                </div>
              </div>
            </div>

            <div className="flex items-center gap-3 pt-3 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setModalOpen(false)}
                className="flex-1 py-2.5 border border-slate-200 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-50 cursor-pointer"
              >
                Hủy
              </button>
              <button
                type="button"
                disabled={!form.code.trim() || createVoucherMutation.isPending}
                 onClick={() => createVoucherMutation.mutate({
                   code: form.code.trim().toUpperCase(),
                   name: form.code.trim().toUpperCase(),
                   discountType: form.discountType === 'Percentage' ? 1 : 2,
                   discountValue: form.discountValue,
                   minOrderValue: form.minOrderAmount,
                   maxUsagePerCustomer: 1,
                   totalUsageLimit: form.usageLimit,
                   startDate: `${form.startDate}T00:00:00`,
                   endDate: `${form.endDate}T23:59:59`,
                   isActive: true,
                 })}
                className="flex-1 py-2.5 bg-amber-500 hover:bg-amber-600 text-slate-950 font-bold rounded-xl text-xs cursor-pointer disabled:opacity-50"
              >
                {createVoucherMutation.isPending ? 'Đang tạo...' : 'Tạo Voucher'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
