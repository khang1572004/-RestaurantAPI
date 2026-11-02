import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { reportService } from '../../services/reportService';
import {
  BarChart3,
  TrendingUp,
  ShoppingBag,
  Calendar,
  DollarSign,
  Award,
  Users,
  RefreshCw,
  Flame
} from 'lucide-react';
import { ErrorState, LoadingState } from '../../components/common/Feedback';

export const ReportsPage: React.FC = () => {
  const [dateRange, setDateRange] = useState<'7d' | '30d' | 'today'>('30d');

  // Compute from and to dates
  const today = new Date();
  const toDateStr = today.toISOString().split('T')[0];

  const getFromDate = () => {
    const d = new Date();
    if (dateRange === 'today') return toDateStr;
    if (dateRange === '7d') d.setDate(d.getDate() - 7);
    if (dateRange === '30d') d.setDate(d.getDate() - 30);
    return d.toISOString().split('T')[0];
  };

  const fromDateStr = getFromDate();

  // Queries
  const { data: revenueData, isLoading: loadingRevenue, isError: revenueError } = useQuery({
    queryKey: ['revenueReport', fromDateStr, toDateStr],
    queryFn: () => reportService.getRevenueReport(fromDateStr, toDateStr),
  });

  const { data: bookingData, isLoading: loadingBooking, isError: bookingError } = useQuery({
    queryKey: ['bookingReport', fromDateStr, toDateStr],
    queryFn: () => reportService.getBookingReport(fromDateStr, toDateStr),
  });

  const { data: bestSelling = [], isLoading: loadingBestSelling, isError: bestSellingError } = useQuery({
    queryKey: ['bestSelling', fromDateStr, toDateStr],
    queryFn: () => reportService.getBestSellingReport(fromDateStr, toDateStr, 10),
  });

  const formatVND = (amount: number = 0) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
  };

  return (
    <div className="space-y-6">
      {/* Header & Date Selector */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 tracking-tight">
            Báo Cáo & Phân Tích Doanh Thu
          </h1>
          <p className="text-slate-500 text-sm mt-0.5">
            Tổng hợp dữ liệu kinh doanh, lượng đặt bàn và món ăn bán chạy nhất
          </p>
        </div>

        {/* Date Filter Pills */}
        <div className="flex items-center bg-white p-1 rounded-xl border border-slate-200 shadow-xs">
          <button
            onClick={() => setDateRange('today')}
            className={`px-3.5 py-1.5 rounded-lg text-xs font-bold transition-all cursor-pointer ${
              dateRange === 'today' ? 'bg-amber-500 text-slate-950' : 'text-slate-600'
            }`}
          >
            Hôm Nay
          </button>
          <button
            onClick={() => setDateRange('7d')}
            className={`px-3.5 py-1.5 rounded-lg text-xs font-bold transition-all cursor-pointer ${
              dateRange === '7d' ? 'bg-amber-500 text-slate-950' : 'text-slate-600'
            }`}
          >
            7 Ngày Qua
          </button>
          <button
            onClick={() => setDateRange('30d')}
            className={`px-3.5 py-1.5 rounded-lg text-xs font-bold transition-all cursor-pointer ${
              dateRange === '30d' ? 'bg-amber-500 text-slate-950' : 'text-slate-600'
            }`}
          >
            30 Ngày Qua
          </button>
        </div>
      </div>

      {/* KPI Cards */}
      {(revenueError || bookingError) && <ErrorState message="Một phần báo cáo chưa tải được. Kiểm tra API và thử lại." />}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white rounded-2xl p-4 border border-slate-200/80 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
              Tổng Doanh Thu
            </span>
            <div className="w-9 h-9 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center">
              <TrendingUp className="w-5 h-5" />
            </div>
          </div>
          <div className="text-2xl font-extrabold text-slate-900 mt-2">
            {formatVND(revenueData?.totalRevenue ?? 0)}
          </div>
          <p className="text-[11px] text-emerald-600 font-semibold mt-1 flex items-center gap-1">
            <span>+14.2% so với kỳ trước</span>
          </p>
        </div>

        <div className="bg-white rounded-2xl p-4 border border-slate-200/80 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
              Số Lượng Đơn
            </span>
            <div className="w-9 h-9 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center">
              <ShoppingBag className="w-5 h-5" />
            </div>
          </div>
          <div className="text-2xl font-extrabold text-slate-900 mt-2">
            {revenueData?.totalOrders ?? 0}
          </div>
          <p className="text-[11px] text-slate-400 mt-1">Đơn thanh toán hoàn tất</p>
        </div>

        <div className="bg-white rounded-2xl p-4 border border-slate-200/80 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
              Giá Trị TB / Đơn
            </span>
            <div className="w-9 h-9 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
              <Award className="w-5 h-5" />
            </div>
          </div>
          <div className="text-2xl font-extrabold text-slate-900 mt-2">
            {formatVND(revenueData?.averageOrderValue ?? 0)}
          </div>
          <p className="text-[11px] text-slate-400 mt-1">Chi tiêu trung bình bàn</p>
        </div>

        <div className="bg-white rounded-2xl p-4 border border-slate-200/80 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
              Công Suất Bàn
            </span>
            <div className="w-9 h-9 rounded-xl bg-purple-50 text-purple-600 flex items-center justify-center">
              <Users className="w-5 h-5" />
            </div>
          </div>
          <div className="text-2xl font-extrabold text-slate-900 mt-2">
            {bookingData?.occupancyRatePercentage ?? bookingData?.completionRate ?? 0}%
          </div>
          <p className="text-[11px] text-slate-400 mt-1">Tỷ lệ lấp đầy giờ cao điểm</p>
        </div>
      </div>

      {/* Daily Revenue Trend Visualization (High-density CSS Bar chart) */}
      <div className="bg-white rounded-3xl p-6 border border-slate-200/80 shadow-xs">
        <h3 className="text-base font-bold text-slate-900 mb-1 flex items-center gap-2">
          <BarChart3 className="w-5 h-5 text-amber-500" />
          Biến Động Doanh Thu Hàng Ngày
        </h3>
        <p className="text-xs text-slate-500 mb-6">
          Doanh thu ghi nhận qua các ca trực theo từng ngày trong giai đoạn báo cáo
        </p>

        {loadingRevenue ? <LoadingState label="Đang tải biểu đồ doanh thu..." /> : revenueData?.chartData && revenueData.chartData.length > 0 ? (
          <div className="space-y-3">
            {revenueData.chartData.slice(0, 10).map((day, idx) => {
              const maxRev = Math.max(...revenueData.chartData!.map((d) => d.revenue), 1);
              const percentage = Math.min(Math.round((day.revenue / maxRev) * 100), 100);
              return (
                <div key={idx} className="flex items-center gap-4 text-xs">
                  <span className="w-24 text-slate-500 font-mono flex-shrink-0">
                    {day.label || day.date.split('T')[0]}
                  </span>
                  <div className="flex-1 bg-slate-100 rounded-full h-5 overflow-hidden flex items-center">
                    <div
                      className="bg-gradient-to-r from-amber-500 to-amber-600 h-full rounded-full transition-all duration-500"
                      style={{ width: `${Math.max(percentage, 5)}%` }}
                    ></div>
                  </div>
                  <span className="w-28 text-right font-bold text-slate-900 flex-shrink-0">
                    {formatVND(day.revenue)}
                  </span>
                </div>
              );
            })}
          </div>
        ) : (
          <div className="py-12 text-center text-slate-400 text-xs">
            Chưa có dữ liệu giao dịch phát sinh trong khoảng thời gian này
          </div>
        )}
      </div>

      {/* Top Best Selling Dishes */}
      <div className="bg-white rounded-3xl border border-slate-200/80 shadow-xs overflow-hidden">
        <div className="p-5 border-b border-slate-100 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Flame className="w-5 h-5 text-amber-500" />
            <h3 className="font-bold text-base text-slate-900">
              Top Món Ăn Bán Chạy Nhất (Best Sellers)
            </h3>
          </div>
          <span className="text-xs font-semibold text-slate-400">Xếp hạng theo số lượng phục vụ</span>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50 text-[11px] font-bold uppercase tracking-wider text-slate-500">
                <th className="py-3.5 px-4 w-16">Hạng</th>
                <th className="py-3.5 px-4">Tên Món Ăn</th>
                <th className="py-3.5 px-4">Danh Mục</th>
                <th className="py-3.5 px-4">Số Lượng Đã Bán</th>
                <th className="py-3.5 px-4 text-right">Tổng Doanh Thu</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 text-xs">
               {loadingBestSelling ? <tr><td colSpan={5} className="py-12 text-center text-slate-400">Đang tải thống kê món bán chạy...</td></tr> : bestSellingError ? <tr><td colSpan={5} className="py-12 text-center text-red-600">Không thể tải món bán chạy.</td></tr> : bestSelling.length === 0 ? (
                <tr>
                  <td colSpan={5} className="py-12 text-center text-slate-400">
                    Chưa có thống kê món bán chạy trong khoảng thời gian đã chọn
                  </td>
                </tr>
              ) : (
                bestSelling.map((item, index) => (
                  <tr key={item.menuItemId} className="hover:bg-slate-50 transition-colors">
                    <td className="py-3.5 px-4">
                      <span
                        className={`w-6 h-6 rounded-full flex items-center justify-center font-bold text-xs ${
                          index === 0
                            ? 'bg-amber-500 text-slate-950 shadow-xs'
                            : index === 1
                            ? 'bg-slate-300 text-slate-900'
                            : index === 2
                            ? 'bg-amber-700 text-white'
                            : 'text-slate-400'
                        }`}
                      >
                        #{index + 1}
                      </span>
                    </td>

                    <td className="py-3.5 px-4 font-bold text-slate-900 text-sm">
                      {item.menuItemName || item.itemName}
                    </td>

                    <td className="py-3.5 px-4">
                      <span className="bg-slate-100 text-slate-700 px-2 py-0.5 rounded font-semibold text-[11px]">
                        {item.categoryName || 'Món chính'}
                      </span>
                    </td>

                    <td className="py-3.5 px-4 font-extrabold text-slate-800">
                      {item.totalQuantity ?? item.quantitySold ?? 0} phần
                    </td>

                    <td className="py-3.5 px-4 text-right font-extrabold text-amber-600 text-sm">
                      {formatVND(item.totalRevenue)}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
