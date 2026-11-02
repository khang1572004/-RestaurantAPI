import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { tableService } from '../services/tableService';
import { bookingService } from '../services/bookingService';
import { orderService } from '../services/orderService';
import { TableStatus } from '../types/api';
import {
  Users,
  RefreshCw,
  PlusCircle,
  Clock,
  CheckCircle,
  AlertTriangle,
  ChevronRight,
  Filter,
  Check,
  X,
  ShoppingBag
} from 'lucide-react';
import { ErrorState, LoadingState } from '../components/common/Feedback';

export const TablesPage: React.FC = () => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [selectedZone, setSelectedZone] = useState<string>('all');
  const [searchFilter, setSearchFilter] = useState('');
  const [checkInModal, setCheckInModal] = useState<{ open: boolean; table: TableStatus | null }>({
    open: false,
    table: null,
  });
  const [lateCheckInReason, setLateCheckInReason] = useState('');
  const [isLateCheckIn, setIsLateCheckIn] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  // Fetch Table Statuses
  const { data: tables = [], isLoading, isError, refetch, isRefetching } = useQuery({
    queryKey: ['tableStatus'],
    queryFn: tableService.getTableStatus,
    refetchInterval: 10000, // auto refresh every 10s
  });

  // Check-in Mutation
  const checkInMutation = useMutation({
    mutationFn: async ({ bookingCode, reason }: { bookingCode: string; reason?: string }) => {
      if (reason) {
        return await bookingService.lateCheckIn(bookingCode);
      }
      return await bookingService.checkIn(bookingCode);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tableStatus'] });
      setCheckInModal({ open: false, table: null });
      setIsLateCheckIn(false);
      setLateCheckInReason('');
      setActionError(null);
    },
    onError: (err: any) => {
      if (err.message?.includes('USE_LATE_CHECKIN_ENDPOINT') || err.message?.includes('muộn')) {
        setIsLateCheckIn(true);
        setActionError('Khách đến muộn hơn thời gian đặt bàn. Vui lòng nhập lý do để tiếp tục check-in.');
      } else {
        setActionError(err.message || 'Check-in thất bại');
      }
    },
  });

  // Create Walk-In Order Mutation
  const createWalkInMutation = useMutation({
    mutationFn: async (tableId: number) => {
      return await orderService.createOrder({ tableId });
    },
    onSuccess: (newOrder) => {
      queryClient.invalidateQueries({ queryKey: ['tableStatus'] });
      navigate(`/pos?tableId=${newOrder.tableId}&orderId=${newOrder.id}`);
    },
    onError: (err: any) => {
      alert(`Không thể tạo đơn: ${err.message}`);
    },
  });

  // Extract unique zones
  const zones = Array.from(new Set(tables.map((t) => t.zoneName).filter(Boolean))) as string[];

  // Counts
  const totalCount = tables.length;
  const availableCount = tables.filter((t) => t.status === 'Available').length;
  const reservedCount = tables.filter((t) => t.status === 'Reserved').length;
  const occupiedCount = tables.filter((t) => t.status === 'Occupied').length;

  // Filtered tables
  const filteredTables = tables.filter((table) => {
    const matchesZone = selectedZone === 'all' || table.zoneName === selectedZone;
    const matchesSearch =
      searchFilter === '' ||
      table.tableNumber.toLowerCase().includes(searchFilter.toLowerCase()) ||
      table.customerName?.toLowerCase().includes(searchFilter.toLowerCase()) ||
      table.currentBookingCode?.toLowerCase().includes(searchFilter.toLowerCase());
    return matchesZone && matchesSearch;
  });

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Available':
        return (
          <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold bg-emerald-100 text-emerald-800 border border-emerald-300">
            <span className="w-1.5 h-1.5 rounded-full bg-emerald-600"></span>
            Bàn Trống
          </span>
        );
      case 'Reserved':
        return (
          <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold bg-amber-100 text-amber-800 border border-amber-300">
            <span className="w-1.5 h-1.5 rounded-full bg-amber-600 animate-pulse"></span>
            Đã Đặt
          </span>
        );
      case 'Occupied':
        return (
          <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold bg-rose-100 text-rose-800 border border-rose-300">
            <span className="w-1.5 h-1.5 rounded-full bg-rose-600 animate-pulse"></span>
            Đang Dùng Bữa
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold bg-slate-100 text-slate-700">
            {status}
          </span>
        );
    }
  };

  const getCardBorder = (status: string) => {
    switch (status) {
      case 'Available':
        return 'border-emerald-200 bg-white hover:border-emerald-400 hover:shadow-emerald-500/10';
      case 'Reserved':
        return 'border-amber-300 bg-amber-50/20 hover:border-amber-500 hover:shadow-amber-500/10';
      case 'Occupied':
        return 'border-rose-300 bg-rose-50/20 hover:border-rose-500 hover:shadow-rose-500/10';
      default:
        return 'border-slate-200 bg-white';
    }
  };

  return (
    <div className="space-y-6">
      {/* Top Header & Refresh */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Sơ Đồ Bàn Trực Quan</h1>
          <p className="text-slate-500 text-sm mt-0.5">
            Theo dõi trạng thái phục vụ thời gian thực tại các khu vực nhà hàng
          </p>
        </div>

        <button
          onClick={() => refetch()}
          disabled={isLoading || isRefetching}
          className="inline-flex items-center gap-2 px-4 py-2 bg-white border border-slate-200 rounded-xl text-xs font-bold text-slate-700 hover:bg-slate-50 shadow-xs cursor-pointer transition-all disabled:opacity-50"
        >
          <RefreshCw className={`w-4 h-4 text-amber-600 ${isRefetching ? 'animate-spin' : ''}`} />
          <span>Cập nhật sơ đồ</span>
        </button>
      </div>

      {/* Overview Statistics Cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white rounded-2xl p-4 border border-slate-200/80 shadow-xs flex items-center justify-between">
          <div>
            <p className="text-xs font-semibold text-slate-500 uppercase tracking-wider">Tổng Bàn</p>
            <p className="text-2xl font-extrabold text-slate-900 mt-1">{totalCount}</p>
          </div>
          <div className="w-11 h-11 rounded-xl bg-slate-100 flex items-center justify-center text-slate-700">
            <Users className="w-5 h-5" />
          </div>
        </div>

        <div className="bg-white rounded-2xl p-4 border border-emerald-200/80 shadow-xs flex items-center justify-between">
          <div>
            <p className="text-xs font-semibold text-emerald-600 uppercase tracking-wider">Bàn Trống</p>
            <p className="text-2xl font-extrabold text-emerald-700 mt-1">{availableCount}</p>
          </div>
          <div className="w-11 h-11 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
            <CheckCircle className="w-5 h-5" />
          </div>
        </div>

        <div className="bg-white rounded-2xl p-4 border border-amber-200/80 shadow-xs flex items-center justify-between">
          <div>
            <p className="text-xs font-semibold text-amber-600 uppercase tracking-wider">Đã Đặt Trước</p>
            <p className="text-2xl font-extrabold text-amber-700 mt-1">{reservedCount}</p>
          </div>
          <div className="w-11 h-11 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center">
            <Clock className="w-5 h-5" />
          </div>
        </div>

        <div className="bg-white rounded-2xl p-4 border border-rose-200/80 shadow-xs flex items-center justify-between">
          <div>
            <p className="text-xs font-semibold text-rose-600 uppercase tracking-wider">Đang Có Khách</p>
            <p className="text-2xl font-extrabold text-rose-700 mt-1">{occupiedCount}</p>
          </div>
          <div className="w-11 h-11 rounded-xl bg-rose-50 text-rose-600 flex items-center justify-center">
            <ShoppingBag className="w-5 h-5" />
          </div>
        </div>
      </div>

      {/* Zone Tabs & Search Bar */}
      <div className="bg-white rounded-2xl p-3 border border-slate-200/80 shadow-xs flex flex-col md:flex-row items-stretch md:items-center justify-between gap-3">
        {/* Zone Pills */}
        <div className="flex items-center gap-1.5 overflow-x-auto pb-1 md:pb-0">
          <button
            onClick={() => setSelectedZone('all')}
            className={`px-3.5 py-1.5 rounded-xl text-xs font-bold transition-colors cursor-pointer whitespace-nowrap ${
              selectedZone === 'all'
                ? 'bg-slate-900 text-white'
                : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            Tất Cả Khu Vực ({totalCount})
          </button>
          {zones.map((zone) => {
            const countInZone = tables.filter((t) => t.zoneName === zone).length;
            return (
              <button
                key={zone}
                onClick={() => setSelectedZone(zone)}
                className={`px-3.5 py-1.5 rounded-xl text-xs font-bold transition-colors cursor-pointer whitespace-nowrap ${
                  selectedZone === zone
                    ? 'bg-amber-500 text-white'
                    : 'text-slate-600 hover:bg-slate-100'
                }`}
              >
                {zone} ({countInZone})
              </button>
            );
          })}
        </div>

        {/* Search */}
        <div className="relative">
          <input
            type="text"
            value={searchFilter}
            onChange={(e) => setSearchFilter(e.target.value)}
            placeholder="Tìm theo số bàn, khách, mã..."
            className="w-full md:w-64 pl-9 pr-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500 focus:bg-white transition-all"
          />
          <Filter className="w-3.5 h-3.5 text-slate-400 absolute left-3 top-2.5" />
        </div>
      </div>

      {/* Table Grid Matrix */}
      {isLoading ? (
        <LoadingState label="Đang tải dữ liệu sơ đồ bàn..." />
      ) : isError ? (
        <ErrorState message="Không thể tải sơ đồ bàn." onRetry={() => refetch()} />
      ) : filteredTables.length === 0 ? (
        <div className="bg-white rounded-2xl p-12 text-center border border-slate-200">
          <AlertTriangle className="w-10 h-10 text-amber-500 mx-auto mb-3" />
          <h3 className="text-base font-bold text-slate-800">Không tìm thấy bàn phù hợp</h3>
          <p className="text-xs text-slate-500 mt-1">
            Vui lòng thử chọn khu vực khác hoặc thay đổi từ khóa tìm kiếm.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          {filteredTables.map((table) => (
            <div
              key={table.id}
              className={`rounded-2xl p-4 border transition-all duration-200 shadow-xs flex flex-col justify-between ${getCardBorder(
                table.status
              )}`}
            >
              <div>
                {/* Header: Table Number & Status */}
                <div className="flex items-center justify-between mb-2">
                  <div className="flex items-center gap-2">
                    <span className="text-lg font-extrabold text-slate-900">
                      {table.tableNumber}
                    </span>
                    {table.zoneName && (
                      <span className="text-[11px] font-semibold text-slate-500 bg-slate-100 px-2 py-0.5 rounded-md">
                        {table.zoneName}
                      </span>
                    )}
                  </div>
                  {getStatusBadge(table.status)}
                </div>

                {/* Body details depending on status */}
                <div className="my-3 text-xs space-y-1.5 min-h-[52px]">
                  {table.status === 'Reserved' && (
                    <div className="p-2.5 bg-amber-100/60 rounded-xl text-amber-900 border border-amber-200/60">
                      <div className="font-bold flex items-center justify-between">
                        <span>{table.customerName || 'Khách đặt trước'}</span>
                        <span className="text-[11px] font-mono text-amber-700">
                          {table.currentBookingCode}
                        </span>
                      </div>
                      <div className="text-[11px] text-amber-800/80 mt-1 flex items-center justify-between">
                        <span>Giờ đến: {table.bookingTime ? new Date(table.bookingTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : 'N/A'}</span>
                        <span>{table.numGuests || 2} khách</span>
                      </div>
                    </div>
                  )}

                  {table.status === 'Occupied' && (
                    <div className="p-2.5 bg-rose-100/60 rounded-xl text-rose-900 border border-rose-200/60">
                      <div className="font-bold flex items-center justify-between">
                        <span>Đang có khách ngồi</span>
                        <span className="text-[11px] text-rose-700 font-mono">
                          {table.currentBookingCode || 'Khách vãng lai'}
                        </span>
                      </div>
                      <div className="text-[11px] text-rose-700 mt-1">
                        Sẵn sàng nhận thêm món và thanh toán
                      </div>
                    </div>
                  )}

                  {table.status === 'Available' && (
                    <p className="text-slate-400 text-xs italic pt-1">
                      Bàn đã được vệ sinh sạch sẽ, sẵn sàng đón khách.
                    </p>
                  )}
                </div>
              </div>

              {/* Card Actions */}
              <div className="pt-2 border-t border-slate-100 flex items-center gap-2">
                {table.status === 'Available' && (
                  <button
                    onClick={() => createWalkInMutation.mutate(table.id)}
                    disabled={createWalkInMutation.isPending}
                    className="w-full py-2 px-3 rounded-xl bg-slate-900 hover:bg-amber-600 text-white text-xs font-bold transition-colors flex items-center justify-center gap-1.5 cursor-pointer shadow-xs"
                  >
                    <PlusCircle className="w-4 h-4 text-amber-400" />
                    <span>Mở Bàn (Gọi Món)</span>
                  </button>
                )}

                {table.status === 'Reserved' && (
                  <button
                    onClick={() => {
                      setActionError(null);
                      setIsLateCheckIn(false);
                      setCheckInModal({ open: true, table });
                    }}
                    className="w-full py-2 px-3 rounded-xl bg-amber-500 hover:bg-amber-600 text-slate-950 font-bold text-xs transition-colors flex items-center justify-center gap-1.5 cursor-pointer shadow-xs"
                  >
                    <Check className="w-4 h-4" />
                    <span>Check-in Khách</span>
                  </button>
                )}

                {table.status === 'Occupied' && (
                  <button
                    onClick={() => navigate(`/pos?tableId=${table.id}`)}
                    className="w-full py-2 px-3 rounded-xl bg-rose-600 hover:bg-rose-700 text-white font-bold text-xs transition-colors flex items-center justify-center gap-1.5 cursor-pointer shadow-xs"
                  >
                    <ShoppingBag className="w-4 h-4" />
                    <span>Màn Hình POS</span>
                    <ChevronRight className="w-4 h-4 ml-auto" />
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Check-in Modal */}
      {checkInModal.open && checkInModal.table && (
        <div className="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl max-w-md w-full p-6 shadow-2xl border border-slate-200">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <h3 className="text-lg font-bold text-slate-900">
                Xác Nhận Check-in Khách Đến
              </h3>
              <button
                onClick={() => setCheckInModal({ open: false, table: null })}
                className="p-1 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 cursor-pointer"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {actionError && (
              <div className="my-4 p-3 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-xs flex items-start gap-2">
                <AlertTriangle className="w-4 h-4 flex-shrink-0 mt-0.5" />
                <span>{actionError}</span>
              </div>
            )}

            <div className="my-5 space-y-3 bg-slate-50 p-4 rounded-2xl border border-slate-200/80 text-xs text-slate-700">
              <div className="flex justify-between">
                <span className="text-slate-500">Số bàn:</span>
                <span className="font-bold text-slate-900 text-sm">
                  {checkInModal.table.tableNumber} ({checkInModal.table.zoneName})
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-500">Mã Booking:</span>
                <span className="font-mono font-bold text-amber-700">
                  {checkInModal.table.currentBookingCode}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-500">Tên khách hàng:</span>
                <span className="font-semibold text-slate-900">
                  {checkInModal.table.customerName || 'N/A'}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-500">Số lượng khách:</span>
                <span className="font-semibold text-slate-900">
                  {checkInModal.table.numGuests || 2} người
                </span>
              </div>
            </div>

            {isLateCheckIn && (
              <div className="mb-4">
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                  Lý do check-in muộn (bắt buộc):
                </label>
                <input
                  type="text"
                  value={lateCheckInReason}
                  onChange={(e) => setLateCheckInReason(e.target.value)}
                  placeholder="Ví dụ: Khách kẹt xe gọi điện báo trước..."
                  className="w-full px-3.5 py-2.5 bg-white border border-slate-300 rounded-xl text-xs focus:outline-none focus:border-amber-500"
                />
              </div>
            )}

            <div className="flex items-center gap-3 pt-2">
              <button
                type="button"
                onClick={() => setCheckInModal({ open: false, table: null })}
                className="flex-1 py-2.5 border border-slate-200 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-50 cursor-pointer"
              >
                Đóng
              </button>
              <button
                type="button"
                disabled={
                  checkInMutation.isPending ||
                  (isLateCheckIn && !lateCheckInReason.trim())
                }
                onClick={() => {
                  if (checkInModal.table?.currentBookingCode) {
                    checkInMutation.mutate({
                      bookingCode: checkInModal.table.currentBookingCode,
                      reason: isLateCheckIn ? lateCheckInReason : undefined,
                    });
                  }
                }}
                className="flex-1 py-2.5 bg-amber-500 hover:bg-amber-600 text-slate-950 font-bold rounded-xl text-xs shadow-md shadow-amber-500/20 cursor-pointer disabled:opacity-50"
              >
                {checkInMutation.isPending ? 'Đang xử lý...' : 'Xác Nhận Cho Vào Bàn'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
