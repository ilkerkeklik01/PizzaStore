# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Documentation Updates

After completing any changes, ask the user: **"Should I update the README and CHANGELOG files to keep them up-to-date?"** Do NOT update `README.md` or `CHANGELOG.md` without explicit user approval.

## Commands

```bash
# Install dependencies (see security note on Axios below before running)
npm install

# Start dev server at http://localhost:5173
npm run dev

# Type-check + production build
npm run build

# Preview production build
npm run preview
```

There are no tests yet. When tests are added, this section should be updated.

**Setup:** The backend API must be running at `https://localhost:5001` before starting the dev server. The Vite proxy forwards all `/api/*` requests there.

## Security: Axios Version Lock

**Do NOT change `"axios": "1.15.0"` to a range (`^`) or upgrade it without verifying the release is clean.**

### History
- `1.14.1` and `0.30.4` — **MALICIOUS** (supply-chain attack, 2026-03-31, UNC1069/North Korea-nexus). Injected `plain-crypto-js@4.2.1` post-install hook to deploy a cross-platform RAT via C2. Attributed by Google Threat Intelligence Group on 2026-04-01.
- `1.14.0` — last known-clean 1.x release before the attack.
- `1.15.0` — **current safe pin**. Released post-attack by confirmed legitimate maintainers. Fixes `GHSA-3p68-rc4w-qgx5` (NO_PROXY hostname normalization bypass → SSRF, 2026-04-09, Critical).

### Rules
- Keep the version as an **exact pin** (no `^` or `~`).
- Never run `npm audit fix --force` — it resolves to `latest` which may not be verified clean.
- Before any future upgrade, check the [axios GitHub releases](https://github.com/axios/axios/releases) and cross-reference with [npm advisories](https://github.com/advisories?query=axios) to confirm no new compromise.

## Architecture

### Layer Overview

```
src/
├── api/          # Axios client + per-domain API functions
├── store/        # Zustand auth store (persisted to localStorage)
├── hooks/        # Thin wrappers over store selectors
├── components/   # Shared UI (ProtectedRoute)
├── pages/        # One file per route
├── types/        # TypeScript interfaces matching backend DTOs
└── lib/          # cn() utility only
```

### Auth Flow

`LoginPage` → `loginUser()` (Axios POST `/api/auth/login`) → `authStore.setAuth(token, user)` → role-based `navigate()`.

The store decodes the JWT itself to extract the role. The .NET backend serialises `ClaimTypes.Role` to the key `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` inside the JWT payload — this is handled in `authStore.ts`. **Never re-decode the JWT at call sites** — always read `useAuthStore.getState().role` or the `role` value from `useAuth()`.

### Adding New API Modules

Create `src/api/{domain}.api.ts` importing `apiClient` from `./client`. The client automatically attaches the Bearer token and handles 401s (except for `/auth/*` endpoints — those propagate errors to the UI for form display).

### Adding New Pages

1. Create `src/pages/{Name}Page.tsx`
2. Add the route in `src/App.tsx` — wrap in `<ProtectedRoute>` (authenticated) or `<ProtectedRoute requireAdmin>` (admin only)
3. For data fetching use TanStack Query (`useQuery` / `useMutation`) — do not call `apiClient` directly from components

### State Management Rules

- **Auth state** lives exclusively in `authStore`. Never duplicate token or role in component state.
- Access auth in components via `useAuth()` hook — it uses a single shallow-equality selector and avoids excess re-renders.
- **Server state** (API data) belongs in TanStack Query, not Zustand.
- Do not call `logout()` or any Zustand `set()` during a component's render body — use `useEffect` or event handlers.

### Route Protection

`ProtectedRoute` checks `isAuthenticated` and client-side token expiry. It does **not** call `logout()` during render (React rule). Actual cleanup happens in the Axios 401 interceptor (`client.ts`) and in `onRehydrateStorage` (`authStore.ts`).

### Design System

All auth pages follow the **Napoletana** theme — dark coal background (`#1C1A17`), parchment card (`#F5ECD7`), terracotta accent (`#C44536`). Fonts loaded via Google Fonts in `index.html`: **Bodoni Moda** for display headings (always italic), **DM Sans** for body/UI. Animations are CSS keyframes defined in `index.css` (`slideUp`, `fadeIn`, `rotateSlow`). Apply them via inline `style={{ animation: '...' }}` with `opacity: 0` as initial state so staggered delays work correctly.
