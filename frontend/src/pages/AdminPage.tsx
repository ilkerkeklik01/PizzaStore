import { useState, useEffect, useCallback } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '@/hooks/useAuth'
import Navbar from '@/components/Navbar'
import type { Order, OrderStatus, PagedResult } from '@/types/order'
import type { AdminUser } from '@/types/admin'
import { getAllUsers, getOrdersByUserId, getAllOrders, updateOrderStatus } from '@/api/admin.api'
import { Users, ClipboardList, X, ChevronLeft, ChevronRight, Shield } from 'lucide-react'

// ─── Status metadata ─────────────────────────────────────────────────────────

const STATUS_COLORS: Record<OrderStatus, { bg: string; text: string; dot: string }> = {
  Pending:        { bg: 'rgba(212, 164, 76, 0.12)',  text: '#D4A44C', dot: '#D4A44C' },
  Confirmed:      { bg: 'rgba(100, 160, 220, 0.12)', text: '#7EACD8', dot: '#7EACD8' },
  Preparing:      { bg: 'rgba(196, 69, 54, 0.12)',   text: '#C44536', dot: '#C44536' },
  OutForDelivery: { bg: 'rgba(130, 100, 220, 0.12)', text: '#A87EE0', dot: '#A87EE0' },
  Delivered:      { bg: 'rgba(90, 186, 90, 0.12)',   text: '#5ABA5A', dot: '#5ABA5A' },
  Cancelled:      { bg: 'rgba(139, 126, 114, 0.12)', text: '#8B7E72', dot: '#8B7E72' },
}

const STATUS_LABEL: Record<OrderStatus, string> = {
  Pending: 'Pending', Confirmed: 'Confirmed', Preparing: 'Preparing',
  OutForDelivery: 'Out for Delivery', Delivered: 'Delivered', Cancelled: 'Cancelled',
}

const ALL_STATUSES: OrderStatus[] = ['Pending', 'Confirmed', 'Preparing', 'OutForDelivery', 'Delivered', 'Cancelled']

// ─── Shared styles ────────────────────────────────────────────────────────────

const inputStyle: React.CSSProperties = {
  background: '#252320', color: '#F5ECD7',
  border: '1px solid rgba(245, 236, 215, 0.12)', borderRadius: '8px',
  padding: '8px 12px', fontSize: '13px', fontFamily: '"DM Sans", sans-serif',
  outline: 'none', cursor: 'pointer', colorScheme: 'dark' as React.CSSProperties['colorScheme'], width: '100%',
}

const labelStyle: React.CSSProperties = {
  fontSize: '10px', fontWeight: 600, letterSpacing: '0.1em',
  textTransform: 'uppercase', color: '#8B7E72', fontFamily: '"DM Sans", sans-serif',
  marginBottom: '5px', display: 'block',
}

// ─── Filter state ─────────────────────────────────────────────────────────────

interface OrderFilterState {
  status: OrderStatus | ''
  userId: string
  fromDate: string
  toDate: string
  page: number
  pageSize: number
}

const DEFAULT_ORDER_FILTERS: OrderFilterState = {
  status: '', userId: '', fromDate: '', toDate: '', page: 1, pageSize: 10,
}

// ─── Toast ────────────────────────────────────────────────────────────────────

function Toast({ message, type, onDismiss }: { message: string; type: 'success' | 'error'; onDismiss: () => void }) {
  useEffect(() => {
    const t = setTimeout(onDismiss, 3500)
    return () => clearTimeout(t)
  }, [onDismiss])

  return (
    <div
      style={{
        position: 'fixed', bottom: '24px', right: '24px', zIndex: 400,
        background: type === 'success' ? 'rgba(90, 186, 90, 0.12)' : 'rgba(196, 69, 54, 0.12)',
        border: `1px solid ${type === 'success' ? 'rgba(90,186,90,0.3)' : 'rgba(196,69,54,0.3)'}`,
        borderRadius: '12px', padding: '14px 20px',
        display: 'flex', alignItems: 'center', gap: '12px',
        backdropFilter: 'blur(8px)',
        boxShadow: '0 8px 32px rgba(0,0,0,0.4)',
        animation: 'slideUp 0.25s ease both',
        fontFamily: '"DM Sans", sans-serif',
        minWidth: '260px',
      }}
    >
      <span
        style={{
          width: '8px', height: '8px', borderRadius: '50%', flexShrink: 0,
          background: type === 'success' ? '#5ABA5A' : '#C44536',
        }}
      />
      <span style={{ fontSize: '13px', color: '#F5ECD7', flex: 1 }}>{message}</span>
      <button
        onClick={onDismiss}
        style={{
          background: 'none', border: 'none', cursor: 'pointer',
          color: '#8B7E72', padding: '2px', display: 'flex', alignItems: 'center',
          transition: 'color 0.2s',
        }}
        onMouseEnter={(e) => ((e.currentTarget as HTMLElement).style.color = '#F5ECD7')}
        onMouseLeave={(e) => ((e.currentTarget as HTMLElement).style.color = '#8B7E72')}
      >
        <X size={14} />
      </button>
    </div>
  )
}

