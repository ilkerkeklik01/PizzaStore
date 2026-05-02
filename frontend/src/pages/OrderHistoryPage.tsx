import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ArrowRight, ClipboardList, ChevronLeft, ChevronRight } from 'lucide-react'
import { getMyOrders } from '@/api/order.api'
import { useAuth } from '@/hooks/useAuth'
import Navbar from '@/components/Navbar'
import type { Order, OrderStatus } from '@/types/order'

// ─── Status metadata ──────────────────────────────────────────────────────────

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

// ─── Filter state ─────────────────────────────────────────────────────────────

interface FilterState {
  status: OrderStatus | ''
  fromDate: string
  toDate: string
  page: number
  pageSize: number
}

const DEFAULT_FILTERS: FilterState = {
  status: '',
  fromDate: '',
  toDate: '',
  page: 1,
  pageSize: 10,
}

// ─── Shared styles ────────────────────────────────────────────────────────────

const inputStyle: React.CSSProperties = {
  background: '#252320',
  color: '#F5ECD7',
  border: '1px solid rgba(245, 236, 215, 0.12)',
  borderRadius: '8px',
  padding: '8px 12px',
  fontSize: '13px',
  fontFamily: '"DM Sans", sans-serif',
  outline: 'none',
  cursor: 'pointer',
  colorScheme: 'dark',
  width: '100%',
}

const labelStyle: React.CSSProperties = {
  fontSize: '10px',
  fontWeight: 600,
  letterSpacing: '0.1em',
  textTransform: 'uppercase',
  color: '#8B7E72',
  fontFamily: '"DM Sans", sans-serif',
  marginBottom: '5px',
  display: 'block',
}

