import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Link, useNavigate } from 'react-router-dom'
import { Eye, EyeOff, AlertCircle } from 'lucide-react'
import { useAuthStore } from '@/store/authStore'
import { loginUser } from '@/api/auth.api'

const schema = z.object({
  email: z.string().email('Please enter a valid email address'),
  password: z.string().min(1, 'Password is required'),
})
type FormData = z.infer<typeof schema>

// ─── SVG helpers ─────────────────────────────────────────────────────────────

function PizzaWheelWatermark() {
  const sliceAngles = [0, 45, 90, 135]
  const R = 145
  const cx = 150
  const cy = 150

  return (
    <svg
      className="absolute inset-0 m-auto pointer-events-none animate-spin-slow"
      width="680"
      height="680"
      viewBox="0 0 300 300"
      style={{ opacity: 0.038 }}
      aria-hidden="true"
    >
      {/* Outer crust ring */}
      <circle cx={cx} cy={cy} r={R} stroke="#F5ECD7" strokeWidth="1.5" fill="none" />
      {/* Inner sauce ring */}
      <circle cx={cx} cy={cy} r={98} stroke="#F5ECD7" strokeWidth="0.75" fill="none" />
      {/* Centre */}
      <circle cx={cx} cy={cy} r={22} stroke="#F5ECD7" strokeWidth="0.75" fill="none" />
      {/* Slice cuts */}
      {sliceAngles.map((deg) => {
        const rad = (deg * Math.PI) / 180
        return (
          <line
            key={deg}
            x1={cx + R * Math.cos(rad)}
            y1={cy + R * Math.sin(rad)}
            x2={cx - R * Math.cos(rad)}
            y2={cy - R * Math.sin(rad)}
            stroke="#F5ECD7"
            strokeWidth="0.6"
          />
        )
      })}
      {/* Decorative dots (toppings) */}
      {[
        { x: 110, y: 120 }, { x: 190, y: 115 }, { x: 155, y: 175 },
        { x: 130, y: 195 }, { x: 175, y: 200 }, { x: 100, y: 165 },
        { x: 200, y: 170 }, { x: 148, y: 80 },
      ].map((pt, i) => (
        <circle key={i} cx={pt.x} cy={pt.y} r="4" fill="#F5ECD7" opacity="0.6" />
      ))}
    </svg>
  )
}

function PizzaSliceIcon() {
  return (
    <svg width="44" height="44" viewBox="0 0 44 44" fill="none" aria-hidden="true">
      <path d="M22 3 L3 40 L41 40 Z" fill="#C44536" opacity="0.95" />
      <path d="M22 3 L3 40 L41 40 Z" stroke="#A83528" strokeWidth="0.5" fill="none" />
      {/* Crust */}
      <path d="M5 38 Q22 42 39 38" stroke="#EAD8B8" strokeWidth="2.5" fill="none" strokeLinecap="round" />
      {/* Toppings */}
      <circle cx="22" cy="28" r="2.8" fill="#FAF4E8" opacity="0.85" />
      <circle cx="15" cy="34" r="2" fill="#FAF4E8" opacity="0.85" />
      <circle cx="29" cy="34" r="2" fill="#FAF4E8" opacity="0.85" />
      <circle cx="22" cy="18" r="1.8" fill="#FAF4E8" opacity="0.7" />
    </svg>
  )
}

function Divider() {
  return (
    <div className="flex items-center gap-3 w-full mt-5">
      <div className="flex-1 h-px bg-[#D5C8B4]/50" />
      <svg width="8" height="8" viewBox="0 0 8 8" fill="#8B7E72" opacity="0.5" aria-hidden="true">
        <rect x="1" y="4" width="4.2" height="4.2" transform="rotate(-45 1 4)" />
      </svg>
      <div className="flex-1 h-px bg-[#D5C8B4]/50" />
    </div>
  )
}

