import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { UtensilsCrossed, CalendarDays, Menu as MenuIcon, UserRound, LogOut } from 'lucide-react';
import { useAuthStore } from '../../store/authStore';

export function CustomerLayout() {
  const { user, logout } = useAuthStore(); const navigate = useNavigate();
  const customer = user?.role === 'Customer';
  const nav = [{ to: '/', label: 'Trang chủ' }, { to: '/menu', label: 'Thực đơn' }, { to: '/book-table', label: 'Đặt bàn' }];
  return <div className="min-h-screen overflow-x-hidden bg-[#fef2f2] text-[#450a0a]">
    <header className="sticky top-0 z-30 border-b border-red-100 bg-white/95 backdrop-blur"><div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-3 sm:px-6">
      <Link to="/" className="flex items-center gap-2 font-bold"><span className="grid h-9 w-9 place-items-center rounded-xl bg-red-600 text-white"><UtensilsCrossed className="h-5 w-5" aria-hidden="true" /></span><span className="font-serif text-lg">Gourmet Palace</span></Link>
      <nav className="hidden gap-5 sm:flex">{nav.map(i => <NavLink key={i.to} to={i.to} className={({isActive}) => `text-sm font-bold ${isActive ? 'text-red-700' : 'text-slate-600 hover:text-red-700'}`}>{i.label}</NavLink>)}</nav>
      <div className="flex items-center gap-2">{customer ? <><Link to="/my-bookings" className="inline-flex items-center gap-1 rounded-lg px-3 py-2 text-xs font-bold text-red-800 hover:bg-red-50"><UserRound className="h-4 w-4" aria-hidden="true" />Đặt bàn của tôi</Link><button aria-label="Đăng xuất" onClick={() => { logout(); navigate('/'); }} className="rounded-lg p-2 text-slate-500 hover:bg-red-50 hover:text-red-700"><LogOut className="h-4 w-4" /></button></> : <Link to="/login" className="rounded-xl bg-red-600 px-3 py-2 text-xs font-bold text-white hover:bg-red-700">Đăng nhập</Link>}<Link to="/book-table" className="hidden rounded-xl bg-[#a16207] px-3 py-2 text-xs font-bold text-white hover:bg-amber-800 sm:inline-flex"><CalendarDays className="mr-1 h-4 w-4" aria-hidden="true" />Đặt ngay</Link></div>
    </div></header><main><Outlet /></main><footer className="mt-16 bg-[#450a0a] px-6 py-10 text-red-100"><div className="mx-auto grid max-w-7xl gap-4 sm:grid-cols-2"><div><p className="font-serif text-xl text-white">Gourmet Palace</p><p className="mt-2 text-sm">Ẩm thực tinh tế, trải nghiệm trọn vẹn.</p></div><div className="text-sm"><p className="font-bold text-white">Giờ phục vụ</p><p className="mt-2">Thứ 2 - Chủ nhật: 10:00 - 22:30</p></div></div></footer>
  </div>;
}