// ─── StatusBadge ─────────────────────────────────────────────────────────────

function StatusBadge({ status }: { status: OrderStatus }) {
  const s = STATUS_COLORS[status] ?? STATUS_COLORS.Pending
  return (
    <span style={{
      display: 'inline-flex', alignItems: 'center', gap: '6px',
      background: s.bg, color: s.text, fontSize: '11px', fontWeight: 700,
      letterSpacing: '0.1em', textTransform: 'uppercase',
      padding: '4px 10px', borderRadius: '20px', whiteSpace: 'nowrap',
    }}>
      <span style={{ width: '5px', height: '5px', borderRadius: '50%', background: s.dot, flexShrink: 0 }} />
      {STATUS_LABEL[status]}
    </span>
  )
}

// ─── User Detail Drawer ───────────────────────────────────────────────────────

function UserDetailDrawer({ isOpen, onClose, user }: { isOpen: boolean; onClose: () => void; user: AdminUser | null }) {
  const { data: userOrders, isLoading: ordersLoading } = useQuery({
    queryKey: ['admin', 'user-orders', user?.id],
    queryFn: () => getOrdersByUserId(user!.id),
    enabled: isOpen && !!user,
  })

  return (
    <>
      {/* Backdrop */}
      <div
        onClick={onClose}
        style={{
          position: 'fixed', inset: 0,
          background: 'rgba(0,0,0,0.55)', zIndex: 200,
          opacity: isOpen ? 1 : 0,
          pointerEvents: isOpen ? 'all' : 'none',
          transition: 'opacity 0.3s ease',
          backdropFilter: isOpen ? 'blur(2px)' : 'none',
        }}
      />

      {/* Panel */}
      <div style={{
        position: 'fixed', top: 0, right: 0, bottom: 0,
        width: '480px', maxWidth: '92vw',
        background: '#1A1815',
        borderLeft: '1px solid rgba(245, 236, 215, 0.08)',
        zIndex: 201,
        transform: isOpen ? 'translateX(0)' : 'translateX(100%)',
        transition: 'transform 0.38s cubic-bezier(0.32, 0, 0.24, 1)',
        display: 'flex', flexDirection: 'column',
      }}>
        {/* Header */}
        <div style={{
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          padding: '20px 24px',
          borderBottom: '1px solid rgba(245, 236, 215, 0.08)',
          flexShrink: 0,
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Shield size={17} color="#C44536" />
            <span style={{
              fontFamily: '"Bodoni Moda", Georgia, serif', fontStyle: 'italic',
              fontSize: '19px', color: '#F5ECD7',
            }}>
              User Details
            </span>
          </div>
          <button
            onClick={onClose}
            style={{
              background: 'rgba(245, 236, 215, 0.06)',
              border: '1px solid rgba(245, 236, 215, 0.1)',
              borderRadius: '8px', cursor: 'pointer', color: '#8B7E72',
              padding: '6px', display: 'flex', alignItems: 'center',
              transition: 'color 0.2s, background 0.2s',
            }}
            onMouseEnter={(e) => {
              const el = e.currentTarget as HTMLElement
              el.style.color = '#F5ECD7'; el.style.background = 'rgba(245,236,215,0.1)'
            }}
            onMouseLeave={(e) => {
              const el = e.currentTarget as HTMLElement
              el.style.color = '#8B7E72'; el.style.background = 'rgba(245,236,215,0.06)'
            }}
          >
            <X size={16} />
          </button>
        </div>

        {/* Body */}
        <div style={{ flex: 1, overflowY: 'auto', padding: '20px 24px' }}>
          {!user ? null : (
            <>
              {/* User info card */}
              <div style={{
                background: 'rgba(245,236,215,0.03)',
                border: '1px solid rgba(245,236,215,0.07)',
                borderRadius: '12px', padding: '20px', marginBottom: '24px',
              }}>
                <div style={{ marginBottom: '16px' }}>
                  <span style={labelStyle}>Username</span>
                  <span style={{
                    fontFamily: '"Bodoni Moda", Georgia, serif', fontStyle: 'italic',
                    fontSize: '22px', color: '#F5ECD7', display: 'block',
                  }}>
                    {user.userName}
                  </span>
                </div>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
                  <div>
                    <span style={labelStyle}>Email</span>
                    <span style={{ fontSize: '13px', color: '#F5ECD7', wordBreak: 'break-all' }}>{user.email}</span>
                  </div>
                  <div>
                    <span style={labelStyle}>Phone</span>
                    <span style={{ fontSize: '13px', color: user.phoneNumber ? '#F5ECD7' : '#8B7E72' }}>
                      {user.phoneNumber || '—'}
                    </span>
                  </div>
                </div>
                <div>
                  <span style={labelStyle}>Roles</span>
                  <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px' }}>
                    {user.roles.map((role) => (
                      <span key={role} style={{
                        background: 'rgba(196,69,54,0.12)', color: '#C44536',
                        fontSize: '11px', fontWeight: 600, padding: '3px 10px', borderRadius: '12px',
                        letterSpacing: '0.06em', textTransform: 'uppercase',
                      }}>
                        {role}
                      </span>
                    ))}
                  </div>
                </div>
                <div style={{ marginTop: '14px', paddingTop: '14px', borderTop: '1px solid rgba(245,236,215,0.06)' }}>
                  <span style={labelStyle}>User ID</span>
                  <span style={{
                    fontSize: '11px', color: '#8B7E72', fontFamily: 'monospace',
                    letterSpacing: '0.02em',
                  }}>
                    {user.id}
                  </span>
                </div>
              </div>

              {/* Orders section */}
              <div>
                <p style={{
                  fontSize: '11px', letterSpacing: '0.28em', fontWeight: 700,
                  color: '#C44536', textTransform: 'uppercase', marginBottom: '14px',
                  fontFamily: '"DM Sans", sans-serif',
                }}>
                  Order History
                </p>

                {ordersLoading ? (
                  <div style={{ display: 'flex', justifyContent: 'center', paddingTop: '40px' }}>
                    <div className="spin-loader" style={{
                      width: '28px', height: '28px',
                      border: '2px solid rgba(245,236,215,0.1)',
                      borderTopColor: '#C44536', borderRadius: '50%',
                    }} />
                  </div>
                ) : !userOrders || userOrders.length === 0 ? (
                  <div style={{ textAlign: 'center', padding: '40px 0' }}>
                    <ClipboardList size={28} color="#8B7E72" style={{ margin: '0 auto 12px', display: 'block' }} />
                    <p style={{ fontSize: '13px', color: '#8B7E72' }}>No orders placed</p>
                  </div>
                ) : (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                    {userOrders.map((order) => (
                      <div key={order.id} style={{
                        background: '#1E1C19',
                        border: '1px solid rgba(245,236,215,0.07)',
                        borderRadius: '10px', padding: '14px 16px',
                        display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '12px',
                      }}>
                        <div style={{ flex: 1, minWidth: 0 }}>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '5px', flexWrap: 'wrap' }}>
                            <StatusBadge status={order.status} />
                          </div>
                          <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
                            <span style={{ fontSize: '11px', color: '#8B7E72', fontFamily: 'monospace' }}>
                              #{order.id.slice(0, 8).toUpperCase()}
                            </span>
                            <span style={{ fontSize: '11px', color: '#8B7E72' }}>
                              {new Date(order.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
                            </span>
                          </div>
                        </div>
                        <div style={{
                          fontFamily: '"Bodoni Moda", Georgia, serif', fontStyle: 'italic',
                          fontSize: '17px', color: '#F5ECD7', flexShrink: 0,
                        }}>
                          ${order.totalPrice.toFixed(2)}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </>
          )}
        </div>
      </div>
    </>
  )
}

// ─── Users Panel ──────────────────────────────────────────────────────────────

function UsersPanel({ onSelectUser }: { onSelectUser: (user: AdminUser) => void }) {
  const [userSearch, setUserSearch] = useState('')
  const { data: users = [], isLoading } = useQuery({
    queryKey: ['admin', 'users'],
    queryFn: getAllUsers,
  })

  const s = userSearch.toLowerCase().trim()
  const filteredUsers = s
    ? users.filter((u) => u.userName.toLowerCase().includes(s) || u.email.toLowerCase().includes(s))
    : users

  return (
    <div>
      {/* Search bar */}
      <div style={{ marginBottom: '20px' }}>
        <div style={{ maxWidth: '360px' }}>
          <label style={labelStyle}>Search users</label>
          <input
            type="text"
            placeholder="Username or email…"
            value={userSearch}
            onChange={(e) => setUserSearch(e.target.value)}
            style={{ ...inputStyle, cursor: 'text' }}
          />
        </div>
      </div>

      {isLoading ? (
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '16px', padding: '60px 0' }}>
          <div className="spin-loader" style={{
            width: '32px', height: '32px',
            border: '2px solid rgba(245,236,215,0.1)',
            borderTopColor: '#C44536', borderRadius: '50%',
          }} />
          <span style={{ fontSize: '12px', color: '#8B7E72', letterSpacing: '0.1em' }}>Loading users…</span>
        </div>
      ) : filteredUsers.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '80px 0' }}>
          <div style={{
            width: '64px', height: '64px', borderRadius: '50%',
            background: 'rgba(245,236,215,0.04)',
            border: '1px solid rgba(245,236,215,0.1)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            margin: '0 auto 24px',
          }}>
            <Users size={28} color="#8B7E72" />
          </div>
          <p style={{ fontSize: '16px', color: '#F5ECD7', marginBottom: '8px' }}>
            {userSearch ? 'No users match your search' : 'No users found'}
          </p>
          {userSearch && (
            <button
              onClick={() => setUserSearch('')}
              style={{
                marginTop: '16px', background: '#C44536', color: '#F5ECD7',
                border: 'none', borderRadius: '32px', padding: '10px 24px',
                fontSize: '13px', cursor: 'pointer', fontWeight: 500,
                transition: 'background 0.2s',
              }}
              onMouseEnter={(e) => ((e.currentTarget as HTMLElement).style.background = '#A8352A')}
              onMouseLeave={(e) => ((e.currentTarget as HTMLElement).style.background = '#C44536')}
            >
              Clear search
            </button>
          )}
        </div>
      ) : (
        <div style={{
          background: '#1E1C19',
          border: '1px solid rgba(245,236,215,0.07)',
          borderRadius: '14px', overflow: 'hidden',
        }}>
          {/* Header row */}
          <div style={{
            display: 'grid', gridTemplateColumns: '2fr 2fr 1fr 1fr',
            background: 'rgba(245,236,215,0.03)',
            borderBottom: '1px solid rgba(245,236,215,0.07)',
            padding: '10px 20px',
            fontSize: '10px', letterSpacing: '0.12em',
            textTransform: 'uppercase', color: '#8B7E72',
            fontFamily: '"DM Sans", sans-serif', fontWeight: 600,
          }}>
            <span>Username</span>
            <span>Email</span>
            <span>Phone</span>
            <span>Roles</span>
          </div>

          {/* Data rows */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '4px', padding: '8px' }}>
            {filteredUsers.map((user, i) => (
              <div
                key={user.id}
                onClick={() => onSelectUser(user)}
                style={{
                  display: 'grid', gridTemplateColumns: '2fr 2fr 1fr 1fr',
                  background: 'rgba(245,236,215,0.02)',
                  border: '1px solid rgba(245,236,215,0.06)',
                  borderRadius: '10px', padding: '14px 20px',
                  cursor: 'pointer', alignItems: 'center',
                  transition: 'border-color 0.25s, transform 0.25s, box-shadow 0.25s',
                  animation: `fadeIn 0.3s ease both`,
                  animationDelay: `${i * 0.04}s`,
                  opacity: 0,
                }}
                onMouseEnter={(e) => {
                  const el = e.currentTarget as HTMLElement
                  el.style.borderColor = 'rgba(196,69,54,0.25)'
                  el.style.transform = 'translateY(-1px)'
                  el.style.boxShadow = '0 8px 24px rgba(0,0,0,0.3)'
                }}
                onMouseLeave={(e) => {
                  const el = e.currentTarget as HTMLElement
                  el.style.borderColor = 'rgba(245,236,215,0.06)'
                  el.style.transform = 'none'
                  el.style.boxShadow = 'none'
                }}
              >
                <div style={{ minWidth: 0 }}>
                  <span style={{
                    fontSize: '14px', fontWeight: 600, color: '#F5ECD7',
                    overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
                    display: 'block',
                  }}>
                    {user.userName}
                  </span>
                </div>
                <div style={{ minWidth: 0 }}>
                  <span style={{
                    fontSize: '13px', color: '#8B7E72',
                    overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
                    display: 'block',
                  }}>
                    {user.email}
                  </span>
                </div>
                <div>
                  <span style={{ fontSize: '13px', color: user.phoneNumber ? '#F5ECD7' : '#8B7E72' }}>
                    {user.phoneNumber || '—'}
                  </span>
                </div>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '4px' }}>
                  {user.roles.map((role) => (
                    <span key={role} style={{
                      background: 'rgba(196,69,54,0.12)', color: '#C44536',
                      fontSize: '10px', fontWeight: 700, padding: '2px 8px',
                      borderRadius: '12px', letterSpacing: '0.05em',
                      textTransform: 'uppercase',
                    }}>
                      {role}
                    </span>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {!isLoading && users.length > 0 && (
        <p style={{ fontSize: '12px', color: '#8B7E72', marginTop: '12px', fontFamily: '"DM Sans", sans-serif' }}>
          {filteredUsers.length} of {users.length} user{users.length !== 1 ? 's' : ''}
          {userSearch && ' matching search'}
        </p>
      )}
    </div>
  )
}

// ─── Orders Panel ─────────────────────────────────────────────────────────────

function OrdersPanel({ showToast }: { showToast: (msg: string, type: 'success' | 'error') => void }) {
  const queryClient = useQueryClient()
  const [orderFilters, setOrderFilters] = useState<OrderFilterState>(DEFAULT_ORDER_FILTERS)
  const [pendingOrderId, setPendingOrderId] = useState<string | null>(null)

  const hasActiveFilters = !!(orderFilters.status || orderFilters.userId || orderFilters.fromDate || orderFilters.toDate)

  const { data, isLoading, isFetching } = useQuery({
    queryKey: ['admin', 'orders', orderFilters],
    queryFn: () => getAllOrders({
      status: orderFilters.status || undefined,
      userId: orderFilters.userId || undefined,
      fromDate: orderFilters.fromDate || undefined,
      toDate: orderFilters.toDate || undefined,
      page: orderFilters.page,
      pageSize: orderFilters.pageSize,
    }),
  })

  const orders = data?.items ?? []
  const totalCount = data?.totalCount ?? 0
  const totalPages = data?.totalPages ?? 1
  const rangeStart = totalCount === 0 ? 0 : (orderFilters.page - 1) * orderFilters.pageSize + 1
  const rangeEnd = Math.min(orderFilters.page * orderFilters.pageSize, totalCount)

  const { mutate: changeStatus } = useMutation({
    mutationFn: ({ orderId, newStatus }: { orderId: string; newStatus: OrderStatus }) =>
      updateOrderStatus(orderId, newStatus),
    onMutate: ({ orderId }) => setPendingOrderId(orderId),
    onSuccess: (updatedOrder) => {
      setPendingOrderId(null)
      queryClient.setQueryData(
        ['admin', 'orders', orderFilters],
        (old: PagedResult<Order> | undefined) =>
          old ? { ...old, items: old.items.map((o) => (o.id === updatedOrder.id ? updatedOrder : o)) } : old,
      )
      showToast('Order status updated', 'success')
    },
    onError: () => {
      setPendingOrderId(null)
      showToast('Failed to update order status', 'error')
    },
  })

  const updateOrderFilter = (update: Partial<Omit<OrderFilterState, 'page' | 'pageSize'>>) => {
    setOrderFilters((prev) => ({ ...prev, ...update, page: 1 }))
  }

  const handleReset = () => setOrderFilters(DEFAULT_ORDER_FILTERS)
  const handlePageChange = (page: number) => setOrderFilters((prev) => ({ ...prev, page }))

  return (
    <div>
      {/* Filter bar */}
      <div style={{
        background: '#1E1C19',
        border: '1px solid rgba(245,236,215,0.07)',
        borderRadius: '12px', padding: '20px 22px', marginBottom: '24px',
      }}>
        <div style={{ display: 'flex', alignItems: 'flex-end', flexWrap: 'wrap', gap: '12px' }}>
          {/* Status */}
          <div style={{ display: 'flex', flexDirection: 'column', minWidth: '150px' }}>
            <label style={labelStyle}>Status</label>
            <select
              value={orderFilters.status}
              onChange={(e) => updateOrderFilter({ status: e.target.value as OrderStatus | '' })}
              style={inputStyle}
            >
              <option value="">All Statuses</option>
              {ALL_STATUSES.map((s) => (
                <option key={s} value={s}>{STATUS_LABEL[s]}</option>
              ))}
            </select>
          </div>

          {/* User ID */}
          <div style={{ display: 'flex', flexDirection: 'column', minWidth: '200px' }}>
            <label style={labelStyle}>User ID</label>
            <input
              type="text"
              placeholder="Filter by user ID…"
              value={orderFilters.userId}
              onChange={(e) => updateOrderFilter({ userId: e.target.value })}
              style={{ ...inputStyle, cursor: 'text' }}
            />
          </div>

          {/* From date */}
          <div style={{ display: 'flex', flexDirection: 'column', minWidth: '148px' }}>
            <label style={labelStyle}>From date</label>
            <input
              type="date"
              value={orderFilters.fromDate}
              onChange={(e) => updateOrderFilter({ fromDate: e.target.value })}
              style={inputStyle}
            />
          </div>

          {/* To date */}
          <div style={{ display: 'flex', flexDirection: 'column', minWidth: '148px' }}>
            <label style={labelStyle}>To date</label>
            <input
              type="date"
              value={orderFilters.toDate}
              onChange={(e) => updateOrderFilter({ toDate: e.target.value })}
              style={inputStyle}
            />
          </div>

          {hasActiveFilters && (
            <button
              onClick={handleReset}
              style={{
                background: 'none', border: '1px solid rgba(245,236,215,0.12)',
                borderRadius: '8px', color: '#8B7E72', fontSize: '13px',
                fontFamily: '"DM Sans", sans-serif', padding: '8px 14px',
                cursor: 'pointer', alignSelf: 'flex-end',
                transition: 'color 0.2s, border-color 0.2s', whiteSpace: 'nowrap',
              }}
              onMouseEnter={(e) => {
                const el = e.currentTarget as HTMLElement
                el.style.color = '#F5ECD7'; el.style.borderColor = 'rgba(245,236,215,0.3)'
              }}
              onMouseLeave={(e) => {
                const el = e.currentTarget as HTMLElement
                el.style.color = '#8B7E72'; el.style.borderColor = 'rgba(245,236,215,0.12)'
              }}
            >
              Clear filters
            </button>
          )}
        </div>
      </div>

      {/* Table */}
      {isLoading ? (
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '16px', padding: '60px 0' }}>
          <div className="spin-loader" style={{
            width: '32px', height: '32px',
            border: '2px solid rgba(245,236,215,0.1)',
            borderTopColor: '#C44536', borderRadius: '50%',
          }} />
          <span style={{ fontSize: '12px', color: '#8B7E72', letterSpacing: '0.1em' }}>Loading orders…</span>
        </div>
      ) : orders.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '80px 0' }}>
          <div style={{
            width: '64px', height: '64px', borderRadius: '50%',
            background: 'rgba(245,236,215,0.04)',
            border: '1px solid rgba(245,236,215,0.1)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            margin: '0 auto 24px',
          }}>
            <ClipboardList size={28} color="#8B7E72" />
          </div>
          <p style={{ fontSize: '16px', color: '#F5ECD7', marginBottom: '8px' }}>
            {hasActiveFilters ? 'No orders match your filters' : 'No orders found'}
          </p>
          {hasActiveFilters && (
            <button
              onClick={handleReset}
              style={{
                marginTop: '16px', background: '#C44536', color: '#F5ECD7',
                border: 'none', borderRadius: '32px', padding: '12px 28px',
                fontSize: '14px', cursor: 'pointer', fontWeight: 500,
                transition: 'background 0.2s',
              }}
              onMouseEnter={(e) => ((e.currentTarget as HTMLElement).style.background = '#A8352A')}
              onMouseLeave={(e) => ((e.currentTarget as HTMLElement).style.background = '#C44536')}
            >
              Clear Filters
            </button>
          )}
        </div>
      ) : (
        <>
          <div style={{
            background: '#1E1C19',
            border: '1px solid rgba(245,236,215,0.07)',
            borderRadius: '14px', overflow: 'hidden',
            opacity: isFetching && !isLoading ? 0.55 : 1,
            transition: 'opacity 0.15s',
          }}>
            {/* Header row */}
            <div style={{
              display: 'grid', gridTemplateColumns: '2fr 2fr 1.2fr 0.8fr 1.5fr',
              background: 'rgba(245,236,215,0.03)',
              borderBottom: '1px solid rgba(245,236,215,0.07)',
              padding: '10px 20px',
              fontSize: '10px', letterSpacing: '0.12em',
              textTransform: 'uppercase', color: '#8B7E72',
              fontFamily: '"DM Sans", sans-serif', fontWeight: 600,
            }}>
              <span>Order ID</span>
              <span>User</span>
              <span>Date</span>
              <span>Total</span>
              <span>Status</span>
            </div>

            {/* Data rows */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '4px', padding: '8px' }}>
              {orders.map((order, i) => {
                const isFinal = order.status === 'Delivered' || order.status === 'Cancelled'
                const isUpdating = pendingOrderId === order.id
                return (
                  <div
                    key={order.id}
                    style={{
                      display: 'grid', gridTemplateColumns: '2fr 2fr 1.2fr 0.8fr 1.5fr',
                      background: 'rgba(245,236,215,0.02)',
                      border: '1px solid rgba(245,236,215,0.06)',
                      borderRadius: '10px', padding: '14px 20px',
                      alignItems: 'center', gap: '8px',
                      transition: 'border-color 0.2s',
                      animation: 'fadeIn 0.3s ease both',
                      animationDelay: `${i * 0.03}s`,
                      opacity: 0,
                    }}
                    onMouseEnter={(e) => {
                      (e.currentTarget as HTMLElement).style.borderColor = 'rgba(196,69,54,0.2)'
                    }}
                    onMouseLeave={(e) => {
                      (e.currentTarget as HTMLElement).style.borderColor = 'rgba(245,236,215,0.06)'
                    }}
                  >
                    {/* Order ID */}
                    <div style={{ minWidth: 0 }}>
                      <span style={{ fontSize: '12px', color: '#8B7E72', fontFamily: 'monospace', letterSpacing: '0.02em' }}>
                        #{order.id.slice(0, 8).toUpperCase()}
                      </span>
                    </div>

                    {/* User */}
                    <div style={{ minWidth: 0 }}>
                      <span style={{
                        fontSize: '12px', color: '#8B7E72', fontFamily: 'monospace',
                        overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', display: 'block',
                      }}>
                        {order.userId.slice(0, 16)}…
                      </span>
                    </div>

                    {/* Date */}
                    <div>
                      <span style={{ fontSize: '12px', color: '#8B7E72' }}>
                        {new Date(order.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
                      </span>
                    </div>

                    {/* Total */}
                    <div>
                      <span style={{
                        fontFamily: '"Bodoni Moda", Georgia, serif', fontStyle: 'italic',
                        fontSize: '16px', color: '#F5ECD7',
                      }}>
                        ${order.totalPrice.toFixed(2)}
                      </span>
                    </div>

                    {/* Status */}
                    <div style={{ display: 'flex', alignItems: 'center' }}>
                      {isUpdating ? (
                        <div className="spin-loader" style={{
                          width: '20px', height: '20px',
                          border: '2px solid rgba(245,236,215,0.1)',
                          borderTopColor: '#C44536', borderRadius: '50%',
                        }} />
                      ) : (
                        <select
                          value={order.status}
                          disabled={isFinal}
                          onChange={(e) => changeStatus({ orderId: order.id, newStatus: e.target.value as OrderStatus })}
                          style={{
                            ...inputStyle,
                            width: 'auto', minWidth: '140px',
                            opacity: isFinal ? 0.5 : 1,
                            cursor: isFinal ? 'not-allowed' : 'pointer',
                            fontSize: '12px', padding: '6px 10px',
                          }}
                        >
                          {ALL_STATUSES.map((s) => (
                            <option key={s} value={s}>{STATUS_LABEL[s]}</option>
                          ))}
                        </select>
                      )}
                    </div>
                  </div>
                )
              })}
            </div>
          </div>

          {/* Pagination */}
          <div style={{
            display: 'flex', justifyContent: 'space-between', alignItems: 'center',
            marginTop: '20px', padding: '16px 0',
            borderTop: '1px solid rgba(245,236,215,0.07)',
            flexWrap: 'wrap', gap: '12px',
          }}>
            <span style={{ fontSize: '12px', color: '#8B7E72', fontFamily: '"DM Sans", sans-serif' }}>
              Showing {rangeStart}–{rangeEnd} of {totalCount} order{totalCount !== 1 ? 's' : ''}
            </span>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <button
                onClick={() => handlePageChange(orderFilters.page - 1)}
                disabled={orderFilters.page === 1}
                style={{
                  background: 'transparent',
                  border: `1px solid ${orderFilters.page === 1 ? 'rgba(245,236,215,0.1)' : '#C44536'}`,
                  borderRadius: '8px', padding: '7px', cursor: orderFilters.page === 1 ? 'default' : 'pointer',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  opacity: orderFilters.page === 1 ? 0.35 : 1,
                  transition: 'opacity 0.2s, border-color 0.2s',
                }}
              >
                <ChevronLeft size={15} color={orderFilters.page === 1 ? '#8B7E72' : '#C44536'} />
              </button>

              <span style={{
                fontSize: '13px', color: '#8B7E72',
                minWidth: '100px', textAlign: 'center',
                fontFamily: '"DM Sans", sans-serif',
              }}>
                Page {orderFilters.page} of {totalPages}
              </span>

              <button
                onClick={() => handlePageChange(orderFilters.page + 1)}
                disabled={orderFilters.page >= totalPages}
                style={{
                  background: 'transparent',
                  border: `1px solid ${orderFilters.page >= totalPages ? 'rgba(245,236,215,0.1)' : '#C44536'}`,
                  borderRadius: '8px', padding: '7px', cursor: orderFilters.page >= totalPages ? 'default' : 'pointer',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  opacity: orderFilters.page >= totalPages ? 0.35 : 1,
                  transition: 'opacity 0.2s, border-color 0.2s',
                }}
              >
                <ChevronRight size={15} color={orderFilters.page >= totalPages ? '#8B7E72' : '#C44536'} />
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  )
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function AdminPage() {
  const { user } = useAuth()
  const [activeTab, setActiveTab] = useState<'users' | 'orders'>('users')
  const [userDrawerOpen, setUserDrawerOpen] = useState(false)
  const [selectedUser, setSelectedUser] = useState<AdminUser | null>(null)
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null)

  const showToast = useCallback((message: string, type: 'success' | 'error') => setToast({ message, type }), [])

  const handleSelectUser = (u: AdminUser) => {
    setSelectedUser(u)
    setUserDrawerOpen(true)
  }

  return (
    <div style={{ minHeight: '100vh', background: '#1C1A17' }}>
      <Navbar onCartOpen={() => {}} />

      {toast && (
        <Toast
          message={toast.message}
          type={toast.type}
          onDismiss={() => setToast(null)}
        />
      )}

      <UserDetailDrawer
        isOpen={userDrawerOpen}
        onClose={() => setUserDrawerOpen(false)}
        user={selectedUser}
      />

      <div style={{
        maxWidth: '1200px',
        margin: '0 auto',
        padding: '100px clamp(20px, 5vw, 48px) 80px',
      }}>
        {/* Page header */}
        <div style={{ marginBottom: '36px' }}>
          <p style={{
            fontSize: '11px', letterSpacing: '0.32em', fontWeight: 700,
            color: '#C44536', textTransform: 'uppercase', marginBottom: '12px',
            fontFamily: '"DM Sans", sans-serif',
          }}>
            Napoletana — Admin
          </p>
          <h1 style={{
            fontFamily: '"Bodoni Moda", Georgia, serif', fontStyle: 'italic',
            fontSize: 'clamp(32px, 5vw, 52px)', color: '#F5ECD7',
            margin: '0 0 8px 0', lineHeight: 1.1,
          }}>
            Dashboard
          </h1>
          <p style={{ fontSize: '14px', color: '#8B7E72', margin: 0, fontFamily: '"DM Sans", sans-serif' }}>
            Welcome, {user?.firstName}.
          </p>
        </div>

        {/* Tab switcher */}
        <div style={{ display: 'flex', gap: '8px', marginBottom: '28px', flexWrap: 'wrap' }}>
          {([
            { key: 'users', label: 'Users', icon: Users },
            { key: 'orders', label: 'Orders', icon: ClipboardList },
          ] as const).map(({ key, label, icon: Icon }) => {
            const active = activeTab === key
            return (
              <button
                key={key}
                onClick={() => setActiveTab(key)}
                style={{
                  display: 'flex', alignItems: 'center', gap: '7px',
                  padding: '9px 18px', borderRadius: '32px',
                  border: active ? '1px solid #C44536' : '1px solid rgba(245,236,215,0.15)',
                  background: active ? '#C44536' : 'transparent',
                  color: active ? '#F5ECD7' : '#8B7E72',
                  fontSize: '13px', fontWeight: 500,
                  fontFamily: '"DM Sans", sans-serif',
                  cursor: 'pointer',
                  transition: 'background 0.2s, border-color 0.2s, color 0.2s',
                  letterSpacing: '0.02em',
                }}
                onMouseEnter={(e) => {
                  if (!active) {
                    const el = e.currentTarget as HTMLElement
                    el.style.borderColor = 'rgba(245,236,215,0.3)'
                    el.style.color = '#F5ECD7'
                  }
                }}
                onMouseLeave={(e) => {
                  if (!active) {
                    const el = e.currentTarget as HTMLElement
                    el.style.borderColor = 'rgba(245,236,215,0.15)'
                    el.style.color = '#8B7E72'
                  }
                }}
              >
                <Icon size={14} />
                {label}
              </button>
            )
          })}
        </div>

        {/* Tab panels */}
        {activeTab === 'users' ? (
          <UsersPanel onSelectUser={handleSelectUser} />
        ) : (
          <OrdersPanel showToast={showToast} />
        )}
      </div>
    </div>
  )
}
