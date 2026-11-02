import React, { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { menuService } from '../services/menuService';
import { orderService } from '../services/orderService';
import { tableService } from '../services/tableService';
import { MenuItem, Category, Order } from '../types/api';
import {
  Search,
  Plus,
  Minus,
  Trash2,
  Send,
  CreditCard,
  QrCode,
  Banknote,
  Sparkles,
  Ticket,
  CheckCircle2,
  X,
  AlertCircle,
  ReceiptText,
  ShoppingBag
} from 'lucide-react';
import { PAYMENT_METHODS } from '../constants/business';

export const POSPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [searchParams] = useSearchParams();

  // Selected table ID from URL query or state
  const initialTableId = searchParams.get('tableId')
    ? parseInt(searchParams.get('tableId')!)
    : null;
  const [selectedTableId, setSelectedTableId] = useState<number | null>(initialTableId);
  const [activeOrderId, setActiveOrderId] = useState<number | null>(
    searchParams.get('orderId') ? parseInt(searchParams.get('orderId')!) : null
  );
  const [orderSaved, setOrderSaved] = useState(false);

  const [selectedCategory, setSelectedCategory] = useState<number | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [voucherCode, setVoucherCode] = useState('');
  const [voucherMessage, setVoucherMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  // Payment Modal
  const [paymentModalOpen, setPaymentModalOpen] = useState(false);
  const [selectedPaymentMethod, setSelectedPaymentMethod] = useState<number>(PAYMENT_METHODS.CASH);
  const [paymentSuccessModal, setPaymentSuccessModal] = useState(false);

  // Fetch Tables
  const { data: tables = [] } = useQuery({
    queryKey: ['tableStatus'],
    queryFn: tableService.getTableStatus,
  });

  // Fetch Categories
  const { data: categories = [] } = useQuery<Category[]>({
    queryKey: ['categories'],
    queryFn: menuService.getCategories,
  });

  // Fetch Menu Items
  const { data: menuItems = [] } = useQuery<MenuItem[]>({
    queryKey: ['menuItems', selectedCategory],
    queryFn: () => menuService.getMenuItems(selectedCategory || undefined),
  });

  // Fetch Active Order if activeOrderId is known
  const { data: currentOrder, refetch: refetchOrder } = useQuery<Order | null>({
    queryKey: ['order', activeOrderId],
    queryFn: async () => {
      if (!activeOrderId) return null;
      return await orderService.getOrderById(activeOrderId);
    },
    enabled: !!activeOrderId,
  });

  // Re-open the pending invoice when staff switches back to a table.
  useEffect(() => {
    let cancelled = false;
    if (!selectedTableId) return;
    orderService.getHistory({ tableId: selectedTableId }).then((orders) => {
      if (cancelled) return;
      const openOrder = orders.find((order) => order.status === 'Pending');
      setActiveOrderId(openOrder?.id ?? null);
      setOrderSaved(Boolean(openOrder));
    }).catch(() => {
      if (!cancelled) {
        setActiveOrderId(null);
        setOrderSaved(false);
      }
    });
    return () => { cancelled = true; };
  }, [selectedTableId]);

  // Ensure table selection initializes correctly
  useEffect(() => {
    if (!selectedTableId && tables.length > 0) {
      // Pick first occupied or first available table
      const occupied = tables.find((t) => t.status === 'Occupied');
      setSelectedTableId(occupied?.id ?? null);
    }
  }, [tables, selectedTableId]);

  // Local Cart for drafting items before sending to backend or immediate API dispatch
  const addItemMutation = useMutation({
    mutationFn: async ({ orderId, menuItemId }: { orderId: number; menuItemId: number }) => {
      return await orderService.addOrderItem(orderId, { menuItemId, quantity: 1 });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['order', activeOrderId] });
      await refetchOrder();
    },
  });

  const updateItemMutation = useMutation({
    mutationFn: async ({ orderId, itemId, quantity }: { orderId: number; itemId: number; quantity: number }) => {
      return await orderService.updateOrderItem(orderId, itemId, quantity);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['order', activeOrderId] });
      await refetchOrder();
    },
  });

  const removeItemMutation = useMutation({
    mutationFn: async ({ orderId, itemId }: { orderId: number; itemId: number }) => {
      return await orderService.removeOrderItem(orderId, itemId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['order', activeOrderId] });
    },
  });

  const applyVoucherMutation = useMutation({
    mutationFn: async ({ orderId, code }: { orderId: number; code: string }) => {
      return await orderService.applyVoucher(orderId, code);
    },
    onSuccess: (data) => {
      setVoucherMessage({ type: 'success', text: `Áp dụng mã giảm ${formatVND(data.discountAmount)} thành công!` });
      queryClient.invalidateQueries({ queryKey: ['order', activeOrderId] });
    },
    onError: (err: any) => {
      setVoucherMessage({ type: 'error', text: err.message || 'Mã giảm giá không hợp lệ' });
    },
  });

  const processPaymentMutation = useMutation({
    mutationFn: async ({ orderId, method }: { orderId: number; method: number }) => {
      return await orderService.processPayment(orderId, method);
    },
    onSuccess: () => {
      setPaymentModalOpen(false);
      setPaymentSuccessModal(true);
      if (activeOrderId) queryClient.removeQueries({ queryKey: ['order', activeOrderId] });
      setActiveOrderId(null);
      setOrderSaved(false);
      queryClient.invalidateQueries({ queryKey: ['tableStatus'] });
      queryClient.invalidateQueries({ queryKey: ['order', activeOrderId] });
      queryClient.invalidateQueries({ queryKey: ['todayBookings'] });
      queryClient.invalidateQueries({ queryKey: ['dateBookings'] });
    },
    onError: (err: any) => {
      alert(`Lỗi thanh toán: ${err.message}`);
    },
  });

  // Handler to add dish
  const handleAddDish = async (dish: MenuItem) => {
    if (!selectedTableId) {
      alert('Vui lòng chọn bàn cần phục vụ trước');
      return;
    }

    let orderId = activeOrderId;
    // If no active order exists yet, create one
    if (!orderId) {
      try {
        const newOrder = await orderService.createOrder({ tableId: selectedTableId });
        orderId = newOrder.id;
        setActiveOrderId(orderId);
        setOrderSaved(true);
      } catch (err: any) {
        alert(`Không thể tạo order cho bàn: ${err.message}`);
        return;
      }
    }

    // Check if item is already in order
    const existingItem = currentOrder?.items?.find((i) => i.menuItemId === dish.id);
    if (existingItem) {
      updateItemMutation.mutate({
        orderId,
        itemId: existingItem.id,
        quantity: existingItem.quantity + 1,
      });
    } else {
      addItemMutation.mutate({
        orderId,
        menuItemId: dish.id,
      });
    }
  };

  // Filter menu items by search query
  const filteredMenuItems = menuItems.filter((item) =>
    item.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const formatVND = (amount: number = 0) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
  };

  const selectedTable = tables.find((t) => t.id === selectedTableId);

  return (
    <div className="flex flex-col lg:flex-row gap-6 h-[calc(100vh-7.5rem)]">
      {/* Left Column: Menu Catalog (65% width) */}
      <div className="flex-1 flex flex-col bg-white rounded-3xl border border-slate-200/80 shadow-xs overflow-hidden">
        {/* Table Selector Header */}
        <div className="p-4 border-b border-slate-100 flex flex-wrap items-center justify-between gap-3 bg-slate-50/50">
          <div className="flex items-center gap-2">
            <span className="text-xs font-bold text-slate-500 uppercase">Đang phục vụ bàn:</span>
            <div className="flex items-center gap-1.5 overflow-x-auto max-w-xl">
              {tables.map((table) => {
                const isSelected = table.id === selectedTableId;
                const isOccupied = table.status === 'Occupied';
                return (
                  <button
                    key={table.id}
                    disabled={!isOccupied}
                    onClick={() => {
                      if (!isOccupied) return;
                      setSelectedTableId(table.id);
                      setActiveOrderId(null); // Will reset or load table order
                      setOrderSaved(false);
                    }}
                    className={`px-3 py-1.5 rounded-xl text-xs font-bold transition-all whitespace-nowrap flex items-center gap-1.5 ${isOccupied ? 'cursor-pointer' : 'cursor-not-allowed opacity-50'} ${
                      isSelected
                        ? 'bg-amber-500 text-slate-950 shadow-xs'
                        : isOccupied
                        ? 'bg-rose-100 text-rose-800 hover:bg-rose-200'
                        : 'bg-white border border-slate-200 text-slate-700 hover:bg-slate-100'
                    }`}
                  >
                    <span>{table.tableNumber}</span>
                    {isOccupied && <span className="w-1.5 h-1.5 rounded-full bg-rose-600"></span>}
                  </button>
                );
              })}
            </div>
          </div>

          {/* Search Input */}
          <div className="relative w-full sm:w-60">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Tìm món ăn, đồ uống..."
              className="w-full pl-9 pr-3 py-1.5 bg-white border border-slate-200 rounded-xl text-xs focus:outline-none focus:border-amber-500 transition-all"
            />
          </div>
        </div>

        {/* Category Pills */}
        <div className="px-4 py-2.5 border-b border-slate-100 flex items-center gap-2 overflow-x-auto">
          <button
            onClick={() => setSelectedCategory(null)}
            className={`px-3.5 py-1.5 rounded-xl text-xs font-bold transition-colors cursor-pointer whitespace-nowrap ${
              selectedCategory === null
                ? 'bg-slate-900 text-white'
                : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            Tất Cả Danh Mục
          </button>
          {categories.map((cat) => (
            <button
              key={cat.id}
              onClick={() => setSelectedCategory(cat.id)}
              className={`px-3.5 py-1.5 rounded-xl text-xs font-bold transition-colors cursor-pointer whitespace-nowrap ${
                selectedCategory === cat.id
                  ? 'bg-amber-500 text-white'
                  : 'text-slate-600 hover:bg-slate-100'
              }`}
            >
              {cat.name}
            </button>
          ))}
        </div>

        {/* Dish Grid */}
        <div className="flex-1 p-4 overflow-y-auto">
          <div className="grid grid-cols-2 sm:grid-cols-3 xl:grid-cols-4 gap-3.5">
            {filteredMenuItems.map((dish) => (
              <div
                key={dish.id}
                onClick={() => handleAddDish(dish)}
                className="group bg-white border border-slate-200 hover:border-amber-400 rounded-2xl p-3 shadow-2xs hover:shadow-md transition-all cursor-pointer flex flex-col justify-between"
              >
                <div>
                  {/* Dish Thumbnail */}
                  <div className="w-full h-28 bg-slate-100 rounded-xl overflow-hidden mb-2.5 relative flex items-center justify-center">
                    {dish.imageUrl ? (
                      <img
                        src={dish.imageUrl}
                        alt={dish.name}
                        className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                      />
                    ) : (
                      <div className="text-amber-500/80 flex flex-col items-center gap-1">
                        <Sparkles className="w-6 h-6" />
                        <span className="text-[10px] font-medium text-slate-400">Ẩm thực cao cấp</span>
                      </div>
                    )}
                    {dish.isChefSpecial && (
                      <span className="absolute top-2 left-2 px-2 py-0.5 rounded-md text-[10px] font-bold bg-amber-500 text-slate-950">
                        Đặc biệt
                      </span>
                    )}
                  </div>

                  <h4 className="font-bold text-slate-900 text-xs line-clamp-1 group-hover:text-amber-600 transition-colors">
                    {dish.name}
                  </h4>
                  <p className="text-[11px] text-slate-400 line-clamp-1 mt-0.5">
                    {dish.description || 'Món ăn chế biến theo công thức nhà hàng'}
                  </p>
                </div>

                <div className="flex items-center justify-between mt-3 pt-2 border-t border-slate-100">
                  <span className="font-extrabold text-amber-600 text-xs">
                    {formatVND(dish.price)}
                  </span>
                  <div className="w-7 h-7 rounded-lg bg-slate-900 group-hover:bg-amber-500 text-white flex items-center justify-center transition-colors">
                    <Plus className="w-3.5 h-3.5" />
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Right Column: Active Order Ticket (35% width) */}
      <div className="w-full lg:w-96 bg-white rounded-3xl border border-slate-200/80 shadow-xs flex flex-col justify-between overflow-hidden">
        {/* Ticket Header */}
        <div className="p-4 bg-slate-900 text-white">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <ReceiptText className="w-5 h-5 text-amber-400" />
              <h3 className="font-bold text-base">Hóa Đơn Phục Vụ</h3>
            </div>
            <span className="px-2.5 py-1 rounded-full bg-amber-500/20 text-amber-400 border border-amber-500/40 font-bold text-xs">
              {selectedTable ? selectedTable.tableNumber : 'Chưa chọn bàn'}
            </span>
          </div>
          <div className="text-xs text-slate-400 mt-1 flex justify-between">
            <span>Khu vực: {selectedTable?.zoneName || 'Trong nhà'}</span>
            <span>Mã đơn: #{currentOrder?.id || 'Mới'}</span>
          </div>
        </div>

        {/* Order Items List */}
        <div className="flex-1 p-4 overflow-y-auto space-y-2.5 divide-y divide-slate-100">
          {!currentOrder || currentOrder.items.length === 0 ? (
            <div className="py-16 text-center text-slate-400">
              <ShoppingBag className="w-10 h-10 mx-auto text-slate-300 mb-2" />
              <p className="text-xs font-semibold">Chưa có món nào được chọn</p>
              <p className="text-[11px] text-slate-400 mt-0.5">
                Nhấp vào món ăn bên trái để thêm vào hóa đơn bàn này
              </p>
            </div>
          ) : (
            currentOrder.items.map((item) => (
              <div key={item.id} className="pt-2.5 first:pt-0 flex items-center justify-between gap-2">
                <div className="flex-1 min-w-0">
                  <h5 className="text-xs font-bold text-slate-900 truncate">
                    {item.menuItemName}
                  </h5>
                  <p className="text-[11px] text-slate-500">
                    {formatVND(item.price)} x {item.quantity}
                  </p>
                </div>

                {/* Quantity Stepper */}
                <div className="flex items-center gap-1.5">
                  <button
                    onClick={() => {
                      if (item.quantity > 1) {
                        updateItemMutation.mutate({
                          orderId: currentOrder.id,
                          itemId: item.id,
                          quantity: item.quantity - 1,
                        });
                      } else {
                        removeItemMutation.mutate({
                          orderId: currentOrder.id,
                          itemId: item.id,
                        });
                      }
                    }}
                    className="w-6 h-6 rounded-md bg-slate-100 hover:bg-slate-200 text-slate-700 flex items-center justify-center cursor-pointer"
                  >
                    <Minus className="w-3 h-3" />
                  </button>

                  <span className="w-6 text-center text-xs font-bold text-slate-900">
                    {item.quantity}
                  </span>

                  <button
                    onClick={() => {
                      updateItemMutation.mutate({
                        orderId: currentOrder.id,
                        itemId: item.id,
                        quantity: item.quantity + 1,
                      });
                    }}
                    className="w-6 h-6 rounded-md bg-slate-100 hover:bg-slate-200 text-slate-700 flex items-center justify-center cursor-pointer"
                  >
                    <Plus className="w-3 h-3" />
                  </button>
                </div>

                <div className="text-right min-w-[70px]">
                  <span className="text-xs font-extrabold text-slate-900">
                    {formatVND(item.subtotal)}
                  </span>
                </div>

                <button
                  onClick={() => {
                    removeItemMutation.mutate({
                      orderId: currentOrder.id,
                      itemId: item.id,
                    });
                  }}
                  className="text-slate-400 hover:text-rose-600 p-1 cursor-pointer"
                  title="Xóa món"
                >
                  <Trash2 className="w-3.5 h-3.5" />
                </button>
              </div>
            ))
          )}
        </div>

        {/* Voucher & Bill Calculation Footer */}
        <div className="p-4 bg-slate-50 border-t border-slate-200/80 space-y-3">
          {/* Voucher input */}
          <div className="flex items-center gap-2">
            <div className="relative flex-1">
              <Ticket className="w-3.5 h-3.5 text-slate-400 absolute left-2.5 top-2.5" />
              <input
                type="text"
                value={voucherCode}
                onChange={(e) => setVoucherCode(e.target.value.toUpperCase())}
                placeholder="Nhập mã voucher..."
                className="w-full pl-8 pr-2 py-1.5 bg-white border border-slate-200 rounded-xl text-xs focus:outline-none focus:border-amber-500 font-mono uppercase"
              />
            </div>
            <button
              onClick={() => {
                if (currentOrder?.id && voucherCode.trim()) {
                  applyVoucherMutation.mutate({
                    orderId: currentOrder.id,
                    code: voucherCode.trim(),
                  });
                }
              }}
              disabled={!currentOrder?.id || !voucherCode.trim() || applyVoucherMutation.isPending}
              className="px-3 py-1.5 bg-slate-900 hover:bg-amber-600 text-white rounded-xl text-xs font-bold transition-colors cursor-pointer disabled:opacity-50"
            >
              Áp Dụng
            </button>
          </div>

          {voucherMessage && (
            <p
              className={`text-[11px] font-semibold ${
                voucherMessage.type === 'success' ? 'text-emerald-600' : 'text-rose-600'
              }`}
            >
              {voucherMessage.text}
            </p>
          )}

          {/* Price Breakdown */}
          <div className="text-xs space-y-1.5 pt-1 text-slate-600">
            <div className="flex justify-between">
              <span>Tạm tính tiền món:</span>
              <span className="font-semibold text-slate-900">
                {formatVND(currentOrder?.subtotal || 0)}
              </span>
            </div>

            {currentOrder?.discountAmount ? (
              <div className="flex justify-between text-emerald-600 font-semibold">
                <span>Giảm giá (Voucher):</span>
                <span>-{formatVND(currentOrder.discountAmount)}</span>
              </div>
            ) : null}

            {currentOrder?.depositAmount ? (
              <div className="flex justify-between text-amber-600 font-semibold">
                <span>Đã trừ tiền cọc bàn:</span>
                <span>-{formatVND(currentOrder.depositAmount)}</span>
              </div>
            ) : null}

            <div className="flex justify-between text-sm font-extrabold text-slate-900 pt-2 border-t border-slate-200">
              <span>TỔNG THANH TOÁN:</span>
              <span className="text-base text-amber-600">
                {formatVND(currentOrder?.finalAmount || currentOrder?.subtotal || 0)}
              </span>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="grid grid-cols-2 gap-2 pt-1">
            <button
              onClick={() => { refetchOrder(); setOrderSaved(true); }}
              disabled={!currentOrder || currentOrder.items.length === 0}
              className="py-2.5 px-3 bg-emerald-100 hover:bg-emerald-200 text-emerald-800 rounded-xl text-xs font-bold transition-colors flex items-center justify-center gap-1.5 cursor-pointer disabled:opacity-50"
            >
              <ReceiptText className="w-4 h-4" />
              <span>{orderSaved ? 'Đã lưu hóa đơn' : 'Lưu hóa đơn'}</span>
            </button>
            <button
              onClick={() => {
                refetchOrder();
                alert('Đã gửi thông báo chế biến món đến nhà bếp!');
              }}
              disabled={!currentOrder || currentOrder.items.length === 0}
              className="py-2.5 px-3 bg-slate-100 hover:bg-slate-200 text-slate-800 rounded-xl text-xs font-bold transition-colors flex items-center justify-center gap-1.5 cursor-pointer disabled:opacity-50"
            >
              <Send className="w-4 h-4 text-slate-600" />
              <span>Gửi Nhà Bếp</span>
            </button>

            <button
              onClick={() => setPaymentModalOpen(true)}
              disabled={!currentOrder || currentOrder.items.length === 0}
              className="py-2.5 px-3 bg-amber-500 hover:bg-amber-600 text-slate-950 rounded-xl text-xs font-bold transition-all shadow-md shadow-amber-500/20 flex items-center justify-center gap-1.5 cursor-pointer disabled:opacity-50"
            >
              <CreditCard className="w-4 h-4" />
              <span>Thanh Toán</span>
            </button>
          </div>
        </div>
      </div>

      {/* Payment Method Modal */}
      {paymentModalOpen && currentOrder && (
        <div className="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl max-w-md w-full p-6 shadow-2xl border border-slate-200">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <h3 className="text-lg font-bold text-slate-900">
                Chọn Phương Thức Thanh Toán
              </h3>
              <button
                onClick={() => setPaymentModalOpen(false)}
                className="p-1 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 cursor-pointer"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="my-5 p-4 rounded-2xl bg-amber-50 border border-amber-200/60 text-center">
              <p className="text-xs text-amber-800 font-medium">Số tiền cần thanh toán</p>
              <p className="text-2xl font-extrabold text-amber-700 mt-1">
                {formatVND(currentOrder.finalAmount || currentOrder.subtotal)}
              </p>
              <p className="text-xs text-slate-500 mt-1">
                Bàn: <strong>{currentOrder.tableNumber}</strong> | Mã đơn #{currentOrder.id}
              </p>
            </div>

            <div className="space-y-2.5">
              {[
                { id: PAYMENT_METHODS.CASH, name: 'Tiền Mặt (Cash)', icon: Banknote, desc: 'Thu tiền trực tiếp tại quầy' },
                { id: PAYMENT_METHODS.BANK_TRANSFER, name: 'Chuyển khoản', icon: CreditCard, desc: 'Xác nhận giao dịch chuyển khoản' },
                { id: PAYMENT_METHODS.VNPAY, name: 'Cổng VNPay QR', icon: QrCode, desc: 'Quét mã VietQR / Ứng dụng ngân hàng' },
                { id: PAYMENT_METHODS.MOMO, name: 'Ví Điện Tử MoMo', icon: QrCode, desc: 'Quét mã thanh toán MoMo' },
              ].map((method) => {
                const Icon = method.icon;
                const isSelected = selectedPaymentMethod === method.id;
                return (
                  <div
                    key={method.id}
                    onClick={() => setSelectedPaymentMethod(method.id)}
                    className={`p-3.5 rounded-xl border flex items-center justify-between cursor-pointer transition-all ${
                      isSelected
                        ? 'border-amber-500 bg-amber-500/10'
                        : 'border-slate-200 hover:bg-slate-50'
                    }`}
                  >
                    <div className="flex items-center gap-3">
                      <div className={`w-9 h-9 rounded-lg flex items-center justify-center ${isSelected ? 'bg-amber-500 text-slate-950' : 'bg-slate-100 text-slate-700'}`}>
                        <Icon className="w-5 h-5" />
                      </div>
                      <div>
                        <p className="text-xs font-bold text-slate-900">{method.name}</p>
                        <p className="text-[11px] text-slate-400">{method.desc}</p>
                      </div>
                    </div>
                    {isSelected && <CheckCircle2 className="w-5 h-5 text-amber-600" />}
                  </div>
                );
              })}
            </div>

            <div className="flex items-center gap-3 pt-6 mt-4 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setPaymentModalOpen(false)}
                className="flex-1 py-2.5 border border-slate-200 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-50 cursor-pointer"
              >
                Hủy
              </button>
              <button
                type="button"
                disabled={processPaymentMutation.isPending}
                onClick={() => {
                  processPaymentMutation.mutate({
                    orderId: currentOrder.id,
                    method: selectedPaymentMethod,
                  });
                }}
                className="flex-1 py-2.5 bg-amber-500 hover:bg-amber-600 text-slate-950 font-bold rounded-xl text-xs shadow-md shadow-amber-500/20 cursor-pointer disabled:opacity-50"
              >
                {processPaymentMutation.isPending ? 'Đang xử lý...' : 'Xác Nhận Đã Thu Tiền'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Payment Success Modal */}
      {paymentSuccessModal && (
        <div className="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl max-w-sm w-full p-6 text-center shadow-2xl border border-slate-200">
            <div className="w-14 h-14 rounded-full bg-emerald-100 text-emerald-600 flex items-center justify-center mx-auto mb-3">
              <CheckCircle2 className="w-8 h-8" />
            </div>
            <h3 className="text-lg font-bold text-slate-900">Thanh Toán Hoàn Tất!</h3>
            <p className="text-xs text-slate-500 mt-1">
              Đơn hàng đã được thanh toán thành công. Bàn đã được giải phóng và sẵn sàng phục vụ lượt khách mới.
            </p>

            <button
              onClick={() => {
                setPaymentSuccessModal(false);
                setActiveOrderId(null);
              }}
              className="mt-6 w-full py-2.5 bg-slate-900 hover:bg-amber-600 text-white rounded-xl text-xs font-bold transition-colors cursor-pointer"
            >
              Tiếp Tục Bán Hàng
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
