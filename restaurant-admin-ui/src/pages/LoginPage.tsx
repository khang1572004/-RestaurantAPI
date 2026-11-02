import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import { authService } from '../services/authService';
import { UtensilsCrossed, Lock, Mail, AlertCircle, ArrowRight, ShieldCheck } from 'lucide-react';

export const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const { login } = useAuthStore();

  const [email, setEmail] = useState('admin@restaurant.com');
  const [password, setPassword] = useState('Admin@123456');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const data = await authService.login(email, password);
      login(data.token, data.user, data.refreshToken);
      navigate(data.user.role === 'Customer' ? '/' : '/tables');
    } catch (err: any) {
      setError(err.message || 'Đăng nhập không thành công. Vui lòng kiểm tra lại tài khoản.');
    } finally {
      setLoading(false);
    }
  };

  const handleQuickFill = (userEmail: string, userPass: string) => {
    setEmail(userEmail);
    setPassword(userPass);
  };

  return (
    <div className="min-h-screen bg-slate-950 flex flex-col justify-center items-center p-4 relative overflow-hidden">
      {/* Background ambient glow */}
      <div className="absolute top-1/4 -left-32 w-96 h-96 bg-amber-500/10 rounded-full blur-3xl pointer-events-none"></div>
      <div className="absolute bottom-1/4 -right-32 w-96 h-96 bg-rose-500/10 rounded-full blur-3xl pointer-events-none"></div>

      <div className="w-full max-w-md z-10">
        {/* Header Branding */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-amber-500 text-white mb-4 shadow-lg shadow-amber-500/30">
            <UtensilsCrossed className="w-8 h-8" />
          </div>
          <h1 className="text-2xl sm:text-3xl font-bold text-white tracking-tight">
            Restaurant Master
          </h1>
          <p className="text-slate-400 text-sm mt-1.5">
            Cổng quản trị & Điểm bán POS dành cho Quản Lý & Nhân Viên
          </p>
        </div>

        {/* Card */}
        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-8 shadow-2xl backdrop-blur-xl">
          {error && (
            <div className="mb-6 p-4 rounded-xl bg-rose-500/10 border border-rose-500/30 flex items-start gap-3 text-rose-400 text-sm">
              <AlertCircle className="w-5 h-5 flex-shrink-0 mt-0.5" />
              <span>{error}</span>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-2">
                Email hoặc Tên đăng nhập
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
                  <Mail className="w-5 h-5" />
                </div>
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="admin@restaurant.com"
                  className="w-full pl-11 pr-4 py-3 bg-slate-950 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:border-amber-500 focus:ring-1 focus:ring-amber-500 text-sm transition-all"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-2">
                Mật khẩu
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
                  <Lock className="w-5 h-5" />
                </div>
                <input
                  type="password"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  className="w-full pl-11 pr-4 py-3 bg-slate-950 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:border-amber-500 focus:ring-1 focus:ring-amber-500 text-sm transition-all"
                />
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full py-3.5 px-4 bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700 text-slate-950 font-bold rounded-xl transition-all shadow-lg shadow-amber-500/25 flex items-center justify-center gap-2 cursor-pointer disabled:opacity-60 disabled:cursor-not-allowed text-sm"
            >
              {loading ? (
                <span>Đang xác thực...</span>
              ) : (
                <>
                  <span>Đăng Nhập Vào Hệ Thống</span>
                  <ArrowRight className="w-4 h-4" />
                </>
              )}
            </button>
          </form>

          {/* Quick Demo Credentials */}
          <div className="mt-8 pt-6 border-t border-slate-800/80">
            <p className="text-xs text-slate-400 font-medium mb-3 flex items-center gap-1.5">
              <ShieldCheck className="w-4 h-4 text-amber-500" />
              Điền nhanh tài khoản thử nghiệm:
            </p>
            <div className="grid grid-cols-2 gap-2">
              <button
                type="button"
                onClick={() => handleQuickFill('admin@restaurant.com', 'Admin@123456')}
                className="py-2 px-3 rounded-lg bg-slate-800/70 hover:bg-slate-800 text-slate-300 text-xs font-medium text-left border border-slate-700/50 hover:border-slate-600 transition-colors cursor-pointer"
              >
                <span className="font-bold text-amber-400 block">Tài khoản Admin</span>
                <span className="text-[11px] text-slate-400">admin@restaurant.com</span>
              </button>

              <button
                type="button"
                onClick={() => handleQuickFill('staff@restaurant.com', 'Staff@123456')}
                className="py-2 px-3 rounded-lg bg-slate-800/70 hover:bg-slate-800 text-slate-300 text-xs font-medium text-left border border-slate-700/50 hover:border-slate-600 transition-colors cursor-pointer"
              >
                <span className="font-bold text-blue-400 block">Tài khoản Staff</span>
                <span className="text-[11px] text-slate-400">staff@restaurant.com</span>
              </button>
            </div>
          </div>
        </div>

        <div className="text-center mt-6 text-xs text-slate-400">
          Hệ thống Restaurant Booking & Management Solution © 2026
        </div>
      </div>
    </div>
  );
};
