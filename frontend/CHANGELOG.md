# Changelog

All notable changes to the PizzaStore Frontend will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.1] - 2026-04-06

### 🔒 Security

- **Upgraded Axios `1.7.9` → `1.14.0`** (exact pin, no `^`) — `1.7.9` had 3 high-severity CVEs:
  - [GHSA-jr5f-v2jv-69x6](https://github.com/advisories/GHSA-jr5f-v2jv-69x6) — SSRF and credential leakage via absolute URL
  - [GHSA-4hjh-wcwx-xvwj](https://github.com/advisories/GHSA-4hjh-wcwx-xvwj) — DoS via unchecked data size
  - [GHSA-43fc-jf86-j433](https://github.com/advisories/GHSA-43fc-jf86-j433) — DoS via `__proto__` key in `mergeConfig`
- `1.14.0` is the last known-clean release. `1.14.1` (current `latest` tag) remains compromised by the 2026-03-31 supply-chain attack — `npm audit fix --force` was deliberately avoided as it resolves against the `latest` tag.
- `npm audit` now reports **0 vulnerabilities**.

---

## [0.1.0] - 2026-04-06

### ✨ Added

#### Project Scaffold
- **Vite + React 18 + TypeScript** project initialised from scratch (`vite.config.ts`, `tsconfig.json`, `tsconfig.node.json`, `postcss.config.js`)
- **Tailwind CSS v3** with custom Napoletana design tokens — `coal`, `parchment`, `terracotta`, `ash` palette; `Bodoni Moda` display font + `DM Sans` body font
- **Path alias** `@/` → `src/` configured in both Vite and TypeScript
- **Dev proxy** — `/api/*` forwarded to `https://localhost:5001` to avoid CORS in development

#### Authentication Layer
- **`src/types/auth.ts`** — `UserInfo`, `AuthResponse`, `LoginDto`, `RegisterDto` TypeScript types matching the backend DTOs
- **`src/api/client.ts`** — Axios instance (`baseURL: '/api'`) with:
  - Request interceptor: auto-attaches `Authorization: Bearer {token}` from Zustand store
  - Response interceptor: on `401` from non-auth endpoints → `logout()` + redirect to `/login` (auth endpoints are excluded to preserve form error messages)
- **`src/api/auth.api.ts`** — `loginUser(dto)` and `registerUser(dto)` wrappers over `POST /api/auth/login` and `POST /api/auth/register`
- **`src/store/authStore.ts`** — Zustand store with `persist` middleware:
  - Stores `token`, `user`, `role`, `isAuthenticated`
  - `setAuth()` validates token expiry and extracts `.NET ClaimTypes.Role` from the JWT payload (`http://schemas.microsoft.com/ws/2008/06/identity/claims/role`)
  - `logout()` clears all auth state
  - `onRehydrateStorage` uses `useAuthStore.setState()` directly to clear expired tokens on hydration (avoids unsafe mid-hydration side effects)
- **`src/hooks/useAuth.ts`** — Single shallow-equality Zustand selector (one subscription, not six) exposing `token`, `user`, `role`, `isAuthenticated`, `isAdmin`, `setAuth`, `logout`

#### Routing & Route Protection
- **`src/App.tsx`** — React Router v6 route tree:
  - Public routes: `/login`, `/register`
  - Authenticated routes: `/` (wrapped in `<ProtectedRoute>`)
  - Admin-only routes: `/admin` (wrapped in `<ProtectedRoute requireAdmin>`)
  - Catch-all `*` → redirect to `/login`
- **`src/components/ProtectedRoute.tsx`** — Route guard component:
  - Redirects unauthenticated users to `/login`
  - Checks client-side token expiry without calling `logout()` during render (React side-effect rule compliant)
  - `requireAdmin` prop redirects non-Admin users to `/`

#### Pages
- **`src/pages/LoginPage.tsx`** — Napoletana-themed login page:
  - Dark coal background with rotating pizza wheel SVG watermark
  - Warm parchment form card with drop shadow and spring-eased entrance animation
  - Bodoni Moda italic "Benvenuto" heading
  - Email + password fields with staggered `slideUp` animations and show/hide password toggle
  - Terracotta CTA button ("Entra — Sign In") with glow shadow
  - Server error banner with `AlertCircle` icon
  - Zod schema validation matching backend FluentValidation rules
  - Post-login role-based redirect: `Admin` → `/admin`, `User` → `/`
  - Role read from `useAuthStore.getState().role` (no duplicate JWT decode)
- **`src/pages/RegisterPage.tsx`** — Napoletana-themed registration page:
  - Same visual system as `LoginPage` with counter-rotating pizza watermark
  - Bodoni Moda italic "Unisciti" heading
  - Two-column first/last name row + email + password fields with staggered animations
  - Password validation: min 6 chars, uppercase, lowercase, digit (mirrors `RegisterUserDtoValidator`)
  - On success: `setAuth()` + navigate to `/` (backend returns JWT immediately on registration)
- **`src/pages/HomePage.tsx`** — Authenticated user placeholder (welcome message + sign out)
- **`src/pages/AdminPage.tsx`** — Admin placeholder (dashboard coming soon + sign out)

#### Design System
- **`src/index.css`** — Tailwind base, global styles:
  - Grain texture overlay via SVG `feTurbulence` `::after` pseudo-element (2.8% opacity, fixed position)
  - `slideUp`, `fadeIn`, `rotateSlow`, `spinLoader` keyframe animations
  - `animate-spin-slow` utility class (60s rotation)
  - `spin-loader` utility class for button loading spinner
- **`src/lib/utils.ts`** — `cn()` helper combining `clsx` + `tailwind-merge`
- **`index.html`** — Google Fonts preconnect + Bodoni Moda / DM Sans `<link>`, SVG pizza-slice favicon

#### Dependencies
- React 18.3.1, React DOM 18.3.1
- React Router DOM 6.28.0
- TanStack Query 5.64.0 (configured with 5-minute stale time, 1 retry)
- Zustand 5.0.3 + persist middleware
- Axios **1.14.0** (exact pin — see security note below; upgraded from initial `1.7.9` in v0.1.1)
- React Hook Form 7.54.2 + `@hookform/resolvers` 3.9.1
- Zod 3.24.1
- jwt-decode 4.0.0
- Radix UI Label 2.1.1, Slot 1.1.1
- Lucide React 0.469.0
- clsx 2.1.1, tailwind-merge 2.6.0, class-variance-authority 0.7.1
- Tailwind CSS 3.4.17, Autoprefixer 10.4.20, PostCSS 8.4.49

### 🔒 Security

#### Axios Supply-Chain Pin
- **Axios pinned to exactly `1.7.9`** — `^` caret removed from `package.json`
- Axios versions `1.14.1` (`latest` tag) and `0.30.4` (`legacy` tag) are compromised by a supply-chain attack discovered 2026-03-31 (hijacked npm maintainer account `jasonsaayman`)
- The malicious versions install `plain-crypto-js` via a `postinstall` script which downloads a multi-platform backdoor from `sfrclak.com:8000`
- macOS payload disguised as Apple daemon at `/Library/Caches/com.apple.act.mond`
- **Machine verified clean** — no `plain-crypto-js` or malware binary found on this system
- npm cache verified with `npm cache verify` — no compromised packages cached
- Do not upgrade Axios until a clean version is published by the axios maintainers

### 🐛 Fixed (post-review)

The following issues were identified and resolved during the initial QA review:

- **Critical:** `authStore.ts` — `state.logout()` inside `onRehydrateStorage` replaced with `useAuthStore.setState()` to avoid unsafe mid-hydration state mutation
- **Critical:** `ProtectedRoute.tsx` — removed `logout()` call from component render body (React rules violation); component now only redirects without triggering side effects
- **Critical:** `LoginPage.tsx` — removed duplicate `jwtDecode` + `ROLE_CLAIM` logic; role is now read from `useAuthStore.getState().role` after `setAuth()` completes
- **High:** `client.ts` — 401 interceptor now skips `/auth/*` endpoints so invalid-credential errors surface as form error messages instead of triggering redirect
- **High:** `useAuth.ts` — replaced six independent `useAuthStore()` calls with a single shallow-equality selector, reducing subscriptions from 6 to 1 per component
- **High:** `App.tsx` — catch-all `*` route changed from `<Navigate to="/" />` (a guarded path) to `<Navigate to="/login" />`

### ✅ Verification

- ✅ **Build:** TypeScript compilation clean (0 errors)
- ✅ **Proxy:** `/api/*` → `https://localhost:5001` configured
- ✅ **Auth flow:** Login → JWT decode → role-based redirect implemented end-to-end
- ✅ **Security:** Axios supply-chain vulnerability assessed and mitigated
- ✅ **Machine:** No compromise indicators found
