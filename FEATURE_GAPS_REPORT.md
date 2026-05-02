# PizzaStore — Backend-to-Frontend Feature Gap Report

**Date:** 2026-04-30  
**Branch:** `feature/add-topping-checkout-order-features`

---

## Overview

The following features are fully implemented on the backend but partially or completely missing from the frontend. Grouped by similarity for efficient implementation.

---

## Group A — Cart Enhancements

> Both items touch the same cart flow and can be implemented together.

### #5 · SpecialInstructions Field

**Status:** Backend complete. Frontend types & API layer ready. UI missing.

**Backend (complete):**
| File | Line | Detail |
|------|------|--------|
| `backend/src/.../Cart/Commands/AddPizzaToCart/AddPizzaToCartDto.cs` | 24 | `public string? SpecialInstructions { get; set; }` |
| `backend/src/.../Cart/Commands/UpdateCartItemQuantity/UpdateCartItemQuantityDto.cs` | 24 | `public string? SpecialInstructions { get; set; }` |
| `backend/src/.../Cart/Commands/AddPizzaToCart/CartItemDto.cs` | 56 | `public string? SpecialInstructions { get; set; }` — mapped in `FromEntity()` at line 96 |
| `backend/src/.../Controllers/CartController.cs` | 54–63 | `POST /api/cart/items` — accepts `AddPizzaToCartDto` with `SpecialInstructions` |
| `backend/src/.../Controllers/CartController.cs` | 98–114 | `PUT /api/cart/items/{cartItemId}` — accepts `UpdateCartItemQuantityDto` with `SpecialInstructions` |

**Frontend (partial):**
| File | Line | Detail |
|------|------|--------|
| `frontend/src/types/cart.ts` | 35 | `specialInstructions?: string` — type already defined |
| `frontend/src/types/cart.ts` | 15 | `specialInstructions: string \| null` — on `CartItem` type |
| `frontend/src/api/cart.api.ts` | 7–8 | `addToCart()` already passes the full `AddToCartDto` including `specialInstructions` |

**What is missing:**
1. `frontend/src/pages/HomePage.tsx` — No text input in the `PizzaCard` component to capture special instructions before calling `addToCart()`. Add between the toppings selector and the price/add button area.
2. `frontend/src/components/CartDrawer.tsx` — Cart item rows (~line 218) do not display the `specialInstructions` value returned by the API.
3. No inline edit UI for special instructions within the drawer.

**Acceptance criteria:**
- `<textarea maxLength={500}>` in pizza card, collapsible (hidden by default, toggled with a link like "Add special request").
- Instructions shown beneath item name in CartDrawer if non-null.
- Value passed through to checkout confirmation.

---

### #8 · PUT /api/cart/items/{id} — Direct Quantity Setter

**Status:** Backend complete. Frontend has no API wrapper; uses PATCH increment/decrement only.

**Backend (complete):**
| File | Line | Detail |
|------|------|--------|
| `backend/src/.../Controllers/CartController.cs` | 98–114 | `PUT /api/cart/items/{cartItemId}` — sets exact `Quantity` and optional `SpecialInstructions` in one call |

**Frontend (missing):**
| File | Line | Detail |
|------|------|--------|
| `frontend/src/api/cart.api.ts` | 16–17 | `increaseQuantity()` — `PATCH /cart/items/{id}/increase` |
| `frontend/src/api/cart.api.ts` | 19–20 | `decreaseQuantity()` — `PATCH /cart/items/{id}/decrease` |

No `updateCartItem(id, quantity, specialInstructions)` function exists.

**What is missing:**
1. Add `updateCartItem()` to `cart.api.ts` calling `PUT /api/cart/items/{id}`.
2. This becomes necessary if #5 is implemented with an edit flow in the CartDrawer (user edits instructions → single PUT call updates both quantity and instructions atomically).

**Note:** Increment/decrement PATCH endpoints remain valid for the `+/-` buttons. The PUT endpoint is the correct call when editing special instructions alongside quantity.

