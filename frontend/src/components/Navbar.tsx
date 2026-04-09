import { useState, useEffect } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { ShoppingCart, LogOut, LayoutDashboard, ChevronDown, User } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { useAuth } from '@/hooks/useAuth'
import { getCart } from '@/api/cart.api'

interface NavbarProps {
  onCartOpen: () => void
}

export default function Navbar({ onCartOpen }: NavbarProps) {
  const { user, isAuthenticated, isAdmin, logout } = useAuth()
  const navigate = useNavigate()
  const [scrolled, setScrolled] = useState(false)
  const [profileOpen, setProfileOpen] = useState(false)

  const { data: cart } = useQuery({
    queryKey: ['cart'],
    queryFn: getCart,
    enabled: isAuthenticated,
    retry: false,
    staleTime: 1000 * 30,
  })

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 24)
    window.addEventListener('scroll', onScroll, { passive: true })
    return () => window.removeEventListener('scroll', onScroll)
  }, [])

  const cartQuantity = cart?.totalQuantity ?? 0

  const handleLogout = () => {
    logout()
    setProfileOpen(false)
    navigate('/login')
  }

  return (
    <nav
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        zIndex: 100,
        height: '64px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '0 clamp(20px, 4vw, 48px)',
        transition: 'background 0.4s ease, box-shadow 0.4s ease',
        background: scrolled ? 'rgba(28, 26, 23, 0.94)' : 'transparent',
        backdropFilter: scrolled ? 'blur(16px)' : 'none',
        WebkitBackdropFilter: scrolled ? 'blur(16px)' : 'none',
        boxShadow: scrolled ? '0 1px 0 rgba(245, 236, 215, 0.06)' : 'none',
      }}
    >
      {/* Logo */}
      <Link to="/" style={{ textDecoration: 'none', display: 'flex', alignItems: 'center', gap: '10px' }}>
        <div
          style={{
            width: '32px',
            height: '32px',
            borderRadius: '50%',
            background: 'linear-gradient(135deg, #C44536 0%, #8A2515 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: '15px',
            flexShrink: 0,
          }}
        >
          🔥
        </div>
        <span
          style={{
            fontFamily: '"Bodoni Moda", Georgia, serif',
            fontStyle: 'italic',
            fontSize: '18px',
            color: '#F5ECD7',
            letterSpacing: '0.04em',
          }}
        >
          Napoletana
        </span>
      </Link>

      {/* Right side */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
        {isAdmin && (
          <Link
            to="/admin"
            style={{
              fontSize: '11px',
              letterSpacing: '0.14em',
              fontWeight: 600,
              color: '#D4A44C',
              textDecoration: 'none',
              textTransform: 'uppercase',
              display: 'flex',
              alignItems: 'center',
              gap: '5px',
              padding: '6px 12px',
              borderRadius: '6px',
              border: '1px solid rgba(212, 164, 76, 0.25)',
              transition: 'background 0.2s',
            }}
            onMouseEnter={(e) => ((e.currentTarget as HTMLElement).style.background = 'rgba(212, 164, 76, 0.1)')}
            onMouseLeave={(e) => ((e.currentTarget as HTMLElement).style.background = 'transparent')}
          >
            <LayoutDashboard size={13} />
            Admin
          </Link>
        )}

        {isAuthenticated && (
          <button
            onClick={onCartOpen}
            aria-label={`Open cart${cartQuantity > 0 ? `, ${cartQuantity} items` : ''}`}
            style={{
              position: 'relative',
              background: 'none',
              border: '1px solid rgba(245, 236, 215, 0.1)',
              borderRadius: '8px',
              cursor: 'pointer',
              color: '#F5ECD7',
              padding: '8px 10px',
              display: 'flex',
              alignItems: 'center',
              transition: 'border-color 0.2s, background 0.2s',
            }}
            onMouseEnter={(e) => {
              const el = e.currentTarget as HTMLElement
              el.style.background = 'rgba(245, 236, 215, 0.06)'
              el.style.borderColor = 'rgba(245, 236, 215, 0.2)'
            }}
            onMouseLeave={(e) => {
              const el = e.currentTarget as HTMLElement
              el.style.background = 'none'
              el.style.borderColor = 'rgba(245, 236, 215, 0.1)'
            }}
          >
            <ShoppingCart size={18} />
            {cartQuantity > 0 && (
              <span
                style={{
                  position: 'absolute',
                  top: '-5px',
                  right: '-5px',
                  background: '#C44536',
                  color: '#F5ECD7',
                  fontSize: '10px',
                  fontWeight: 700,
                  minWidth: '17px',
                  height: '17px',
                  borderRadius: '9px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  padding: '0 3px',
                  lineHeight: 1,
                }}
              >
                {cartQuantity > 9 ? '9+' : cartQuantity}
              </span>
            )}
          </button>
        )}

        {isAuthenticated ? (
          <div style={{ position: 'relative' }}>
            <button
              onClick={() => setProfileOpen((v) => !v)}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '7px',
                background: profileOpen ? 'rgba(245, 236, 215, 0.1)' : 'rgba(245, 236, 215, 0.06)',
                border: '1px solid rgba(245, 236, 215, 0.12)',
                borderRadius: '32px',
                padding: '6px 12px 6px 8px',
                cursor: 'pointer',
                color: '#F5ECD7',
                fontSize: '13px',
                transition: 'background 0.2s',
              }}
            >
              <div
                style={{
                  width: '24px',
                  height: '24px',
                  borderRadius: '50%',
                  background: 'linear-gradient(135deg, #C44536 0%, #8A2515 100%)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '11px',
                  fontWeight: 700,
                  flexShrink: 0,
                }}
              >
                {user?.firstName?.[0]?.toUpperCase() ?? <User size={12} />}
              </div>
              <span style={{ maxWidth: '80px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                {user?.firstName}
              </span>
              <ChevronDown
                size={13}
                style={{
                  opacity: 0.5,
                  transition: 'transform 0.2s',
                  transform: profileOpen ? 'rotate(180deg)' : 'none',
                }}
              />
            </button>

            {profileOpen && (
              <>
                <div
                  onClick={() => setProfileOpen(false)}
                  style={{ position: 'fixed', inset: 0, zIndex: 1 }}
                />
                <div
                  style={{
                    position: 'absolute',
                    top: 'calc(100% + 8px)',
                    right: 0,
                    minWidth: '200px',
                    background: '#242220',
                    border: '1px solid rgba(245, 236, 215, 0.1)',
                    borderRadius: '12px',
                    padding: '8px',
                    zIndex: 2,
                    boxShadow: '0 16px 48px rgba(0,0,0,0.6)',
                    animation: 'slideUp 0.15s ease both',
                  }}
                >
                  <div
                    style={{
                      padding: '8px 12px 12px',
                      borderBottom: '1px solid rgba(245, 236, 215, 0.08)',
                      marginBottom: '4px',
                    }}
                  >
                    <div style={{ fontSize: '11px', color: '#8B7E72', marginBottom: '2px', letterSpacing: '0.05em' }}>
                      Signed in as
                    </div>
                    <div style={{ fontSize: '13px', color: '#F5ECD7', fontWeight: 500 }}>{user?.email}</div>
                    {isAdmin && (
                      <div
                        style={{
                          display: 'inline-block',
                          marginTop: '6px',
                          fontSize: '10px',
                          letterSpacing: '0.12em',
                          fontWeight: 600,
                          textTransform: 'uppercase',
                          color: '#D4A44C',
                          background: 'rgba(212, 164, 76, 0.1)',
                          padding: '2px 8px',
                          borderRadius: '10px',
                        }}
                      >
                        Admin
                      </div>
                    )}
                  </div>
                  <button
                    onClick={handleLogout}
                    style={{
                      width: '100%',
                      display: 'flex',
                      alignItems: 'center',
                      gap: '10px',
                      padding: '9px 12px',
                      background: 'none',
                      border: 'none',
                      cursor: 'pointer',
                      color: '#C44536',
                      fontSize: '13px',
                      borderRadius: '8px',
                      textAlign: 'left',
                      transition: 'background 0.15s',
                    }}
                    onMouseEnter={(e) => ((e.currentTarget as HTMLElement).style.background = 'rgba(196, 69, 54, 0.1)')}
                    onMouseLeave={(e) => ((e.currentTarget as HTMLElement).style.background = 'none')}
                  >
                    <LogOut size={14} />
                    Sign out
                  </button>
                </div>
              </>
            )}
          </div>
        ) : (
          <Link
            to="/login"
            style={{
              background: '#C44536',
              color: '#F5ECD7',
              borderRadius: '32px',
              padding: '8px 22px',
              fontSize: '13px',
              fontWeight: 500,
              cursor: 'pointer',
              textDecoration: 'none',
              letterSpacing: '0.04em',
              transition: 'background 0.2s, transform 0.15s',
              display: 'inline-block',
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
            Sign in
          </Link>
        )}
      </div>
    </nav>
  )
}
