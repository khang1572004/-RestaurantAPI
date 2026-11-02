// API Response wrapper
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

// User & Auth
export interface User {
  id: number;
  email: string;
  fullName: string;
  phone?: string;
  role: 'Admin' | 'Staff' | 'Customer';
  isEmailVerified: boolean;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

// Table & Zone
export interface Zone {
  id: number;
  name: string;
  description?: string;
  tableCount?: number;
}

export interface TableType {
  id: number;
  name: string;
  capacity: number;
  depositAmount: number;
  description?: string;
}

export interface Table {
  id: number;
  tableNumber: string;
  zoneId: number;
  zoneName?: string;
  tableTypeId: number;
  tableTypeName?: string;
  capacity: number;
  status: 'Available' | 'Reserved' | 'Occupied' | 'NeedsCleaning' | 'Maintenance' | string;
  notes?: string;
}

export interface TableStatus {
  id: number;
  tableNumber: string;
  zoneName?: string;
  status: 'Available' | 'Reserved' | 'Occupied' | string;
  currentBookingCode?: string;
  customerName?: string;
  bookingTime?: string;
  numGuests?: number;
}

// Booking
export interface Booking {
  id: number;
  bookingCode: string;
  customerName: string;
  customerPhone: string;
  customerEmail?: string;
  bookingDate: string;
  bookingTime: string;
  numGuests: number;
  tableId?: number;
  tableNumber?: string;
  zoneId?: number;
  zoneName?: string;
  status: 'Pending' | 'PendingApproval' | 'AwaitingDeposit' | 'Confirmed' | 'CheckedIn' | 'Completed' | 'Cancelled' | 'NoShow' | string;
  specialRequests?: string;
  depositAmount?: number;
  isDepositPaid?: boolean;
  paymentStatus?: string;
  checkInTime?: string;
  checkOutTime?: string;
  notes?: string;
  createdAt: string;
}

export interface BookingFilter {
  date?: string;
  status?: string;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface PaginatedBookings {
  items: Booking[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Menu & Category
export interface Category {
  id: number;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  menuItemCount?: number;
}

export interface MenuItem {
  id: number;
  categoryId: number;
  categoryName?: string;
  name: string;
  description?: string;
  price: number;
  imageUrl?: string;
  isAvailable: boolean;
  isChefSpecial?: boolean;
  preparationTimeMinutes?: number;
}

// Order & POS
export interface OrderItem {
  id: number;
  menuItemId: number;
  menuItemName: string;
  menuItemImage?: string;
  quantity: number;
  price: number;
  subtotal: number;
  notes?: string;
  status?: 'Pending' | 'Preparing' | 'Served' | 'Cancelled' | string;
}

export interface Order {
  id: number;
  bookingId?: number;
  bookingCode?: string;
  tableId: number;
  tableNumber: string;
  staffId?: number;
  staffName?: string;
  subtotal: number;
  discountAmount: number;
  voucherId?: number;
  voucherCode?: string;
  depositAmount: number;
  finalAmount: number;
  paymentMethod?: 'Cash' | 'BankTransfer' | 'VNPay' | 'MoMo' | string;
  status: 'Pending' | 'Serving' | 'Paid' | 'Cancelled' | string;
  createdAt: string;
  paidAt?: string;
  items: OrderItem[];
}

// Staff & Shift
export interface StaffShift {
  id: number;
  userId: number;
  userName: string;
  shiftName: string;
  startTime: string;
  endTime: string;
  checkInTime?: string;
  checkOutTime?: string;
  status: 'Scheduled' | 'CheckedIn' | 'Completed' | 'Absent' | string;
}

// Voucher
export interface Voucher {
  id: number;
  code: string;
  description?: string;
  discountType: 'Percentage' | 'FixedAmount' | string;
  discountValue: number;
  minOrderAmount?: number;
  minOrderValue?: number;
  maxDiscountAmount?: number;
  startDate: string;
  endDate: string;
  usageLimit?: number;
  totalUsageLimit?: number;
  usedCount: number;
  isActive: boolean;
}

// Reports
export interface RevenueReport {
  totalRevenue: number;
  totalOrders: number;
  averageOrderValue: number;
  dailyRevenue: { date: string; revenue: number; orderCount: number }[];
  chartData?: { label: string; date: string; revenue: number; orderCount: number }[];
  paymentMethodBreakdown?: { method: string; total: number; count: number }[];
}

export interface BookingReport {
  totalBookings: number;
  confirmedCount: number;
  completedCount: number;
  cancelledCount: number;
  noShowCount: number;
  occupancyRatePercentage: number;
  completionRate?: number;
  cancellationRate?: number;
  noShowRate?: number;
}

export interface BestSellingItem {
  menuItemId: number;
  menuItemName: string;
  categoryName: string;
  totalQuantity: number;
  totalRevenue: number;
  itemName?: string;
  quantitySold?: number;
}

export interface Review { id: number; customerId: number; customerName: string; bookingId: number; bookingCode: string; rating: number; comment?: string; createdAt: string; }
