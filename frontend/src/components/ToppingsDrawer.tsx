import { useQuery } from '@tanstack/react-query'
import { X, Layers } from 'lucide-react'
import { getAllToppings } from '@/api/topping.api'
import type { Topping } from '@/types/topping'

interface ToppingsDrawerProps {
  isOpen: boolean
  onClose: () => void
}

export default function ToppingsDrawer({ isOpen, onClose }: ToppingsDrawerProps) {
  const { data: toppings = [], isLoading } = useQuery({
    queryKey: ['toppings'],
    queryFn: getAllToppings,
    staleTime: 1000 * 60 * 5,
  })

  const availableCount = toppings.filter((t: Topping) => t.isAvailable).length

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
          pointerEvents: isOpen ? 'auto' : 'none',
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
          width: '400px',
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
            <Layers size={17} color="#C44536" />
            <span
              style={{
                fontFamily: '"Bodoni Moda", Georgia, serif',
                fontStyle: 'italic',
                fontSize: '19px',
                color: '#F5ECD7',
              }}
            >
              Toppings
            </span>
          </div>
          <button
            type="button"
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

        {/* Body */}
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
          ) : toppings.length === 0 ? (
            <div
              style={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                paddingTop: '80px',
                gap: '14px',
              }}
            >
              <div style={{ fontSize: '52px', opacity: 0.35, lineHeight: 1 }}>🧀</div>
              <p style={{ fontSize: '14px', color: '#8B7E72', textAlign: 'center', margin: 0 }}>
                No toppings available.
              </p>
            </div>
          ) : (
            <div>
              {toppings.map((topping: Topping) => (
                <div
                  key={topping.id}
                  style={{
                    background: 'rgba(245, 236, 215, 0.04)',
                    border: '1px solid rgba(245, 236, 215, 0.08)',
                    borderRadius: '12px',
                    padding: '14px 16px',
                    marginBottom: '8px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                  }}
                >
                  <div style={{ display: 'flex', alignItems: 'center' }}>
                    <span
                      style={{
                        fontSize: '14px',
                        fontWeight: 600,
                        color: topping.isAvailable ? '#F5ECD7' : '#8B7E72',
                        opacity: topping.isAvailable ? 1 : 0.5,
                      }}
                    >
                      {topping.name}
                    </span>
                    {!topping.isAvailable && (
                      <span
                        style={{
                          fontSize: '10px',
                          color: '#8B7E72',
                          background: 'rgba(245, 236, 215, 0.06)',
                          border: '1px solid rgba(245, 236, 215, 0.1)',
                          borderRadius: '4px',
                          padding: '2px 6px',
                          marginLeft: '8px',
                        }}
                      >
                        Unavailable
                      </span>
                    )}
                  </div>
                  <span
                    style={{
                      fontSize: '14px',
                      fontWeight: 700,
                      color: topping.isAvailable ? '#F5ECD7' : '#8B7E72',
                      opacity: topping.isAvailable ? 1 : 0.5,
                    }}
                  >
                    ${topping.price.toFixed(2)}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Footer */}
        <div
          style={{
            borderTop: '1px solid rgba(245, 236, 215, 0.08)',
            padding: '16px 24px',
            flexShrink: 0,
          }}
        >
          <span style={{ fontSize: '12px', color: '#8B7E72', letterSpacing: '0.05em' }}>
            {availableCount} topping{availableCount !== 1 ? 's' : ''} available
          </span>
        </div>
      </div>
    </>
  )
}