---

## Group C — User Profile & Identity

> Both items share the same data source (`GET /api/auth/me`) and can be delivered as one page.

### #6 · Profile Page (`/profile`)

**Status:** Backend endpoint complete. No frontend page, route, or API call exists.

**Backend (complete):**
| File | Line | Detail |
|------|------|--------|
| `backend/src/.../Controllers/AuthController.cs` | 61–77 | `GET /api/auth/me` — requires `[Authorize]` |

Response shape (anonymous object):
```json
{
  "userId": "string",
  "email": "string",
  "roles": ["string"]
}
```

**Frontend (missing):**
| File | Line | Detail |
|------|------|--------|
| `frontend/src/App.tsx` | — | No `/profile` route defined. Existing routes: `/`, `/login`, `/register`, `/admin`, `/checkout`, `/orders`, `/orders/:id` |
| `frontend/src/api/auth.api.ts` | — | Only `loginUser()` and `registerUser()` — no `getMe()` function |
| `frontend/src/pages/` | — | No `ProfilePage.tsx` file |

**What is missing:**
1. `frontend/src/api/auth.api.ts` — add `getMe()` calling `GET /api/auth/me`.
2. `frontend/src/pages/ProfilePage.tsx` — read-only page displaying UserId, Email, Roles.
3. `frontend/src/App.tsx` — add protected route `/profile → <ProfilePage />`.
4. Wire the profile dropdown item to navigate to `/profile` instead of doing nothing.

**Acceptance criteria:**
- Page is protected (redirects to `/login` if unauthenticated).
- Displays UserId, Email, and a Roles badge list fetched fresh from the server.
- Profile link in the navbar dropdown navigates to `/profile`.

---

### #13 · JWT-Decoded User vs. Server-Side User Data

