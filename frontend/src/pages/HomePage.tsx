import { useState, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ChevronDown, Plus, Check, Lock, ChevronUp } from 'lucide-react'
import { useAuth } from '@/hooks/useAuth'
import { getAllPizzas } from '@/api/pizza.api'
import { addToCart } from '@/api/cart.api'
import { getAllToppings } from '@/api/topping.api'
import Navbar from '@/components/Navbar'
import CartDrawer from '@/components/CartDrawer'
import type { Pizza, PizzaSize, PizzaType } from '@/types/pizza'

// ─── Constants ────────────────────────────────────────────────────────────────

const ALL_TYPES: ('All' | PizzaType)[] = [
  'All',
  'Margherita',
  'Supreme',
  'MeatLovers',
  'Hawaiian',
  'Vegetarian',
  'Veggie',
  'Custom',
]

const TYPE_GRADIENTS: Record<string, string> = {
  Margherita: 'linear-gradient(150deg, #7A1515 0%, #C44536 45%, #2D5A27 100%)',
  Vegetarian: 'linear-gradient(150deg, #1A4A1A 0%, #4A8A2A 55%, #C8A84B 100%)',
  Veggie: 'linear-gradient(150deg, #2A5A1A 0%, #6AAF30 60%, #EFC840 100%)',
  MeatLovers: 'linear-gradient(150deg, #2E0E04 0%, #6E2810 50%, #C44536 100%)',
  Hawaiian: 'linear-gradient(150deg, #5A3200 0%, #CC8010 50%, #E8C060 100%)',
  Supreme: 'linear-gradient(150deg, #160A30 0%, #4E2070 50%, #B03060 100%)',
  Custom: 'linear-gradient(150deg, #0A1828 0%, #204868 55%, #68A8C4 100%)',
}

const SIZE_LABEL: Record<PizzaSize, string> = {
  Small: 'S',
  Medium: 'M',
  Large: 'L',
  ExtraLarge: 'XL',
}

// ─── Pizza Card ───────────────────────────────────────────────────────────────

