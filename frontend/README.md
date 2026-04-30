# 🍕 PizzaStore - React + Vite Frontend

A production-ready React frontend for the PizzaStore pizza ordering system, built with **Vite**, **TypeScript**, **Tailwind CSS**, and a **"Napoletana"** artisan-pizzeria design system. Features JWT authentication, role-based routing, a fully functional public homepage with pizza browsing, category filtering, and a real-time shopping cart.

## 📋 Table of Contents
- [Overview](#-overview)
- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Getting Started](#-getting-started)
- [Project Structure](#-project-structure)
- [Pages & Routing](#-pages--routing)
- [Authentication & Authorization](#-authentication--authorization)
- [Design System](#-design-system)
- [API Integration](#-api-integration)
- [State Management](#-state-management)

## 🌟 Overview

PizzaStore Frontend is a React SPA that targets the [PizzaStore Backend API](../backend/README.md). It provides a pizza/food themed UI with JWT authentication, role-based route protection (User vs. Admin), and a scalable architecture built to support all 6 backend controllers and 31 endpoints.

## 🎯 Features

### Implemented
- ✅ **Login Page** — Artisan Napoletana design, JWT login, role-based redirect
- ✅ **Register Page** — Full registration form with client-side validation mirroring backend rules
- ✅ **JWT Auth** — Token stored in `localStorage` via Zustand persist, auto-hydrated on app load
- ✅ **Role-Based Routing** — `User` redirected to `/`, `Admin` redirected to `/admin` after login
- ✅ **Protected Routes** — `<ProtectedRoute>` and `<ProtectedRoute requireAdmin>` wrappers
- ✅ **Token Expiry Handling** — Client-side expiry check on route guard and store hydration
- ✅ **401 Interceptor** — Axios response interceptor auto-clears auth and redirects to `/login`
- ✅ **Public Homepage** — Accessible to all three roles (unauthenticated, user, admin)
  - Cinematic hero section with animated decorative rings and scroll CTA
  - Stats bar (categories, sizes, quality indicators)
  - 8-category filter pills with live pizza grid filtering
  - Pizza cards with size selector, topping selector, live price update, "Add to Cart" / "Sign in to Order" (role-aware), "Added!" 2.2s confirmation
  - Cart drawer — add, increase/decrease quantity, remove item, clear cart, "Proceed to Checkout" navigates to `/checkout`
- ✅ **Topping Selection** — Collapsible per-card topping grid; gold highlight on selection; live price update includes topping cost; `toppingIds` sent to cart API
- ✅ **Navbar** — Fixed top bar, scroll-aware frosted-glass blur, cart badge (real-time quantity), "Orders" link, profile dropdown (email, Admin badge, sign out), admin-only panel link
- ✅ **Cart Integration** — TanStack Query `['cart']` cache invalidated on every mutation; badge stays in sync across drawer and pizza cards
- ✅ **Checkout Page** — Order review with item list and totals; `POST /api/order/checkout`; success confirmation with order ID and "Track Order" link
- ✅ **Order History Page** — Full list of past orders sorted by date; colour-coded status badges; clickable rows navigate to order detail
- ✅ **Order Detail Page** — Full order breakdown with progress timeline, item/topping snapshot, timestamps, and "Cancel Order" with confirmation modal
- ✅ **Admin Page** — Protected placeholder (admin role required)

### Scaffold Ready
- 🔲 Admin Dashboard (users, orders, pizza/topping CRUD)

## 🛠️ Technology Stack

| Category | Library | Version |
|---|---|---|
| Framework | React | ^18.3.1 |
| Build Tool | Vite | ^6.0.7 |
| Language | TypeScript | ^5.7.3 |
| Styling | Tailwind CSS | ^3.4.17 |
| Routing | React Router v6 | ^6.28.0 |
| Server State | TanStack Query | ^5.64.0 |
| Client State | Zustand + persist | ^5.0.3 |
| HTTP Client | Axios | 1.15.0 (exact pin) |
| Forms | React Hook Form | ^7.54.2 |
| Validation | Zod | ^3.24.1 |
| Token Decode | jwt-decode | ^4.0.0 |
| UI Primitives | Radix UI | ^2.1.1 |
| Icons | Lucide React | ^0.469.0 |

> **Note on Axios version:** Axios is pinned to exactly `1.15.0` (no `^` caret). `1.15.0` is the current confirmed-clean release — it fixes `GHSA-3p68-rc4w-qgx5` (NO_PROXY SSRF, 2026-04-09) and was published by legitimate maintainers after the 2026-03-31 supply-chain attack that compromised `1.14.1` and `0.30.4`. Never run `npm audit fix --force`. See [CLAUDE.md](./CLAUDE.md#security-axios-version-lock) for full history.

## 🚀 Getting Started

### Prerequisites

- Node.js 20+
- npm 10+
- PizzaStore Backend running at `https://localhost:5001` (see [backend README](../backend/README.md))

### Setup

1. **Navigate to the frontend directory**
   ```bash
   cd PizzaStore/frontend
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start the development server**
   ```bash
   npm run dev
   ```
   The Vite dev server starts at `http://localhost:5173` and proxies all `/api/*` requests to `https://localhost:5001`.

4. **Build for production**
   ```bash
   npm run build
   ```

5. **Preview production build**
   ```bash
   npm run preview
   ```

### Backend Dependency

The frontend requires the backend API to be running. Start it first:
```bash
cd ../backend/src/PizzaStore.API
dotnet run
```

Default seed credentials available for testing:
- **Admin:** `admin@pizzastore.com` / `Admin123!` → redirects to `/admin`
- **User:** `user@pizzastore.com` / `User123!` → redirects to `/`

## 📐 Project Structure

```
frontend/
├── src/
│   ├── api/
│   │   ├── client.ts           # Axios instance — Bearer token injection, 401 handling
│   │   ├── auth.api.ts         # loginUser(), registerUser()
│   │   ├── pizza.api.ts        # getAllPizzas(), getPizzaById(), getPizzasByType()
│   │   ├── cart.api.ts         # getCart(), addToCart(), removeFromCart(), clearCart(), increase/decreaseQuantity()
│   │   ├── topping.api.ts      # getAllToppings(), getToppingById()
│   │   └── order.api.ts        # checkoutCart(), getMyOrders(), getOrderById(), cancelOrder()
│   ├── components/
│   │   ├── ProtectedRoute.tsx  # Role-aware route guard (requireAdmin prop)
│   │   ├── Navbar.tsx          # Fixed top bar — Orders link, cart badge, profile dropdown, scroll blur
│   │   └── CartDrawer.tsx      # Right-side slide-in cart panel — full CRUD + checkout navigation
│   ├── hooks/
│   │   └── useAuth.ts          # Single shallow-equality Zustand selector (useShallow)
│   ├── pages/
│   │   ├── LoginPage.tsx       # Napoletana auth UI — sign in
│   │   ├── RegisterPage.tsx    # Napoletana auth UI — create account
│   │   ├── HomePage.tsx        # Public homepage — hero, category filters, pizza grid + topping selector
│   │   ├── CheckoutPage.tsx    # Order review + place order + success confirmation (protected)
│   │   ├── OrderHistoryPage.tsx # Paginated order list with status badges (protected)
│   │   ├── OrderDetailPage.tsx  # Full order detail, progress timeline, cancel modal (protected)
│   │   └── AdminPage.tsx       # Admin dashboard (protected, placeholder)
│   ├── store/
│   │   └── authStore.ts        # Zustand store — token, user, role, expiry validation
│   ├── types/
│   │   ├── auth.ts             # UserInfo, AuthResponse, LoginDto, RegisterDto
│   │   ├── pizza.ts            # PizzaType, PizzaSize, PizzaVariant, Pizza
│   │   ├── cart.ts             # CartItemTopping, CartItem, Cart, AddToCartDto
│   │   ├── topping.ts          # Topping
│   │   └── order.ts            # OrderStatus, OrderItemTopping, OrderItem, Order
│   ├── lib/
│   │   └── utils.ts            # cn() — clsx + tailwind-merge helper
│   ├── App.tsx                 # Router — public, protected, admin-only routes
│   ├── main.tsx                # React entry point
│   └── index.css               # Tailwind base, grain texture, keyframe animations
├── index.html                  # Google Fonts (Bodoni Moda + DM Sans), favicon
├── package.json
├── vite.config.ts              # Path alias (@), /api proxy
├── tailwind.config.ts          # Napoletana palette, custom font families
├── tsconfig.json
├── tsconfig.node.json
└── postcss.config.js
```

## 🗺️ Pages & Routing

| Route | Component | Auth | Description |
|---|---|---|---|
| `/` | `HomePage` | Public | Pizza menu, hero, topping selector, cart (full features require auth) |
| `/login` | `LoginPage` | Public | Email + password login |
| `/register` | `RegisterPage` | Public | New account creation |
| `/checkout` | `CheckoutPage` | User | Order review, place order, success confirmation |
| `/orders` | `OrderHistoryPage` | User | All past orders with status badges |
| `/orders/:id` | `OrderDetailPage` | User | Order detail, progress timeline, cancel |
| `/admin` | `AdminPage` | Admin role | Admin dashboard (placeholder) |
| `*` | Redirect | — | Catch-all → `/` |

### Role-Based Redirect After Login

```
POST /api/auth/login
  → setAuth(token, user)              # store decodes role from JWT
  → role === "Admin" → navigate("/admin")
  → role === "User"  → navigate("/")
```

### Role-Aware Homepage Behaviour

| State | Hero Buttons | Pizza Cards | Navbar |
|---|---|---|---|
| Unauthenticated | Explore Our Menu + Create Account | "Sign in to Order" (no topping selector) | Sign in link |
| Regular User | Explore Our Menu | "Add to Cart" + topping selector | Orders link + cart badge + profile dropdown |
| Admin | Explore Our Menu | "Add to Cart" + topping selector | Orders link + cart badge + profile dropdown + Admin link |

## 🔐 Authentication & Authorization

### Flow

1. User submits login form → `POST /api/auth/login`
2. Backend returns `{ token, user }` — JWT is HMAC-SHA256, default 60-minute expiry
3. `authStore.setAuth()` decodes the token, extracts the role claim, persists to `localStorage`
4. All subsequent API calls automatically include `Authorization: Bearer {token}` via the Axios interceptor
5. On 401 from any non-auth endpoint → auto-logout and redirect to `/login`

### JWT Role Claim

The .NET backend uses `ClaimTypes.Role` which serialises to the full URI in the JWT payload:
```
http://schemas.microsoft.com/ws/2008/06/identity/claims/role
```
The `authStore` handles this key internally — consumers just read `role` from the store.

### Route Protection

```tsx
// Authenticated users only
<Route element={<ProtectedRoute />}>
  <Route path="/checkout" element={<CheckoutPage />} />
  <Route path="/orders" element={<OrderHistoryPage />} />
  <Route path="/orders/:id" element={<OrderDetailPage />} />
</Route>

// Admin role required
<Route element={<ProtectedRoute requireAdmin />}>
  <Route path="/admin" element={<AdminPage />} />
</Route>
```

Token expiry is checked client-side on every protected route render. Expired tokens redirect to `/login` without triggering side effects during render.

## 🎨 Design System

### Napoletana Theme

Inspired by Italian artisan pizzeria menus — warm, editorial, distinctive.

| Token | Value | Usage |
|---|---|---|
| `coal` | `#1C1A17` | Page background |
| `coal-light` | `#2A2721` | Elevated surfaces |
| `parchment` | `#F5ECD7` | Form cards, light surfaces |
| `parchment-warm` | `#FAF4E8` | Input backgrounds |
| `terracotta` | `#C44536` | Primary action, accent |
| `ash` | `#8B7E72` | Secondary text |
| `ash-border` | `#D5C8B4` | Input borders |

### Typography

| Role | Font | Weights |
|---|---|---|
| Display / Headings | Bodoni Moda (serif, italic) | 400, 700 |
| Body / UI | DM Sans | 300, 400, 500, 600 |

### Visual Effects

- **Grain texture** — fixed `::after` overlay via SVG `feTurbulence` filter (2.8% opacity)
- **Rotating pizza wheel** — SVG watermark, 60-second full rotation (`animate-spin-slow`)
- **Staggered form animations** — `slideUp` keyframe with `animation-delay` per field
- **Card entrance** — `slideUp` with `cubic-bezier(0.16,1,0.3,1)` spring easing
- **Terracotta glow button** — `box-shadow: 0 4px 18px rgba(196,69,54,0.38)`

## 🔌 API Integration

### Axios Client (`src/api/client.ts`)

- Base URL: `/api` (proxied to `https://localhost:5001` in development)
- **Request interceptor:** attaches `Authorization: Bearer {token}` from Zustand store
- **Response interceptor:** on 401 from non-auth endpoints → `logout()` + `window.location.href = '/login'`

### Adding New API Modules

Follow the established pattern:
```typescript
// src/api/pizza.api.ts
import { apiClient } from './client'
import type { Pizza } from '@/types/pizza'

export async function getAllPizzas(): Promise<Pizza[]> {
  const { data } = await apiClient.get<Pizza[]>('/pizza')
  return data
}
```

## 🗂️ State Management

### Auth Store (`src/store/authStore.ts`)

Zustand store with `persist` middleware — survives page refresh via `localStorage`.

| State | Type | Description |
|---|---|---|
| `token` | `string \| null` | Raw JWT string |
| `user` | `UserInfo \| null` | `{ id, firstName, lastName, email }` |
| `role` | `string \| null` | Extracted from JWT on `setAuth` |
| `isAuthenticated` | `boolean` | True when token present and not expired |

| Action | Description |
|---|---|
| `setAuth(token, user)` | Validates expiry, extracts role, sets all state |
| `logout()` | Clears all auth state |

### Server State

[TanStack Query](https://tanstack.com/query) is configured in `App.tsx` with sensible defaults (5-minute stale time, 1 retry). Use it for all API data fetching in feature pages.

## 📄 License

This is a demonstration project for learning purposes.

## 📖 Additional Documentation

- **README.md** (this file) — Frontend project documentation
- **CHANGELOG.md** — Version history and changes
- **[Backend README](../backend/README.md)** — API documentation and endpoint reference
