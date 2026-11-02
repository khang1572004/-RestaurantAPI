import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { customerService, type AvailableTable } from '../../services/customerService';
import { paymentService } from '../../services/paymentService';
import { PAYMENT_METHODS } from '../../constants/business';
import { useAuthStore } from '../../store/authStore';
import { Link, useNavigate } from 'react-router-dom';
import { Calendar, Clock, Users, CheckCircle2, AlertCircle, Sparkles, Check, ArrowRight } from 'lucide-react';

export function BookTablePage() {
  const navigate = useNavigate();
  const today = new Date().toISOString().slice(0, 10);
  const [date, setDate] = useState(today);
  const [time, setTime] = useState('19:00');
  const [guests, setGuests] = useState(2);
  const [tables, setTables] = useState<AvailableTable[]>([]);
  const [selectedTable, setSelectedTable] = useState<AvailableTable | null>(null);
  const [note, setNote] = useState('');
  const [successBooking, setSuccessBooking] = useState<any>(null);
  const [createdBooking, setCreatedBooking] = useState<any>(null);

  const { user, isAuthenticated } = useAuthStore();
  const isCustomer = user?.role === 'Customer';

  const searchMutation = useMutation({
    mutationFn: () =>
      customerService.availability({
        bookingDate: date,
        bookingTime: time,
        numGuests: guests,
        serviceDurationMinutes: 120,
      }),
    onSuccess: (data) => {
      setTables(data);
      setSelectedTable(null);
    },
  });

  const bookMutation = useMutation({
    mutationFn: () =>
      customerService.createBooking({
        tableId: selectedTable!.id,
        bookingDate: date,
        bookingTime: time,
        numGuests: guests,
        serviceDurationMinutes: 120,
        notes: note,
      }),
    onSuccess: (data) => {
      setCreatedBooking(data);
    },
  });

  const depositMutation = useMutation({
    mutationFn: () => paymentService.createDeposit(createdBooking.id, PAYMENT_METHODS.VNPAY),
    onSuccess: (payment) => {
      if (payment.paymentUrl) window.location.assign(payment.paymentUrl);
      else setSuccessBooking(createdBooking);
    },
  });

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 sm:py-12">
      <div className="mb-8">
        <p className="text-xs sm:text-sm font-bold uppercase tracking-wider text-red-600">
          Trực tuyến 24/7 • Xác nhận tức thì
        </p>
        <h1 className="mt-1 font-serif text-2xl sm:text-4xl text-[#450a0a]">
          Đặt bàn cho bữa tiệc của bạn
        </h1>
        <p className="mt-2 text-sm sm:text-base text-slate-600">
          Ba bước đơn giản: chọn thời gian, chọn vị trí bàn mong muốn và hoàn tất xác nhận.
        </p>
      </div>

      {successBooking ? (
        <div className="rounded-2xl border border-emerald-200 bg-white p-6 sm:p-8 text-center shadow-lg">
          <div className="mx-auto grid h-16 w-16 place-items-center rounded-full bg-emerald-100 text-emerald-600">
            <CheckCircle2 className="h-8 w-8" />
          </div>
          <h2 className="mt-4 font-serif text-2xl text-slate-900">Đặt bàn thành công!</h2>
          <p className="mt-2 text-slate-600 text-sm max-w-md mx-auto">
            Cảm ơn bạn đã lựa chọn Gourmet Palace. Bàn của bạn đã được ghi nhận trong hệ thống.
          </p>
          <div className="my-6 inline-block rounded-xl bg-slate-50 border border-slate-200 p-4 text-left text-sm">
            <p><span className="text-slate-500">Mã đặt bàn:</span> <b className="text-slate-900">#{successBooking.id || successBooking.bookingId || 'Confirmed'}</b></p>
            <p><span className="text-slate-500">Ngày giờ:</span> <b className="text-slate-900">{date} lúc {time}</b></p>
            <p><span className="text-slate-500">Số lượng khách:</span> <b className="text-slate-900">{guests} người</b></p>
            {selectedTable && <p><span className="text-slate-500">Bàn:</span> <b className="text-slate-900">Bàn số {selectedTable.tableNumber} ({selectedTable.zoneName || selectedTable.tableTypeName || 'Tiêu chuẩn'})</b></p>}
          </div>
          <div className="flex flex-wrap justify-center gap-3">
            <Link
              to="/my-bookings"
              className="inline-flex items-center rounded-xl bg-red-600 px-5 py-2.5 text-sm font-bold text-white hover:bg-red-700 shadow-sm"
            >
              Xem danh sách đặt bàn
            </Link>
            <button
              onClick={() => {
                setSuccessBooking(null);
                setSelectedTable(null);
                setTables([]);
              }}
              className="rounded-xl border border-slate-300 px-5 py-2.5 text-sm font-semibold text-slate-700 hover:bg-slate-50"
            >
              Đặt bàn khác
            </button>
          </div>
        </div>
      ) : createdBooking ? (
        <div className="rounded-2xl border border-amber-200 bg-white p-6 shadow-lg">
          <p className="text-xs font-bold uppercase tracking-wider text-amber-700">Booking đã được tạo</p>
          <h2 className="mt-2 font-serif text-2xl text-slate-900">Thanh toán cọc để xác nhận bàn</h2>
          <p className="mt-2 text-sm text-slate-600">Mã đặt bàn: <strong>{createdBooking.bookingCode}</strong>. Booking chỉ được xác nhận sau khi cổng thanh toán trả kết quả thành công.</p>
          <button onClick={() => depositMutation.mutate()} disabled={depositMutation.isPending} className="mt-6 rounded-xl bg-blue-600 px-5 py-3 text-sm font-bold text-white disabled:opacity-50">{depositMutation.isPending ? 'Đang tạo giao dịch...' : 'Thanh toán cọc qua VNPay Sandbox'}</button>
          {depositMutation.isError && <p className="mt-3 text-sm text-red-600">Không thể tạo giao dịch cọc. Vui lòng thử lại.</p>}
        </div>
      ) : (
        <div className="grid gap-6 lg:grid-cols-3">
          {/* Step 1 & 2 */}
          <div className="space-y-6 lg:col-span-2">
            {/* Step 1: Choose time and guest count */}
            <section className="rounded-2xl border border-red-100 bg-white p-5 sm:p-6 shadow-sm">
              <div className="flex items-center gap-2 mb-4">
                <span className="grid h-6 w-6 place-items-center rounded-full bg-red-100 text-xs font-bold text-red-700">1</span>
                <h2 className="font-bold text-slate-900">Thời gian & số lượng khách</h2>
              </div>

              <div className="grid gap-3 sm:grid-cols-3">
                <label className="text-xs font-semibold text-slate-700">
                  <span className="flex items-center gap-1 mb-1.5"><Calendar className="h-3.5 w-3.5 text-red-600" /> Ngày đến</span>
                  <input
                    type="date"
                    min={today}
                    value={date}
                    onChange={(e) => setDate(e.target.value)}
                    className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm focus:border-red-500 focus:bg-white focus:outline-none"
                  />
                </label>

                <label className="text-xs font-semibold text-slate-700">
                  <span className="flex items-center gap-1 mb-1.5"><Clock className="h-3.5 w-3.5 text-red-600" /> Giờ đến</span>
                  <input
                    type="time"
                    value={time}
                    onChange={(e) => setTime(e.target.value)}
                    className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm focus:border-red-500 focus:bg-white focus:outline-none"
                  />
                </label>

                <label className="text-xs font-semibold text-slate-700">
                  <span className="flex items-center gap-1 mb-1.5"><Users className="h-3.5 w-3.5 text-red-600" /> Số khách</span>
                  <input
                    type="number"
                    min={1}
                    max={30}
                    value={guests}
                    onChange={(e) => setGuests(Math.max(1, parseInt(e.target.value) || 1))}
                    className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm focus:border-red-500 focus:bg-white focus:outline-none"
                  />
                </label>
              </div>

              <button
                onClick={() => searchMutation.mutate()}
                disabled={searchMutation.isPending}
                className="mt-4 inline-flex items-center rounded-xl bg-red-600 px-4 py-2.5 text-xs sm:text-sm font-bold text-white hover:bg-red-700 disabled:opacity-50 transition-colors shadow-sm cursor-pointer"
              >
                {searchMutation.isPending ? 'Đang tìm kiếm...' : 'Kiểm tra bàn trống'}
                <ArrowRight className="ml-1.5 h-4 w-4" />
              </button>

              {searchMutation.isError && (
                <div className="mt-3 flex items-center gap-2 text-xs text-rose-600 bg-rose-50 p-2.5 rounded-lg border border-rose-100">
                  <AlertCircle className="h-4 w-4 shrink-0" />
                  <span>Không thể kiểm tra bàn trống lúc này. Vui lòng thử lại.</span>
                </div>
              )}
            </section>

            {/* Step 2: Pick a table */}
            {tables.length > 0 && (
              <section className="rounded-2xl border border-red-100 bg-white p-5 sm:p-6 shadow-sm">
                <div className="flex items-center justify-between mb-4">
                  <div className="flex items-center gap-2">
                    <span className="grid h-6 w-6 place-items-center rounded-full bg-red-100 text-xs font-bold text-red-700">2</span>
                    <h2 className="font-bold text-slate-900">Chọn bàn phù hợp</h2>
                  </div>
                  <span className="text-xs text-slate-500">{tables.length} bàn khả dụng</span>
                </div>

                <div className="grid gap-3 sm:grid-cols-2">
                  {tables.map((t) => {
                    const isPicked = selectedTable?.id === t.id;
                    return (
                      <button
                        key={t.id}
                        type="button"
                        onClick={() => setSelectedTable(t)}
                        className={`text-left p-3.5 rounded-xl border transition-all cursor-pointer ${
                          isPicked
                            ? 'border-red-600 bg-red-50/70 ring-2 ring-red-600/20'
                            : 'border-slate-200 bg-white hover:border-red-200 hover:bg-slate-50'
                        }`}
                      >
                        <div className="flex items-center justify-between">
                          <b className="font-bold text-slate-900">Bàn số {t.tableNumber}</b>
                          {isPicked && <Check className="h-4 w-4 text-red-600" />}
                        </div>
                        <p className="mt-1 text-xs text-slate-500">
                          Sức chứa: <span className="font-semibold text-slate-700">{t.capacity} khách</span> • Vị trí: <span className="font-semibold text-slate-700">{t.zoneName || t.tableTypeName || 'Khu vực chung'}</span>
                        </p>
                        <p className="mt-2 text-xs font-bold text-red-700">
                          Tiền cọc: {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(t.depositAmount || 0)}
                        </p>
                      </button>
                    );
                  })}
                </div>
              </section>
            )}

            {searchMutation.isSuccess && tables.length === 0 && (
              <div className="rounded-2xl border border-amber-200 bg-amber-50 p-5 text-center text-sm text-amber-800">
                Rất tiếc, không còn bàn trống phù hợp với {guests} khách vào khung giờ {time} ngày {date}. Quý khách vui lòng chọn giờ hoặc ngày khác.
              </div>
            )}
          </div>

          {/* Step 3: Confirmation Summary */}
          <div>
            <section className="rounded-2xl border border-red-900 bg-[#350c0c] p-5 text-white shadow-md sticky top-20">
              <div className="flex items-center gap-2 mb-4">
                <Sparkles className="h-4 w-4 text-amber-400" />
                <h2 className="font-serif text-lg">Thông tin đặt bàn</h2>
              </div>

              <div className="space-y-3 text-xs border-b border-red-800/80 pb-4 text-red-100">
                <div className="flex justify-between">
                  <span className="text-red-300">Ngày:</span>
                  <span className="font-bold text-white">{date}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-red-300">Thời gian:</span>
                  <span className="font-bold text-white">{time}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-red-300">Số khách:</span>
                  <span className="font-bold text-white">{guests} người</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-red-300">Bàn đã chọn:</span>
                  <span className="font-bold text-amber-300">
                    {selectedTable ? `Bàn ${selectedTable.tableNumber} (${selectedTable.zoneName || selectedTable.tableTypeName || 'Khu vực chung'})` : 'Chưa chọn bàn'}
                  </span>
                </div>
                <div className="mt-3 flex justify-between border-t border-red-800/80 pt-3 text-xs">
                <span className="text-red-300">Tiền cọc:</span>
                <span className="font-bold text-amber-300">{selectedTable ? new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(selectedTable.depositAmount || 0) : 'Chọn bàn để xem'}</span>
                </div>
              </div>

              {/* Note input */}
              <div className="mt-4">
                <label className="block text-xs font-semibold text-red-200 mb-1.5">
                  Ghi chú cho nhà hàng (tùy chọn):
                </label>
                <textarea
                  rows={2}
                  value={note}
                  onChange={(e) => setNote(e.target.value)}
                  placeholder="Ví dụ: Bàn gần cửa sổ, tiệc sinh nhật..."
                  className="w-full rounded-lg border border-red-800 bg-red-950/60 p-2.5 text-xs text-white placeholder-red-300/40 focus:border-amber-400 focus:outline-none resize-none"
                />
              </div>

              {/* Action or Login prompt */}
              <div className="mt-5">
                {!isAuthenticated ? (
                  <div className="rounded-xl bg-red-950/80 border border-red-800/80 p-3 text-center">
                    <p className="text-xs text-red-200 mb-2">
                      Quý khách vui lòng đăng nhập để xác nhận đặt bàn.
                    </p>
                    <Link
                      to="/login"
                      className="inline-block w-full rounded-lg bg-red-600 py-2 text-xs font-bold text-white hover:bg-red-700 transition-colors"
                    >
                      Đăng nhập tài khoản
                    </Link>
                  </div>
                ) : !isCustomer ? (
                  <div className="rounded-xl bg-amber-950/80 border border-amber-800/80 p-3 text-center text-xs text-amber-200">
                    Tài khoản hiện tại ({user?.role}) không phải Khách hàng. Vui lòng đăng nhập với tài khoản khách hàng để đặt bàn.
                  </div>
                ) : (
                  <button
                    onClick={() => bookMutation.mutate()}
                    disabled={!selectedTable || bookMutation.isPending}
                    className="w-full rounded-xl bg-[#a16207] py-2.5 text-xs font-bold text-white hover:bg-amber-700 disabled:opacity-40 transition-colors cursor-pointer shadow-sm"
                  >
                    {bookMutation.isPending ? 'Đang xác nhận...' : 'Xác nhận đặt bàn'}
                  </button>
                )}

                {bookMutation.isError && (
                  <p className="mt-2 text-center text-xs text-rose-300">
                    {(bookMutation.error as any)?.message || 'Đặt bàn thất bại. Vui lòng thử lại.'}
                  </p>
                )}
              </div>
            </section>
          </div>
        </div>
      )}
    </div>
  );
}
