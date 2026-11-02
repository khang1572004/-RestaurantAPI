import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { tableService } from '../../services/tableService';
import { Table, Zone, TableType } from '../../types/api';
import {
  Layers,
  Plus,
  Trash2,
  Users,
  MapPin,
  X
} from 'lucide-react';

export const TablesManagementPage: React.FC = () => {
  const queryClient = useQueryClient();

  const [activeTab, setActiveTab] = useState<'tables' | 'zones' | 'types'>('tables');

  // Modal for new table
  const [tableModal, setTableModal] = useState(false);
  const [tableForm, setTableForm] = useState({
    tableNumber: '',
    zoneId: 0,
    tableTypeId: 0,
    notes: '',
  });

  // Modal for new zone
  const [zoneModal, setZoneModal] = useState(false);
  const [zoneForm, setZoneForm] = useState({ name: '', description: '' });

  // Queries
  const { data: tables = [] } = useQuery<Table[]>({
    queryKey: ['tables'],
    queryFn: tableService.getTables,
  });

  const { data: zones = [] } = useQuery<Zone[]>({
    queryKey: ['zones'],
    queryFn: tableService.getZones,
  });

  const { data: tableTypes = [] } = useQuery<TableType[]>({
    queryKey: ['tableTypes'],
    queryFn: tableService.getTableTypes,
  });

  // Mutations
  const createTableMutation = useMutation({
    mutationFn: tableService.createTable,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tables'] });
      queryClient.invalidateQueries({ queryKey: ['tableStatus'] });
      setTableModal(false);
      setTableForm({ tableNumber: '', zoneId: zones[0]?.id || 0, tableTypeId: tableTypes[0]?.id || 0, notes: '' });
      alert('Thêm bàn mới thành công!');
    },
  });

  const deleteTableMutation = useMutation({
    mutationFn: tableService.deleteTable,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tables'] });
      queryClient.invalidateQueries({ queryKey: ['tableStatus'] });
      alert('Đã xóa bàn!');
    },
  });

  const createZoneMutation = useMutation({
    mutationFn: tableService.createZone,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['zones'] });
      setZoneModal(false);
      setZoneForm({ name: '', description: '' });
      alert('Thêm khu vực mới thành công!');
    },
  });

  const openCreateTable = () => {
    setTableForm({
      tableNumber: `Bàn ${tables.length + 1}`,
      zoneId: zones[0]?.id || 0,
      tableTypeId: tableTypes[0]?.id || 0,
      notes: '',
    });
    setTableModal(true);
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 tracking-tight">
            Quản Trị Sơ Đồ Bàn & Khu Vực
          </h1>
          <p className="text-slate-500 text-sm mt-0.5">
            Cấu hình danh sách bàn ăn, khu vực phòng và sức chứa
          </p>
        </div>

        {/* Tab switch & Add buttons */}
        <div className="flex items-center gap-3">
          <div className="flex items-center bg-white p-1 rounded-xl border border-slate-200 shadow-xs">
            <button
              onClick={() => setActiveTab('tables')}
              className={`px-4 py-2 rounded-lg text-xs font-bold transition-all cursor-pointer ${
                activeTab === 'tables' ? 'bg-amber-500 text-slate-950' : 'text-slate-600'
              }`}
            >
              Bàn Ăn ({tables.length})
            </button>
            <button
              onClick={() => setActiveTab('zones')}
              className={`px-4 py-2 rounded-lg text-xs font-bold transition-all cursor-pointer ${
                activeTab === 'zones' ? 'bg-amber-500 text-slate-950' : 'text-slate-600'
              }`}
            >
              Khu Vực ({zones.length})
            </button>
            <button
              onClick={() => setActiveTab('types')}
              className={`px-4 py-2 rounded-lg text-xs font-bold transition-all cursor-pointer ${
                activeTab === 'types' ? 'bg-amber-500 text-slate-950' : 'text-slate-600'
              }`}
            >
              Loại Bàn ({tableTypes.length})
            </button>
          </div>

          <button
            onClick={activeTab === 'tables' ? openCreateTable : () => setZoneModal(true)}
            className="inline-flex items-center gap-2 px-4 py-2 bg-slate-900 hover:bg-amber-600 text-white rounded-xl text-xs font-bold shadow-xs cursor-pointer transition-colors"
          >
            <Plus className="w-4 h-4 text-amber-400" />
            <span>{activeTab === 'tables' ? 'Thêm Bàn' : 'Thêm Khu Vực'}</span>
          </button>
        </div>
      </div>

      {activeTab === 'tables' && (
        <div className="bg-white rounded-3xl border border-slate-200/80 shadow-xs overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="border-b border-slate-200 bg-slate-50 text-[11px] font-bold uppercase tracking-wider text-slate-500">
                  <th className="py-3.5 px-4">Số / Mã Bàn</th>
                  <th className="py-3.5 px-4">Khu Vực (Zone)</th>
                  <th className="py-3.5 px-4">Loại Bàn & Sức Chứa</th>
                  <th className="py-3.5 px-4">Trạng Thái</th>
                  <th className="py-3.5 px-4 text-right">Thao Tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 text-xs">
                {tables.map((table) => (
                  <tr key={table.id} className="hover:bg-slate-50 transition-colors">
                    <td className="py-3.5 px-4 font-bold text-slate-900 text-sm">
                      {table.tableNumber}
                    </td>
                    <td className="py-3.5 px-4">
                      <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md bg-slate-100 text-slate-700 font-semibold">
                        <MapPin className="w-3.5 h-3.5 text-amber-600" />
                        {table.zoneName || 'Chung'}
                      </span>
                    </td>
                    <td className="py-3.5 px-4">
                      <div className="font-semibold text-slate-800">{table.tableTypeName || 'Bàn tiêu chuẩn'}</div>
                      <div className="text-[11px] text-slate-400 flex items-center gap-1">
                        <Users className="w-3 h-3" />
                        <span>Sức chứa: {table.capacity} khách</span>
                      </div>
                    </td>
                    <td className="py-3.5 px-4">
                      <span className="font-semibold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded border border-emerald-200">
                        {table.status}
                      </span>
                    </td>
                    <td className="py-3.5 px-4 text-right">
                      <button
                        onClick={() => {
                          if (confirm(`Xóa bàn "${table.tableNumber}"?`)) {
                            deleteTableMutation.mutate(table.id);
                          }
                        }}
                        className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg cursor-pointer"
                        title="Xóa bàn"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'zones' && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {zones.map((zone) => (
            <div key={zone.id} className="bg-white rounded-2xl p-5 border border-slate-200/80 shadow-xs">
              <div className="flex items-center justify-between">
                <h4 className="font-bold text-slate-900 text-base flex items-center gap-2">
                  <MapPin className="w-5 h-5 text-amber-500" />
                  {zone.name}
                </h4>
                <span className="text-xs font-bold text-slate-500 bg-slate-100 px-2.5 py-1 rounded-full">
                  {zone.tableCount || 0} bàn
                </span>
              </div>
              <p className="text-xs text-slate-400 mt-2">
                {zone.description || 'Khu vực phục vụ khách tại nhà hàng'}
              </p>
            </div>
          ))}
        </div>
      )}

      {activeTab === 'types' && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {tableTypes.map((type) => (
            <div key={type.id} className="bg-white rounded-2xl p-5 border border-slate-200/80 shadow-xs">
              <div className="flex items-center justify-between">
                <h4 className="font-bold text-slate-900 text-sm flex items-center gap-2">
                  <Users className="w-4 h-4 text-amber-500" />
                  {type.name}
                </h4>
                <span className="text-xs font-bold text-amber-600 bg-amber-50 px-2 py-0.5 rounded border border-amber-200">
                  {type.capacity} chỗ ngồi
                </span>
              </div>
              <p className="text-xs text-slate-400 mt-2">
                {type.description || 'Loại bàn ăn'}
              </p>
            </div>
          ))}
        </div>
      )}

      {/* Add Table Modal */}
      {tableModal && (
        <div className="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl max-w-sm w-full p-6 shadow-2xl border border-slate-200">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <h3 className="text-base font-bold text-slate-900">Thêm Bàn Ăn Mới</h3>
              <button
                onClick={() => setTableModal(false)}
                className="p-1 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 cursor-pointer"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="my-4 space-y-3 text-xs">
              <div>
                <label className="block font-semibold text-slate-700 mb-1">Mã / Tên Bàn:</label>
                <input
                  type="text"
                  value={tableForm.tableNumber}
                  onChange={(e) => setTableForm({ ...tableForm, tableNumber: e.target.value })}
                  placeholder="Ví dụ: Bàn 01, VIP-01..."
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
                />
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Khu Vực (Zone):</label>
                <select
                  value={tableForm.zoneId}
                  onChange={(e) => setTableForm({ ...tableForm, zoneId: parseInt(e.target.value) })}
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500 bg-white"
                >
                  {zones.map((z) => (
                    <option key={z.id} value={z.id}>{z.name}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Loại Bàn:</label>
                <select
                  value={tableForm.tableTypeId}
                  onChange={(e) => setTableForm({ ...tableForm, tableTypeId: parseInt(e.target.value) })}
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500 bg-white"
                >
                  {tableTypes.map((tt) => (
                    <option key={tt.id} value={tt.id}>{tt.name} ({tt.capacity} người)</option>
                  ))}
                </select>
              </div>
            </div>

            <div className="flex items-center gap-3 pt-3 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setTableModal(false)}
                className="flex-1 py-2.5 border border-slate-200 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-50 cursor-pointer"
              >
                Hủy
              </button>
              <button
                type="button"
                disabled={!tableForm.tableNumber.trim() || createTableMutation.isPending}
                onClick={() => createTableMutation.mutate(tableForm)}
                className="flex-1 py-2.5 bg-amber-500 hover:bg-amber-600 text-slate-950 font-bold rounded-xl text-xs cursor-pointer disabled:opacity-50"
              >
                {createTableMutation.isPending ? 'Đang tạo...' : 'Tạo Bàn'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Add Zone Modal */}
      {zoneModal && (
        <div className="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl max-w-sm w-full p-6 shadow-2xl border border-slate-200">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <h3 className="text-base font-bold text-slate-900">Thêm Khu Vực Mới</h3>
              <button
                onClick={() => setZoneModal(false)}
                className="p-1 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 cursor-pointer"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="my-4 space-y-3 text-xs">
              <div>
                <label className="block font-semibold text-slate-700 mb-1">Tên Khu Vực:</label>
                <input
                  type="text"
                  value={zoneForm.name}
                  onChange={(e) => setZoneForm({ ...zoneForm, name: e.target.value })}
                  placeholder="Tầng 1, Sân Thượng, Phòng VIP 2..."
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
                />
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Mô Tả:</label>
                <textarea
                  rows={2}
                  value={zoneForm.description}
                  onChange={(e) => setZoneForm({ ...zoneForm, description: e.target.value })}
                  placeholder="Không gian thoáng mát, view hồ bơi..."
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500 resize-none"
                />
              </div>
            </div>

            <div className="flex items-center gap-3 pt-3 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setZoneModal(false)}
                className="flex-1 py-2.5 border border-slate-200 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-50 cursor-pointer"
              >
                Hủy
              </button>
              <button
                type="button"
                disabled={!zoneForm.name.trim() || createZoneMutation.isPending}
                onClick={() => createZoneMutation.mutate(zoneForm)}
                className="flex-1 py-2.5 bg-amber-500 hover:bg-amber-600 text-slate-950 font-bold rounded-xl text-xs cursor-pointer disabled:opacity-50"
              >
                {createZoneMutation.isPending ? 'Đang tạo...' : 'Tạo'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
