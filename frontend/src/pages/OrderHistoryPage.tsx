import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ArrowRight, ClipboardList } from 'lucide-react'
import { getMyOrders } from '@/api/order.api'
import { useAuth } from '@/hooks/useAuth'
import Navbar from '@/components/Navbar'
import type { Order, OrderStatus } from '@/types/order'

const STATUS_COLORS: Record<OrderStatus, { bg: string; text: string; dot: string }> = {
  Pending:        { bg: 'rgba(212, 164, 76, 0.12)',  text: '#D4A44C', dot: '#D4A44C' },
  Confirmed:      { bg: 'rgba(100, 160, 220, 0.12)', text: '#7EACD8', dot: '#7EACD8' },
  Preparing:      { bg: 'rgba(196, 69, 54, 0.12)',   text: '#C44536', dot: '#C44536' },
  OutForDelivery: { bg: 'rgba(130, 100, 220, 0.12)', text: '#A87EE0', dot: '#A87EE0' },
  Delivered:      { bg: 'rgba(90, 186, 90, 0.12)',   text: '#5ABA5A', dot: '#5ABA5A' },
  Cancelled:      { bg: 'rgba(139, 126, 114, 0.12)', text: '#8B7E72', dot: '#8B7E72' },
}

const STATUS_LABEL: Record<OrderStatus, string> = {
  Pending:        'Pending',
  Confirmed:      'Confirmed',
  Preparing:      'Preparing',
  OutForDelivery: 'Out for Delivery',
  Delivered:      'Delivered',
  Cancelled:      'Cancelled',
}

function StatusBadge({ status }: { status: OrderStatus }) {
  const s = STATUS_COLORS[status] ?? STATUS_COLORS.Pending
  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: '6px',
        background: s.bg,
        color: s.text,
        fontSize: '11px',
        fontWeight: 700,
        letterSpacing: '0.1em',
        textTransform: 'uppercase',
        padding: '4px 10px',
        borderRadius: '20px',
      }}
    >
      <span
        style={{
          width: '5px',
          height: '5px',
          borderRadius: '50%',
          background: s.dot,
          flexShrink: 0,
        }}
      />
      {STATUS_LABEL[status]}
    </span>
  )
}

function OrderRow({ order }: { order: Order }) {
  const navigate = useNavigate()
  const date = new Date(order.createdAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
  const time = new Date(order.createdAt).toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit',
  })

  return (
    <div
      onClick={() => navigate(`/orders/${order.id}`)}
      style={{
        background: '#1E1C19',
        border: '1px solid rgba(245, 236, 215, 0.07)',
        borderRadius: '14px',
        padding: '20px 22px',
        cursor: 'pointer',
        transition: 'border-color 0.25s, transform 0.25s, box-shadow 0.25s',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        gap: '16px',
      }}
      onMouseEnter={(e) => {
        const el = e.currentTarget as HTMLElement
        el.style.borderColor = 'rgba(196, 69, 54, 0.25)'
        el.style.transform = 'translateY(-2px)'
        el.style.boxShadow = '0 12px 32px rgba(0,0,0,0.35)'
      }}
      onMouseLeave={(e) => {
        const el = e.currentTarget as HTMLElement
        el.style.borderColor = 'rgba(245, 236, 215, 0.07)'
        el.style.transform = 'none'
        el.style.boxShadow = 'none'
      }}
    >
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '8px', flexWrap: 'wrap' }}>
          <StatusBadge status={order.status} />
          <span style={{ fontSize: '11px', color: '#8B7E72' }}>
            {date} · {time}
          </span>
        </div>

        <div style={{ fontSize: '13px', color: '#8B7E72', lineHeight: 1.5 }}>
          {order.items.slice(0, 2).map((item) => (
            <span key={item.id}>
              {item.pizzaNameAtOrder} ({item.pizzaSizeAtOrder})
              {item.quantity > 1 && ` ×${item.quantity}`}
            </span>
          )).reduce<React.ReactNode[]>((acc, el, i) => (i === 0 ? [el] : [...acc, ', ', el]), [])}
          {order.items.length > 2 && (
            <span style={{ color: '#C44536' }}> +{order.items.length - 2} more</span>
          )}
        </div>

        <div
          style={{
            fontSize: '11px',
            color: '#8B7E72',
            marginTop: '6px',
            fontFamily: 'monospace',
            letterSpacing: '0.02em',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
          }}
        >
          #{order.id.slice(0, 8).toUpperCase()}
        </div>
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: '16px', flexShrink: 0 }}>
        <div style={{ textAlign: 'right' }}>
          <div style={{ fontSize: '11px', color: '#8B7E72', marginBottom: '2px' }}>Total</div>
          <div
            style={{
              fontFamily: '"Bodoni Moda", Georgia, serif',
              fontStyle: 'italic',
              fontSize: '20px',
              color: '#F5ECD7',
              lineHeight: 1,
            }}
          >
            ${order.totalPrice.toFixed(2)}
          </div>
        </div>
        <ArrowRight size={16} color="#8B7E72" />
      </div>
    </div>
  )
}

