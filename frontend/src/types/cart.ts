export interface CartItemTopping {
  toppingId: string
  toppingName: string
  price: number
}

export interface CartItem {
  id: string
  cartId: string
  pizzaVariantId: string
  pizzaVariantName: string
  pizzaName: string
  basePrice: number
  quantity: number
  specialInstructions: string | null
  toppings: CartItemTopping[]
  toppingsTotal: number
  itemPrice: number
  subTotal: number
}

export interface Cart {
  id: string
  userId: string
  items: CartItem[]
  subTotal: number
  total: number
  itemCount: number
  totalQuantity: number
}

export interface AddToCartDto {
  pizzaVariantId: string
  quantity: number
  specialInstructions?: string
  toppingIds: string[]
}
