# Changelog

All notable changes to the PizzaStore Frontend will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.7.0] - 2026-05-02

### ✨ Added

#### Full Admin Dashboard (`src/pages/AdminPage.tsx` — full rewrite)

- Replaced "Admin panel coming soon" placeholder with a complete, production-ready tabbed dashboard.

**Tab switcher**
- Terracotta pill buttons: **Users** and **Orders**, matching the pizza-type filter pill pattern from `HomePage.tsx`.

**Users tab**
- Client-side searchable table (Username / Email / Phone / Roles columns) built with CSS Grid — no HTML `<table>`
- Live search filters by username or email across the already-fetched list
- Row count shown below the table (`X of Y users matching search`)
- Click any row → **User Detail Drawer** slides in from the right

**User Detail Drawer** (`CartDrawer` shell pattern)
- Displays: username (Bodoni Moda italic heading), email, phone, roles as terracotta pill badges, full user ID (monospace)
- Fetches the selected user's order history on open (`GET /api/admin/users/{id}/orders`)
- Compact order rows: status badge, date, short order ID (`#XXXXXXXX`), total price

**Orders tab**
- Filter bar (identical card style to `OrderHistoryPage`): Status, User ID (text), From Date, To Date, "Clear filters" button (conditional)
- Server-side pagination with Prev / Next chevrons and "Showing X–Y of Z orders" range label
- CSS Grid table: Order ID / User / Date / Total / Status columns
- **Inline status update** — each row has a status `<select>` that fires `PUT /api/admin/orders/{id}/status`
  - Optimistic cache update via `queryClient.setQueryData` on success
  - Loading spinner replaces the dropdown while the mutation is in-flight for that specific row
  - `Delivered` and `Cancelled` orders have the dropdown **disabled** (`opacity: 0.5; cursor: not-allowed`) — backend enforces this as a terminal state
- Background re-fetch dim (same `opacity: 0.55` pattern as `OrderHistoryPage`)

**Toast notifications** (new minimal component, inline in file)
- `position: fixed; bottom: 24px; right: 24px; zIndex: 400`
- `success` (green dot/border) and `error` (terracotta dot/border) variants
- Auto-dismisses after 3.5 s; manual dismiss via ✕ button
- Slide-up entrance animation (`slideUp` keyframe)
- Fired for status update success and error

#### New API Module (`src/api/admin.api.ts` — new file)
- `getAllUsers()` — `GET /api/admin/users` → `AdminUser[]`
- `getUserById(id)` — `GET /api/admin/users/{id}` → `AdminUser`
- `getOrdersByUserId(id)` — `GET /api/admin/users/{id}/orders` → `Order[]`
- `getAllOrders(params?)` — `GET /api/admin/orders` → `PagedResult<Order>` (now paginated)
- `updateOrderStatus(orderId, newStatus)` — `PUT /api/admin/orders/{id}/status` → `Order`
- Exports `AdminOrderFilterParams` interface

#### New TypeScript Type (`src/types/admin.ts` — new file)
- `AdminUser` interface: `id`, `userName`, `email`, `phoneNumber`, `roles: string[]`

### 🔧 Changed

- **`src/pages/AdminPage.tsx`** — Completely replaced Tailwind stub with inline-style Napoletana implementation (all other pages use inline styles; the stub was the only outlier)
- **`src/App.tsx`** — No route change required; `/admin → <AdminPage />` inside `<ProtectedRoute requireAdmin />` was already in place

### 🔒 Backend: `GET /api/admin/orders` — Server-Side Pagination Added

The admin orders endpoint was previously unpaginated (returned `List<OrderDto>`). It now returns `PagedResult<OrderDto>` with the same shape used by `GET /api/order`. See backend CHANGELOG for details.

### ✅ Verification

- ✅ **Build:** TypeScript compilation clean (0 errors, `tsc --noEmit`)

---

## [0.6.0] - 2026-05-02

### ✨ Added

#### Order History Filtering & Pagination (`src/pages/OrderHistoryPage.tsx`)
- **Filter bar** — dark card with three inputs:
  - Status dropdown: "All Statuses", Pending, Confirmed, Preparing, Out for Delivery, Delivered, Cancelled (native `<select>`, styled dark with parchment text)
  - "From date" labeled date picker (native `<input type="date">`, visually labeled for clarity)
  - "To date" labeled date picker (native `<input type="date">`, visually labeled for clarity)
  - "Clear filters" button appears only when at least one filter is active (hidden otherwise)
