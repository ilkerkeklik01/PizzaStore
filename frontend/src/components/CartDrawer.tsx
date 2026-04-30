import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { X, Plus, Minus, Trash2, ShoppingBag, ArrowRight } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { getCart, removeFromCart, increaseQuantity, decreaseQuantity, clearCart } from '@/api/cart.api'

interface CartDrawerProps {
  isOpen: boolean
  onClose: () => void
}

export default function CartDrawer({ isOpen, onClose }: CartDrawerProps) {
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const { data: cart, isLoading } = useQuery({
    queryKey: ['cart'],
    queryFn: getCart,
    enabled: isAuthenticated,
    retry: false,
  })

  const { mutate: increase, isPending: isIncreasing } = useMutation({
    mutationFn: increaseQuantity,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  })

  const { mutate: decrease, isPending: isDecreasing } = useMutation({
    mutationFn: decreaseQuantity,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  })

  const { mutate: remove } = useMutation({
    mutationFn: removeFromCart,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  })

  const { mutate: clear, isPending: isClearing } = useMutation({
    mutationFn: clearCart,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  })

  const isMutating = isIncreasing || isDecreasing

  return (
    <>
      {/* Backdrop */}
      <div
        onClick={onClose}
        style={{
          position: 'fixed',
          inset: 0,
          background: 'rgba(0,0,0,0.55)',
          zIndex: 200,
          opacity: isOpen ? 1 : 0,
          pointerEvents: isOpen ? 'all' : 'none',
          transition: 'opacity 0.3s ease',
          backdropFilter: isOpen ? 'blur(2px)' : 'none',
        }}
      />

      {/* Drawer panel */}
      <div
        style={{
          position: 'fixed',
          top: 0,
          right: 0,
          bottom: 0,
          width: '420px',
          maxWidth: '92vw',
          background: '#1A1815',
          borderLeft: '1px solid rgba(245, 236, 215, 0.08)',
          zIndex: 201,
          transform: isOpen ? 'translateX(0)' : 'translateX(100%)',
          transition: 'transform 0.38s cubic-bezier(0.32, 0, 0.24, 1)',
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        {/* Header */}
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            padding: '20px 24px',
            borderBottom: '1px solid rgba(245, 236, 215, 0.08)',
            flexShrink: 0,
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <ShoppingBag size={17} color="#C44536" />
            <span
              style={{
                fontFamily: '"Bodoni Moda", Georgia, serif',
                fontStyle: 'italic',
                fontSize: '19px',
                color: '#F5ECD7',
              }}
            >
              Your Order
            </span>
            {cart && cart.totalQuantity > 0 && (
              <span
                style={{
                  background: '#C44536',
                  color: '#F5ECD7',
                  fontSize: '11px',
                  fontWeight: 700,
                  padding: '2px 8px',
                  borderRadius: '12px',
                  lineHeight: 1.4,
                }}
              >
                {cart.totalQuantity}
              </span>
            )}
          </div>
          <button
            onClick={onClose}
            style={{
              background: 'rgba(245, 236, 215, 0.06)',
              border: '1px solid rgba(245, 236, 215, 0.1)',
              borderRadius: '8px',
              cursor: 'pointer',
              color: '#8B7E72',
              padding: '6px',
              display: 'flex',
              alignItems: 'center',
              transition: 'color 0.2s, background 0.2s',
            }}
            onMouseEnter={(e) => {
              const el = e.currentTarget as HTMLElement
              el.style.color = '#F5ECD7'
              el.style.background = 'rgba(245, 236, 215, 0.1)'
            }}
            onMouseLeave={(e) => {
              const el = e.currentTarget as HTMLElement
              el.style.color = '#8B7E72'
              el.style.background = 'rgba(245, 236, 215, 0.06)'
            }}
          >
            <X size={16} />
          </button>
        </div>

        {/* Item list */}
        <div style={{ flex: 1, overflowY: 'auto', padding: '16px 24px' }}>
          {isLoading ? (
            <div style={{ display: 'flex', justifyContent: 'center', paddingTop: '80px' }}>
              <div
                className="spin-loader"
                style={{
                  width: '28px',
                  height: '28px',
                  border: '2px solid rgba(245, 236, 215, 0.12)',
                  borderTopColor: '#C44536',
                  borderRadius: '50%',
                }}
              />
            </div>
          ) : !cart || cart.items.length === 0 ? (
            <div
              style={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                paddingTop: '80px',
                gap: '14px',
              }}
            >
              <div style={{ fontSize: '52px', opacity: 0.35, lineHeight: 1 }}>🍕</div>
              <p
                style={{
                  fontSize: '14px',
                  color: '#8B7E72',
                  textAlign: 'center',
                  margin: 0,
                  lineHeight: 1.6,
                }}
              >
                Your cart is empty.
                <br />
                Add some pizzas to get started!
              </p>
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
              {cart.items.map((item) => (
                <div
                  key={item.id}
                  style={{
                    background: 'rgba(245, 236, 215, 0.04)',
                    border: '1px solid rgba(245, 236, 215, 0.08)',
                    borderRadius: '12px',
                    padding: '14px',
                    opacity: isMutating ? 0.7 : 1,
                    transition: 'opacity 0.2s',
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '10px' }}>
                    <div>
                      <div
                        style={{
                          fontSize: '14px',
                          fontWeight: 600,
                          color: '#F5ECD7',
                          marginBottom: '3px',
                        }}
                      >
                        {item.pizzaName}
                      </div>
                      <div style={{ fontSize: '12px', color: '#8B7E72' }}>
                        {item.pizzaVariantName}
                        {item.toppings.length > 0 &&
                          ` · ${item.toppings.length} topping${item.toppings.length > 1 ? 's' : ''}`}
                      </div>
                    </div>
                    <button
                      onClick={() => remove(item.id)}
                      style={{
                        background: 'none',
                        border: 'none',
                        cursor: 'pointer',
                        color: '#8B7E72',
                        padding: '2px',
                        display: 'flex',
                        alignItems: 'center',
                        transition: 'color 0.2s',
                      }}
                      onMouseEnter={(e) => ((e.currentTarget as HTMLElement).style.color = '#C44536')}
                      onMouseLeave={(e) => ((e.currentTarget as HTMLElement).style.color = '#8B7E72')}
                    >
                      <Trash2 size={14} />
                    </button>
                  </div>

                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                      <button
                        onClick={() => decrease(item.id)}
                        disabled={isMutating}
                        style={{
                          width: '26px',
                          height: '26px',
                          borderRadius: '50%',
                          background: 'rgba(245, 236, 215, 0.06)',
                          border: '1px solid rgba(245, 236, 215, 0.12)',
                          cursor: 'pointer',
                          color: '#F5ECD7',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          transition: 'background 0.15s',
                        }}
                        onMouseEnter={(e) => ((e.currentTarget as HTMLElement).style.background = 'rgba(245, 236, 215, 0.12)')}
                        onMouseLeave={(e) => ((e.currentTarget as HTMLElement).style.background = 'rgba(245, 236, 215, 0.06)')}
                      >
                        <Minus size={11} />
                      </button>
                      <span
                        style={{
                          fontSize: '14px',
                          fontWeight: 700,
                          color: '#F5ECD7',
                          minWidth: '20px',
                          textAlign: 'center',
                        }}
                      >
                        {item.quantity}
                      </span>
                      <button
                        onClick={() => increase(item.id)}
                        disabled={isMutating}
                        style={{
                          width: '26px',
                          height: '26px',
                          borderRadius: '50%',
                          background: 'rgba(245, 236, 215, 0.06)',
                          border: '1px solid rgba(245, 236, 215, 0.12)',
                          cursor: 'pointer',
                          color: '#F5ECD7',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          transition: 'background 0.15s',
                        }}
                        onMouseEnter={(e) => ((e.currentTarget as HTMLElement).style.background = 'rgba(245, 236, 215, 0.12)')}
                        onMouseLeave={(e) => ((e.currentTarget as HTMLElement).style.background = 'rgba(245, 236, 215, 0.06)')}
                      >
                        <Plus size={11} />
                      </button>
                    </div>
                    <div style={{ fontSize: '15px', fontWeight: 700, color: '#F5ECD7' }}>
                      ${item.subTotal.toFixed(2)}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Footer */}
        {cart && cart.items.length > 0 && (
          <div
            style={{
              borderTop: '1px solid rgba(245, 236, 215, 0.08)',
              padding: '20px 24px',
              flexShrink: 0,
            }}
          >
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '16px',
              }}
            >
              <span style={{ fontSize: '13px', color: '#8B7E72', letterSpacing: '0.05em' }}>
                {cart.totalQuantity} item{cart.totalQuantity !== 1 ? 's' : ''}
              </span>
              <div style={{ textAlign: 'right' }}>
                <div style={{ fontSize: '11px', color: '#8B7E72', marginBottom: '2px' }}>Total</div>
                <div
                  style={{
                    fontFamily: '"Bodoni Moda", Georgia, serif',
                    fontStyle: 'italic',
                    fontSize: '24px',
                    color: '#F5ECD7',
                    lineHeight: 1,
                  }}
                >
                  ${cart.total.toFixed(2)}
                </div>
              </div>
            </div>

            <button
              onClick={() => {
                onClose()
                navigate('/checkout')
              }}
              style={{
                background: '#C44536',
                color: '#F5ECD7',
                border: 'none',
                borderRadius: '12px',
                padding: '14px 24px',
                fontSize: '14px',
                fontWeight: 500,
                cursor: 'pointer',
                width: '100%',
                letterSpacing: '0.05em',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '8px',
                transition: 'background 0.2s, transform 0.15s',
                marginBottom: '10px',
              }}
              onMouseEnter={(e) => {
                const el = e.currentTarget as HTMLElement
                el.style.background = '#A8352A'
                el.style.transform = 'translateY(-1px)'
              }}
              onMouseLeave={(e) => {
                const el = e.currentTarget as HTMLElement
                el.style.background = '#C44536'
                el.style.transform = 'none'
              }}
            >
              Proceed to Checkout
              <ArrowRight size={15} />
            </button>

            <button
              onClick={() => clear()}
              disabled={isClearing}
              style={{
                background: 'none',
                border: 'none',
                color: '#8B7E72',
                fontSize: '12px',
                cursor: 'pointer',
                padding: '4px',
                width: '100%',
                textAlign: 'center',
                letterSpacing: '0.05em',
                opacity: isClearing ? 0.5 : 1,
                transition: 'color 0.2s',
              }}
              onMouseEnter={(e) => ((e.currentTarget as HTMLElement).style.color = '#C44536')}
              onMouseLeave={(e) => ((e.currentTarget as HTMLElement).style.color = '#8B7E72')}
            >
              {isClearing ? 'Clearing…' : 'Clear cart'}
            </button>
          </div>
        )}
      </div>
    </>
  )
}
