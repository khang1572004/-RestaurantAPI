import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { menuService } from '../../services/menuService';
import { ArrowRight, Utensils, Star, ShieldCheck, Clock } from 'lucide-react';

const formatMoney = (v: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(v);

export function HomePage() {
  const { data: dishes = [] } = useQuery({
    queryKey: ['public-menu'],
    queryFn: () => menuService.getMenuItems(),
  });

  return (
    <div className="w-full">
      {/* Hero Section */}
      <section className="bg-[#450a0a] px-4 py-12 sm:px-6 sm:py-20 text-white">
        <div className="mx-auto grid max-w-7xl gap-8 lg:grid-cols-2 lg:items-center">
          <div>
            <p className="mb-3 text-xs sm:text-sm font-bold uppercase tracking-[.2em] text-amber-300">
              Fine Vietnamese Dining
            </p>
            <h1 className="font-serif text-3xl sm:text-5xl lg:text-6xl leading-tight">
              Một bữa tối đáng nhớ bắt đầu từ chiếc bàn hoàn hảo.
            </h1>
            <p className="mt-4 max-w-xl text-sm sm:text-base text-red-100">
              Khám phá thực đơn tuyển chọn bởi bếp trưởng và đặt bàn trực tuyến tiện lợi trong vài phút.
            </p>
            <div className="mt-6 flex flex-wrap gap-3">
              <Link
                to="/book-table"
                className="inline-flex items-center rounded-xl bg-[#a16207] px-5 py-3 text-xs sm:text-sm font-bold text-white hover:bg-amber-800 transition-colors shadow-lg shadow-amber-950/40"
              >
                Đặt bàn ngay <ArrowRight className="ml-2 h-4 w-4" aria-hidden="true" />
              </Link>
              <Link
                to="/menu"
                className="rounded-xl border border-red-200/40 px-5 py-3 text-xs sm:text-sm font-bold text-white hover:bg-white/10 transition-colors"
              >
                Xem thực đơn
              </Link>
            </div>
          </div>

          <div className="rounded-3xl border border-red-800 bg-gradient-to-br from-red-900/80 to-amber-950/80 p-5 sm:p-8 shadow-2xl backdrop-blur">
            <p className="font-serif text-xl sm:text-2xl text-white">Tinh hoa trên từng hương vị</p>
            <p className="mt-2 text-xs sm:text-sm text-red-200">
              Không gian sang trọng kết hợp ẩm thực truyền thống Việt Nam đương đại.
            </p>
            <div className="mt-6 grid grid-cols-3 gap-2 sm:gap-4 text-center">
              <div className="p-2 rounded-xl bg-red-950/50 border border-red-800/40">
                <b className="block text-xl sm:text-2xl text-amber-300">08</b>
                <span className="text-[11px] sm:text-xs text-red-200">Món tuyển chọn</span>
              </div>
              <div className="p-2 rounded-xl bg-red-950/50 border border-red-800/40">
                <b className="block text-xl sm:text-2xl text-amber-300">03</b>
                <span className="text-[11px] sm:text-xs text-red-200">Không gian</span>
              </div>
              <div className="p-2 rounded-xl bg-red-950/50 border border-red-800/40">
                <b className="block text-xl sm:text-2xl text-amber-300">4.9</b>
                <span className="text-[11px] sm:text-xs text-red-200">Đánh giá</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Featured Dishes */}
      <section className="mx-auto max-w-7xl px-4 py-12 sm:px-6 sm:py-16">
        <div className="mb-8 flex items-end justify-between">
          <div>
            <p className="text-xs sm:text-sm font-bold uppercase tracking-wider text-red-600">
              Chef Selection
            </p>
            <h2 className="font-serif text-2xl sm:text-3xl text-slate-900">
              Thực đơn được yêu thích
            </h2>
          </div>
          <Link to="/menu" className="text-xs sm:text-sm font-bold text-red-700 hover:text-red-800">
            Xem tất cả →
          </Link>
        </div>

        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {dishes.slice(0, 4).map((d) => (
            <article
              key={d.id}
              className="group rounded-2xl border border-red-100 bg-white p-4 shadow-sm hover:shadow-md transition-shadow flex flex-col justify-between"
            >
              <div>
                <div
                  className="grid h-36 place-items-center rounded-xl bg-red-50 text-red-700 group-hover:bg-red-100 transition-colors"
                  aria-hidden="true"
                >
                  <Utensils className="h-8 w-8" />
                </div>
                <h3 className="mt-3 font-bold text-slate-900 text-sm">{d.name}</h3>
                <p className="mt-1 line-clamp-2 text-xs text-slate-500">
                  {d.description || 'Món ăn đặc sắc với nguyên liệu cao cấp tươi mới mỗi ngày.'}
                </p>
              </div>
              <div className="mt-4 flex items-center justify-between pt-2 border-t border-slate-100">
                <span className="font-bold text-red-700 text-sm">{formatMoney(d.price)}</span>
                <span className="text-[11px] text-slate-400">Có sẵn</span>
              </div>
            </article>
          ))}
        </div>
      </section>

      {/* Highlights / Features */}
      <section className="bg-white border-y border-red-100 py-10 px-4 sm:px-6">
        <div className="mx-auto max-w-7xl grid gap-6 sm:grid-cols-3 text-center sm:text-left">
          <div className="flex flex-col sm:flex-row items-center sm:items-start gap-3">
            <div className="grid h-10 w-10 place-items-center rounded-xl bg-red-100 text-red-700 shrink-0">
              <Star className="h-5 w-5" />
            </div>
            <div>
              <b className="text-sm font-bold text-slate-900">Chuẩn vị Thượng hạng</b>
              <p className="text-xs text-slate-500 mt-1">Nguyên liệu được tuyển chọn kỹ lưỡng mỗi ngày.</p>
            </div>
          </div>

          <div className="flex flex-col sm:flex-row items-center sm:items-start gap-3">
            <div className="grid h-10 w-10 place-items-center rounded-xl bg-amber-100 text-amber-800 shrink-0">
              <Clock className="h-5 w-5" />
            </div>
            <div>
              <b className="text-sm font-bold text-slate-900">Đặt bàn tức thì</b>
              <p className="text-xs text-slate-500 mt-1">Chọn bàn và giữ chỗ trực tuyến không cần chờ đợi.</p>
            </div>
          </div>

          <div className="flex flex-col sm:flex-row items-center sm:items-start gap-3">
            <div className="grid h-10 w-10 place-items-center rounded-xl bg-emerald-100 text-emerald-800 shrink-0">
              <ShieldCheck className="h-5 w-5" />
            </div>
            <div>
              <b className="text-sm font-bold text-slate-900">Dịch vụ chu đáo</b>
              <p className="text-xs text-slate-500 mt-1">Đội ngũ phục vụ chuyên nghiệp, tận tâm.</p>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