function Spinner() {
  return (
    <svg
      className="spin-loader"
      width="16"
      height="16"
      viewBox="0 0 24 24"
      fill="none"
      aria-hidden="true"
    >
      <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="3" opacity="0.25" />
      <path
        d="M12 2a10 10 0 0 1 10 10"
        stroke="currentColor"
        strokeWidth="3"
        strokeLinecap="round"
      />
    </svg>
  )
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function LoginPage() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((s) => s.setAuth)

  const [showPassword, setShowPassword] = useState(false)
  const [serverError, setServerError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({ resolver: zodResolver(schema) })

  const onSubmit = async (data: FormData) => {
    setIsLoading(true)
    setServerError(null)
    try {
      const res = await loginUser(data)
      setAuth(res.token, res.user)

      // Role is already extracted by the store's setAuth — read it directly
      const role = useAuthStore.getState().role
      navigate(role === 'Admin' ? '/admin' : '/', { replace: true })
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } }
      setServerError(
        axiosErr?.response?.data?.message ?? 'Login failed. Please try again.'
      )
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-[#1C1A17] flex items-center justify-center p-6 relative overflow-hidden">

      {/* Rotating pizza wheel watermark */}
      <PizzaWheelWatermark />

      {/* Warm terracotta radial glow behind the card */}
      <div
        className="absolute pointer-events-none rounded-full"
        style={{
          width: 900,
          height: 900,
          background:
            'radial-gradient(circle, rgba(196,69,54,0.07) 0%, rgba(196,69,54,0.02) 40%, transparent 65%)',
        }}
        aria-hidden="true"
      />

      {/* ─── Card ─── */}
      <div
        className="relative z-10 w-full max-w-[420px] bg-[#F5ECD7] rounded-2xl px-10 py-12"
        style={{
          boxShadow: '0 32px 64px rgba(0,0,0,0.55), 0 8px 24px rgba(0,0,0,0.3)',
          animation: 'slideUp 0.65s cubic-bezier(0.16,1,0.3,1) both',
        }}
      >

        {/* ── Logo lockup ── */}
        <div className="flex flex-col items-center">
          <PizzaSliceIcon />
          <p
            className="mt-3 tracking-[0.32em] text-[10px] font-semibold text-[#C44536] uppercase"
            style={{ fontFamily: '"DM Sans", system-ui, sans-serif' }}
          >
            Napoletana
          </p>
          <Divider />
        </div>

        {/* ── Heading ── */}
        <h1
          className="mt-6 text-center text-[2.6rem] leading-none text-[#1C1A17]"
          style={{
            fontFamily: '"Bodoni Moda", Georgia, serif',
            fontStyle: 'italic',
            fontWeight: 400,
            animation: 'fadeIn 0.5s 0.2s both',
          }}
        >
          Benvenuto
        </h1>
        <p
          className="mt-2 text-center text-sm text-[#8B7E72]"
          style={{ animation: 'fadeIn 0.5s 0.3s both' }}
        >
          Sign in to your account
        </p>

        {/* ── Form ── */}
        <form
          onSubmit={handleSubmit(onSubmit)}
          className="mt-8 space-y-5"
          noValidate
        >
          {/* Server error banner */}
          {serverError && (
            <div
              className="flex items-start gap-2.5 rounded-xl border border-[#C44536]/25 bg-[#C44536]/10 px-4 py-3 text-sm text-[#C44536]"
              style={{ animation: 'slideUp 0.3s both' }}
              role="alert"
            >
              <AlertCircle size={15} className="mt-0.5 shrink-0" aria-hidden="true" />
              <span>{serverError}</span>
            </div>
          )}

          {/* Email */}
          <div style={{ animation: 'slideUp 0.5s 0.35s both', opacity: 0 }}>
            <label
              htmlFor="email"
              className="mb-1.5 block text-[11px] font-semibold uppercase tracking-widest text-[#2A2721]"
            >
              Email address
            </label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              placeholder="you@example.com"
              {...register('email')}
              className={[
                'w-full rounded-xl border bg-[#FAF4E8] px-4 py-3 text-sm text-[#1C1A17]',
                'placeholder:text-[#B5A89C] outline-none transition-all duration-200',
                'focus:ring-2 focus:ring-[#C44536]/35 focus:border-[#C44536]',
                errors.email
                  ? 'border-[#C44536] ring-2 ring-[#C44536]/20'
                  : 'border-[#D5C8B4] hover:border-[#B5A89C]',
              ].join(' ')}
            />
            {errors.email && (
              <p className="mt-1.5 text-xs text-[#C44536]">{errors.email.message}</p>
            )}
          </div>

          {/* Password */}
          <div style={{ animation: 'slideUp 0.5s 0.45s both', opacity: 0 }}>
            <label
              htmlFor="password"
              className="mb-1.5 block text-[11px] font-semibold uppercase tracking-widest text-[#2A2721]"
            >
              Password
            </label>
            <div className="relative">
              <input
                id="password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="current-password"
                placeholder="••••••••"
                {...register('password')}
                className={[
                  'w-full rounded-xl border bg-[#FAF4E8] px-4 py-3 pr-11 text-sm text-[#1C1A17]',
                  'placeholder:text-[#B5A89C] outline-none transition-all duration-200',
                  'focus:ring-2 focus:ring-[#C44536]/35 focus:border-[#C44536]',
                  errors.password
                    ? 'border-[#C44536] ring-2 ring-[#C44536]/20'
                    : 'border-[#D5C8B4] hover:border-[#B5A89C]',
                ].join(' ')}
              />
              <button
                type="button"
                onClick={() => setShowPassword((v) => !v)}
                className="absolute right-3.5 top-1/2 -translate-y-1/2 text-[#8B7E72] transition-colors hover:text-[#1C1A17]"
                aria-label={showPassword ? 'Hide password' : 'Show password'}
              >
                {showPassword ? <EyeOff size={15} /> : <Eye size={15} />}
              </button>
            </div>
            {errors.password && (
              <p className="mt-1.5 text-xs text-[#C44536]">{errors.password.message}</p>
            )}
          </div>

          {/* Submit */}
          <div className="pt-1" style={{ animation: 'slideUp 0.5s 0.55s both', opacity: 0 }}>
            <button
              type="submit"
              disabled={isLoading}
              className={[
                'w-full rounded-xl py-3.5 text-sm font-semibold uppercase tracking-[0.13em]',
                'bg-[#C44536] text-[#F5ECD7] transition-all duration-200',
                'hover:bg-[#B33C2F] active:scale-[0.98]',
                'disabled:cursor-not-allowed disabled:opacity-60',
              ].join(' ')}
              style={{
                boxShadow: isLoading
                  ? 'none'
                  : '0 4px 18px rgba(196,69,54,0.38)',
              }}
            >
              {isLoading ? (
                <span className="flex items-center justify-center gap-2">
                  <Spinner />
                  Signing in…
                </span>
              ) : (
                'Entra — Sign In'
              )}
            </button>
          </div>
        </form>

        {/* ── Register link ── */}
        <p
          className="mt-8 text-center text-sm text-[#8B7E72]"
          style={{ animation: 'fadeIn 0.5s 0.65s both', opacity: 0 }}
        >
          New to Napoletana?{' '}
          <Link
            to="/register"
            className="font-medium text-[#C44536] transition-colors hover:text-[#B33C2F] hover:underline"
          >
            Create an account
          </Link>
        </p>
      </div>
    </div>
  )
}
