import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, CheckCircle, ShoppingBag, AlertCircle } from 'lucide-react'
import { getCart } from '@/api/cart.api'
import { checkoutCart } from '@/api/order.api'
import { useAuth } from '@/hooks/useAuth'
import Navbar from '@/components/Navbar'
import type { Order } from '@/types/order'

export default function CheckoutPage() {
  const navigate = useNavigate()
  const { isAuthenticated } = useAuth()
  const queryClient = useQueryClient()
  const [cartOpen, setCartOpen] = useState(false)
  const [placedOrder, setPlacedOrder] = useState<Order | null>(null)
  const [error, setError] = useState<string | null>(null)

  const { data: cart, isLoading: cartLoading } = useQuery({
    queryKey: ['cart'],
    queryFn: getCart,
    enabled: isAuthenticated,
  })

  const { mutate: placeOrder, isPending } = useMutation({
    mutationFn: checkoutCart,
    onSuccess: (order) => {
      setPlacedOrder(order)
      queryClient.invalidateQueries({ queryKey: ['cart'] })
      queryClient.invalidateQueries({ queryKey: ['orders'] })
    },
    onError: (err: unknown) => {
      const msg =
        err && typeof err === 'object' && 'response' in err
          ? (err as { response?: { data?: { message?: string } } }).response?.data?.message
          : null
      setError(msg ?? 'Something went wrong. Please try again.')
    },
  })

  // ── Success state ──────────────────────────────────────────────────────────
  if (placedOrder) {
    return (
      <div style={{ minHeight: '100vh', background: '#1C1A17' }}>
        <Navbar onCartOpen={() => setCartOpen(false)} />
        <div
          style={{
            minHeight: '100vh',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            padding: '100px 24px 60px',
            position: 'relative',
          }}
        >
          {/* Glow */}
          <div
            style={{
              position: 'absolute',
              inset: 0,
              background:
                'radial-gradient(ellipse 60% 40% at 50% 50%, rgba(196, 69, 54, 0.1) 0%, transparent 70%)',
              pointerEvents: 'none',
            }}
          />

          <div
            style={{
              position: 'relative',
              zIndex: 1,
              textAlign: 'center',
              maxWidth: '480px',
              animation: 'scaleIn 0.5s ease both',
            }}
          >
            <div
              style={{
                width: '72px',
                height: '72px',
                borderRadius: '50%',
                background: 'rgba(90, 186, 90, 0.12)',
                border: '1px solid rgba(90, 186, 90, 0.3)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                margin: '0 auto 28px',
              }}
            >
              <CheckCircle size={34} color="#5ABA5A" />
            </div>

            <p
              style={{
                fontSize: '11px',
                letterSpacing: '0.32em',
                fontWeight: 700,
                color: '#C44536',
                textTransform: 'uppercase',
                marginBottom: '14px',
              }}
            >
              Order Confirmed
            </p>

            <h1
              style={{
                fontFamily: '"Bodoni Moda", Georgia, serif',
                fontStyle: 'italic',
                fontSize: 'clamp(32px, 6vw, 52px)',
                color: '#F5ECD7',
                margin: '0 0 16px 0',
                lineHeight: 1.1,
              }}
            >
              Grazie!
            </h1>

            <p style={{ fontSize: '15px', color: '#8B7E72', lineHeight: 1.7, marginBottom: '8px' }}>
              Your order is being prepared with love and fire.
            </p>

            <div
              style={{
                background: 'rgba(245, 236, 215, 0.04)',
                border: '1px solid rgba(245, 236, 215, 0.1)',
                borderRadius: '12px',
                padding: '16px 20px',
                margin: '28px 0',
                textAlign: 'left',
              }}
            >
              <div style={{ fontSize: '11px', color: '#8B7E72', letterSpacing: '0.1em', marginBottom: '6px' }}>
                ORDER ID
              </div>
              <div
                style={{
                  fontFamily: '"Bodoni Moda", Georgia, serif',
                  fontStyle: 'italic',
                  fontSize: '15px',
                  color: '#F5ECD7',
                  wordBreak: 'break-all',
                }}
              >
                {placedOrder.id}
              </div>
              <div
                style={{
                  marginTop: '14px',
                  paddingTop: '14px',
                  borderTop: '1px solid rgba(245, 236, 215, 0.08)',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                }}
              >
                <span style={{ fontSize: '13px', color: '#8B7E72' }}>Total</span>
                <span
                  style={{
                    fontFamily: '"Bodoni Moda", Georgia, serif',
                    fontStyle: 'italic',
                    fontSize: '22px',
                    color: '#F5ECD7',
                  }}
                >
                  ${placedOrder.totalPrice.toFixed(2)}
                </span>
              </div>
            </div>

            <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap', justifyContent: 'center' }}>
              <button
                onClick={() => navigate(`/orders/${placedOrder.id}`)}
                style={{
                  background: '#C44536',
                  color: '#F5ECD7',
                  border: 'none',
                  borderRadius: '32px',
                  padding: '12px 28px',
                  fontSize: '14px',
                  fontWeight: 500,
                  cursor: 'pointer',
                  letterSpacing: '0.04em',
                  transition: 'background 0.2s, transform 0.15s',
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
                Track Order
              </button>

              <button
                onClick={() => navigate('/')}
                style={{
                  background: 'transparent',
                  color: '#8B7E72',
                  border: '1px solid rgba(245, 236, 215, 0.15)',
                  borderRadius: '32px',
                  padding: '12px 28px',
                  fontSize: '14px',
                  fontWeight: 400,
                  cursor: 'pointer',
                  letterSpacing: '0.04em',
                  transition: 'all 0.2s',
                }}
                onMouseEnter={(e) => {
                  const el = e.currentTarget as HTMLElement
                  el.style.color = '#F5ECD7'
                  el.style.borderColor = 'rgba(245, 236, 215, 0.3)'
                }}
                onMouseLeave={(e) => {
                  const el = e.currentTarget as HTMLElement
                  el.style.color = '#8B7E72'
                  el.style.borderColor = 'rgba(245, 236, 215, 0.15)'
                }}
              >
                Continue Shopping
              </button>
            </div>
          </div>
        </div>
      </div>
    )
  }

  // ── Checkout form ──────────────────────────────────────────────────────────
  return (
    <div style={{ minHeight: '100vh', background: '#1C1A17' }}>
      <Navbar onCartOpen={() => setCartOpen(cartOpen)} />

      <div
        style={{
          maxWidth: '680px',
          margin: '0 auto',
          padding: '100px clamp(20px, 5vw, 48px) 80px',
        }}
      >
        {/* Back */}
        <button
          onClick={() => navigate('/')}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            background: 'none',
            border: 'none',
            color: '#8B7E72',
            fontSize: '13px',
            cursor: 'pointer',
            padding: '0 0 32px 0',
            letterSpacing: '0.04em',
            transition: 'color 0.2s',
          }}
          onMouseEnter={(e) => ((e.currentTarget as HTMLElement).style.color = '#F5ECD7')}
          onMouseLeave={(e) => ((e.currentTarget as HTMLElement).style.color = '#8B7E72')}
        >
          <ArrowLeft size={14} />
          Back to menu
        </button>

        <p
          style={{
            fontSize: '11px',
            letterSpacing: '0.32em',
            fontWeight: 700,
            color: '#C44536',
            textTransform: 'uppercase',
            marginBottom: '12px',
          }}
        >
          Review & Place
        </p>

        <h1
          style={{
            fontFamily: '"Bodoni Moda", Georgia, serif',
            fontStyle: 'italic',
            fontSize: 'clamp(32px, 5vw, 48px)',
            color: '#F5ECD7',
            margin: '0 0 40px 0',
            lineHeight: 1.1,
          }}
        >
          Your Order
        </h1>

        {/* Error banner */}
        {error && (
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '10px',
              background: 'rgba(196, 69, 54, 0.1)',
              border: '1px solid rgba(196, 69, 54, 0.3)',
              borderRadius: '10px',
              padding: '14px 16px',
              marginBottom: '24px',
              animation: 'scaleIn 0.2s ease both',
            }}
          >
            <AlertCircle size={16} color="#C44536" style={{ flexShrink: 0 }} />
            <span style={{ fontSize: '13px', color: '#C44536' }}>{error}</span>
          </div>
        )}

        {/* Cart loading */}
        {cartLoading ? (
          <div style={{ display: 'flex', justifyContent: 'center', padding: '60px 0' }}>
            <div
              className="spin-loader"
              style={{
                width: '32px',
                height: '32px',
                border: '2px solid rgba(245, 236, 215, 0.1)',
                borderTopColor: '#C44536',
                borderRadius: '50%',
              }}
            />
          </div>
        ) : !cart || cart.items.length === 0 ? (
          <div
            style={{
              textAlign: 'center',
              padding: '60px 0',
              color: '#8B7E72',
            }}
          >
            <ShoppingBag size={40} style={{ opacity: 0.3, marginBottom: '16px' }} />
            <p style={{ fontSize: '14px', margin: '0 0 24px 0' }}>Your cart is empty.</p>
            <button
              onClick={() => navigate('/')}
              style={{
                background: '#C44536',
                color: '#F5ECD7',
                border: 'none',
                borderRadius: '32px',
                padding: '12px 28px',
                fontSize: '13px',
                cursor: 'pointer',
              }}
            >
              Browse Menu
            </button>
          </div>
        ) : (
          <>
            {/* Items list */}
            <div
              style={{
                background: 'rgba(245, 236, 215, 0.03)',
                border: '1px solid rgba(245, 236, 215, 0.08)',
                borderRadius: '16px',
                overflow: 'hidden',
                marginBottom: '24px',
              }}
            >
              {cart.items.map((item, idx) => (
                <div
                  key={item.id}
                  style={{
                    padding: '18px 20px',
                    borderBottom:
                      idx < cart.items.length - 1
                        ? '1px solid rgba(245, 236, 215, 0.06)'
                        : 'none',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'flex-start',
                    gap: '16px',
                  }}
                >
                  <div style={{ flex: 1 }}>
                    <div
                      style={{
                        fontSize: '15px',
                        fontWeight: 600,
                        color: '#F5ECD7',
                        marginBottom: '4px',
                      }}
                    >
                      {item.pizzaName}
                    </div>
                    <div style={{ fontSize: '12px', color: '#8B7E72', lineHeight: 1.6 }}>
                      {item.pizzaVariantName} · Qty {item.quantity}
                      {item.toppings.length > 0 && (
                        <>
                          <br />
                          {item.toppings.map((t) => t.toppingName).join(', ')}
                        </>
                      )}
                    </div>
                  </div>
                  <div
                    style={{
                      fontFamily: '"Bodoni Moda", Georgia, serif',
                      fontStyle: 'italic',
                      fontSize: '18px',
                      color: '#F5ECD7',
                      flexShrink: 0,
                    }}
                  >
                    ${item.subTotal.toFixed(2)}
                  </div>
                </div>
              ))}
            </div>

            {/* Totals */}
            <div
              style={{
                background: 'rgba(245, 236, 215, 0.03)',
                border: '1px solid rgba(245, 236, 215, 0.08)',
                borderRadius: '16px',
                padding: '20px',
                marginBottom: '28px',
              }}
            >
              <div
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  marginBottom: '12px',
                }}
              >
                <span style={{ fontSize: '13px', color: '#8B7E72' }}>Subtotal</span>
                <span style={{ fontSize: '13px', color: '#F5ECD7' }}>
                  ${cart.subTotal.toFixed(2)}
                </span>
              </div>
              <div
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  paddingTop: '14px',
                  borderTop: '1px solid rgba(245, 236, 215, 0.08)',
                }}
              >
                <span style={{ fontSize: '15px', color: '#F5ECD7', fontWeight: 600 }}>
                  Total
                </span>
                <span
                  style={{
                    fontFamily: '"Bodoni Moda", Georgia, serif',
                    fontStyle: 'italic',
                    fontSize: '26px',
                    color: '#F5ECD7',
                    lineHeight: 1,
                  }}
                >
                  ${cart.total.toFixed(2)}
                </span>
              </div>
            </div>

            {/* Place order button */}
            <button
              onClick={() => {
                setError(null)
                placeOrder()
              }}
              disabled={isPending}
              style={{
                background: isPending ? 'rgba(196, 69, 54, 0.5)' : '#C44536',
                color: '#F5ECD7',
                border: 'none',
                borderRadius: '14px',
                padding: '16px 32px',
                fontSize: '15px',
                fontWeight: 500,
                cursor: isPending ? 'not-allowed' : 'pointer',
                width: '100%',
                letterSpacing: '0.06em',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '10px',
                transition: 'background 0.2s, transform 0.15s',
              }}
              onMouseEnter={(e) => {
                if (isPending) return
                const el = e.currentTarget as HTMLElement
                el.style.background = '#A8352A'
                el.style.transform = 'translateY(-1px)'
              }}
              onMouseLeave={(e) => {
                if (isPending) return
                const el = e.currentTarget as HTMLElement
                el.style.background = '#C44536'
                el.style.transform = 'none'
              }}
            >
              {isPending ? (
                <>
                  <div
                    className="spin-loader"
                    style={{
                      width: '16px',
                      height: '16px',
                      border: '2px solid rgba(245, 236, 215, 0.3)',
                      borderTopColor: '#F5ECD7',
                      borderRadius: '50%',
                    }}
                  />
                  Placing order…
                </>
              ) : (
                'Place Order'
              )}
            </button>

            <p
              style={{
                textAlign: 'center',
                fontSize: '12px',
                color: '#8B7E72',
                marginTop: '16px',
                lineHeight: 1.6,
              }}
            >
              By placing your order you confirm your selections above.
              <br />
              Prices shown are final.
            </p>
          </>
        )}
      </div>
    </div>
  )
}
