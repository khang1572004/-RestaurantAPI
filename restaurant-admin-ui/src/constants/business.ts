export const PAYMENT_METHODS = {
  CASH: 1,
  BANK_TRANSFER: 2,
  VNPAY: 3,
  MOMO: 4,
} as const;

export const PAYMENT_METHOD_OPTIONS = [
  { id: PAYMENT_METHODS.CASH, name: 'Tiền mặt' },
  { id: PAYMENT_METHODS.BANK_TRANSFER, name: 'Chuyển khoản' },
  { id: PAYMENT_METHODS.VNPAY, name: 'VNPay' },
  { id: PAYMENT_METHODS.MOMO, name: 'MoMo' },
];

export const BOOKING_STATUS_LABELS: Record<string, string> = {
  Pending: 'Chờ thanh toán cọc',
  PendingApproval: 'Chờ duyệt',
  AwaitingDeposit: 'Chờ thanh toán cọc',
  Confirmed: 'Đã xác nhận',
  CheckedIn: 'Đã check-in',
  Completed: 'Hoàn thành',
  Cancelled: 'Đã hủy',
  NoShow: 'Không đến',
};

export const PAYMENT_STATUS_LABELS: Record<string, string> = {
  Unpaid: 'Chưa thanh toán',
  Partial: 'Đã thanh toán cọc',
  Paid: 'Đã thanh toán đủ',
  Pending: 'Đang xử lý',
  Success: 'Thành công',
  Failed: 'Thất bại',
  Refunded: 'Đã hoàn tiền',
};

export function businessStatusLabel(value?: string) {
  return (value && (BOOKING_STATUS_LABELS[value] || PAYMENT_STATUS_LABELS[value])) || value || 'Chưa xác định';
}