function PizzaCard({
  pizza,
  isAuthenticated,
  onAddToCart,
}: {
  pizza: Pizza
  isAuthenticated: boolean
  onAddToCart: (variantId: string, toppingIds: string[]) => void
}) {
  const navigate = useNavigate()
  const available = pizza.variants.filter((v) => v.isAvailable)
  const [selectedId, setSelectedId] = useState(available[0]?.id ?? '')
  const [selectedToppingIds, setSelectedToppingIds] = useState<string[]>([])
  const [toppingsOpen, setToppingsOpen] = useState(false)
  const [added, setAdded] = useState(false)

  const { data: toppings = [] } = useQuery({
    queryKey: ['toppings'],
    queryFn: getAllToppings,
    staleTime: 1000 * 60 * 5,
  })

  const availableToppings = toppings.filter((t) => t.isAvailable)

  const selectedVariant = available.find((v) => v.id === selectedId)
  const gradient = TYPE_GRADIENTS[pizza.type] ?? TYPE_GRADIENTS['Custom']

  const toppingTotal = selectedToppingIds.reduce((sum, tid) => {
    const t = availableToppings.find((t) => t.id === tid)
    return sum + (t?.price ?? 0)
  }, 0)

  const totalPrice = (selectedVariant?.price ?? 0) + toppingTotal

  const toggleTopping = (id: string) => {
    setSelectedToppingIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
    )
  }

  const handleAdd = () => {
    if (!isAuthenticated) {
      navigate('/login')
      return
    }
    if (!selectedId) return
    onAddToCart(selectedId, selectedToppingIds)
    setAdded(true)
    setTimeout(() => {
      setAdded(false)
      setSelectedToppingIds([])
      setToppingsOpen(false)
    }, 2200)
  }

  return (
    <article
      style={{
        background: '#1E1C19',
        border: '1px solid rgba(245, 236, 215, 0.07)',
        borderRadius: '18px',
        overflow: 'hidden',
        display: 'flex',
        flexDirection: 'column',
        transition: 'transform 0.3s ease, box-shadow 0.3s ease, border-color 0.3s ease',
        opacity: pizza.isAvailable ? 1 : 0.55,
      }}
      onMouseEnter={(e) => {
        const el = e.currentTarget as HTMLElement
        el.style.transform = 'translateY(-5px)'
        el.style.boxShadow = '0 24px 48px rgba(0,0,0,0.45)'
        el.style.borderColor = 'rgba(196, 69, 54, 0.25)'
      }}
      onMouseLeave={(e) => {
        const el = e.currentTarget as HTMLElement
        el.style.transform = 'translateY(0)'
        el.style.boxShadow = 'none'
        el.style.borderColor = 'rgba(245, 236, 215, 0.07)'
      }}
    >
      {/* Image area */}
      <div style={{ position: 'relative', height: '210px', overflow: 'hidden', flexShrink: 0 }}>
        {pizza.imageUrl ? (
          <img
            src={pizza.imageUrl}
            alt={pizza.name}
            loading="lazy"
            style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }}
            onError={(e) => {
              const img = e.currentTarget as HTMLImageElement
              img.style.display = 'none'
              const fallback = img.nextElementSibling as HTMLElement | null
              if (fallback) fallback.style.display = 'flex'
            }}
          />
        ) : null}

        {/* Gradient fallback */}
        <div
          style={{
            display: pizza.imageUrl ? 'none' : 'flex',
            width: '100%',
            height: '100%',
            background: gradient,
            alignItems: 'center',
            justifyContent: 'center',
            position: 'relative',
          }}
        >
          {/* Concentric circles */}
          {[160, 116, 76].map((size, i) => (
            <div
              key={size}
              style={{
                position: 'absolute',
                width: `${size}px`,
                height: `${size}px`,
                borderRadius: '50%',
                border: `${i === 2 ? '0' : '1px'} solid rgba(255,255,255,${0.06 + i * 0.03})`,
                background: i === 2 ? 'rgba(255,255,255,0.07)' : 'transparent',
              }}
            />
          ))}
          <span style={{ fontSize: '30px', position: 'relative', zIndex: 1, filter: 'drop-shadow(0 2px 8px rgba(0,0,0,0.5))' }}>
            🍕
          </span>
        </div>

        {/* Type badge */}
        <div
          style={{
            position: 'absolute',
            top: '12px',
            left: '12px',
            background: 'rgba(20, 18, 16, 0.82)',
            backdropFilter: 'blur(10px)',
            WebkitBackdropFilter: 'blur(10px)',
            borderRadius: '20px',
            padding: '4px 10px',
            fontSize: '10px',
            fontWeight: 700,
            letterSpacing: '0.1em',
            textTransform: 'uppercase',
            color: '#D4A44C',
          }}
        >
          {pizza.type}
        </div>

        {!pizza.isAvailable && (
          <div
            style={{
              position: 'absolute',
              inset: 0,
              background: 'rgba(20, 18, 16, 0.68)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <span
              style={{
                fontSize: '11px',
                color: '#8B7E72',
                fontWeight: 700,
                letterSpacing: '0.15em',
                textTransform: 'uppercase',
              }}
            >
              Currently Unavailable
            </span>
          </div>
        )}
      </div>

      {/* Content */}
      <div style={{ padding: '18px', display: 'flex', flexDirection: 'column', flex: 1 }}>
        <h3
          style={{
            fontFamily: '"Bodoni Moda", Georgia, serif',
            fontStyle: 'italic',
            fontSize: '21px',
            color: '#F5ECD7',
            margin: '0 0 6px 0',
            lineHeight: 1.2,
          }}
        >
          {pizza.name}
        </h3>
        <p
          style={{
            fontSize: '13px',
            color: '#8B7E72',
            margin: '0 0 16px 0',
            lineHeight: 1.6,
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical',
            overflow: 'hidden',
            flex: 1,
          }}
        >
          {pizza.description}
        </p>

        {/* Size selector */}
        {available.length > 0 && (
          <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap', marginBottom: '14px' }}>
            {available.map((v) => (
              <button
                key={v.id}
                onClick={() => setSelectedId(v.id)}
                style={{
                  padding: '4px 11px',
                  borderRadius: '20px',
                  fontSize: '12px',
                  fontWeight: 500,
                  cursor: 'pointer',
                  transition: 'all 0.18s',
                  background: selectedId === v.id ? 'rgba(196, 69, 54, 0.18)' : 'transparent',
                  color: selectedId === v.id ? '#C44536' : '#8B7E72',
                  border:
                    selectedId === v.id
                      ? '1px solid rgba(196, 69, 54, 0.5)'
                      : '1px solid rgba(245, 236, 215, 0.1)',
                }}
              >
                {SIZE_LABEL[v.size as PizzaSize] ?? v.size}
              </button>
            ))}
          </div>
        )}

        {/* Toppings toggle */}
        {isAuthenticated && availableToppings.length > 0 && (
          <div style={{ marginBottom: '14px' }}>
            <button
              onClick={() => setToppingsOpen((v) => !v)}
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                width: '100%',
                background: 'rgba(245, 236, 215, 0.04)',
                border: '1px solid rgba(245, 236, 215, 0.1)',
                borderRadius: '10px',
                padding: '8px 12px',
                cursor: 'pointer',
                color: selectedToppingIds.length > 0 ? '#D4A44C' : '#8B7E72',
                fontSize: '12px',
                fontWeight: 500,
                letterSpacing: '0.04em',
                transition: 'all 0.2s',
              }}
              onMouseEnter={(e) => {
                const el = e.currentTarget as HTMLElement
                el.style.borderColor = 'rgba(245, 236, 215, 0.2)'
                el.style.background = 'rgba(245, 236, 215, 0.07)'
              }}
              onMouseLeave={(e) => {
                const el = e.currentTarget as HTMLElement
                el.style.borderColor = 'rgba(245, 236, 215, 0.1)'
                el.style.background = 'rgba(245, 236, 215, 0.04)'
              }}
            >
              <span>
                {selectedToppingIds.length > 0
                  ? `${selectedToppingIds.length} topping${selectedToppingIds.length > 1 ? 's' : ''} selected`
                  : 'Add toppings'}
              </span>
              {toppingsOpen ? <ChevronUp size={13} /> : <ChevronDown size={13} />}
            </button>

            {toppingsOpen && (
              <div
                style={{
                  marginTop: '8px',
                  display: 'grid',
                  gridTemplateColumns: '1fr 1fr',
                  gap: '6px',
                  animation: 'slideUp 0.15s ease both',
                }}
              >
                {availableToppings.map((topping) => {
                  const selected = selectedToppingIds.includes(topping.id)
                  return (
                    <button
                      key={topping.id}
                      onClick={() => toggleTopping(topping.id)}
                      style={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                        padding: '7px 10px',
                        borderRadius: '8px',
                        fontSize: '11px',
                        cursor: 'pointer',
                        transition: 'all 0.15s',
                        background: selected ? 'rgba(212, 164, 76, 0.12)' : 'rgba(245, 236, 215, 0.04)',
                        color: selected ? '#D4A44C' : '#8B7E72',
                        border: selected
                          ? '1px solid rgba(212, 164, 76, 0.35)'
                          : '1px solid rgba(245, 236, 215, 0.1)',
                        textAlign: 'left',
                        gap: '4px',
                      }}
                    >
                      <span style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', flex: 1 }}>
                        {topping.name}
                      </span>
                      <span style={{ flexShrink: 0, fontWeight: 600 }}>
                        +${topping.price.toFixed(2)}
                      </span>
                    </button>
                  )
                })}
              </div>
            )}
          </div>
        )}

        {/* Price + CTA */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            {selectedVariant ? (
              <div>
                <span
                  style={{
                    fontFamily: '"Bodoni Moda", Georgia, serif',
                    fontStyle: 'italic',
                    fontSize: '22px',
                    color: '#F5ECD7',
                    lineHeight: 1,
                  }}
                >
                  ${totalPrice.toFixed(2)}
                </span>
                {toppingTotal > 0 && (
                  <div style={{ fontSize: '10px', color: '#8B7E72', marginTop: '2px' }}>
                    incl. +${toppingTotal.toFixed(2)} toppings
                  </div>
                )}
              </div>
            ) : (
              <span style={{ fontSize: '12px', color: '#8B7E72' }}>No sizes</span>
            )}
          </div>

          {pizza.isAvailable && (
            <button
              onClick={handleAdd}
              disabled={!selectedId}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '6px',
                padding: '9px 18px',
                borderRadius: '24px',
                fontSize: '13px',
                fontWeight: 500,
                cursor: selectedId ? 'pointer' : 'not-allowed',
                transition: 'all 0.25s ease',
                background: added
                  ? 'rgba(60, 130, 60, 0.18)'
                  : isAuthenticated
                    ? '#C44536'
                    : 'transparent',
                color: added ? '#5ABA5A' : isAuthenticated ? '#F5ECD7' : '#8B7E72',
                border: added
                  ? '1px solid rgba(90, 186, 90, 0.4)'
                  : isAuthenticated
                    ? '1px solid transparent'
                    : '1px solid rgba(245, 236, 215, 0.15)',
                whiteSpace: 'nowrap',
              }}
              onMouseEnter={(e) => {
                if (added) return
                const el = e.currentTarget as HTMLElement
                if (isAuthenticated) {
                  el.style.background = '#A8352A'
                  el.style.transform = 'translateY(-1px)'
                } else {
                  el.style.background = 'rgba(245, 236, 215, 0.08)'
                  el.style.color = '#F5ECD7'
                }
              }}
              onMouseLeave={(e) => {
                if (added) return
                const el = e.currentTarget as HTMLElement
                el.style.transform = 'none'
                if (isAuthenticated) {
                  el.style.background = '#C44536'
                } else {
                  el.style.background = 'transparent'
                  el.style.color = '#8B7E72'
                }
              }}
            >
              {added ? (
                <>
                  <Check size={13} />
                  Added!
                </>
              ) : isAuthenticated ? (
                <>
                  <Plus size={13} />
                  Add to Cart
                </>
              ) : (
                <>
                  <Lock size={12} />
                  Sign in to Order
                </>
              )}
            </button>
          )}
        </div>
      </div>
    </article>
  )
}