export default function OrderHistoryPage() {
  const navigate = useNavigate()
  const { isAuthenticated } = useAuth()
  const [cartOpen, setCartOpen] = useState(false)

  const { data: orders = [], isLoading } = useQuery({
    queryKey: ['orders'],
    queryFn: getMyOrders,
    enabled: isAuthenticated,
  })

  const sorted = [...orders].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  )

  return (
    <div style={{ minHeight: '100vh', background: '#1C1A17' }}>
      <Navbar onCartOpen={() => setCartOpen(!cartOpen)} />

      <div
        style={{
          maxWidth: '800px',
          margin: '0 auto',
          padding: '100px clamp(20px, 5vw, 48px) 80px',
        }}
      >
        {/* Header */}
        <div style={{ marginBottom: '48px' }}>
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
            Account
          </p>
          <h1
            style={{
              fontFamily: '"Bodoni Moda", Georgia, serif',
              fontStyle: 'italic',
              fontSize: 'clamp(32px, 5vw, 52px)',
              color: '#F5ECD7',
              margin: '0 0 12px 0',
              lineHeight: 1.1,
            }}
          >
            Order History
          </h1>
          {!isLoading && orders.length > 0 && (
            <p style={{ fontSize: '14px', color: '#8B7E72', margin: 0 }}>
              {orders.length} order{orders.length !== 1 ? 's' : ''} placed
            </p>
          )}
        </div>

        {isLoading ? (
          <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '16px', padding: '60px 0' }}>
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
            <span style={{ fontSize: '12px', color: '#8B7E72', letterSpacing: '0.1em' }}>
              Loading orders…
            </span>
          </div>
        ) : sorted.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '80px 0' }}>
            <div
              style={{
                width: '64px',
                height: '64px',
                borderRadius: '50%',
                background: 'rgba(245, 236, 215, 0.04)',
                border: '1px solid rgba(245, 236, 215, 0.1)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                margin: '0 auto 24px',
              }}
            >
              <ClipboardList size={28} color="#8B7E72" />
            </div>
            <p style={{ fontSize: '16px', color: '#F5ECD7', marginBottom: '8px' }}>No orders yet</p>
            <p style={{ fontSize: '13px', color: '#8B7E72', marginBottom: '28px' }}>
              Your order history will appear here.
            </p>
            <button
              onClick={() => navigate('/')}
              style={{
                background: '#C44536',
                color: '#F5ECD7',
                border: 'none',
                borderRadius: '32px',
                padding: '12px 28px',
                fontSize: '14px',
                cursor: 'pointer',
                fontWeight: 500,
                letterSpacing: '0.04em',
                transition: 'background 0.2s',
              }}
              onMouseEnter={(e) => ((e.currentTarget as HTMLElement).style.background = '#A8352A')}
              onMouseLeave={(e) => ((e.currentTarget as HTMLElement).style.background = '#C44536')}
            >
              Order Now
            </button>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
            {sorted.map((order) => (
              <OrderRow key={order.id} order={order} />
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
