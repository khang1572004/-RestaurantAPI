import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { adminExtrasService } from '../../services/adminExtrasService';
import { ErrorState, LoadingState } from '../../components/common/Feedback';

const money = (value: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);

export function RefundsPage() {
  const queryClient = useQueryClient();
  const [note, setNote] = useState('');
  const { data = [], isLoading, isError } = useQuery({ queryKey: ['refunds'], queryFn: adminExtrasService.refunds });
  const action = useMutation({
    mutationFn: ({ id, action: refundAction }: { id: number; action: 'approve' | 'reject' }) => adminExtrasService.processRefund(id, refundAction, note),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['refunds'] }),
  });
  const execute = useMutation({ mutationFn: (id: number) => adminExtrasService.executeRefund(id), onSuccess: () => queryClient.invalidateQueries({ queryKey: ['refunds'] }) });

  return <div>
    <h1 className="text-2xl font-bold">Yêu cầu hoàn tiền cọc</h1>
    <p className="mt-1 text-sm text-slate-500">Duyệt và xử lý hoàn tiền theo trạng thái gateway.</p>
    {isError && <ErrorState message="Không thể tải danh sách hoàn tiền." />}
    <div className="mt-6 space-y-3">
      {data.map((refund) => <article key={refund.id} className="rounded-2xl border bg-white p-5">
        <div className="flex flex-wrap justify-between gap-3"><div><b>{refund.customerName}</b><p className="text-sm text-slate-500">{refund.bookingCode} · {refund.reason}</p></div><b className="text-amber-700">{money(refund.refundAmount)}</b></div>
        <div className="mt-3 flex flex-wrap items-center gap-2"><span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-bold">{refund.status}</span>
          {refund.status === 'Pending' && <><input aria-label="Ghi chú hoàn tiền" value={note} onChange={(event) => setNote(event.target.value)} placeholder="Ghi chú quản trị" className="rounded-lg border px-3 py-2 text-sm" /><button disabled={action.isPending} onClick={() => action.mutate({ id: refund.id, action: 'approve' })} className="rounded-lg bg-teal-600 px-3 py-2 text-sm font-bold text-white disabled:opacity-50">Duyệt</button><button disabled={action.isPending} onClick={() => action.mutate({ id: refund.id, action: 'reject' })} className="rounded-lg border border-red-200 px-3 py-2 text-sm font-bold text-red-700 disabled:opacity-50">Từ chối</button></>}
          {refund.status === 'Approved' && <button disabled={execute.isPending} onClick={() => execute.mutate(refund.id)} className="rounded-lg bg-amber-500 px-3 py-2 text-sm font-bold text-slate-950 disabled:opacity-50">Xử lý gateway</button>}
        </div>
      </article>)}
      {isLoading && <LoadingState label="Đang tải yêu cầu hoàn tiền..." />}
      {!isLoading && !data.length && <p className="rounded-xl bg-white p-5 text-slate-500">Không có yêu cầu hoàn tiền.</p>}
    </div>
  </div>;
}