- **Immediate filtering** — Filters apply instantly on selection/input change (no "Apply Filters" button); TanStack Query `queryKey: ['orders', filters]` re-fetches on any filter change
- **Pagination controls** — Always-visible pagination bar showing:
  - "Showing X–Y of Z orders" result count
  - "Page X of Y" indicator
  - Prev/Next buttons (disabled and visually muted when not applicable: prev disabled on page 1, next disabled on last page)
- **Empty state with filters** — When filters are active and no orders match: "No orders match your filters" heading + "Try adjusting or clearing the filters above" message + "Clear Filters" button (styled as terracotta CTA)
- **Order count in header** — "X order(s) placed" text shows total count from server (only visible when not loading and totalCount > 0)

### 🔧 Changed

- **Backend-driven pagination** — `GET /api/order` now accepts optional query params:
  - `status` (OrderStatus enum value or null for all statuses)
  - `fromDate` (ISO date string, inclusive boundary)
  - `toDate` (ISO date string, inclusive boundary; server adds 1 day for boundary matching)
  - `page` (default 1; clamped to min 1)
  - `pageSize` (default 10; clamped to min 1, max 100)
- **Response format** — `GET /api/order` now returns `PagedResult<OrderDto>` containing:
  - `items: OrderDto[]` (paginated list, sorted by createdAt descending)
  - `totalCount: int` (total matching orders across all pages)
  - `page: int` (current page number, 1-based)
  - `pageSize: int` (orders per page)
  - `totalPages: int` (calculated as ⌈totalCount / pageSize⌉)
- **API client** — `src/api/order.api.ts`:
  - `getMyOrders()` now accepts optional `OrderFilterParams` with status, fromDate, toDate, page, pageSize
  - Return type changed from `Order[]` to `PagedResult<Order>`
- **Page header order count** — Updated to display server-reported `totalCount` instead of client-side `orders.length`

### ✅ Verification

- ✅ **Build:** TypeScript compilation clean (0 errors, `tsc --noEmit`)
- ✅ **Testing:** Playwright verified:
  - Date picker labels ("From date", "To date") are visible and clear
  - Status filter applies immediately without Apply button
  - Date filters apply immediately on input
  - Pagination controls always visible (even with 1 order); disabled states visually distinct
  - "Clear filters" button appears/disappears based on filter state
  - Empty state messaging correct for active filters vs. no orders

---

## [0.5.0] - 2026-05-01

### ✨ Added

#### Toppings Browser Drawer (`src/components/ToppingsDrawer.tsx` — new)
- Right-side slide-in panel (400px, same `cubic-bezier(0.32,0,0.24,1)` transition as `CartDrawer`) triggered by a "View All Toppings" button in the menu section header
- Lists all toppings (available and unavailable); available toppings show name + price in full parchment; unavailable toppings are greyed-out at 50% opacity with an "Unavailable" badge
- Footer shows available count: `X topping(s) available`
- **Public** — no authentication required; accessible to guests and signed-in users alike
- Shares the `['toppings']` TanStack Query cache key with `PizzaCard` — opening the drawer while pizza cards are mounted never fires a duplicate `GET /api/topping` request

#### "View All Toppings" Button (`src/pages/HomePage.tsx`)
- Ghost pill button added to the right side of the "Our Menu" section header
- Opens `ToppingsDrawer` on click; visible to all users

### 🔧 Changed

#### Server-Side Pizza Type Filtering (`src/pages/HomePage.tsx`)
- Pizza type filter buttons now call `GET /api/pizza/type/{type}` via `getPizzasByType()` instead of filtering the already-fetched full list client-side
- Query key updated from `['pizzas']` to `['pizzas', activeType]` — each type gets its own cache entry; switching back to a previously-seen type is instant from cache
- Selecting "All" continues to call `GET /api/pizza` (via `getAllPizzas`); its cache key is `['pizzas', 'All']`
- Removed the client-side `filteredPizzas` derived variable; JSX uses the query result directly
- Existing `isLoading` spinner covers in-flight type-filter fetches with no UI change needed

### ✅ Verification

- ✅ **Build:** TypeScript compilation clean (0 errors, `tsc --noEmit`)

---

## [0.4.1] - 2026-05-01

### 🐛 Fixed

- **`src/pages/HomePage.tsx`** — Pizza card grid layout shift when expanding toppings or special instructions panel. CSS Grid's default `align-items: stretch` caused every card in the same row to grow to the height of the tallest card; adding `alignItems: 'start'` to the grid container makes each card size independently so expanding one card never affects its neighbours.

### ✅ Verification

- ✅ **Build:** TypeScript compilation clean (0 errors, `tsc --noEmit`)
- ✅ **Manual:** Playwright screenshots confirmed adjacent cards are unaffected when toppings or special instructions are expanded on any card

