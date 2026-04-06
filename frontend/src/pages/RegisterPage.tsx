import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Link, useNavigate } from 'react-router-dom'
import { Eye, EyeOff, AlertCircle } from 'lucide-react'
import { useAuthStore } from '@/store/authStore'
import { registerUser } from '@/api/auth.api'

// Password rules mirror the backend FluentValidation RegisterUserDtoValidator
const schema = z.object({
  firstName: z
    .string()
    .min(1, 'First name is required')
    .max(50, 'Maximum 50 characters'),
  lastName: z
    .string()
    .min(1, 'Last name is required')
    .max(50, 'Maximum 50 characters'),
  email: z.string().email('Please enter a valid email address'),
  password: z
    .string()
    .min(6, 'Password must be at least 6 characters')
    .regex(/[A-Z]/, 'Must contain at least one uppercase letter')
    .regex(/[a-z]/, 'Must contain at least one lowercase letter')
    .regex(/[0-9]/, 'Must contain at least one number'),
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
      style={{ opacity: 0.038, animationDirection: 'reverse' }}
      aria-hidden="true"
    >
      <circle cx={cx} cy={cy} r={R} stroke="#F5ECD7" strokeWidth="1.5" fill="none" />
      <circle cx={cx} cy={cy} r={98} stroke="#F5ECD7" strokeWidth="0.75" fill="none" />
      <circle cx={cx} cy={cy} r={22} stroke="#F5ECD7" strokeWidth="0.75" fill="none" />
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
      <path d="M5 38 Q22 42 39 38" stroke="#EAD8B8" strokeWidth="2.5" fill="none" strokeLinecap="round" />
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
    <svg className="spin-loader" width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="3" opacity="0.25" />
      <path d="M12 2a10 10 0 0 1 10 10" stroke="currentColor" strokeWidth="3" strokeLinecap="round" />
    </svg>
  )
}