**Status:** Working as designed — but profile page (#6) closes the gap.

**Current implementation:**
| File | Line | Detail |
|------|------|--------|
| `frontend/src/store/authStore.ts` | 6–7 | `ROLE_CLAIM` constant for Microsoft identity claim URI |
| `frontend/src/store/authStore.ts` | 16–25 | `extractRole()` decodes JWT locally via `jwtDecode` |
| `frontend/src/store/authStore.ts` | 53–63 | `setAuth()` stores token and immediately calls `extractRole()` |

The JWT decode is intentional for role-based routing without a round-trip. Only the role claim is extracted this way; all other user data comes from the login response.

**Impact of #6:** Once the profile page calls `GET /api/auth/me`, fresh server-side user data will be available. The `authStore` JWT decode should **not** be replaced — it serves a different purpose (fast client-side role check). The profile page is additive.

---

## Group D — Discovery & Browsing

> Both items relate to the catalog browsing experience; low effort, can ship together.

### #7 · Toppings Browser (Public)

**Status:** Backend complete. API already called inside `PizzaCard`. No standalone browse experience before a user opens a pizza card.

**Backend (complete):**
| File | Line | Detail |
|------|------|--------|
| `backend/src/.../Controllers/ToppingController.cs` | 27–34 | `GET /api/topping` — public, no auth |
| `backend/src/.../Features/Topping/Queries/DTOs/ToppingResponseDto.cs` | — | Fields: `Id`, `Name`, `Price`, `IsAvailable` |

**Frontend (partial):**
| File | Line | Detail |
|------|------|--------|
| `frontend/src/api/topping.api.ts` | 4–6 | `getAllToppings()` exists and calls `GET /api/topping` |
| `frontend/src/pages/HomePage.tsx` | 61–67 | Fetches toppings with 5-min stale time; filters to available only |
| `frontend/src/pages/HomePage.tsx` | 278–364 | Toppings shown as selectable list **inside** pizza card dropdown only |

**What is missing:**
- A dedicated section on the homepage (e.g., "Available Toppings" card grid or collapsible panel) visible without opening a pizza card — so users can check what toppings exist and their prices before deciding.

**What is NOT missing:** The API call, the type, and the in-card UI are all done.

**Acceptance criteria:**
- Section visible on homepage below the pizza grid (or as a modal triggered by a "View All Toppings" button).
- Shows `Name` and `Price` for each available topping.
- No auth required; works for guests.

---

### #9 · Pizza Type Filtering — Server-Side

**Status:** Backend endpoint exists. Frontend has the API wrapper but uses client-side filtering instead.

**Backend (complete):**
| File | Line | Detail |
|------|------|--------|
| `backend/src/.../Controllers/PizzaController.cs` | 67–74 | `GET /api/pizza/type/{type}` — enum: `Vegetarian`, `MeatLovers`, `Hawaiian`, `Veggie`, `Custom`, `Supreme`, `Margherita` |

**Frontend (client-side only):**
| File | Line | Detail |
|------|------|--------|
| `frontend/src/api/pizza.api.ts` | 10–11 | `getPizzasByType(type)` exists and calls `GET /api/pizza/type/{type}` |
| `frontend/src/pages/HomePage.tsx` | 475 | `activeType` state tracks selected filter |
| `frontend/src/pages/HomePage.tsx` | 487–488 | `filteredPizzas = activeType === 'All' ? pizzas : pizzas.filter(p => p.type === activeType)` — client-side |
| `frontend/src/pages/HomePage.tsx` | 804–847 | Filter button row (All, Margherita, Supreme, …) |

**What is missing:**
- `getPizzasByType()` is never called. When a type button is clicked, the frontend filters the already-fetched full list locally.

**Recommendation:** Switch the type filter buttons to call `getPizzasByType()` and replace the pizza query result, instead of filtering client-side. For the current dataset size this is optional, but it aligns with the backend design and removes stale data risk when new pizza types are added.

**Acceptance criteria:**
- Selecting a type button calls `GET /api/pizza/type/{type}` via `getPizzasByType()`.
- Selecting "All" calls `GET /api/pizza` as before.
- Loading state shown during fetch.

---

## Group E — Order History Filters

### #14 · Order History Filtering & Pagination

**Status:** Admin endpoint fully supports filters. User-facing endpoint has no filters. Frontend has no filter UI.

**Backend (complete — admin only):**
| File | Line | Detail |
|------|------|--------|
| `backend/src/.../Controllers/AdminController.cs` | 98–111 | `GET /api/admin/orders` — query params: `status?`, `userId?`, `fromDate?`, `toDate?` |
| `backend/src/.../Features/Admin/Queries/GetAllOrders/GetAllOrdersQueryHandler.cs` | 25–53 | Filters applied: status, userId, date range; sorted by `CreatedAt` desc |

**Backend (user-facing — no filters):**
| File | Line | Detail |
|------|------|--------|
| `backend/src/.../Controllers/OrderController.cs` | 46–54 | `GET /api/order` — returns all orders for authenticated user, no filter params |

**Frontend (no filters):**
| File | Line | Detail |
|------|------|--------|
| `frontend/src/api/order.api.ts` | 9–11 | `getMyOrders()` — no parameters, calls `GET /api/order` |
| `frontend/src/pages/OrderHistoryPage.tsx` | 162–170 | Fetches with `getMyOrders()`, sorts client-side by `createdAt` desc — no filter UI |

**What is missing:**
1. Filter UI in `OrderHistoryPage.tsx`: status dropdown (`Pending`, `Confirmed`, `Preparing`, `OutForDelivery`, `Delivered`, `Cancelled`) and date range inputs.
2. For regular users: backend `GET /api/order` needs optional `status` and date range query params added so user-facing filtering works without admin role.
3. For admin users: add `getAllOrders(params)` to the frontend API layer calling the admin endpoint.

**Acceptance criteria:**
- Status filter chips/dropdown narrows the order list.
- Date range picker (From / To) narrows by `createdAt`.
- Filters reset to "All / no date" by default.
- Admin users see all orders; regular users see only their own.

---

## Group F — Admin Dashboard

### #10 · Full Admin UI

**Status:** All 5 backend admin endpoints complete. Frontend `AdminPage` is a placeholder with no functional UI and no admin API service file.

**Backend endpoints (all complete):**
| Method | Route | Params | Response |
|--------|-------|--------|----------|
| `GET` | `/api/admin/users` | — | `List<UserDto>` |
| `GET` | `/api/admin/users/{id}` | `id` (route) | `UserDto` |
| `GET` | `/api/admin/users/{id}/orders` | `id` (route) | `List<OrderDto>` |
| `GET` | `/api/admin/orders` | `status?`, `userId?`, `fromDate?`, `toDate?` | `List<OrderDto>` |
| `PUT` | `/api/admin/orders/{id}/status` | `id` (route), `newStatus` (body) | `OrderDto` |

**Key DTOs:**
- `UserDto` — `Id`, `UserName`, `Email`, `PhoneNumber`, `Roles` (`backend/src/.../Features/Admin/Queries/UserDto.cs`)
- `OrderDto` — `Id`, `UserId`, `TotalPrice`, `Status`, `CreatedAt`, `ConfirmedAt`, `CompletedAt`, `CancelledAt`, `Items` (`backend/src/.../Features/Order/Queries/OrderDto.cs`)

**Frontend (placeholder only):**
| File | Line | Detail |
|------|------|--------|
| `frontend/src/pages/AdminPage.tsx` | 15 | Renders "Dashboard" heading |
| `frontend/src/pages/AdminPage.tsx` | 19 | Renders "Admin panel coming soon" — no functional UI |
| `frontend/src/App.tsx` | 34–36 | Route `/admin → <AdminPage />` wrapped in `<ProtectedRoute requireAdmin />` |
| `frontend/src/api/` | — | No `admin.api.ts` file exists |

**What is missing:**

**1. `frontend/src/api/admin.api.ts`** (new file):
```typescript
export const getAllUsers = () =>
  apiClient.get<UserDto[]>('/admin/users')

export const getUserById = (id: string) =>
  apiClient.get<UserDto>(`/admin/users/${id}`)

export const getOrdersByUserId = (id: string) =>
  apiClient.get<OrderDto[]>(`/admin/users/${id}/orders`)

export const getAllOrders = (params?: {
  status?: OrderStatus
  userId?: string
  fromDate?: string
  toDate?: string
}) => apiClient.get<OrderDto[]>('/admin/orders', { params })

export const updateOrderStatus = (orderId: string, newStatus: OrderStatus) =>
  apiClient.put<OrderDto>(`/admin/orders/${orderId}/status`, newStatus)
```

**2. `frontend/src/pages/AdminPage.tsx`** — replace placeholder with tabbed dashboard:
- **Users tab:** Searchable table of users (`Id`, `UserName`, `Email`, `Roles`). Click row → user detail drawer with their order history.
- **Orders tab:** Filterable order table (reuse filter logic from Group E). Inline status update via dropdown → calls `PUT /api/admin/orders/{id}/status`.

**Acceptance criteria:**
- Admin dashboard accessible only to users with `Admin` role.
- Users tab lists all users; clicking a user shows their orders.
- Orders tab lists all orders with status/date filters.
- Order status can be updated inline; UI reflects new status immediately (optimistic update or refetch).

---

## Implementation Order

| Priority | Group | Effort | Dependency |
|----------|-------|--------|------------|
| 1 | **A — Cart** (#5 + #8) | Low | None |
| 2 | **C — Profile** (#6 + #13) | Low | None |
| 3 | **D — Discovery** (#7 + #9) | Low | None |
| 4 | **E — Order Filters** (#14) | Medium | Needs backend param addition for user endpoint |
| 5 | **F — Admin Dashboard** (#10) | High | Group E filter logic can be reused |

Groups A, C, and D have no inter-dependencies and can be built in parallel.
