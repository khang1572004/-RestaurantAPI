import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { bookingService } from '../services/bookingService';
import { orderService } from '../services/orderService';
import { useAuthStore } from '../store/authStore';
import { Booking } from '../types/api';
import {
  CalendarCheck,
  Search,
  CheckCircle,
  XCircle,
  Clock,
  Check,
  Calendar,
  Phone,
  Users,
  AlertTriangle,
  X,
  CreditCard
} from 'lucide-react';

export const BookingsPage: React.FC = () => {
  const queryClient = useQueryClient();
  const { user } = useAuthStore();
  const isAdmin = user?.role === 'Admin';

  const [activeTab, setActiveTab] = useState<'today' | 'all'>('today');
  const [selectedDate, setSelectedDate] = useState<string>(
    new Date().toISOString().split('T')[0]
  );
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');

  // Modal for Cancel / Reject reason
  const [actionModal, setActionModal] = useState<{
    open: boolean;
    type: 'reject' | 'cancel';
    booking: Booking | null;
  }>({
    open: false,
    type: 'reject',
    booking: null,
  });
  const [reason, setReason] = useState('');
  const [historyBooking, setHistoryBooking] = useState<Booking | null>(null);
  const [historyOrders, setHistoryOrders] = useState<any[]>([]);
  const [historyLoading, setHistoryLoading] = useState(false);

  const openHistory = async (booking: Booking) => {
    setHistoryBooking(booking);
    setHistoryLoading(true);
    try { setHistoryOrders(await orderService.getHistory({ bookingId: booking.id })); }
    finally { setHistoryLoading(false); }
  };

  // Queries
  const { data: todayBookings = [], isLoading: loadingToday } = useQuery({
    queryKey: ['todayBookings'],
    queryFn: bookingService.getTodayBookings,
    enabled: activeTab === 'today',
  });

  const { data: dateBookings = [], isLoading: loadingDate } = useQuery({
    queryKey: ['dateBookings', selectedDate],
    queryFn: () => bookingService.getBookingsByDate(selectedDate),
    enabled: activeTab === 'all',
  });

  const bookingsList = activeTab === 'today' ? todayBookings : dateBookings;

  // Check-In Mutation
  const checkInMutation = useMutation({
    mutationFn: (code: string) => bookingService.checkIn(code),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todayBookings'] });
      queryClient.invalidateQueries({ queryKey: ['dateBookings'] });
      queryClient.invalidateQueries({ queryKey: ['tableStatus'] });
      alert('Check-in khách thành công!');
    },
    onError: (err: any) => {
      alert(`Lỗi check-in: ${err.message}`);
    },
  });

  // Approve Mutation
  const approveMutation = useMutation({
    mutationFn: (id: number) => bookingService.approveBooking(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todayBookings'] });
      queryClient.invalidateQueries({ queryKey: ['dateBookings'] });
      alert('Đã duyệt đơn đặt bàn thành công!');
    },
    onError: (err: any) => {
      alert(`Lỗi duyệt: ${err.message}`);
    },
  });

  // Reject / Cancel Mutation
  const rejectCancelMutation = useMutation({
    mutationFn: async () => {
      if (!actionModal.booking) return;
      if (actionModal.type === 'reject') {
        await bookingService.rejectBooking(actionModal.booking.id, reason);
      } else {
        await bookingService.cancelBooking(actionModal.booking.id, reason);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todayBookings'] });
      queryClient.invalidateQueries({ queryKey: ['dateBookings'] });
      setActionModal({ open: false, type: 'reject', booking: null });
      setReason('');
      alert('Cập nhật trạng thái đặt bàn thành công!');
    },
    onError: (err: any) => {
      alert(`Lỗi: ${err.message}`);
    },
  });

  // Filtered List
  const filteredBookings = bookingsList.filter((b) => {
    const matchesSearch =
      searchQuery === '' ||
      b.bookingCode.toLowerCase().includes(searchQuery.toLowerCase()) ||
      b.customerName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      b.customerPhone.includes(searchQuery);

    const matchesStatus = statusFilter === 'all' || b.status === statusFilter;

    return matchesSearch && matchesStatus;
  });

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Pending':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold bg-amber-100 text-amber-800 border border-amber-300">
            <Clock className="w-3.5 h-3.5" />
            Chờ Duyệt
          </span>
        );
      case 'Confirmed':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold bg-blue-100 text-blue-800 border border-blue-300">
            <Check className="w-3.5 h-3.5" />
            Đã Xác Nhận
          </span>
        );
      case 'Arrived':
      case 'Seated':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold bg-emerald-100 text-emerald-800 border border-emerald-300">
            <CheckCircle className="w-3.5 h-3.5" />
            Đã Đến Bàn
          </span>
        );
      case 'Completed':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold bg-slate-100 text-slate-700">
            Hoàn Thành
          </span>
        );
      case 'Cancelled':
      case 'Rejected':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold bg-rose-100 text-rose-800 border border-rose-300">
            <XCircle className="w-3.5 h-3.5" />
            Đã Hủy
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-bold bg-slate-100 text-slate-700">
            {status}
          </span>
        );
    }
  };

  const formatVND = (val?: number) => {
    if (!val) return '0 đ';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 tracking-tight">
            Quản Lý Đặt Bàn (Reservations)
          </h1>
          <p className="text-slate-500 text-sm mt-0.5">
            Tiếp nhận, duyệt và làm thủ tục check-in khách đến nhà hàng
          </p>
        </div>

        {/* Tab switcher */}
        <div className="flex items-center bg-white p-1 rounded-xl border border-slate-200 shadow-xs">
          <button
            onClick={() => setActiveTab('today')}
            className={`px-4 py-2 rounded-lg text-xs font-bold transition-all cursor-pointer ${
              activeTab === 'today'
                ? 'bg-amber-500 text-slate-950 shadow-xs'
                : 'text-slate-600 hover:text-slate-900'
            }`}
          >
            Hôm Nay
          </button>
          <button
            onClick={() => setActiveTab('all')}
            className={`px-4 py-2 rounded-lg text-xs font-bold transition-all cursor-pointer ${
              activeTab === 'all'
                ? 'bg-amber-500 text-slate-950 shadow-xs'
                : 'text-slate-600 hover:text-slate-900'
            }`}
          >
            Theo Lịch Ngày
          </button>
        </div>
      </div>

      {/* Filter and Date Bar */}
      <div className="bg-white rounded-2xl p-4 border border-slate-200/80 shadow-xs flex flex-wrap items-center justify-between gap-4">
        <div className="flex flex-wrap items-center gap-3">
          {activeTab === 'all' && (
            <div className="flex items-center gap-2 bg-slate-50 px-3 py-1.5 rounded-xl border border-slate-200">
              <Calendar className="w-4 h-4 text-slate-500" />
              <input
                type="date"
                value={selectedDate}
                onChange={(e) => setSelectedDate(e.target.value)}
                className="text-xs font-bold bg-transparent text-slate-800 focus:outline-none"
              />
            </div>
          )}

          {/* Status filter pills */}
          <div className="flex items-center gap-1.5 overflow-x-auto">
            {['all', 'Pending', 'PendingApproval', 'AwaitingDeposit', 'Confirmed', 'CheckedIn', 'Cancelled'].map((st) => (
              <button
                key={st}
                onClick={() => setStatusFilter(st)}
                className={`px-3 py-1 rounded-lg text-xs font-bold transition-colors cursor-pointer ${
                  statusFilter === st
                    ? 'bg-slate-900 text-white'
                    : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                }`}
              >
                {st === 'all'
                  ? 'Tất cả'
                  : st === 'Pending'
                  ? 'Chờ duyệt'
                   : st === 'PendingApproval'
                   ? 'Chờ duyệt party'
                   : st === 'AwaitingDeposit'
                   ? 'Chờ cọc'
                   : st === 'Confirmed'
                   ? 'Đã xác nhận'
                   : st === 'CheckedIn'
                   ? 'Đã đến bàn'
                   : 'Đã hủy'}
              </button>
            ))}
          </div>
        </div>

        {/* Search */}
        <div className="relative w-full sm:w-64">
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Tìm theo mã, tên, SĐT..."
            className="w-full pl-9 pr-3 py-1.5 bg-slate-50 border border-slate-200 rounded-xl text-xs focus:outline-none focus:border-amber-500 focus:bg-white transition-all"
          />
        </div>
      </div>

      {/* Bookings Table View */}
      <div className="bg-white rounded-3xl border border-slate-200/80 shadow-xs overflow-hidden">
        {loadingToday || loadingDate ? (
          <div className="py-20 text-center text-slate-400">
            <Clock className="w-8 h-8 animate-spin mx-auto text-amber-500 mb-2" />
            <p className="text-xs font-semibold">Đang tải danh sách đặt bàn...</p>
          </div>
        ) : filteredBookings.length === 0 ? (
          <div className="py-20 text-center text-slate-400">
            <CalendarCheck className="w-12 h-12 mx-auto text-slate-300 mb-2" />
            <h4 className="text-sm font-bold text-slate-700">Không có đơn đặt bàn nào</h4>
            <p className="text-xs text-slate-400 mt-1">Chưa có khách đặt bàn trong khung thời gian này</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="border-b border-slate-200 bg-slate-50/75 text-[11px] font-bold uppercase tracking-wider text-slate-500">
                  <th className="py-3.5 px-4">Mã Đơn</th>
                  <th className="py-3.5 px-4">Khách Hàng</th>
                  <th className="py-3.5 px-4">Thời Gian</th>
                  <th className="py-3.5 px-4">Bàn & Khu Vực</th>
                  <th className="py-3.5 px-4">Tiền Cọc</th>
                  <th className="py-3.5 px-4">Trạng Thái</th>
                  <th className="py-3.5 px-4">Check-in / Check-out</th>
                  <th className="py-3.5 px-4 text-right">Thao Tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 text-xs text-slate-700">
                {filteredBookings.map((booking) => (
                  <tr key={booking.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="py-3.5 px-4">
                      <span className="font-mono font-bold text-amber-700 bg-amber-50 px-2 py-0.5 rounded border border-amber-200/60">
                        {booking.bookingCode}
                      </span>
                    </td>

                    <td className="py-3.5 px-4">
                      <div className="font-bold text-slate-900">{booking.customerName}</div>
                      <div className="flex items-center gap-1.5 text-slate-400 text-[11px] mt-0.5">
                        <Phone className="w-3 h-3" />
                        <span>{booking.customerPhone}</span>
                      </div>
                    </td>

                    <td className="py-3.5 px-4">
                      <div className="font-semibold text-slate-900">
                        {booking.bookingTime?.slice(0, 5) || '19:00'}
                      </div>
                      <div className="text-slate-400 text-[11px]">
                        {booking.bookingDate?.split('T')[0]}
                      </div>
                    </td>

                    <td className="py-3.5 px-4">
                      <div className="font-bold text-slate-800">
                        {booking.tableNumber || 'Chưa gán bàn'}
                      </div>
                      <div className="text-slate-400 text-[11px] flex items-center gap-1">
                        <Users className="w-3 h-3" />
                        <span>{booking.numGuests} khách</span>
                      </div>
                    </td>

                    <td className="py-3.5 px-4">
                      <div className="font-semibold text-slate-900">
                        {formatVND(booking.depositAmount)}
                      </div>
                      <div className="text-[11px]">
                         {(booking.isDepositPaid || booking.paymentStatus === 'Partial' || booking.paymentStatus === 'Paid') ? (
                          <span className="text-emerald-600 font-bold flex items-center gap-1">
                            <Check className="w-3 h-3" /> Đã thu cọc
                          </span>
                        ) : (
                          <span className="text-slate-400">Chưa cọc</span>
                        )}
                      </div>
                    </td>

                    <td className="py-3.5 px-4">
                      {getStatusBadge(booking.status)}
                    </td>

                    <td className="py-3.5 px-4 text-[11px] text-slate-500">
                      <div>Vào: <span className="font-semibold text-slate-700">{booking.checkInTime ? new Date(booking.checkInTime).toLocaleString('vi-VN') : '—'}</span></div>
                      <div>Ra: <span className="font-semibold text-emerald-700">{booking.checkOutTime ? new Date(booking.checkOutTime).toLocaleString('vi-VN') : '—'}</span></div>
                    </td>

                    <td className="py-3.5 px-4 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          <button onClick={() => openHistory(booking)} className="rounded-lg border border-slate-200 px-2.5 py-1.5 text-[11px] font-bold text-slate-600 hover:bg-slate-50">Chi tiết</button>
                        {/* Check-in button for confirmed bookings */}
                        {(booking.status === 'Confirmed' || booking.status === 'Pending') && (
                          <button
                            onClick={() => checkInMutation.mutate(booking.bookingCode)}
                            disabled={checkInMutation.isPending}
                            className="px-3 py-1.5 rounded-lg bg-amber-500 hover:bg-amber-600 text-slate-950 font-bold text-xs transition-colors cursor-pointer shadow-xs disabled:opacity-50"
                            title="Làm thủ tục cho khách vào bàn"
                          >
                            Check-in
                          </button>
                        )}

                        {/* Admin actions */}
                        {isAdmin && booking.status === 'Pending' && (
                          <>
                            <button
                              onClick={() => approveMutation.mutate(booking.id)}
                              disabled={approveMutation.isPending}
                              className="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded-lg cursor-pointer"
                              title="Duyệt đặt bàn"
                            >
                              <CheckCircle className="w-4 h-4" />
                            </button>

                            <button
                              onClick={() => {
                                setActionModal({ open: true, type: 'reject', booking });
                                setReason('');
                              }}
                              className="p-1.5 text-rose-600 hover:bg-rose-50 rounded-lg cursor-pointer"
                              title="Từ chối đặt bàn"
                            >
                              <XCircle className="w-4 h-4" />
                            </button>
                          </>
                        )}

                        {isAdmin && booking.status === 'Confirmed' && (
                          <button
                            onClick={() => {
                              setActionModal({ open: true, type: 'cancel', booking });
                              setReason('');
                            }}
                            className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg cursor-pointer text-xs font-semibold"
                            title="Hủy đặt bàn"
                          >
                            Hủy
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Reject / Cancel Reason Modal */}
      {actionModal.open && actionModal.booking && (
        <div className="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl max-w-md w-full p-6 shadow-2xl border border-slate-200">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <h3 className="text-base font-bold text-slate-900">
                {actionModal.type === 'reject' ? 'Từ Chối Đặt Bàn' : 'Hủy Đơn Đặt Bàn'}
              </h3>
              <button
                onClick={() => setActionModal({ open: false, type: 'reject', booking: null })}
                className="p-1 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 cursor-pointer"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <p className="text-xs text-slate-500 my-4">
              Bạn đang thao tác với mã booking{' '}
              <strong className="text-slate-900 font-mono">
                {actionModal.booking.bookingCode}
              </strong>{' '}
              của khách hàng <strong>{actionModal.booking.customerName}</strong>.
            </p>

            <div className="mb-4">
              <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                Lý do hủy / từ chối (bắt buộc):
              </label>
              <textarea
                rows={3}
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                placeholder="Nhập lý do gửi đến khách hàng..."
                className="w-full px-3 py-2 text-xs bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500 focus:bg-white resize-none"
              />
            </div>

            <div className="flex items-center gap-3 pt-2">
              <button
                type="button"
                onClick={() => setActionModal({ open: false, type: 'reject', booking: null })}
                className="flex-1 py-2.5 border border-slate-200 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-50 cursor-pointer"
              >
                Đóng
              </button>
              <button
                type="button"
                disabled={!reason.trim() || rejectCancelMutation.isPending}
                onClick={() => rejectCancelMutation.mutate()}
                className="flex-1 py-2.5 bg-rose-600 hover:bg-rose-700 text-white font-bold rounded-xl text-xs shadow-md shadow-rose-500/20 cursor-pointer disabled:opacity-50"
              >
                {rejectCancelMutation.isPending ? 'Đang gửi...' : 'Xác Nhận'}
              </button>
            </div>
          </div>
        </div>
      )}

      {historyBooking && <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 p-4"><div className="max-h-[80vh] w-full max-w-3xl overflow-hidden rounded-3xl bg-white shadow-2xl"><div className="flex items-center justify-between border-b p-5"><div><h3 className="font-bold text-slate-900">Lịch sử hóa đơn</h3><p className="text-xs text-slate-500">Booking {historyBooking.bookingCode} · {historyBooking.tableNumber}</p></div><button aria-label="Đóng lịch sử hóa đơn" onClick={() => setHistoryBooking(null)} className="rounded-lg p-2 text-slate-400 hover:bg-slate-100"><X className="h-5 w-5" /></button></div><div className="max-h-[60vh] overflow-y-auto p-5">{historyLoading ? <p className="text-sm text-slate-500">Đang tải lịch sử...</p> : historyOrders.length === 0 ? <p className="text-sm text-slate-500">Booking chưa có hóa đơn.</p> : <div className="space-y-3">{historyOrders.map((order) => <div key={order.id} className="rounded-xl border border-slate-200 p-4"><div className="flex justify-between"><b>Hóa đơn #{order.id}</b><span className="font-bold text-amber-700">{formatVND(order.finalAmount)}</span></div><p className="mt-1 text-xs text-slate-500">{order.status} · Tạo lúc {new Date(order.createdAt).toLocaleString('vi-VN')} {order.paidAt ? `· Thanh toán ${new Date(order.paidAt).toLocaleString('vi-VN')}` : ''}</p><div className="mt-2 text-xs text-slate-600">{order.items?.map((item: any) => <div key={item.id} className="flex justify-between"><span>{item.menuItemName} × {item.quantity}</span><span>{formatVND(item.subtotal)}</span></div>)}</div><div className="mt-2 border-t pt-2 text-xs">Đã trừ cọc: <b>{formatVND(order.depositAmount)}</b></div></div>)}</div>}</div></div></div>}
    </div>
  );
};
