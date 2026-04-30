import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, AlertCircle, X } from 'lucide-react'
import { getOrderById, cancelOrder } from '@/api/order.api'
import { useAuth } from '@/hooks/useAuth'
import Navbar from '@/components/Navbar'
import type { Order, OrderStatus } from '@/types/order'

// ─── Status config ─────────────────────────────────────────────────────────

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

const TIMELINE_STEPS: OrderStatus[] = [
  'Pending', 'Confirmed', 'Preparing', 'OutForDelivery', 'Delivered',
]

const STATUS_ORDER: Record<OrderStatus, number> = {
  Pending: 0, Confirmed: 1, Preparing: 2, OutForDelivery: 3, Delivered: 4, Cancelled: -1,
}

function StatusBadge({ status }: { status: OrderStatus }) {
  const s = STATUS_COLORS[status]
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
        padding: '5px 12px',
        borderRadius: '20px',
      }}
    >
      <span style={{ width: '5px', height: '5px', borderRadius: '50%', background: s.dot }} />
      {STATUS_LABEL[status]}
    </span>
  )
}

// ─── Cancel Confirmation Modal ──────────────────────────────────────────────

function CancelModal({
  onConfirm,
  onDismiss,
  isPending,
}: {
  onConfirm: () => void
  onDismiss: () => void
  isPending: boolean
}) {
  return (
    <>
      <div
        onClick={onDismiss}
        style={{
          position: 'fixed',
          inset: 0,
          background: 'rgba(0,0,0,0.6)',
          zIndex: 300,
          backdropFilter: 'blur(4px)',
        }}
      />
      <div
        style={{
          position: 'fixed',
          top: '50%',
          left: '50%',
          transform: 'translate(-50%, -50%)',
          zIndex: 301,
          background: '#1E1C19',
          border: '1px solid rgba(245, 236, 215, 0.12)',
          borderRadius: '18px',
          padding: '32px',
          width: 'min(440px, 90vw)',
          animation: 'scaleIn 0.2s ease both',
        }}
      >
        <button
          onClick={onDismiss}
          style={{
            position: 'absolute',
            top: '16px',
            right: '16px',
            background: 'rgba(245, 236, 215, 0.06)',
            border: '1px solid rgba(245, 236, 215, 0.1)',
            borderRadius: '8px',
            cursor: 'pointer',
            color: '#8B7E72',
            padding: '6px',
            display: 'flex',
            alignItems: 'center',
          }}
        >
          <X size={14} />
        </button>

        <AlertCircle size={28} color="#C44536" style={{ marginBottom: '16px' }} />

        <h2
          style={{
            fontFamily: '"Bodoni Moda", Georgia, serif',
            fontStyle: 'italic',
            fontSize: '22px',
            color: '#F5ECD7',
            margin: '0 0 10px 0',
          }}
        >
          Cancel Order?
        </h2>
        <p style={{ fontSize: '14px', color: '#8B7E72', lineHeight: 1.65, margin: '0 0 24px 0' }}>
          This action cannot be undone. Your cart will not be restored automatically.
        </p>

        <div style={{ display: 'flex', gap: '10px' }}>
          <button
            onClick={onConfirm}
            disabled={isPending}
            style={{
              flex: 1,
              background: isPending ? 'rgba(196, 69, 54, 0.4)' : '#C44536',
              color: '#F5ECD7',
              border: 'none',
              borderRadius: '10px',
              padding: '12px',
              fontSize: '14px',
              fontWeight: 500,
              cursor: isPending ? 'not-allowed' : 'pointer',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: '8px',
              transition: 'background 0.2s',
            }}
            onMouseEnter={(e) => {
              if (isPending) return
              ;(e.currentTarget as HTMLElement).style.background = '#A8352A'
            }}
            onMouseLeave={(e) => {
              if (isPending) return
              ;(e.currentTarget as HTMLElement).style.background = '#C44536'
            }}
          >
            {isPending ? (
              <>
                <div
                  className="spin-loader"
                  style={{
                    width: '14px',
                    height: '14px',
                    border: '2px solid rgba(245,236,215,0.3)',
                    borderTopColor: '#F5ECD7',
                    borderRadius: '50%',
                  }}
                />
                Cancelling…
              </>
            ) : (
              'Yes, Cancel Order'
            )}
          </button>
          <button
            onClick={onDismiss}
            style={{
              flex: 1,
              background: 'transparent',
              color: '#8B7E72',
              border: '1px solid rgba(245, 236, 215, 0.15)',
              borderRadius: '10px',
              padding: '12px',
              fontSize: '14px',
              cursor: 'pointer',
              transition: 'all 0.2s',
            }}
            onMouseEnter={(e) => {
              const el = e.currentTarget as HTMLElement
              el.style.color = '#F5ECD7'
              el.style.borderColor = 'rgba(245,236,215,0.3)'
            }}
            onMouseLeave={(e) => {
              const el = e.currentTarget as HTMLElement
              el.style.color = '#8B7E72'
              el.style.borderColor = 'rgba(245,236,215,0.15)'
            }}
          >
            Keep Order
          </button>
        </div>
      </div>
    </>
  )
}

