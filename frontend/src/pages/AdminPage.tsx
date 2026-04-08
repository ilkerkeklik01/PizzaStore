import { useAuth } from '@/hooks/useAuth'

export default function AdminPage() {
  const { user, logout } = useAuth()

  return (
    <div className="min-h-screen bg-[#1C1A17] flex items-center justify-center p-8">
      <div className="text-center">
        <p className="text-[10px] tracking-[0.35em] font-semibold text-[#C44536] uppercase mb-4">
          Napoletana — Admin
        </p>
        <h1
          className="text-5xl text-[#F5ECD7] mb-3"
          style={{ fontFamily: '"Bodoni Moda", Georgia, serif', fontStyle: 'italic' }}
        >
          Dashboard
        </h1>
        <p className="text-[#8B7E72] text-sm mb-8">
          Welcome, {user?.firstName}. Admin panel coming soon.
        </p>
        <button
          onClick={logout}
          className="text-xs uppercase tracking-widest text-[#8B7E72] hover:text-[#C44536] transition-colors"
        >
          Sign out
        </button>
      </div>
    </div>
  )
}
