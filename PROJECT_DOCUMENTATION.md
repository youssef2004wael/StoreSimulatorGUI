# StoreSimulatorGUI – Project Documentation

## Overview
- Desktop shopping simulator written in F# with Avalonia 11 (Fluent theme).
- Implements a simple store experience: browse products, search/filter, manage a cart, apply discounts/coupons, and check out. Orders are saved to JSON; catalog export helpers are included.
- Architectural style: MVVM. Views are XAML, ViewModels expose observable collections and commands, domain logic lives in standalone modules.

## Solution Structure
- `Program.fs` – entry point; builds Avalonia app and starts the desktop lifetime.
- `App.axaml`/`App.axaml.fs` – loads styles and creates `MainWindow`.
- `Views/MainWindow.axaml`(+`.fs`) – UI layout and DataContext wiring to `MainViewModel`.
- `ViewModels/MainViewModel.fs` – orchestration layer; exposes products, cart items, commands, and status text.
- Domain modules:
  - `Models.fs` – product, cart, discount, order, and filter types.
  - `Catalog.fs` – in-memory product catalog CRUD and stock helpers.
  - `Search.fs` – product filtering, search, and sorting.
  - `Cart.fs` – immutable cart operations (add/remove/update/merge) and summaries.
  - `PriceCalculator.fs` – subtotal/discount/coupon logic, totals, tax/shipping helpers.
  - `FileOperations.fs` – JSON persistence for orders/catalog and text/CSV export.
- Tests in `Tests/` target core modules and integration.

## Data Model (from `Models.fs`)
- `Product` includes `Id`, `Name`, `Description`, `Price`, `Category`, `Stock`.
- `CartItem` wraps a `Product` and `Quantity`.
- `Cart` tracks `Items`, totals (`TotalBeforeDiscount`, `Discount`, `FinalTotal`) and optional `AppliedCoupon`.
- `DiscountRule`: `PercentageOff`, `FixedAmountOff`, `BuyXGetY`, or `NoDiscount`.
- `OrderSummary` snapshot saved on checkout.
- `FilterCriteria` supports category/price/search/stock filters.

## Key Behaviors
- Catalog initialization seeds 15 sample products into a `Map<int, Product>`. Utility helpers support stock updates, price changes, removal, and reporting (low/out-of-stock).
- Cart workflow is immutable: `addToCart`, `updateQuantity`, `increment/decrement`, `removeFromCart`, `mergeCarts`, `clearCart`. Stock limits are enforced on add/update.
- Pricing & discounts:
  - Subtotal is sum of line items.
  - Automatic discounts: 5% at ≥$200, 10% at ≥$500, 15% at ≥$1000.
  - Coupon codes: `SAVE10`, `SAVE20` (min $100), `SAVE25` (min $200), `FLAT50`, `FLAT100` (min $500), `WELCOME15`.
  - `calculateTotal` applies auto discounts; `applyCouponToCart` stores the coupon and recalculates. `recalculateCart` preserves existing coupons.
  - Helpers compute savings %, tax, shipping (free when `FinalTotal` ≥ threshold), and compose detailed summaries.
- Search/filter: name/description search, category filter, price ranges, stock-only toggle, sorting by price/name/stock.
- Checkout flow (in `MainViewModel.CheckoutExecute`):
  - Decreases catalog stock for purchased items.
  - Saves an `OrderSummary` JSON file to `Orders/order_yyyyMMdd_HHmmss.json`.
  - Clears cart and refreshes UI collections; status message communicates outcome.
- Persistence & export:
  - Orders and catalog saved/loaded via Newtonsoft.Json with indented formatting.
  - Export helpers: cart to text, catalog to CSV, backup orders to another directory, list/delete orders, compute total sales.

## UI Overview (Avalonia, `MainWindow.axaml`)
- Header with app title and tagline.
- Left pane: search box + category combo; product list rendered as modern cards; quantity selector and Add-to-cart button.
- Right pane: cart list with per-item remove, coupon section (apply/remove), totals panel (subtotal, discount, total), checkout and clear-cart buttons.
- Status bar displays the latest status message from the ViewModel.

## ViewModel Highlights
- Observable collections: `Products`, `CartItems`, `Categories`.
- State: selected product, quantity, coupon code, search text, selected category, status message, and backing `catalog`/`cart` models.
- Commands: Add to cart, Clear cart, Apply/Remove coupon, Checkout, Remove cart item.
- `FilterProducts` reacts to search/category changes to rebuild the product list.
- `UpdateCart` recalculates totals, repopulates cart items, and raises property change notifications for bound fields.

## Building & Running
- Prereqs: .NET 8 SDK.
- Restore/build/run from repo root:
  - `dotnet restore`
  - `dotnet run`
- Tests (if desired): `dotnet test` (uses `Tests/StoreSimulatorGUI.Tests.fsproj`).

## Data & Files
- Orders: saved under `Orders/` (auto-created). Filenames: `order_YYYYMMDD_HHMMSS.json`.
- Catalog: default seed is in code; helpers exist to save/load a JSON catalog under `Catalog/catalog.json`.
- Assets: `Assets/avalonia-logo.ico` (app icon).

## Extension Ideas
- Add persistent user settings (currency, tax rate, shipping threshold).
- Implement real Buy X Get Y discounts in `applyDiscount`.
- Add quantity editing directly in the cart UI.
- Provide import-from-CSV for catalog updates.
- Integrate notifications/toasts for status instead of status bar text.

