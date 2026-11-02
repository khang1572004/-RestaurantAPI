import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { shiftService } from '../services/shiftService';
import { useAuthStore } from '../store/authStore';
import { StaffShift } from '../types/api';
import { Clock, CheckCircle2, AlertCircle, Calendar, User } from 'lucide-react';

export const ShiftsPage: React.FC = () => {
  const { user } = useAuthStore();

  const { data: shifts = [], isLoading } = useQuery<StaffShift[]>({
    queryKey: ['myShifts'],
    queryFn: () => shiftService.getMyShifts(),
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900 tracking-tight">
          Lịch Làm Việc & Ca Trực
        </h1>
        <p className="text-slate-500 text-sm mt-0.5">
          Theo dõi ca làm việc phân bổ và lịch sử điểm danh của nhân viên
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {shifts.map((shift) => (
          <div
            key={shift.id}
            className="bg-white rounded-2xl p-5 border border-slate-200/80 shadow-xs flex flex-col justify-between"
          >
            <div>
              <div className="flex items-center justify-between">
                <span className="font-bold text-slate-900 text-sm flex items-center gap-2">
                  <Clock className="w-4 h-4 text-amber-500" />
                  {shift.shiftName}
                </span>
                <span className="px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-emerald-100 text-emerald-800">
                  {shift.status}
                </span>
              </div>

              <div className="my-3 space-y-1.5 text-xs text-slate-600">
                <div className="flex justify-between">
                  <span className="text-slate-400">Giờ bắt đầu:</span>
                  <span className="font-semibold text-slate-800">{shift.startTime}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-400">Giờ kết thúc:</span>
                  <span className="font-semibold text-slate-800">{shift.endTime}</span>
                </div>
                {shift.checkInTime && (
                  <div className="flex justify-between text-emerald-700">
                    <span>Đã điểm danh:</span>
                    <span>{new Date(shift.checkInTime).toLocaleTimeString()}</span>
                  </div>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
