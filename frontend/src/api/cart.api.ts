import { apiClient } from './client'
import type { Cart, CartItem, AddToCartDto, UpdateCartItemDto } from '@/types/cart'

export const getCart = (): Promise<Cart> =>
  apiClient.get<Cart>('/cart').then((r) => r.data)

export const addToCart = (dto: AddToCartDto): Promise<CartItem> =>
  apiClient.post<CartItem>('/cart/items', dto).then((r) => r.data)

export const removeFromCart = (cartItemId: string): Promise<void> =>
  apiClient.delete(`/cart/items/${cartItemId}`).then(() => undefined)

export const clearCart = (): Promise<void> =>
  apiClient.delete('/cart').then(() => undefined)

export const increaseQuantity = (cartItemId: string): Promise<CartItem> =>
  apiClient.patch<CartItem>(`/cart/items/${cartItemId}/increase`).then((r) => r.data)

export const decreaseQuantity = (cartItemId: string): Promise<CartItem> =>
  apiClient.patch<CartItem>(`/cart/items/${cartItemId}/decrease`).then((r) => r.data)

export const updateCartItem = (cartItemId: string, dto: UpdateCartItemDto): Promise<CartItem> =>
  apiClient.put<CartItem>(`/cart/items/${cartItemId}`, dto).then((r) => r.data)