---

## [0.4.0] - 2026-04-30

### ✨ Added

#### Profile Page (`src/pages/ProfilePage.tsx` — new)
- Protected route at `/profile` (authenticated users only)
- Identity card: 72px avatar circle with user initials, full name (Bodoni Moda italic), email
- Account details grid: First Name, Last Name (from auth store), Email, User ID (from server, monospace)
- Roles panel: colour-coded dot badges per role — gold for Admin, muted parchment for all others; "No roles assigned" fallback when empty
- Data fetched fresh from `GET /api/auth/me` via TanStack Query (`['auth', 'me']` key, 5-minute stale time, `retry: false`)
- Staggered `fadeIn` entrance animations on each panel section
- Loading spinner + "Loading profile…" text while fetching; "Could not load profile data." error state

#### New API Function
- **`src/api/auth.api.ts`** — `getMe()` — `GET /api/auth/me` returning `{ userId, email, roles: string[] }`

#### New TypeScript Type
- **`src/types/auth.ts`** — `MeResponse` interface (`userId: string`, `email: string`, `roles: string[]`) matching the `GET /api/auth/me` response shape

### 🔧 Changed

- **`src/App.tsx`** — Added `/profile` route inside the existing authenticated `<ProtectedRoute>` outlet block
- **`src/components/Navbar.tsx`** — Added "Profile" link (`UserCircle` icon) in the user dropdown above "Sign out"; navigates to `/profile` and closes the dropdown

### 🔒 Security / Bug Fix

- **`src/api/client.ts`** — Fixed overly broad 401 interceptor skip condition: previously skipped all `/auth/*` endpoints, meaning `GET /api/auth/me` 401s (expired/invalid token) would not trigger auto-logout. Now only `/auth/login` and `/auth/register` are excluded, which restores the intended auto-logout-on-401 behaviour for all other auth-namespace endpoints.

### ✅ Verification

- ✅ **Build:** TypeScript compilation clean (0 errors, `tsc --noEmit`)

---

## [0.3.1] - 2026-04-30

### ✨ Added

#### Special Instructions on Pizza Cards (`src/pages/HomePage.tsx`)
- Collapsible "Add special request" button below the topping selector on each `PizzaCard`
- Expands a textarea (max 500 chars) to capture per-item cooking notes (e.g. "extra crispy, no onions")
- `specialInstructions` passed to `addToCart()` on "Add to Cart"; field resets after successful add
- Order Detail Page already renders special instructions in italics below the item name (in place since v0.3.0)

#### Direct Quantity Setter in Cart Drawer (`src/components/CartDrawer.tsx`)
- Replaces the static quantity span with a `<input type="number">` (min 1, max 99) inline in each cart row
- `blur` or `Enter` fires `PUT /api/cart/items/{id}` via `updateCartItem()` to set quantity directly without step-clicking `−`/`+`
- Special instructions displayed as an amber pill beneath the item name; inline edit flow (textarea + Save / Cancel) also calls `PUT /api/cart/items/{id}` atomically

#### New API Function
- **`src/api/cart.api.ts`** — `updateCartItem(itemId, dto)` — `PUT /api/cart/items/{id}` accepting `{ quantity?, specialInstructions? }`

#### New TypeScript Type
- **`src/types/cart.ts`** — `UpdateCartItemDto` (`quantity?: number`, `specialInstructions?: string`)

### ✅ Verification

- ✅ **Build:** TypeScript compilation clean (0 errors, `tsc --noEmit`)

---

## [0.3.0] - 2026-04-09

### ✨ Added

#### Topping Selection on Pizza Cards (`src/pages/HomePage.tsx`)
- Each `PizzaCard` now fetches available toppings via `GET /api/topping` (TanStack Query, 5-minute stale time, shared cache across all cards via `['toppings']` key)
- Collapsible "Add toppings" toggle button below the size selector — highlights gold when toppings are selected
- 2-column topping grid with toggle buttons: name + `+$price`; selected state uses gold (`#D4A44C`) border and background
- Live price update: base variant price + sum of selected topping prices displayed under the main price
- "incl. +$X.XX toppings" sub-label shown when at least one topping is selected
- `toppingIds: string[]` passed to `addToCart()` on "Add to Cart" — topping selections cleared on successful add
- No impact on unauthenticated users: topping UI hidden (sign-in gate still applies)

