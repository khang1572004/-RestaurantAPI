import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { menuService } from '../../services/menuService';
import { MenuItem, Category } from '../../types/api';
import {
  Plus,
  Edit2,
  Trash2,
  Search,
  CheckCircle,
  XCircle,
  Sparkles,
  BookOpen,
  X,
  Layers
} from 'lucide-react';

export const MenuManagementPage: React.FC = () => {
  const queryClient = useQueryClient();

  const [activeTab, setActiveTab] = useState<'items' | 'categories'>('items');
  const [selectedCategory, setSelectedCategory] = useState<number | null>(null);
  const [searchQuery, setSearchQuery] = useState('');

  // Item Modal
  const [itemModal, setItemModal] = useState<{ open: boolean; item: Partial<MenuItem> | null }>({
    open: false,
    item: null,
  });
  const [itemForm, setItemForm] = useState({
    name: '',
    categoryId: 0,
    price: 0,
    description: '',
    imageUrl: '',
    isAvailable: true,
    isChefSpecial: false,
  });

  // Category Modal
  const [categoryModal, setCategoryModal] = useState<{ open: boolean; category: Partial<Category> | null }>({
    open: false,
    category: null,
  });
  const [categoryForm, setCategoryForm] = useState({
    name: '',
    description: '',
    displayOrder: 1,
  });

  // Fetch Categories
  const { data: categories = [] } = useQuery<Category[]>({
    queryKey: ['categories'],
    queryFn: menuService.getCategories,
  });

  // Fetch Menu Items
  const { data: menuItems = [], isLoading } = useQuery<MenuItem[]>({
    queryKey: ['adminMenuItems', selectedCategory],
    queryFn: () => menuService.getMenuItems(selectedCategory || undefined),
  });

  // Mutations for Menu Items
  const saveItemMutation = useMutation({
    mutationFn: async () => {
      if (itemModal.item?.id) {
        return await menuService.updateMenuItem(itemModal.item.id, itemForm);
      }
      return await menuService.createMenuItem(itemForm);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adminMenuItems'] });
      queryClient.invalidateQueries({ queryKey: ['menuItems'] });
      setItemModal({ open: false, item: null });
      alert('Lưu món ăn thành công!');
    },
    onError: (err: any) => {
      alert(`Lỗi lưu món ăn: ${err.message}`);
    },
  });

  const toggleAvailabilityMutation = useMutation({
    mutationFn: ({ id, isAvailable }: { id: number; isAvailable: boolean }) =>
      menuService.toggleAvailability(id, isAvailable),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adminMenuItems'] });
      queryClient.invalidateQueries({ queryKey: ['menuItems'] });
    },
  });

  const deleteItemMutation = useMutation({
    mutationFn: (id: number) => menuService.deleteMenuItem(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adminMenuItems'] });
      queryClient.invalidateQueries({ queryKey: ['menuItems'] });
      alert('Đã xóa món ăn');
    },
  });

  // Mutations for Categories
  const saveCategoryMutation = useMutation({
    mutationFn: async () => {
      if (categoryModal.category?.id) {
        return await menuService.updateCategory(categoryModal.category.id, categoryForm);
      }
      return await menuService.createCategory(categoryForm);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      setCategoryModal({ open: false, category: null });
      alert('Lưu danh mục thành công!');
    },
  });

  const deleteCategoryMutation = useMutation({
    mutationFn: (id: number) => menuService.deleteCategory(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      alert('Đã xóa danh mục');
    },
  });

  const openCreateItem = () => {
    setItemForm({
      name: '',
      categoryId: categories[0]?.id || 0,
      price: 50000,
      description: '',
      imageUrl: '',
      isAvailable: true,
      isChefSpecial: false,
    });
    setItemModal({ open: true, item: null });
  };

  const openEditItem = (item: MenuItem) => {
    setItemForm({
      name: item.name,
      categoryId: item.categoryId,
      price: item.price,
      description: item.description || '',
      imageUrl: item.imageUrl || '',
      isAvailable: item.isAvailable,
      isChefSpecial: item.isChefSpecial || false,
    });
    setItemModal({ open: true, item });
  };

  const openCreateCategory = () => {
    setCategoryForm({ name: '', description: '', displayOrder: categories.length + 1 });
    setCategoryModal({ open: true, category: null });
  };

  const formatVND = (val: number = 0) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const filteredItems = menuItems.filter((i) =>
    i.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 tracking-tight">
            Quản Trị Thực Đơn & Danh Mục
          </h1>
          <p className="text-slate-500 text-sm mt-0.5">
            Quản lý công thức món, giá bán và tình trạng sẵn sàng phục vụ
          </p>
        </div>

        {/* Tab switch */}
        <div className="flex items-center gap-2">
          <div className="flex items-center bg-white p-1 rounded-xl border border-slate-200 shadow-xs">
            <button
              onClick={() => setActiveTab('items')}
              className={`px-4 py-2 rounded-lg text-xs font-bold transition-all cursor-pointer ${
                activeTab === 'items'
                  ? 'bg-amber-500 text-slate-950 shadow-xs'
                  : 'text-slate-600 hover:text-slate-900'
              }`}
            >
              Món Ăn ({menuItems.length})
            </button>
            <button
              onClick={() => setActiveTab('categories')}
              className={`px-4 py-2 rounded-lg text-xs font-bold transition-all cursor-pointer ${
                activeTab === 'categories'
                  ? 'bg-amber-500 text-slate-950 shadow-xs'
                  : 'text-slate-600 hover:text-slate-900'
              }`}
            >
              Danh Mục ({categories.length})
            </button>
          </div>

          <button
            onClick={activeTab === 'items' ? openCreateItem : openCreateCategory}
            className="inline-flex items-center gap-2 px-4 py-2 bg-slate-900 hover:bg-amber-600 text-white rounded-xl text-xs font-bold shadow-xs cursor-pointer transition-colors"
          >
            <Plus className="w-4 h-4 text-amber-400" />
            <span>{activeTab === 'items' ? 'Thêm Món Mới' : 'Thêm Danh Mục'}</span>
          </button>
        </div>
      </div>

      {activeTab === 'items' ? (
        <>
          {/* Filter Bar */}
          <div className="bg-white rounded-2xl p-4 border border-slate-200/80 shadow-xs flex flex-col md:flex-row items-stretch md:items-center justify-between gap-3">
            {/* Category selection */}
            <div className="flex items-center gap-1.5 overflow-x-auto">
              <button
                onClick={() => setSelectedCategory(null)}
                className={`px-3 py-1.5 rounded-xl text-xs font-bold transition-colors cursor-pointer whitespace-nowrap ${
                  selectedCategory === null
                    ? 'bg-slate-900 text-white'
                    : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                }`}
              >
                Tất cả ({menuItems.length})
              </button>
              {categories.map((c) => (
                <button
                  key={c.id}
                  onClick={() => setSelectedCategory(c.id)}
                  className={`px-3 py-1.5 rounded-xl text-xs font-bold transition-colors cursor-pointer whitespace-nowrap ${
                    selectedCategory === c.id
                      ? 'bg-amber-500 text-white'
                      : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                  }`}
                >
                  {c.name}
                </button>
              ))}
            </div>

            {/* Search */}
            <div className="relative w-full md:w-64">
              <Search className="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Tìm món ăn..."
                className="w-full pl-9 pr-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500 focus:bg-white"
              />
            </div>
          </div>

          {/* Dishes Table */}
          <div className="bg-white rounded-3xl border border-slate-200/80 shadow-xs overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="border-b border-slate-200 bg-slate-50 text-[11px] font-bold uppercase tracking-wider text-slate-500">
                    <th className="py-3.5 px-4">Hình Ảnh & Tên Món</th>
                    <th className="py-3.5 px-4">Danh Mục</th>
                    <th className="py-3.5 px-4">Đơn Giá</th>
                    <th className="py-3.5 px-4">Trạng Thái Bán</th>
                    <th className="py-3.5 px-4 text-right">Thao Tác</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 text-xs">
                  {filteredItems.map((dish) => (
                    <tr key={dish.id} className="hover:bg-slate-50 transition-colors">
                      <td className="py-3.5 px-4">
                        <div className="flex items-center gap-3">
                          <div className="w-12 h-12 rounded-xl bg-slate-100 overflow-hidden flex-shrink-0 flex items-center justify-center text-slate-400 border border-slate-200">
                            {dish.imageUrl ? (
                              <img src={dish.imageUrl} alt={dish.name} className="w-full h-full object-cover" />
                            ) : (
                              <BookOpen className="w-5 h-5 text-amber-500/60" />
                            )}
                          </div>
                          <div>
                            <div className="font-bold text-slate-900 flex items-center gap-1.5">
                              <span>{dish.name}</span>
                              {dish.isChefSpecial && (
                                <span className="px-1.5 py-0.5 rounded text-[10px] font-bold bg-amber-100 text-amber-800">
                                  Bếp Trưởng
                                </span>
                              )}
                            </div>
                            <p className="text-[11px] text-slate-400 line-clamp-1 max-w-sm">
                              {dish.description || 'Không có mô tả chi tiết'}
                            </p>
                          </div>
                        </div>
                      </td>

                      <td className="py-3.5 px-4">
                        <span className="font-semibold text-slate-700 bg-slate-100 px-2.5 py-1 rounded-md">
                          {dish.categoryName || 'Món ăn'}
                        </span>
                      </td>

                      <td className="py-3.5 px-4 font-bold text-amber-600 text-sm">
                        {formatVND(dish.price)}
                      </td>

                      <td className="py-3.5 px-4">
                        <button
                          onClick={() =>
                            toggleAvailabilityMutation.mutate({
                              id: dish.id,
                              isAvailable: !dish.isAvailable,
                            })
                          }
                          className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold cursor-pointer transition-colors ${
                            dish.isAvailable
                              ? 'bg-emerald-100 text-emerald-800 hover:bg-emerald-200'
                              : 'bg-rose-100 text-rose-800 hover:bg-rose-200'
                          }`}
                        >
                          {dish.isAvailable ? (
                            <>
                              <CheckCircle className="w-3.5 h-3.5" />
                              Còn Hàng
                            </>
                          ) : (
                            <>
                              <XCircle className="w-3.5 h-3.5" />
                              Hết Hàng
                            </>
                          )}
                        </button>
                      </td>

                      <td className="py-3.5 px-4 text-right">
                        <div className="flex items-center justify-end gap-1">
                          <button
                            onClick={() => openEditItem(dish)}
                            className="p-1.5 text-slate-500 hover:text-amber-600 hover:bg-amber-50 rounded-lg cursor-pointer"
                            title="Chỉnh sửa món"
                          >
                            <Edit2 className="w-4 h-4" />
                          </button>
                          <button
                            onClick={() => {
                              if (confirm(`Bạn có chắc chắn muốn xóa món "${dish.name}"?`)) {
                                deleteItemMutation.mutate(dish.id);
                              }
                            }}
                            className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg cursor-pointer"
                            title="Xóa món"
                          >
                            <Trash2 className="w-4 h-4" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      ) : (
        /* Categories List View */
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {categories.map((category) => (
            <div
              key={category.id}
              className="bg-white rounded-2xl p-5 border border-slate-200/80 shadow-xs flex items-center justify-between"
            >
              <div>
                <h4 className="font-bold text-slate-900 text-sm flex items-center gap-2">
                  <Layers className="w-4 h-4 text-amber-500" />
                  {category.name}
                </h4>
                <p className="text-xs text-slate-400 mt-1">
                  {category.description || 'Chưa có mô tả'}
                </p>
                <span className="inline-block mt-2 text-[11px] font-semibold text-slate-500 bg-slate-100 px-2 py-0.5 rounded">
                  Thứ tự hiển thị: #{category.displayOrder}
                </span>
              </div>

              <button
                onClick={() => {
                  if (confirm(`Xóa danh mục "${category.name}"?`)) {
                    deleteCategoryMutation.mutate(category.id);
                  }
                }}
                className="p-2 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg cursor-pointer"
                title="Xóa danh mục"
              >
                <Trash2 className="w-4 h-4" />
              </button>
            </div>
          ))}
        </div>
      )}

      {/* Item Modal */}
      {itemModal.open && (
        <div className="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl max-w-lg w-full p-6 shadow-2xl border border-slate-200">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <h3 className="text-base font-bold text-slate-900">
                {itemModal.item?.id ? 'Chỉnh Sửa Món Ăn' : 'Thêm Món Ăn Mới'}
              </h3>
              <button
                onClick={() => setItemModal({ open: false, item: null })}
                className="p-1 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 cursor-pointer"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="my-4 space-y-3.5 text-xs">
              <div>
                <label className="block font-semibold text-slate-700 mb-1">Tên Món Ăn:</label>
                <input
                  type="text"
                  required
                  value={itemForm.name}
                  onChange={(e) => setItemForm({ ...itemForm, name: e.target.value })}
                  placeholder="Ví dụ: Bò Bít Tết Sốt Tiêu Đen..."
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Danh Mục:</label>
                  <select
                    value={itemForm.categoryId}
                    onChange={(e) => setItemForm({ ...itemForm, categoryId: parseInt(e.target.value) })}
                    className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500 bg-white"
                  >
                    {categories.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Đơn Giá (VND):</label>
                  <input
                    type="number"
                    min="0"
                    step="5000"
                    value={itemForm.price}
                    onChange={(e) => setItemForm({ ...itemForm, price: parseFloat(e.target.value) })}
                    className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
                  />
                </div>
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Mô Tả Hương Vị:</label>
                <textarea
                  rows={2}
                  value={itemForm.description}
                  onChange={(e) => setItemForm({ ...itemForm, description: e.target.value })}
                  placeholder="Mô tả nguyên liệu, cách nấu..."
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500 resize-none"
                />
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Link Ảnh (URL):</label>
                <input
                  type="text"
                  value={itemForm.imageUrl}
                  onChange={(e) => setItemForm({ ...itemForm, imageUrl: e.target.value })}
                  placeholder="https://images.unsplash.com/..."
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
                />
              </div>

              <div className="flex items-center gap-4 pt-1">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={itemForm.isAvailable}
                    onChange={(e) => setItemForm({ ...itemForm, isAvailable: e.target.checked })}
                    className="w-4 h-4 rounded text-amber-500"
                  />
                  <span className="font-semibold text-slate-700">Đang phục vụ (Còn hàng)</span>
                </label>

                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={itemForm.isChefSpecial}
                    onChange={(e) => setItemForm({ ...itemForm, isChefSpecial: e.target.checked })}
                    className="w-4 h-4 rounded text-amber-500"
                  />
                  <span className="font-semibold text-slate-700">Món đặc biệt của Bếp trưởng</span>
                </label>
              </div>
            </div>

            <div className="flex items-center gap-3 pt-3 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setItemModal({ open: false, item: null })}
                className="flex-1 py-2.5 border border-slate-200 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-50 cursor-pointer"
              >
                Hủy
              </button>
              <button
                type="button"
                disabled={!itemForm.name.trim() || saveItemMutation.isPending}
                onClick={() => saveItemMutation.mutate()}
                className="flex-1 py-2.5 bg-amber-500 hover:bg-amber-600 text-slate-950 font-bold rounded-xl text-xs shadow-md shadow-amber-500/20 cursor-pointer disabled:opacity-50"
              >
                {saveItemMutation.isPending ? 'Đang lưu...' : 'Lưu Món Ăn'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Category Modal */}
      {categoryModal.open && (
        <div className="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl max-w-sm w-full p-6 shadow-2xl border border-slate-200">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <h3 className="text-base font-bold text-slate-900">Thêm Danh Mục Món</h3>
              <button
                onClick={() => setCategoryModal({ open: false, category: null })}
                className="p-1 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 cursor-pointer"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="my-4 space-y-3 text-xs">
              <div>
                <label className="block font-semibold text-slate-700 mb-1">Tên Danh Mục:</label>
                <input
                  type="text"
                  value={categoryForm.name}
                  onChange={(e) => setCategoryForm({ ...categoryForm, name: e.target.value })}
                  placeholder="Khai Vị, Món Chính, Rượu Vang..."
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
                />
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Thứ Tự Sắp Xếp:</label>
                <input
                  type="number"
                  value={categoryForm.displayOrder}
                  onChange={(e) => setCategoryForm({ ...categoryForm, displayOrder: parseInt(e.target.value) })}
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:border-amber-500"
                />
              </div>
            </div>

            <div className="flex items-center gap-3 pt-3 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setCategoryModal({ open: false, category: null })}
                className="flex-1 py-2.5 border border-slate-200 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-50 cursor-pointer"
              >
                Hủy
              </button>
              <button
                type="button"
                disabled={!categoryForm.name.trim() || saveCategoryMutation.isPending}
                onClick={() => saveCategoryMutation.mutate()}
                className="flex-1 py-2.5 bg-amber-500 hover:bg-amber-600 text-slate-950 font-bold rounded-xl text-xs cursor-pointer disabled:opacity-50"
              >
                {saveCategoryMutation.isPending ? 'Đang lưu...' : 'Lưu'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
