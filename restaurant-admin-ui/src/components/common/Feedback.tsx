import { AlertCircle, CheckCircle2, LoaderCircle } from 'lucide-react';

export function LoadingState({ label = 'Đang tải dữ liệu...' }: { label?: string }) {
  return <div role="status" className="flex items-center justify-center gap-2 rounded-2xl border border-slate-200 bg-white p-8 text-sm text-slate-500"><LoaderCircle className="h-5 w-5 animate-spin text-amber-500" aria-hidden="true" />{label}</div>;
}

export function ErrorState({ message = 'Không thể tải dữ liệu.', onRetry }: { message?: string; onRetry?: () => void }) {
  return <div role="alert" className="flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-red-200 bg-red-50 p-4 text-sm text-red-800"><span className="flex items-center gap-2"><AlertCircle className="h-4 w-4" aria-hidden="true" />{message}</span>{onRetry && <button type="button" onClick={onRetry} className="rounded-lg bg-white px-3 py-1.5 text-xs font-bold text-red-700 ring-1 ring-red-200 hover:bg-red-100">Thử lại</button>}</div>;
}

export function SuccessNotice({ children }: { children: React.ReactNode }) {
  return <div role="status" className="flex items-center gap-2 rounded-xl border border-emerald-200 bg-emerald-50 p-3 text-sm text-emerald-800"><CheckCircle2 className="h-4 w-4" aria-hidden="true" />{children}</div>;
}