#### Checkout Page (`src/pages/CheckoutPage.tsx` — new)
- Protected route at `/checkout` (authenticated users only)
- Displays full cart summary: per-item pizza name, size, quantity, topping list, subtotal; grand total
- "Place Order" button triggers `POST /api/order/checkout` (no body — uses server-side cart)
- Loading spinner during checkout mutation; button disabled and dimmed while pending
- Error banner on failure (400 empty cart, generic server error)
- Success state: animated confirmation card with green `CheckCircle` icon, Italian "Grazie!" heading, order ID + total, "Track Order" → `/orders/{id}` and "Continue Shopping" → `/` buttons
- Invalidates `['cart']` and `['orders']` query keys on success

#### Order History Page (`src/pages/OrderHistoryPage.tsx` — new)
- Protected route at `/orders` (authenticated users only)
- Fetches all user orders via `GET /api/order`, sorted by `createdAt` descending
- Each order row: status badge, date/time, pizza name summary (first 2 items + "+N more"), short order ID, total price, chevron arrow
- Status badges: colour-coded dot + label for all 6 statuses (Pending gold, Confirmed blue, Preparing terracotta, OutForDelivery purple, Delivered green, Cancelled ash)
- Rows are clickable, navigate to `/orders/{id}`; hover: lift + terracotta border glow
- Loading spinner and empty state (with "Order Now" CTA)

#### Order Detail Page (`src/pages/OrderDetailPage.tsx` — new)
- Protected route at `/orders/:id` (authenticated users only)
- Fetches single order via `GET /api/order/{id}`; handles 404 and error states
- Header: short order ID, status badge, creation date, total price
- Status progress timeline (horizontal step indicator) for non-cancelled orders: Pending → Confirmed → Preparing → Out for Delivery → Delivered; past steps green, active step terracotta with outline ring, future steps muted
- Timestamps panel: Confirmed At, Delivered At, Cancelled At (shown only if set)
- Items table: pizza name, size, quantity, base price per row; topping chips (gold badge per topping with name + price); special instructions in italics; subtotal per row; total footer row
- Cancel Order button: shown only when status is `Pending` or `Confirmed`; opens confirmation modal
- Cancel modal: backdrop blur overlay, "Yes, Cancel Order" / "Keep Order" buttons; mutation via `POST /api/order/{id}/cancel`; invalidates `['orders', id]` and `['orders']` on success; error banner on failure

#### New API Modules
- **`src/api/topping.api.ts`** — `getAllToppings()` (`GET /api/topping`), `getToppingById(id)` (`GET /api/topping/{id}`)
- **`src/api/order.api.ts`** — `checkoutCart()` (`POST /api/order/checkout`), `getMyOrders()` (`GET /api/order`), `getOrderById(id)` (`GET /api/order/{id}`), `cancelOrder(id)` (`POST /api/order/{id}/cancel`)

#### New TypeScript Types
- **`src/types/topping.ts`** — `Topping` (`id`, `name`, `price`, `isAvailable`)
- **`src/types/order.ts`** — `OrderStatus` union type, `OrderItemTopping`, `OrderItem`, `Order` interfaces matching backend `OrderDto` / `OrderItemDto` / `OrderItemToppingDto` (all price fields use snapshot-at-order-time naming)

### 🔧 Changed

- **`src/App.tsx`** — Added three new protected routes: `/checkout` (`CheckoutPage`), `/orders` (`OrderHistoryPage`), `/orders/:id` (`OrderDetailPage`); all wrapped in a single `<ProtectedRoute>` outlet block
- **`src/components/Navbar.tsx`** — Added "Orders" nav link (authenticated users only, `ClipboardList` icon) between the admin link and cart button; links to `/orders`; hover: muted ash → parchment with subtle border
- **`src/components/CartDrawer.tsx`** — "Proceed to Checkout" button now navigates to `/checkout` (via `useNavigate`) and closes the drawer on click; previously a non-functional stub
- **`src/pages/HomePage.tsx`** — `PizzaCard.onAddToCart` signature updated from `(variantId: string) => void` to `(variantId: string, toppingIds: string[]) => void`; `handleAddToCart` in `HomePage` passes `toppingIds` through to `addToCart` DTO

### 🔒 Security

- **Upgraded Axios `1.14.0` → `1.15.0`** (exact pin maintained) — `1.15.0` is the first confirmed-clean release post supply-chain attack and fixes `GHSA-3p68-rc4w-qgx5` (NO_PROXY hostname normalisation bypass → SSRF, published 2026-04-09, Critical severity)
- `npm audit` now reports **0 vulnerabilities**
- `CLAUDE.md` security note updated with full attack timeline, attribution (UNC1069, North Korea-nexus, Google TIG 2026-04-01), and per-version safety status

### ✅ Verification