// ─── Sub-components ───────────────────────────────────────────────────────────

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

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function OrderHistoryPage() {
  const navigate = useNavigate()
  const { isAuthenticated } = useAuth()
  const [cartOpen, setCartOpen] = useState(false)
  const [filters, setFilters] = useState<FilterState>(DEFAULT_FILTERS)

  const hasActiveFilters = !!(filters.status || filters.fromDate || filters.toDate)

  const { data, isLoading, isFetching } = useQuery({
    queryKey: ['orders', filters],
    queryFn: () =>
      getMyOrders({
        status: filters.status || undefined,
        fromDate: filters.fromDate || undefined,
        toDate: filters.toDate || undefined,
        page: filters.page,
        pageSize: filters.pageSize,
      }),
    enabled: isAuthenticated,
  })

  const orders = data?.items ?? []
  const totalCount = data?.totalCount ?? 0
  const totalPages = data?.totalPages ?? 1
  const rangeStart = totalCount === 0 ? 0 : (filters.page - 1) * filters.pageSize + 1
  const rangeEnd = Math.min(filters.page * filters.pageSize, totalCount)

  // Changing status or dates resets to page 1
  const updateFilter = (update: Partial<Omit<FilterState, 'page' | 'pageSize'>>) => {
    setFilters((prev) => ({ ...prev, ...update, page: 1 }))
  }

  const handleReset = () => setFilters(DEFAULT_FILTERS)

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }))
  }

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
        <div style={{ marginBottom: '32px' }}>
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
          {!isLoading && totalCount > 0 && (
            <p style={{ fontSize: '14px', color: '#8B7E72', margin: 0 }}>
              {totalCount} order{totalCount !== 1 ? 's' : ''} placed
            </p>
          )}
        </div>

        {/* Filter bar */}
        <div
          style={{
            background: '#1E1C19',
            border: '1px solid rgba(245, 236, 215, 0.07)',
            borderRadius: '12px',
            padding: '20px 22px',
            marginBottom: '28px',
          }}
        >
          <div
            style={{
              display: 'flex',
              alignItems: 'flex-end',
              flexWrap: 'wrap',
              gap: '12px',
            }}
          >
            {/* Status */}
            <div style={{ display: 'flex', flexDirection: 'column', minWidth: '150px' }}>
              <label style={labelStyle}>Status</label>
              <select
                value={filters.status}
                onChange={(e) => updateFilter({ status: e.target.value as OrderStatus | '' })}
                style={inputStyle}
              >
                <option value="">All Statuses</option>
                <option value="Pending">Pending</option>
                <option value="Confirmed">Confirmed</option>
                <option value="Preparing">Preparing</option>
                <option value="OutForDelivery">Out for Delivery</option>
                <option value="Delivered">Delivered</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>

            {/* From date */}
            <div style={{ display: 'flex', flexDirection: 'column', minWidth: '148px' }}>
              <label style={labelStyle}>From date</label>
              <input
                type="date"
                value={filters.fromDate}
                onChange={(e) => updateFilter({ fromDate: e.target.value })}
                style={inputStyle}
              />
            </div>

            {/* To date */}
            <div style={{ display: 'flex', flexDirection: 'column', minWidth: '148px' }}>
              <label style={labelStyle}>To date</label>
              <input
                type="date"
                value={filters.toDate}
                onChange={(e) => updateFilter({ toDate: e.target.value })}
                style={inputStyle}
              />
            </div>

            {/* Clear filters — only when something is active */}
            {hasActiveFilters && (
              <button
                onClick={handleReset}
                style={{
                  background: 'none',
                  border: '1px solid rgba(245, 236, 215, 0.12)',
                  borderRadius: '8px',
                  color: '#8B7E72',
                  fontSize: '13px',
                  fontFamily: '"DM Sans", sans-serif',
                  padding: '8px 14px',
                  cursor: 'pointer',
                  alignSelf: 'flex-end',
                  transition: 'color 0.2s, border-color 0.2s',
                  whiteSpace: 'nowrap',
                }}
                onMouseEnter={(e) => {
                  const el = e.currentTarget as HTMLElement
                  el.style.color = '#F5ECD7'
                  el.style.borderColor = 'rgba(245, 236, 215, 0.3)'
                }}
                onMouseLeave={(e) => {
                  const el = e.currentTarget as HTMLElement
                  el.style.color = '#8B7E72'
                  el.style.borderColor = 'rgba(245, 236, 215, 0.12)'
                }}
              >
                Clear filters
              </button>
            )}
          </div>
        </div>

        {/* Content */}
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
        ) : orders.length === 0 ? (
          hasActiveFilters ? (
            /* Filtered empty state */
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
              <p style={{ fontSize: '16px', color: '#F5ECD7', marginBottom: '8px' }}>
                No orders match your filters
              </p>
              <p style={{ fontSize: '13px', color: '#8B7E72', marginBottom: '28px' }}>
                Try adjusting or clearing the filters above.
              </p>
              <button
                onClick={handleReset}
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
                Clear Filters
              </button>
            </div>
          ) : (
            /* No orders at all */
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
          )
        ) : (
          <>
            {/* Order list — subtle dim while re-fetching */}
            <div
              style={{
                display: 'flex',
                flexDirection: 'column',
                gap: '12px',
                opacity: isFetching && !isLoading ? 0.55 : 1,
                transition: 'opacity 0.15s',
              }}
            >
              {orders.map((order) => (
                <OrderRow key={order.id} order={order} />
              ))}
            </div>

            {/* Pagination — always shown when there are results */}
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginTop: '24px',
                padding: '16px 0',
                borderTop: '1px solid rgba(245, 236, 215, 0.07)',
                flexWrap: 'wrap',
                gap: '12px',
              }}
            >
              {/* Result range */}
              <span style={{ fontSize: '12px', color: '#8B7E72', fontFamily: '"DM Sans", sans-serif' }}>
                Showing {rangeStart}–{rangeEnd} of {totalCount} order{totalCount !== 1 ? 's' : ''}
              </span>

              {/* Page controls */}
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <button
                  onClick={() => handlePageChange(filters.page - 1)}
                  disabled={filters.page === 1}
                  style={{
                    background: 'transparent',
                    border: `1px solid ${filters.page === 1 ? 'rgba(245,236,215,0.1)' : '#C44536'}`,
                    borderRadius: '8px',
                    padding: '7px',
                    cursor: filters.page === 1 ? 'default' : 'pointer',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    opacity: filters.page === 1 ? 0.35 : 1,
                    transition: 'opacity 0.2s, border-color 0.2s',
                  }}
                >
                  <ChevronLeft size={15} color={filters.page === 1 ? '#8B7E72' : '#C44536'} />
                </button>

                <span
                  style={{
                    fontSize: '13px',
                    color: '#8B7E72',
                    minWidth: '100px',
                    textAlign: 'center',
                    fontFamily: '"DM Sans", sans-serif',
                  }}
                >
                  Page {filters.page} of {totalPages}
                </span>

                <button
                  onClick={() => handlePageChange(filters.page + 1)}
                  disabled={filters.page >= totalPages}
                  style={{
                    background: 'transparent',
                    border: `1px solid ${filters.page >= totalPages ? 'rgba(245,236,215,0.1)' : '#C44536'}`,
                    borderRadius: '8px',
                    padding: '7px',
                    cursor: filters.page >= totalPages ? 'default' : 'pointer',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    opacity: filters.page >= totalPages ? 0.35 : 1,
                    transition: 'opacity 0.2s, border-color 0.2s',
                  }}
                >
                  <ChevronRight size={15} color={filters.page >= totalPages ? '#8B7E72' : '#C44536'} />
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  )
}
