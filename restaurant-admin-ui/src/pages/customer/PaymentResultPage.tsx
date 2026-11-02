import { Link, useSearchParams } from 'react-router-dom';
import { CheckCircle2, CircleAlert } from 'lucide-react';

export function PaymentResultPage() {
  const [params] = useSearchParams();
  const responseCode = params.get('vnp_ResponseCode') || params.get('resultCode');
  const success = responseCode === '00' || responseCode === '0' || params.get('status') === 'success' || params.get('success') === 'true';
  return <main className="mx-auto max-w-xl px-4 py-16 text-center sm:px-6"><div className={`mx-auto grid h-16 w-16 place-items-center rounded-full ${success ? 'bg-emerald-100 text-emerald-600' : 'bg-amber-100 text-amber-700'}`}>{success ? <CheckCircle2 className="h-9 w-9" /> : <CircleAlert className="h-9 w-9" />}</div><h1 className="mt-5 font-serif text-3xl text-[#450a0a]">{success ? 'Thanh toán đã được ghi nhận' : 'Thanh toán chưa hoàn tất'}</h1><p className="mt-3 text-sm leading-6 text-slate-600">{success ? 'Nhà hàng đang cập nhật trạng thái booking của bạn. Bạn có thể kiểm tra lại trong danh sách đặt bàn.' : 'Giao dịch chưa được xác nhận thành công. Bạn có thể quay lại booking để thử lại.'}</p><Link to="/my-bookings" className="mt-7 inline-flex rounded-xl bg-red-700 px-5 py-3 text-sm font-bold text-white hover:bg-red-800">Xem booking của tôi</Link></main>;
}