// ─── Order Detail Page ──────────────────────────────────────────────────────

export default function OrderDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { isAuthenticated } = useAuth()
  const queryClient = useQueryClient()
  const [showCancelModal, setShowCancelModal] = useState(false)
  const [cancelError, setCancelError] = useState<string | null>(null)

  const { data: order, isLoading, isError } = useQuery<Order>({
    queryKey: ['orders', id],
    queryFn: () => getOrderById(id!),
    enabled: isAuthenticated && !!id,
  })

  const { mutate: doCancel, isPending: isCancelling } = useMutation({
    mutationFn: () => cancelOrder(id!),
    onSuccess: () => {
      setShowCancelModal(false)
      queryClient.invalidateQueries({ queryKey: ['orders', id] })
      queryClient.invalidateQueries({ queryKey: ['orders'] })
    },
    onError: (err: unknown) => {
      setShowCancelModal(false)
      const msg =
        err && typeof err === 'object' && 'response' in err
          ? (err as { response?: { data?: { message?: string } } }).response?.data?.message
          : null
      setCancelError(msg ?? 'Could not cancel. The order may already be in progress.')
    },
  })

  const canCancel =
    order?.status === 'Pending' || order?.status === 'Confirmed'

  const formatDate = (iso: string) =>
    new Date(iso).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })

  return (
    <div style={{ minHeight: '100vh', background: '#1C1A17' }}>
      <Navbar onCartOpen={() => {}} />

      {showCancelModal && (
        <CancelModal
          onConfirm={() => doCancel()}
          onDismiss={() => setShowCancelModal(false)}
          isPending={isCancelling}
        />
      )}

      <div
        style={{
          maxWidth: '760px',
          margin: '0 auto',
          padding: '100px clamp(20px, 5vw, 48px) 80px',
        }}
      >
        {/* Back */}
        <button
          onClick={() => navigate('/orders')}
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
          Back to orders
        </button>

        {isLoading ? (
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
        ) : isError || !order ? (
          <div style={{ textAlign: 'center', padding: '60px 0', color: '#8B7E72' }}>
            <p style={{ fontSize: '15px', color: '#F5ECD7', marginBottom: '8px' }}>Order not found</p>
            <p style={{ fontSize: '13px', marginBottom: '24px' }}>
              This order doesn't exist or you don't have access to it.
            </p>
            <button
              onClick={() => navigate('/orders')}
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
              View All Orders
            </button>
          </div>
        ) : (
          <>
            {/* Header */}
            <div style={{ marginBottom: '36px' }}>
              <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: '16px', flexWrap: 'wrap' }}>
                <div>
                  <p
                    style={{
                      fontSize: '11px',
                      letterSpacing: '0.32em',
                      fontWeight: 700,
                      color: '#C44536',
                      textTransform: 'uppercase',
                      marginBottom: '10px',
                    }}
                  >
                    Order Details
                  </p>
                  <h1
                    style={{
                      fontFamily: '"Bodoni Moda", Georgia, serif',
                      fontStyle: 'italic',
                      fontSize: 'clamp(28px, 4vw, 42px)',
                      color: '#F5ECD7',
                      margin: '0 0 12px 0',
                      lineHeight: 1.1,
                    }}
                  >
                    #{order.id.slice(0, 8).toUpperCase()}
                  </h1>
                  <StatusBadge status={order.status} />
                </div>

                <div style={{ textAlign: 'right', flexShrink: 0 }}>
                  <div style={{ fontSize: '11px', color: '#8B7E72', marginBottom: '4px' }}>
                    {formatDate(order.createdAt)}
                  </div>
                  <div
                    style={{
                      fontFamily: '"Bodoni Moda", Georgia, serif',
                      fontStyle: 'italic',
                      fontSize: '32px',
                      color: '#F5ECD7',
                      lineHeight: 1,
                    }}
                  >
                    ${order.totalPrice.toFixed(2)}
                  </div>
                </div>
              </div>
            </div>

            {/* Cancel error */}
            {cancelError && (
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
                }}
              >
                <AlertCircle size={15} color="#C44536" style={{ flexShrink: 0 }} />
                <span style={{ fontSize: '13px', color: '#C44536' }}>{cancelError}</span>
              </div>
            )}

            {/* Status timeline */}
            {order.status !== 'Cancelled' && (
              <div
                style={{
                  background: 'rgba(245, 236, 215, 0.03)',
                  border: '1px solid rgba(245, 236, 215, 0.08)',
                  borderRadius: '16px',
                  padding: '22px 24px',
                  marginBottom: '24px',
                }}
              >
                <div style={{ fontSize: '11px', color: '#8B7E72', letterSpacing: '0.12em', textTransform: 'uppercase', marginBottom: '18px' }}>
                  Order Progress
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0' }}>
                  {TIMELINE_STEPS.map((step, idx) => {
                    const currentIdx = STATUS_ORDER[order.status]
                    const stepIdx = STATUS_ORDER[step]
                    const isActive = stepIdx === currentIdx
                    const isPast = stepIdx < currentIdx

                    return (
                      <div key={step} style={{ display: 'flex', alignItems: 'center', flex: idx < TIMELINE_STEPS.length - 1 ? '1' : undefined }}>
                        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '8px' }}>
                          <div
                            style={{
                              width: '10px',
                              height: '10px',
                              borderRadius: '50%',
                              background: isActive
                                ? '#C44536'
                                : isPast
                                ? '#5ABA5A'
                                : 'rgba(245, 236, 215, 0.15)',
                              border: isActive
                                ? '2px solid rgba(196, 69, 54, 0.4)'
                                : 'none',
                              outline: isActive ? '3px solid rgba(196, 69, 54, 0.2)' : 'none',
                              flexShrink: 0,
                            }}
                          />
                          <span
                            style={{
                              fontSize: '10px',
                              color: isActive
                                ? '#F5ECD7'
                                : isPast
                                ? '#5ABA5A'
                                : '#8B7E72',
                              fontWeight: isActive ? 700 : 400,
                              letterSpacing: '0.04em',
                              whiteSpace: 'nowrap',
                              textAlign: 'center',
                            }}
                          >
                            {STATUS_LABEL[step].replace('Out for ', '')}
                          </span>
                        </div>
                        {idx < TIMELINE_STEPS.length - 1 && (
                          <div
                            style={{
                              flex: 1,
                              height: '1px',
                              background: isPast ? '#5ABA5A' : 'rgba(245, 236, 215, 0.1)',
                              marginBottom: '26px',
                              transition: 'background 0.5s',
                            }}
                          />
                        )}
                      </div>
                    )
                  })}
                </div>
              </div>
            )}

            {/* Timestamps */}
            {(order.confirmedAt || order.completedAt || order.cancelledAt) && (
              <div
                style={{
                  background: 'rgba(245, 236, 215, 0.03)',
                  border: '1px solid rgba(245, 236, 215, 0.08)',
                  borderRadius: '14px',
                  padding: '16px 20px',
                  marginBottom: '24px',
                  display: 'flex',
                  flexWrap: 'wrap',
                  gap: '16px',
                }}
              >
                {order.confirmedAt && (
                  <div>
                    <div style={{ fontSize: '10px', color: '#8B7E72', letterSpacing: '0.1em', textTransform: 'uppercase', marginBottom: '3px' }}>
                      Confirmed
                    </div>
                    <div style={{ fontSize: '12px', color: '#F5ECD7' }}>{formatDate(order.confirmedAt)}</div>
                  </div>
                )}
                {order.completedAt && (
                  <div>
                    <div style={{ fontSize: '10px', color: '#8B7E72', letterSpacing: '0.1em', textTransform: 'uppercase', marginBottom: '3px' }}>
                      Delivered
                    </div>
                    <div style={{ fontSize: '12px', color: '#5ABA5A' }}>{formatDate(order.completedAt)}</div>
                  </div>
                )}
                {order.cancelledAt && (
                  <div>
                    <div style={{ fontSize: '10px', color: '#8B7E72', letterSpacing: '0.1em', textTransform: 'uppercase', marginBottom: '3px' }}>
                      Cancelled
                    </div>
                    <div style={{ fontSize: '12px', color: '#C44536' }}>{formatDate(order.cancelledAt)}</div>
                  </div>
                )}
              </div>
            )}

            {/* Items */}
            <div
              style={{
                background: 'rgba(245, 236, 215, 0.03)',
                border: '1px solid rgba(245, 236, 215, 0.08)',
                borderRadius: '16px',
                overflow: 'hidden',
                marginBottom: '24px',
              }}
            >
              <div
                style={{
                  padding: '14px 20px',
                  borderBottom: '1px solid rgba(245, 236, 215, 0.06)',
                  fontSize: '11px',
                  color: '#8B7E72',
                  letterSpacing: '0.12em',
                  textTransform: 'uppercase',
                  display: 'grid',
                  gridTemplateColumns: '1fr auto',
                  gap: '16px',
                }}
              >
                <span>Item</span>
                <span>Subtotal</span>
              </div>

              {order.items.map((item, idx) => (
                <div
                  key={item.id}
                  style={{
                    padding: '18px 20px',
                    borderBottom:
                      idx < order.items.length - 1
                        ? '1px solid rgba(245, 236, 215, 0.06)'
                        : 'none',
                    display: 'grid',
                    gridTemplateColumns: '1fr auto',
                    gap: '16px',
                    alignItems: 'start',
                  }}
                >
                  <div>
                    <div style={{ fontSize: '15px', fontWeight: 600, color: '#F5ECD7', marginBottom: '4px' }}>
                      {item.pizzaNameAtOrder}
                    </div>
                    <div style={{ fontSize: '12px', color: '#8B7E72', lineHeight: 1.6 }}>
                      {item.pizzaSizeAtOrder} · Qty {item.quantity} · ${item.pizzaBasePriceAtOrder.toFixed(2)} base
                    </div>
                    {item.toppings.length > 0 && (
                      <div style={{ marginTop: '6px', display: 'flex', flexWrap: 'wrap', gap: '5px' }}>
                        {item.toppings.map((t) => (
                          <span
                            key={t.id}
                            style={{
                              fontSize: '11px',
                              color: '#D4A44C',
                              background: 'rgba(212, 164, 76, 0.08)',
                              border: '1px solid rgba(212, 164, 76, 0.2)',
                              borderRadius: '12px',
                              padding: '2px 8px',
                            }}
                          >
                            {t.toppingNameAtOrder} +${t.toppingPriceAtOrder.toFixed(2)}
                          </span>
                        ))}
                      </div>
                    )}
                    {item.specialInstructions && (
                      <div style={{ marginTop: '6px', fontSize: '12px', color: '#8B7E72', fontStyle: 'italic' }}>
                        "{item.specialInstructions}"
                      </div>
                    )}
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
                    ${item.subtotalAtOrder.toFixed(2)}
                  </div>
                </div>
              ))}

              <div
                style={{
                  padding: '16px 20px',
                  borderTop: '1px solid rgba(245, 236, 215, 0.08)',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  background: 'rgba(245, 236, 215, 0.02)',
                }}
              >
                <span style={{ fontSize: '15px', fontWeight: 600, color: '#F5ECD7' }}>Total</span>
                <span
                  style={{
                    fontFamily: '"Bodoni Moda", Georgia, serif',
                    fontStyle: 'italic',
                    fontSize: '26px',
                    color: '#F5ECD7',
                    lineHeight: 1,
                  }}
                >
                  ${order.totalPrice.toFixed(2)}
                </span>
              </div>
            </div>

            {/* Cancel button */}
            {canCancel && (
              <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
                <button
                  onClick={() => setShowCancelModal(true)}
                  style={{
                    background: 'transparent',
                    color: '#C44536',
                    border: '1px solid rgba(196, 69, 54, 0.3)',
                    borderRadius: '10px',
                    padding: '11px 22px',
                    fontSize: '13px',
                    fontWeight: 500,
                    cursor: 'pointer',
                    letterSpacing: '0.04em',
                    transition: 'all 0.2s',
                  }}
                  onMouseEnter={(e) => {
                    const el = e.currentTarget as HTMLElement
                    el.style.background = 'rgba(196, 69, 54, 0.1)'
                    el.style.borderColor = 'rgba(196, 69, 54, 0.5)'
                  }}
                  onMouseLeave={(e) => {
                    const el = e.currentTarget as HTMLElement
                    el.style.background = 'transparent'
                    el.style.borderColor = 'rgba(196, 69, 54, 0.3)'
                  }}
                >
                  Cancel Order
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  )
}