// ─── Home Page ────────────────────────────────────────────────────────────────

export default function HomePage() {
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const menuRef = useRef<HTMLDivElement>(null)
  const [cartOpen, setCartOpen] = useState(false)
  const [activeType, setActiveType] = useState<'All' | PizzaType>('All')

  const { data: pizzas = [], isLoading } = useQuery({
    queryKey: ['pizzas'],
    queryFn: getAllPizzas,
  })

  const { mutate: addPizzaToCart } = useMutation({
    mutationFn: addToCart,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  })

  const filteredPizzas =
    activeType === 'All' ? pizzas : pizzas.filter((p) => p.type === activeType)

  const handleAddToCart = (pizzaVariantId: string, toppingIds: string[]) => {
    if (!isAuthenticated) {
      navigate('/login')
      return
    }
    addPizzaToCart({ pizzaVariantId, quantity: 1, toppingIds })
    setCartOpen(true)
  }

  const scrollToMenu = () => {
    menuRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }

  return (
    <div style={{ minHeight: '100vh', background: '#1C1A17' }}>
      <Navbar onCartOpen={() => setCartOpen(true)} />

      {/* ── Hero ──────────────────────────────────────────────────── */}
      <section
        style={{
          minHeight: '100vh',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          position: 'relative',
          overflow: 'hidden',
          textAlign: 'center',
          padding: '80px clamp(20px, 5vw, 60px) 60px',
        }}
      >
        {/* Radial glow */}
        <div
          style={{
            position: 'absolute',
            inset: 0,
            background:
              'radial-gradient(ellipse 90% 55% at 50% 75%, rgba(196, 69, 54, 0.14) 0%, transparent 70%)',
            pointerEvents: 'none',
          }}
        />

        {/* Spinning decorative ring */}
        <div
          className="animate-spin-slow"
          style={{
            position: 'absolute',
            width: 'min(650px, 90vw)',
            height: 'min(650px, 90vw)',
            borderRadius: '50%',
            border: '1px solid rgba(196, 69, 54, 0.07)',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            pointerEvents: 'none',
          }}
        />
        <div
          style={{
            position: 'absolute',
            width: 'min(420px, 60vw)',
            height: 'min(420px, 60vw)',
            borderRadius: '50%',
            border: '1px solid rgba(245, 236, 215, 0.035)',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            pointerEvents: 'none',
          }}
        />

        {/* Hero content */}
        <div style={{ position: 'relative', zIndex: 1, maxWidth: '720px' }}>
          <p
            style={{
              fontSize: '11px',
              letterSpacing: '0.38em',
              fontWeight: 600,
              color: '#C44536',
              textTransform: 'uppercase',
              marginBottom: '28px',
              animation: 'fadeIn 1s ease both',
            }}
          >
            Napoletana · Authentic Italian Pizza
          </p>

          <h1
            style={{
              fontFamily: '"Bodoni Moda", Georgia, serif',
              fontStyle: 'italic',
              fontSize: 'clamp(50px, 9vw, 100px)',
              color: '#F5ECD7',
              lineHeight: 1.02,
              margin: '0 0 28px 0',
              animation: 'slideUp 1s ease 0.2s both',
              letterSpacing: '-0.01em',
            }}
          >
            The Art of
            <br />
            <span style={{ color: '#C44536' }}>Neapolitan</span>
            <br />
            Pizza
          </h1>

          <p
            style={{
              fontSize: '15px',
              color: '#8B7E72',
              maxWidth: '480px',
              margin: '0 auto 44px',
              lineHeight: 1.75,
              animation: 'slideUp 1s ease 0.4s both',
            }}
          >
            Wood-fired perfection crafted with generations of Italian passion.
            Every pizza tells a story of fire, flour, and love.
          </p>

          <div
            style={{
              display: 'flex',
              gap: '14px',
              justifyContent: 'center',
              flexWrap: 'wrap',
              animation: 'slideUp 1s ease 0.6s both',
            }}
          >
            <button
              onClick={scrollToMenu}
              style={{
                background: '#C44536',
                color: '#F5ECD7',
                border: 'none',
                borderRadius: '32px',
                padding: '14px 34px',
                fontSize: '14px',
                fontWeight: 500,
                cursor: 'pointer',
                letterSpacing: '0.05em',
                display: 'flex',
                alignItems: 'center',
                gap: '8px',
                transition: 'background 0.25s, transform 0.15s',
              }}
              onMouseEnter={(e) => {
                const el = e.currentTarget as HTMLElement
                el.style.background = '#A8352A'
                el.style.transform = 'translateY(-2px)'
              }}
              onMouseLeave={(e) => {
                const el = e.currentTarget as HTMLElement
                el.style.background = '#C44536'
                el.style.transform = 'none'
              }}
            >
              Explore Our Menu
              <ChevronDown size={16} />
            </button>

            {!isAuthenticated && (
              <button
                onClick={() => navigate('/register')}
                style={{
                  background: 'transparent',
                  color: '#F5ECD7',
                  border: '1px solid rgba(245, 236, 215, 0.2)',
                  borderRadius: '32px',
                  padding: '14px 34px',
                  fontSize: '14px',
                  fontWeight: 400,
                  cursor: 'pointer',
                  letterSpacing: '0.05em',
                  transition: 'border-color 0.25s, background 0.25s, transform 0.15s',
                }}
                onMouseEnter={(e) => {
                  const el = e.currentTarget as HTMLElement
                  el.style.borderColor = 'rgba(245, 236, 215, 0.4)'
                  el.style.background = 'rgba(245, 236, 215, 0.05)'
                  el.style.transform = 'translateY(-2px)'
                }}
                onMouseLeave={(e) => {
                  const el = e.currentTarget as HTMLElement
                  el.style.borderColor = 'rgba(245, 236, 215, 0.2)'
                  el.style.background = 'transparent'
                  el.style.transform = 'none'
                }}
              >
                Create Account
              </button>
            )}
          </div>
        </div>

        {/* Scroll hint */}
        <div
          style={{
            position: 'absolute',
            bottom: '32px',
            left: '50%',
            transform: 'translateX(-50%)',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: '8px',
            animation: 'fadeIn 2s ease 1s both',
          }}
        >
          <span style={{ fontSize: '10px', letterSpacing: '0.22em', color: '#8B7E72', textTransform: 'uppercase' }}>
            Scroll
          </span>
          <div
            style={{
              width: '1px',
              height: '44px',
              background: 'linear-gradient(to bottom, #8B7E72 0%, transparent 100%)',
            }}
          />
        </div>
      </section>

      {/* ── Stats bar ─────────────────────────────────────────────── */}
      <div
        style={{
          borderTop: '1px solid rgba(245, 236, 215, 0.06)',
          borderBottom: '1px solid rgba(245, 236, 215, 0.06)',
          padding: '28px clamp(20px, 5vw, 60px)',
          display: 'flex',
          justifyContent: 'center',
          gap: 'clamp(28px, 6vw, 80px)',
          flexWrap: 'wrap',
        }}
      >
        {[
          { value: '7', label: 'Categories' },
          { value: '4', label: 'Sizes' },
          { value: '100%', label: 'Italian' },
          { value: 'Daily', label: 'Fresh' },
        ].map((stat) => (
          <div key={stat.label} style={{ textAlign: 'center' }}>
            <div
              style={{
                fontFamily: '"Bodoni Moda", Georgia, serif',
                fontStyle: 'italic',
                fontSize: '28px',
                color: '#C44536',
                lineHeight: 1,
                marginBottom: '5px',
              }}
            >
              {stat.value}
            </div>
            <div
              style={{
                fontSize: '11px',
                letterSpacing: '0.18em',
                color: '#8B7E72',
                textTransform: 'uppercase',
              }}
            >
              {stat.label}
            </div>
          </div>
        ))}
      </div>

      {/* ── Menu section ──────────────────────────────────────────── */}
      <section
        ref={menuRef}
        style={{ padding: 'clamp(48px, 8vw, 96px) clamp(20px, 5vw, 60px)', maxWidth: '1280px', margin: '0 auto' }}
      >
        {/* Section header */}
        <div style={{ marginBottom: '44px' }}>
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
            Our Menu
          </p>
          <h2
            style={{
              fontFamily: '"Bodoni Moda", Georgia, serif',
              fontStyle: 'italic',
              fontSize: 'clamp(30px, 4vw, 52px)',
              color: '#F5ECD7',
              margin: '0 0 16px 0',
              lineHeight: 1.1,
            }}
          >
            Artisanal Pizzas
          </h2>
          <p
            style={{
              fontSize: '14px',
              color: '#8B7E72',
              maxWidth: '480px',
              lineHeight: 1.7,
              margin: 0,
            }}
          >
            {isAuthenticated
              ? 'Select your preferred size and add directly to your cart. Each pizza is hand-crafted to order.'
              : 'Browse our full menu below. Sign in to unlock ordering and cart features.'}
          </p>
        </div>

        {/* Category filter */}
        <div
          style={{
            display: 'flex',
            gap: '8px',
            marginBottom: '40px',
            flexWrap: 'wrap',
          }}
        >
          {ALL_TYPES.map((type) => (
            <button
              key={type}
              onClick={() => setActiveType(type)}
              style={{
                padding: '8px 20px',
                borderRadius: '32px',
                fontSize: '13px',
                fontWeight: 500,
                cursor: 'pointer',
                transition: 'all 0.2s',
                background: activeType === type ? '#C44536' : 'transparent',
                color: activeType === type ? '#F5ECD7' : '#8B7E72',
                border:
                  activeType === type
                    ? '1px solid #C44536'
                    : '1px solid rgba(245, 236, 215, 0.12)',
                letterSpacing: '0.03em',
              }}
              onMouseEnter={(e) => {
                if (activeType === type) return
                const el = e.currentTarget as HTMLElement
                el.style.borderColor = 'rgba(245, 236, 215, 0.25)'
                el.style.color = '#F5ECD7'
              }}
              onMouseLeave={(e) => {
                if (activeType === type) return
                const el = e.currentTarget as HTMLElement
                el.style.borderColor = 'rgba(245, 236, 215, 0.12)'
                el.style.color = '#8B7E72'
              }}
            >
              {type}
            </button>
          ))}
        </div>

        {/* Pizza grid */}
        {isLoading ? (
          <div
            style={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              gap: '16px',
              padding: '80px 0',
            }}
          >
            <div
              className="spin-loader"
              style={{
                width: '34px',
                height: '34px',
                border: '2px solid rgba(245, 236, 215, 0.1)',
                borderTopColor: '#C44536',
                borderRadius: '50%',
              }}
            />
            <span style={{ fontSize: '12px', color: '#8B7E72', letterSpacing: '0.1em' }}>
              Loading menu…
            </span>
          </div>
        ) : filteredPizzas.length === 0 ? (
          <div
            style={{
              textAlign: 'center',
              padding: '80px 0',
              color: '#8B7E72',
            }}
          >
            <div style={{ fontSize: '44px', marginBottom: '16px', opacity: 0.4 }}>🍕</div>
            <p style={{ fontSize: '14px', margin: 0 }}>No pizzas in this category yet.</p>
          </div>
        ) : (
          <div
            style={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fill, minmax(290px, 1fr))',
              gap: '22px',
            }}
          >
            {filteredPizzas.map((pizza) => (
              <PizzaCard
                key={pizza.id}
                pizza={pizza}
                isAuthenticated={isAuthenticated}
                onAddToCart={handleAddToCart}
              />
            ))}
          </div>
        )}
      </section>

      {/* ── Footer ────────────────────────────────────────────────── */}
      <footer
        style={{
          borderTop: '1px solid rgba(245, 236, 215, 0.06)',
          padding: '44px clamp(20px, 5vw, 60px)',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: '10px',
          textAlign: 'center',
        }}
      >
        <span
          style={{
            fontFamily: '"Bodoni Moda", Georgia, serif',
            fontStyle: 'italic',
            fontSize: '22px',
            color: '#F5ECD7',
          }}
        >
          Napoletana
        </span>
        <span
          style={{
            fontSize: '11px',
            letterSpacing: '0.15em',
            color: '#8B7E72',
            textTransform: 'uppercase',
          }}
        >
          Authentic Italian Pizza · Crafted with Love
        </span>
      </footer>

      {/* Cart drawer */}
      <CartDrawer isOpen={cartOpen} onClose={() => setCartOpen(false)} />
    </div>
  )
}