function Field({
  id,
  label,
  error,
  delay,
  children,
}: {
  id: string
  label: string
  error?: string
  delay: string
  children: React.ReactNode
}) {
  return (
    <div style={{ animation: `slideUp 0.5s ${delay} both`, opacity: 0 }}>
      <label
        htmlFor={id}
        className="mb-1.5 block text-[11px] font-semibold uppercase tracking-widest text-[#2A2721]"
      >
        {label}
      </label>
      {children}
      {error && <p className="mt-1.5 text-xs text-[#C44536]">{error}</p>}
    </div>
  )
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function RegisterPage() {
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
      const res = await registerUser(data)
      // Registration returns a JWT immediately — log the user in right away
      setAuth(res.token, res.user)
      navigate('/', { replace: true })
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } }
      setServerError(
        axiosErr?.response?.data?.message ?? 'Registration failed. Please try again.'
      )
    } finally {
      setIsLoading(false)
    }
  }

  const inputClass = (hasError: boolean) =>
    [
      'w-full rounded-xl border bg-[#FAF4E8] px-4 py-3 text-sm text-[#1C1A17]',
      'placeholder:text-[#B5A89C] outline-none transition-all duration-200',
      'focus:ring-2 focus:ring-[#C44536]/35 focus:border-[#C44536]',
      hasError
        ? 'border-[#C44536] ring-2 ring-[#C44536]/20'
        : 'border-[#D5C8B4] hover:border-[#B5A89C]',
    ].join(' ')

  return (
    <div className="min-h-screen bg-[#1C1A17] flex items-center justify-center p-6 relative overflow-hidden">
      <PizzaWheelWatermark />

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
        className="relative z-10 w-full max-w-[440px] bg-[#F5ECD7] rounded-2xl px-10 py-12"
        style={{
          boxShadow: '0 32px 64px rgba(0,0,0,0.55), 0 8px 24px rgba(0,0,0,0.3)',
          animation: 'slideUp 0.65s cubic-bezier(0.16,1,0.3,1) both',
        }}
      >
        {/* Logo lockup */}
        <div className="flex flex-col items-center">
          <PizzaSliceIcon />
          <p className="mt-3 tracking-[0.32em] text-[10px] font-semibold text-[#C44536] uppercase">
            Napoletana
          </p>
          <Divider />
        </div>

        {/* Heading */}
        <h1
          className="mt-6 text-center text-[2.4rem] leading-none text-[#1C1A17]"
          style={{
            fontFamily: '"Bodoni Moda", Georgia, serif',
            fontStyle: 'italic',
            fontWeight: 400,
            animation: 'fadeIn 0.5s 0.2s both',
          }}
        >
          Unisciti
        </h1>
        <p
          className="mt-2 text-center text-sm text-[#8B7E72]"
          style={{ animation: 'fadeIn 0.5s 0.3s both' }}
        >
          Create your account
        </p>

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="mt-8 space-y-4" noValidate>
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

          {/* First + Last name row */}
          <div className="grid grid-cols-2 gap-3" style={{ animation: 'slideUp 0.5s 0.35s both', opacity: 0 }}>
            <div>
              <label
                htmlFor="firstName"
                className="mb-1.5 block text-[11px] font-semibold uppercase tracking-widest text-[#2A2721]"
              >
                First name
              </label>
              <input
                id="firstName"
                type="text"
                autoComplete="given-name"
                placeholder="John"
                {...register('firstName')}
                className={inputClass(!!errors.firstName)}
              />
              {errors.firstName && (
                <p className="mt-1.5 text-xs text-[#C44536]">{errors.firstName.message}</p>
              )}
            </div>
            <div>
              <label
                htmlFor="lastName"
                className="mb-1.5 block text-[11px] font-semibold uppercase tracking-widest text-[#2A2721]"
              >
                Last name
              </label>
              <input
                id="lastName"
                type="text"
                autoComplete="family-name"
                placeholder="Doe"
                {...register('lastName')}
                className={inputClass(!!errors.lastName)}
              />
              {errors.lastName && (
                <p className="mt-1.5 text-xs text-[#C44536]">{errors.lastName.message}</p>
              )}
            </div>
          </div>

          <Field id="email" label="Email address" error={errors.email?.message} delay="0.43s">
            <input
              id="email"
              type="email"
              autoComplete="email"
              placeholder="you@example.com"
              {...register('email')}
              className={inputClass(!!errors.email)}
            />
          </Field>

          <Field id="password" label="Password" error={errors.password?.message} delay="0.51s">
            <div className="relative">
              <input
                id="password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="new-password"
                placeholder="Min 6 chars, upper, lower & number"
                {...register('password')}
                className={inputClass(!!errors.password) + ' pr-11'}
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
          </Field>

          {/* Submit */}
          <div className="pt-2" style={{ animation: 'slideUp 0.5s 0.59s both', opacity: 0 }}>
            <button
              type="submit"
              disabled={isLoading}
              className={[
                'w-full rounded-xl py-3.5 text-sm font-semibold uppercase tracking-[0.13em]',
                'bg-[#C44536] text-[#F5ECD7] transition-all duration-200',
                'hover:bg-[#B33C2F] active:scale-[0.98]',
                'disabled:cursor-not-allowed disabled:opacity-60',
              ].join(' ')}
              style={{ boxShadow: isLoading ? 'none' : '0 4px 18px rgba(196,69,54,0.38)' }}
            >
              {isLoading ? (
                <span className="flex items-center justify-center gap-2">
                  <Spinner />
                  Creating account…
                </span>
              ) : (
                'Iscriviti — Create Account'
              )}
            </button>
          </div>
        </form>

        {/* Login link */}
        <p
          className="mt-7 text-center text-sm text-[#8B7E72]"
          style={{ animation: 'fadeIn 0.5s 0.7s both', opacity: 0 }}
        >
          Already have an account?{' '}
          <Link
            to="/login"
            className="font-medium text-[#C44536] transition-colors hover:text-[#B33C2F] hover:underline"
          >
            Sign in
          </Link>
        </p>
      </div>
    </div>
  )
}