- ✅ **Build:** TypeScript compilation clean (0 errors, `tsc --noEmit`)
- ✅ **Audit:** `npm audit` — 0 vulnerabilities with Axios 1.15.0

---

## [0.2.0] - 2026-04-09

### ✨ Added

#### Public Homepage (`src/pages/HomePage.tsx` — full rewrite)
- Cinematic hero section with animated decorative rings, Bodoni Moda italic heading, tagline, and dual CTAs ("Explore Our Menu" scroll + "Create Account" → `/register` for unauthenticated users)
- Animated "SCROLL" indicator in hero
- Stats bar: 7 categories · 4 sizes · 100% Italian · Daily Fresh
- 8-category filter pills (All, Margherita, Supreme, MeatLovers, Hawaiian, Vegetarian, Veggie, Custom) with active highlight
- Responsive pizza grid — `PizzaCard` sub-component per pizza with:
  - Type-specific CSS linear gradient placeholder (used when `imageUrl` is null or fails to load via `onError`)
  - Size selector buttons (S / M / L / XL) — active selection highlighted
  - Dynamic price display updating on size change
  - "Add to Cart" button (authenticated) / "Sign in to Order" button (unauthenticated, redirects to `/login`)
  - "Added!" 2.2s green confirmation feedback state after successful cart add
  - Drawer opens automatically after add
- Footer: Napoletana branding + tagline
- Homepage is now **public** — accessible to all three roles without authentication

#### Navbar (`src/components/Navbar.tsx` — new)
- Fixed top bar with scroll-aware frosted-glass background (activates after 20px scroll)
- Flame + "Napoletana" logo link → `/`
- Cart icon button (authenticated only) with live item-count badge
- Profile dropdown (authenticated only): avatar circle with initial, full name display, email, Admin role badge (admin only), gold "Admin Panel" link (admin only), "Sign out" button
- "Sign in" link shown to unauthenticated users

#### Cart Drawer (`src/components/CartDrawer.tsx` — new)
- Right-side slide-in panel (420px, `cubic-bezier(0.34,1.56,0.64,1)` spring animation)
- Dark overlay backdrop — click to close
- Per-item row: name, size, quantity `−`/`+` controls, trash icon delete, subtotal price
- Footer: item count, total price, "Proceed to Checkout" CTA, "Clear cart" link
- Empty state: pizza emoji + message
- All mutations (`addToCart`, `increaseQuantity`, `decreaseQuantity`, `removeFromCart`, `clearCart`) invalidate `['cart']` query key, keeping navbar badge in sync

#### API Modules
- **`src/api/pizza.api.ts`** — `getAllPizzas()`, `getPizzaById(id)`, `getPizzasByType(type)` over `GET /api/pizza`
- **`src/api/cart.api.ts`** — `getCart()`, `addToCart(dto)`, `removeFromCart(itemId)`, `clearCart()`, `increaseQuantity(itemId)`, `decreaseQuantity(itemId)`

#### TypeScript Types
- **`src/types/pizza.ts`** — `PizzaType`, `PizzaSize`, `PizzaVariant`, `Pizza` interfaces matching backend `PizzaDto`
- **`src/types/cart.ts`** — `CartItemTopping`, `CartItem`, `Cart`, `AddToCartDto` interfaces matching backend `CartDto`/`CartItemDto`

### 🔧 Changed

- **`src/App.tsx`** — `/` route is now public (removed `<ProtectedRoute>` wrapper); catch-all `*` redirects to `/` instead of `/login`
- **`src/index.css`** — added `scaleIn` and `slideInRight` keyframe animations used by cart drawer and pizza card confirmation state

### 🐛 Fixed

- **`src/hooks/useAuth.ts`** — replaced Zustand v4-style `useAuthStore(selector, shallow)` two-argument call (removed in Zustand v5) with `useAuthStore(useShallow(selector))` using `useShallow` from `zustand/react/shallow`. Eliminated 7 TypeScript errors with no behaviour change.

### ✅ Verification

- ✅ **Build:** TypeScript compilation clean (0 errors)
- ✅ **Playwright:** 37/37 test cases passed across all three roles (unauthenticated, regular user, admin)
- ✅ **Cart:** real-time badge sync across navbar, drawer, and pizza cards confirmed
- ✅ **Auth guards:** expired JWT → ProtectedRoute redirect confirmed; `/admin` blocked for non-admin role confirmed

---

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
- Axios **1.15.0** (exact pin — see security note in CLAUDE.md; `1.14.0` was the initial clean pin in v0.1.1, upgraded to `1.15.0` in v0.3.0)
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
