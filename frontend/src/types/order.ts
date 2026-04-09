export type OrderStatus =
  | 'Pending'
  | 'Confirmed'
  | 'Preparing'
  | 'OutForDelivery'
  | 'Delivered'
  | 'Cancelled'

export interface OrderItemTopping {
  id: string
  toppingId: string
  toppingNameAtOrder: string
  toppingPriceAtOrder: number
}

export interface OrderItem {
  id: string
  orderId: string
  pizzaVariantId: string
  pizzaNameAtOrder: string
  pizzaSizeAtOrder: string
  pizzaBasePriceAtOrder: number
  quantity: number
  specialInstructions: string | null
  subtotalAtOrder: number
  toppings: OrderItemTopping[]
}

export interface Order {
  id: string
  userId: string
  totalPrice: number
  status: OrderStatus
  createdAt: string
  confirmedAt: string | null
  completedAt: string | null
  cancelledAt: string | null
  items: OrderItem[]
}
