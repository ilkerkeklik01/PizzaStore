import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useAuth } from '@/hooks/useAuth'
import { getMe } from '@/api/auth.api'
import Navbar from '@/components/Navbar'

function getRoleBadgeStyle(role: string) {
  if (role === 'Admin') {
    return { bg: 'rgba(212, 164, 76, 0.12)', text: '#D4A44C', dot: '#D4A44C' }
  }
  return { bg: 'rgba(245, 236, 215, 0.08)', text: '#F5ECD7', dot: '#8B7E72' }
}

function DetailRow({ label, value, mono = false }: { label: string; value?: string; mono?: boolean }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
      <span style={{ fontSize: '10px', fontWeight: 700, letterSpacing: '0.2em', textTransform: 'uppercase', color: '#8B7E72' }}>
        {label}
      </span>
      <span
        style={{
          fontSize: '14px',
          color: '#F5ECD7',
          fontFamily: mono ? 'monospace' : "'DM Sans', system-ui, sans-serif",
          letterSpacing: mono ? '0.04em' : undefined,
          overflow: 'hidden',
          textOverflow: 'ellipsis',
          whiteSpace: 'nowrap',
        }}
      >
        {value ?? '—'}
      </span>
    </div>
  )
}

export default function ProfilePage() {
  const [cartOpen, setCartOpen] = useState(false)
  const { user } = useAuth()
  const { data: me, isLoading, isError } = useQuery({
    queryKey: ['auth', 'me'],
    queryFn: getMe,
    staleTime: 1000 * 60 * 5,
    retry: false,
  })

  const initials =
    (user?.firstName?.[0] ?? '') + (user?.lastName?.[0] ?? '')

  return (
    <div style={{ minHeight: '100vh', background: '#1C1A17', fontFamily: "'DM Sans', system-ui, sans-serif" }}>
      <Navbar onCartOpen={() => setCartOpen(!cartOpen)} />

      <div
        style={{
          maxWidth: '800px',
          margin: '0 auto',
          padding: '100px clamp(20px, 5vw, 48px) 80px',
        }}
      >
        {/* Page header */}
        <div
          style={{
            marginBottom: '48px',
            opacity: 0,
            animation: 'fadeIn 0.4s ease both',
            animationDelay: '0s',
          }}
        >
          <p
            style={{
              fontSize: '11px',
              letterSpacing: '0.32em',
              fontWeight: 700,
              color: '#C44536',
              textTransform: 'uppercase',
              marginBottom: '12px',
              margin: '0 0 12px 0',
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
              margin: '0 0 8px 0',
              lineHeight: 1.1,
            }}
          >
            Profile
          </h1>
        </div>

        {isLoading ? (
          <div
            style={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              gap: '16px',
              padding: '60px 0',
            }}
          >
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
              Loading profile…
            </span>
          </div>
        ) : isError ? (
          <div style={{ textAlign: 'center', padding: '60px 0' }}>
            <p style={{ fontSize: '15px', color: '#8B7E72', marginBottom: '8px' }}>
              Could not load profile data.
            </p>
            <p style={{ fontSize: '13px', color: '#8B7E72', opacity: 0.6 }}>
              Please refresh the page to try again.
            </p>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
            {/* Identity card */}
            <div
              style={{
                background: 'rgba(245, 236, 215, 0.03)',
                border: '1px solid rgba(245, 236, 215, 0.08)',
                borderRadius: '16px',
                padding: '24px',
                display: 'flex',
                alignItems: 'center',
                gap: '20px',
                opacity: 0,
                animation: 'fadeIn 0.4s ease both',
                animationDelay: '0.1s',
              }}
            >
              <div
                style={{
                  width: '72px',
                  height: '72px',
                  borderRadius: '50%',
                  background: 'linear-gradient(135deg, #C44536 0%, #8A2515 100%)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '24px',
                  fontWeight: 700,
                  color: '#F5ECD7',
                  flexShrink: 0,
                  letterSpacing: '0.05em',
                }}
              >
                {initials.toUpperCase() || '?'}
              </div>
              <div style={{ minWidth: 0 }}>
                <h2
                  style={{
                    fontFamily: '"Bodoni Moda", Georgia, serif',
                    fontStyle: 'italic',
                    fontSize: 'clamp(20px, 3vw, 28px)',
                    color: '#F5ECD7',
                    margin: '0 0 4px 0',
                    lineHeight: 1.2,
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap',
                  }}
                >
                  {[user?.firstName, user?.lastName].filter(Boolean).join(' ') || '—'}
                </h2>
                <p style={{ fontSize: '13px', color: '#8B7E72', margin: 0 }}>
                  {user?.email}
                </p>
              </div>
            </div>

            {/* Account details panel */}
            <div
              style={{
                background: 'rgba(245, 236, 215, 0.03)',
                border: '1px solid rgba(245, 236, 215, 0.08)',
                borderRadius: '16px',
                padding: '20px 24px',
                opacity: 0,
                animation: 'fadeIn 0.4s ease both',
                animationDelay: '0.2s',
              }}
            >
              <p
                style={{
                  fontSize: '10px',
                  fontWeight: 700,
                  letterSpacing: '0.22em',
                  textTransform: 'uppercase',
                  color: '#8B7E72',
                  margin: '0 0 16px 0',
                }}
              >
                Account Details
              </p>
              <div
                style={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))',
                  gap: '20px 32px',
                }}
              >
                <DetailRow label="First Name" value={user?.firstName} />
                <DetailRow label="Last Name" value={user?.lastName} />
                <DetailRow label="Email" value={me?.email} />
                <DetailRow label="User ID" value={me?.userId} mono />
              </div>
            </div>

            {/* Roles panel */}
            <div
              style={{
                background: 'rgba(245, 236, 215, 0.03)',
                border: '1px solid rgba(245, 236, 215, 0.08)',
                borderRadius: '16px',
                padding: '20px 24px',
                opacity: 0,
                animation: 'fadeIn 0.4s ease both',
                animationDelay: '0.3s',
              }}
            >
              <p
                style={{
                  fontSize: '10px',
                  fontWeight: 700,
                  letterSpacing: '0.22em',
                  textTransform: 'uppercase',
                  color: '#8B7E72',
                  margin: '0 0 14px 0',
                }}
              >
                Roles
              </p>
              {me?.roles && me.roles.length > 0 ? (
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
                  {me.roles.map((role) => {
                    const s = getRoleBadgeStyle(role)
                    return (
                      <span
                        key={role}
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
                        {role}
                      </span>
                    )
                  })}
                </div>
              ) : (
                <p style={{ fontSize: '13px', color: '#8B7E72', margin: 0 }}>No roles assigned.</p>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
