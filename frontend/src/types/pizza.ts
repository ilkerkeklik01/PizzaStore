export type PizzaType =
  | 'Vegetarian'
  | 'MeatLovers'
  | 'Hawaiian'
  | 'Veggie'
  | 'Custom'
  | 'Supreme'
  | 'Margherita'

export type PizzaSize = 'Small' | 'Medium' | 'Large' | 'ExtraLarge'

export interface PizzaVariant {
  id: string
  size: PizzaSize
  price: number
  isAvailable: boolean
}

export interface Pizza {
  id: string
  name: string
  description: string
  type: PizzaType
  imageUrl: string | null
  isAvailable: boolean
  variants: PizzaVariant[]
}
