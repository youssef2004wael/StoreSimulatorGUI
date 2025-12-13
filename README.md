# All Information Received! ✅

I now have everything needed to create comprehensive documentation. Here's the complete documentation:

---

# 📘 F# Store Simulator - Project Documentation

## Complete Technical Documentation

*Version:* 1.0  
*Date:* December 2025  
*Platform:* .NET 8.0 / Avalonia UI  
*Language:* F#

---

# Table of Contents

1. [Project Overview](#1-project-overview)
2. [Features](#2-features)
3. [Architecture](#3-architecture)
4. [Technology Stack](#4-technology-stack)
5. [Project Structure](#5-project-structure)
6. [Installation & Setup](#6-installation--setup)
7. [Data Models](#7-data-models)
8. [Module Documentation](#8-module-documentation)
9. [UI Components](#9-ui-components)
10. [Test Coverage](#10-test-coverage)
11. [Sample Data](#11-sample-data)
12. [Team Contributions](#12-team-contributions)
13. [Usage Guide](#13-usage-guide)
14. [Known Coupons](#14-known-coupons)

---

# 1. Project Overview

## 1.1 Objective

Build a virtual F# store with cart functionality and basic product management. The application is designed to help students learn:
- *Data structures* (List, Map)
- *Pure functions*
- *Immutable state management*
- *MVVM architecture pattern*

## 1.2 Description

Users can browse products, add/remove items from their cart, apply discount coupons, and complete checkout. The application demonstrates functional programming principles using F# with a modern GUI built on Avalonia UI framework.

## 1.3 Application Screenshot

![F# Store Simulator](screenshot.png)

The application features a modern, clean interface with:
- Left panel: Product catalog with search and filtering
- Right panel: Shopping cart with promo code functionality
- Bottom bar: Status messages and notifications

---

# 2. Features

## 2.1 Core Features

| Feature | Description | Module |
|---------|-------------|--------|
| 📦 *Product Catalog* | 15 products stored in immutable Map structure | Catalog.fs |
| 🔍 *Search & Filter* | Search by name, filter by category/price | Search.fs |
| 🛒 *Shopping Cart* | Add/remove products with immutable operations | Cart.fs |
| 💰 *Price Calculator* | Subtotals, discounts, tax, and shipping | PriceCalculator.fs |
| 🎟 *Coupon System* | Apply promotional codes for discounts | PriceCalculator.fs |
| 💾 *Order Persistence* | Save orders to JSON files | FileOperations.fs |
| 🖥 *Modern GUI* | Avalonia-based responsive interface | Views/ |

## 2.2 Discount System

| Cart Value | Automatic Discount |
|------------|-------------------|
| $200+ | 5% off |
| $500+ | 10% off |
| $1000+ | 15% off |

---

# 3. Architecture

## 3.1 Architecture Diagram


┌─────────────────────────────────────────────────────────────────┐
│                        PRESENTATION LAYER                        │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                    MainWindow.axaml                          │ │
│  │  ┌─────────────────────┐  ┌────────────────────────────┐    │ │
│  │  │   Product List      │  │      Shopping Cart         │    │ │
│  │  │   - Search Box      │  │      - Cart Items          │    │ │
│  │  │   - Category Filter │  │      - Promo Code          │    │ │
│  │  │   - Product Cards   │  │      - Price Summary       │    │ │
│  │  └─────────────────────┘  └────────────────────────────┘    │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                        VIEWMODEL LAYER                           │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                    MainViewModel.fs                          │ │
│  │  - ProductViewModel    - CartItemViewModel                   │ │
│  │  - RelayCommand        - INotifyPropertyChanged              │ │
│  │  - ObservableCollections                                     │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                        BUSINESS LOGIC LAYER                      │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────────────────┐ │
│  │  Catalog.fs  │ │   Cart.fs    │ │   PriceCalculator.fs     │ │
│  │  - Products  │ │  - Add/Remove│ │   - Discounts            │ │
│  │  - Stock     │ │  - Update    │ │   - Coupons              │ │
│  └──────────────┘ └──────────────┘ └──────────────────────────┘ │
│  ┌──────────────┐ ┌──────────────────────────────────────────┐  │
│  │  Search.fs   │ │         FileOperations.fs                │  │
│  │  - Filter    │ │         - JSON Save/Load                 │  │
│  │  - Sort      │ │         - CSV Export                     │  │
│  └──────────────┘ └──────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                          DATA LAYER                              │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                      Models.fs                               │ │
│  │  Product | Cart | CartItem | DiscountRule | OrderSummary     │ │
│  └─────────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                    Orders/ (JSON Files)                      │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘


## 3.2 Data Flow


User Action → View → ViewModel → Business Logic → State Update → View Update
     │                                                              │
     └──────────────────────────────────────────────────────────────┘
                         (INotifyPropertyChanged)


---

# 4. Technology Stack

## 4.1 Core Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| *F#* | .NET 8.0 | Primary programming language |
| *Avalonia UI* | 11.3.8 | Cross-platform UI framework |
| *Newtonsoft.Json* | 13.0.3 | JSON serialization |

## 4.2 Testing Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| *xUnit* | 2.6.3 | Test framework |
| *FsUnit.xUnit* | 5.6.1 | F# assertions |
| *Microsoft.NET.Test.Sdk* | 17.8.0 | Test SDK |
| *coverlet.collector* | 6.0.0 | Code coverage |

## 4.3 Dependencies (from .fsproj)

xml
<PackageReference Include="Avalonia" Version="11.3.8" />
<PackageReference Include="Avalonia.Desktop" Version="11.3.8" />
<PackageReference Include="Avalonia.ReactiveUI" Version="11.3.8" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.8" />
<PackageReference Include="Avalonia.Fonts.Inter" Version="11.3.8" />
<PackageReference Include="Avalonia.Diagnostics" Version="11.3.8" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />


---

# 5. Project Structure


StoreSimulatorGUI/
│
├── 📁 Assets/                    # Application assets
├── 📁 Orders/                    # Saved order JSON files
│   ├── order_20251203_022619.json
│   └── order_20251203_024202.json
│
├── 📁 ViewModels/
│   └── MainViewModel.fs          # MVVM ViewModel
│
├── 📁 Views/
│   ├── MainWindow.axaml          # Main UI layout
│   └── MainWindow.axaml.fs       # Code-behind
│
├── 📁 Tests/                     # Test project
│   ├── CartTests.fs              # Cart module tests
│   ├── CatalogTests.fs           # Catalog module tests
│   ├── PriceCalculatorTests.fs   # Price calculator tests
│   ├── SearchTests.fs            # Search module tests
│   ├── FileOperationsTests.fs    # File operations tests
│   ├── IntegrationTests.fs       # End-to-end tests
│   └── StoreSimulatorGUI.Tests.fsproj
│
├── Models.fs                     # Data types/records
├── Catalog.fs                    # Product catalog operations
├── Cart.fs                       # Shopping cart operations
├── PriceCalculator.fs            # Pricing and discounts
├── Search.fs                     # Search and filter logic
├── FileOperations.fs             # JSON/CSV file operations
├── App.axaml                     # Application resources
├── App.axaml.fs                  # Application initialization
├── ViewLocator.fs                # View-ViewModel mapping
├── Program.fs                    # Entry point
└── StoreSimulatorGUI.fsproj      # Project file


---

# 6. Installation & Setup

## 6.1 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- IDE: Visual Studio 2022, VS Code, or JetBrains Rider

## 6.2 Build & Run

bash
# Clone the repository
git clone <repository-url>
cd StoreSimulatorGUI

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run


## 6.3 Run Tests

bash
# Navigate to test project
cd Tests

# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~CartTests"


---

# 7. Data Models

## 7.1 Product

fsharp
type Product = {
    Id: int              // Unique identifier
    Name: string         // Product name
    Description: string  // Product description
    Price: decimal       // Unit price
    Category: string     // Product category
    Stock: int           // Available quantity
}


## 7.2 CartItem

fsharp
type CartItem = {
    Product: Product     // Reference to product
    Quantity: int        // Quantity in cart
}


## 7.3 Cart

fsharp
type Cart = {
    Items: CartItem list          // List of cart items
    TotalBeforeDiscount: decimal  // Subtotal
    Discount: decimal             // Discount amount
    FinalTotal: decimal           // Final price
    AppliedCoupon: string option  // Applied coupon code
}


## 7.4 DiscountRule

fsharp
type DiscountRule =
    | PercentageOff of decimal    // e.g., 10% off
    | FixedAmountOff of decimal   // e.g., $50 off
    | BuyXGetY of int * int       // Buy X get Y free
    | NoDiscount                  // No discount applied


## 7.5 OrderSummary

fsharp
type OrderSummary = {
    OrderId: string              // Unique order ID (GUID)
    OrderDate: DateTime          // Order timestamp
    Items: CartItem list         // Ordered items
    Subtotal: decimal            // Before discount
    Discount: decimal            // Discount applied
    Total: decimal               // Final total
    CouponUsed: string option    // Coupon if used
}


## 7.6 FilterCriteria

fsharp
type FilterCriteria = {
    Category: string option      // Category filter
    MinPrice: decimal option     // Minimum price
    MaxPrice: decimal option     // Maximum price
    SearchTerm: string option    // Search keyword
    InStockOnly: bool            // Only show in-stock
}


---

# 8. Module Documentation

## 8.1 Catalog Module (Catalog.fs)

*Purpose:* Manages the product catalog using immutable Map data structure.

### Functions

| Function | Signature | Description |
|----------|-----------|-------------|
| initializeCatalog | unit → Map<int, Product> | Creates catalog with 15 products |
| getProduct | Map<int, Product> → int → Product option | Get product by ID |
| getAllProducts | Map<int, Product> → Product list | Get all products as list |
| addProduct | Map<int, Product> → Product → Map<int, Product> | Add new product (immutable) |
| updateStock | Map<int, Product> → int → int → Map<int, Product> | Update product stock |
| decreaseStockBatch | Map<int, Product> → CartItem list → Map<int, Product> | Decrease stock for checkout |
| updatePrice | Map<int, Product> → int → decimal → Map<int, Product> | Update product price |
| removeProduct | Map<int, Product> → int → Map<int, Product> | Remove product from catalog |
| hasStock | Map<int, Product> → int → int → bool | Check if sufficient stock |
| getLowStockProducts | Map<int, Product> → int → Product list | Get low stock items |
| getOutOfStockProducts | Map<int, Product> → Product list | Get out of stock items |

### Default Product Catalog

| ID | Name | Price | Category | Stock |
|----|------|-------|----------|-------|
| 1 | Laptop | $999.99 | Electronics | 10 |
| 2 | Mouse | $29.99 | Electronics | 50 |
| 3 | Keyboard | $79.99 | Electronics | 30 |
| 4 | Monitor | $399.99 | Electronics | 15 |
| 5 | Headphones | $199.99 | Electronics | 25 |
| 6 | Desk Chair | $299.99 | Furniture | 20 |
| 7 | Desk Lamp | $49.99 | Furniture | 40 |
| 8 | USB Cable | $14.99 | Accessories | 100 |
| 9 | Webcam | $89.99 | Electronics | 35 |
| 10 | Notebook | $9.99 | Stationery | 60 |
| 11 | Pen Set | $24.99 | Stationery | 45 |
| 12 | Backpack | $59.99 | Accessories | 30 |
| 13 | Phone Stand | $19.99 | Accessories | 55 |
| 14 | Coffee Mug | $15.99 | Kitchen | 70 |
| 15 | Water Bottle | $22.99 | Kitchen | 50 |

---

## 8.2 Cart Module (Cart.fs)

*Purpose:* Manages shopping cart with immutable operations.

### Functions

| Function | Signature | Description |
|----------|-----------|-------------|
| emptyCart | Cart | Creates empty cart |
| addToCart | Cart → Product → int → Result<Cart, string> | Add product to cart |
| removeFromCart | Cart → int → Cart | Remove product by ID |
| updateQuantity | Cart → int → int → Result<Cart, string> | Update item quantity |
| incrementQuantity | Cart → int → Result<Cart, string> | Increase quantity by 1 |
| decrementQuantity | Cart → int → Result<Cart, string> | Decrease quantity by 1 |
| clearCart | Cart → Cart | Clear all items |
| findCartItem | Cart → int → CartItem option | Find item by product ID |
| isInCart | Cart → int → bool | Check if product in cart |
| isEmpty | Cart → bool | Check if cart is empty |
| getItemCount | Cart → int | Get total quantity |
| getUniqueProductCount | Cart → int | Get unique product count |
| mergeCarts | Cart → Cart → Cart | Merge two carts |

### Usage Example

fsharp
let cart = emptyCart
let product = { Id = 1; Name = "Laptop"; Price = 999.99m; ... }

// Add product
match addToCart cart product 2 with
| Ok newCart -> // Success
| Error msg -> // Handle error

// Remove product
let updatedCart = removeFromCart cart 1


---

## 8.3 PriceCalculator Module (PriceCalculator.fs)

*Purpose:* Handles all pricing calculations, discounts, and coupons.

### Functions

| Function | Signature | Description |
|----------|-----------|-------------|
| calculateSubtotal | Cart → decimal | Calculate cart subtotal |
| calculateLineItemTotal | CartItem → decimal | Calculate single item total |
| applyDiscount | decimal → DiscountRule → decimal | Apply discount rule |
| getAutomaticDiscount | decimal → DiscountRule | Get tier-based discount |
| applyCoupon | string → Result<DiscountRule, string> | Validate coupon code |
| applyCouponToCart | Cart → string → Result<Cart, string> | Apply coupon to cart |
| removeCoupon | Cart → Cart | Remove applied coupon |
| recalculateCart | Cart → Cart | Recalculate all totals |
| calculateTax | decimal → decimal → decimal | Calculate tax amount |
| calculateShipping | Cart → decimal → decimal | Calculate shipping |
| calculateGrandTotal | Cart → decimal → decimal → (decimal * decimal * decimal) | Tax, shipping, grand total |
| getSavings | Cart → decimal | Get discount amount |
| getSavingsPercentage | Cart → decimal | Get discount percentage |

### Automatic Discount Tiers

fsharp
let getAutomaticDiscount (subtotal: decimal) : DiscountRule =
    if subtotal >= 1000m then PercentageOff 15m
    elif subtotal >= 500m then PercentageOff 10m
    elif subtotal >= 200m then PercentageOff 5m
    else NoDiscount


---

## 8.4 Search Module (Search.fs)

*Purpose:* Provides search, filter, and sort functionality.

### Functions

| Function | Signature | Description |
|----------|-----------|-------------|
| searchByName | Product list → string → Product list | Search by product name |
| searchByDescription | Product list → string → Product list | Search by description |
| searchByNameOrDescription | Product list → string → Product list | Search in both fields |
| filterByCategory | Product list → string → Product list | Filter by category |
| filterByPriceRange | Product list → decimal → decimal → Product list | Filter by price range |
| filterInStock | Product list → Product list | Filter in-stock only |
| filterLowStock | Product list → int → Product list | Filter low stock items |
| sortByPrice | Product list → bool → Product list | Sort by price |
| sortByName | Product list → bool → Product list | Sort by name |
| sortProducts | Product list → SortBy → Product list | Generic sort |
| getCategories | Product list → string list | Get unique categories |
| filterProducts | Product list → FilterCriteria → Product list | Complex filter |
| getPriceStatistics | Product list → (decimal * decimal * decimal) | Min, max, avg prices |
| getTotalInventoryValue | Product list → decimal | Total inventory value |

### Sort Options

fsharp
type SortBy = 
    | PriceAsc 
    | PriceDesc 
    | NameAsc 
    | NameDesc
    | StockAsc
    | StockDesc


---

## 8.5 FileOperations Module (FileOperations.fs)

*Purpose:* Handles file I/O for orders and catalog data.

### Functions

| Function | Signature | Description |
|----------|-----------|-------------|
| saveOrder | Cart → Result<string, string> | Save order to JSON |
| saveOrderToJson | OrderSummary → string → Result<string, string> | Save to specific path |
| loadOrderFromJson | string → Result<OrderSummary, string> | Load order from file |
| saveCatalogToJson | Map<int, Product> → string → Result<string, string> | Save catalog |
| loadCatalogFromJson | string → Result<Map<int, Product>, string> | Load catalog |
| createOrderSummary | Cart → OrderSummary | Create order from cart |
| generateOrderFileName | unit → string | Generate timestamped filename |
| listSavedOrders | unit → string list | List all order files |
| deleteOrder | string → Result<unit, string> | Delete order file |
| exportCartToText | Cart → string → Result<string, string> | Export as text |
| exportCatalogToCsv | Map<int, Product> → string → Result<string, string> | Export as CSV |
| getTotalSales | unit → decimal | Sum of all orders |
| backupOrders | string → Result<int, string> | Backup orders |

### File Naming Convention


Orders/order_YYYYMMDD_HHMMSS.json


---

# 9. UI Components

## 9.1 MainWindow Layout


┌────────────────────────────────────────────────────────────────┐
│  🛒 F# Store Simulator                                         │
│     Modern Shopping Experience                                  │
├────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────┐ ┌────────────────────────────┐ │
│ │ 🔍 Search products...   [All▼] │ 🛒 Shopping Cart          │ │
│ ├─────────────────────────────┤ │    0 items                 │ │
│ │                             │ ├────────────────────────────┤ │
│ │  ┌─────────────────────┐   │ │                            │ │
│ │  │ Laptop              │   │ │  (Cart items appear here)  │ │
│ │  │ High-performance... │   │ │                            │ │
│ │  │ $999.99    Stock:10 │   │ ├────────────────────────────┤ │
│ │  └─────────────────────┘   │ │ 🎟 Promo Code              │ │
│ │                             │ │ [          ] [Apply]       │ │
│ │  ┌─────────────────────┐   │ ├────────────────────────────┤ │
│ │  │ Mouse               │   │ │ Subtotal:        $0.00     │ │
│ │  │ Wireless ergonomic  │   │ │ Discount:       -$0.00     │ │
│ │  │ $29.99     Stock:50 │   │ │ ─────────────────────────  │ │
│ │  └─────────────────────┘   │ │ TOTAL:           $0.00     │ │
│ │                             │ │                            │ │
│ │  (More products...)        │ │ [💳 CHECKOUT]              │ │
│ │                             │ │ [🗑 Clear Cart]            │ │
│ ├─────────────────────────────┤ └────────────────────────────┘ │
│ │ Quantity: [1 ▲▼] [➕ Add to Cart]                            │
│ └─────────────────────────────┘                                │
├────────────────────────────────────────────────────────────────┤
│ Welcome to F# Store Simulator!                                 │
└────────────────────────────────────────────────────────────────┘


## 9.2 ViewModels

### MainViewModel

fsharp
type MainViewModel() =
    // Observable Collections
    member _.Products : ObservableCollection<ProductViewModel>
    member _.CartItems : ObservableCollection<CartItemViewModel>
    member _.Categories : ObservableCollection<string>
    
    // Bindable Properties
    member _.SelectedProduct : ProductViewModel
    member _.QuantityToAdd : int
    member _.SearchText : string
    member _.SelectedCategory : string
    member _.CouponCode : string
    member _.StatusMessage : string
    
    // Cart Properties
    member _.CartSubtotal : string
    member _.CartDiscount : string
    member _.CartTotal : string
    member _.CartItemCount : string
    member _.HasItems : bool
    member _.HasCoupon : bool
    
    // Commands
    member _.AddToCartCommand : ICommand
    member _.RemoveFromCartCommand : ICommand
    member _.ClearCartCommand : ICommand
    member _.ApplyCouponCommand : ICommand
    member _.RemoveCouponCommand : ICommand
    member _.CheckoutCommand : ICommand


### ProductViewModel

fsharp
type ProductViewModel(product: Product) =
    member _.Id : int
    member _.Name : string
    member _.Description : string
    member _.Price : decimal
    member _.PriceFormatted : string      // "$999.99"
    member _.Category : string
    member _.Stock : int
    member _.StockInfo : string           // "Stock: 10"
    member _.IsInStock : bool
    member _.StockColor : string          // "Green" | "Orange" | "Red"


### CartItemViewModel

fsharp
type CartItemViewModel(item: CartItem, removeCommand: ICommand) =
    member _.ProductName : string
    member _.Price : decimal
    member _.Quantity : int
    member _.Subtotal : decimal
    member _.SubtotalFormatted : string
    member _.ProductId : int
    member _.RemoveCommand : ICommand


---

# 10. Test Coverage

## 10.1 Test Summary

| Test File | Tests | Description |
|-----------|-------|-------------|
| CartTests.fs | 21 | Cart operations |
| CatalogTests.fs | 17 | Catalog management |
| PriceCalculatorTests.fs | 22 | Pricing & discounts |
| SearchTests.fs | 15 | Search & filter |
| FileOperationsTests.fs | 10 | File I/O |
| IntegrationTests.fs | 5 | End-to-end workflows |
| *Total* | *90* | |

## 10.2 Test Categories

### Cart Tests
- ✅ Empty cart creation
- ✅ Add product to cart
- ✅ Remove product from cart
- ✅ Update quantity
- ✅ Merge quantities for same product
- ✅ Stock validation
- ✅ Cart item count
- ✅ Clear cart

### Catalog Tests
- ✅ Initialize catalog (15 products)
- ✅ Get product by ID
- ✅ Product exists check
- ✅ Update stock (immutable)
- ✅ Has stock validation
- ✅ Decrease stock batch
- ✅ Low stock detection

### Price Calculator Tests
- ✅ Calculate subtotal
- ✅ Line item total
- ✅ Percentage discount
- ✅ Fixed amount discount
- ✅ Automatic tier discounts
- ✅ Coupon validation
- ✅ Coupon application
- ✅ Tax calculation
- ✅ Shipping calculation

### Search Tests
- ✅ Search by name
- ✅ Case-insensitive search
- ✅ Partial matching
- ✅ Filter by category
- ✅ Filter by price range
- ✅ Sort by price
- ✅ Get unique categories

### Integration Tests
- ✅ Complete shopping workflow
- ✅ Filter and purchase workflow
- ✅ Stock management
- ✅ Multiple coupons handling
- ✅ Cart persistence

## 10.3 Running Tests

bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run specific test file
dotnet test --filter "FullyQualifiedName~CartTests"

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"


---

# 11. Sample Data

## 11.1 Sample Order (Without Coupon)

*File:* Orders/order_20251203_022619.json

json
{
  "OrderId": "f5b13d5c-9077-4f80-922c-1347ffb2d362",
  "OrderDate": "2025-12-03T02:26:19.5172592+02:00",
  "Items": [
    {
      "Product": {
        "Id": 3,
        "Name": "Keyboard",
        "Description": "Mechanical RGB keyboard",
        "Price": 79.99,
        "Category": "Electronics",
        "Stock": 30
      },
      "Quantity": 2
    }
  ],
  "Subtotal": 159.98,
  "Discount": 0.0,
  "Total": 159.98
}


## 11.2 Sample Order (With Coupon)

*File:* Orders/order_20251203_024202.json

json
{
  "OrderId": "f0792f69-154e-46bd-8559-21cb5019a863",
  "OrderDate": "2025-12-03T02:42:02.7328117+02:00",
  "Items": [
    {
      "Product": {
        "Id": 15,
        "Name": "Water Bottle",
        "Price": 22.99,
        "Category": "Kitchen",
        "Stock": 50
      },
      "Quantity": 1
    },
    {
      "Product": {
        "Id": 7,
        "Name": "Desk Lamp",
        "Price": 49.99,
        "Category": "Furniture",
        "Stock": 40
      },
      "Quantity": 1
    },
    {
      "Product": {
        "Id": 6,
        "Name": "Desk Chair",
        "Price": 299.99,
        "Category": "Furniture",
        "Stock": 20
      },
      "Quantity": 1
    },
    {
      "Product": {
        "Id": 4,
        "Name": "Monitor",
        "Price": 399.99,
        "Category": "Electronics",
        "Stock": 15
      },
      "Quantity": 1
    }
  ],
  "Subtotal": 772.96,
  "Discount": 193.24,
  "Total": 579.72,
  "CouponUsed": "SAVE25"
}


---

# 12. Team Contributions

| Team Member | Role | Responsibilities |
|-------------|------|------------------|
| *Sama Haitham* | Catalog Developer | Product catalog initialization, Map structure, stock management |
| *Sama Sameh* | Cart Logic Developer | Add/remove operations, immutable list management, cart state |
| *Ezz* | Price Calculator | Totals calculation, discount logic, coupon system |
| *Rehab* | Search & Filter Developer | Product filtering, sorting, search functionality |
| *Sara Omar* | File Save/Load Developer | JSON serialization, order persistence, file I/O |
| *Youssef* | UI Developer | Avalonia UI, XAML layouts, styling, user experience |
| *Omay Ayman* | Tester | Unit tests, integration tests, test coverage |
| *Omar Ayad* | GitHub/Documentation Lead | Version control, documentation, code review |

---

# 13. Usage Guide

## 13.1 Browsing Products

1. *View all products* in the left panel
2. *Search* by typing in the search box
3. *Filter by category* using the dropdown

## 13.2 Adding to Cart

1. *Select a product* by clicking on it
2. *Set quantity* using the number picker
3. *Click "Add to Cart"* button
4. View cart updates in the right panel

## 13.3 Managing Cart

1. *Remove items* using the trash icon
2. *Clear cart* using "Clear Cart" button
3. View running totals in real-time

## 13.4 Applying Discounts

1. *Enter coupon code* in promo code field
2. *Click "Apply"* to apply discount
3. *Remove coupon* if needed

## 13.5 Checkout

1. Review cart contents and total
2. *Click "CHECKOUT"* button
3. Order is saved to Orders/ folder
4. Stock is automatically decreased
5. Cart is cleared

---

# 14. Known Coupons

| Code | Discount | Minimum Purchase |
|------|----------|------------------|
| SAVE10 | 10% off | None |
| SAVE20 | 20% off | $100 |
| SAVE25 | 25% off | $200 |
| FLAT50 | $50 off | None |
| FLAT100 | $100 off | $500 |
| WELCOME15 | 15% off | None |

---

# 15. Appendix

## 15.1 Compilation Order

F# requires specific file ordering. The project compiles in this order:

xml
1. Models.fs          <!-- Data types first -->
2. Catalog.fs         <!-- Depends on Models -->
3. Search.fs          <!-- Depends on Models -->
4. Cart.fs            <!-- Depends on Models -->
5. PriceCalculator.fs <!-- Depends on Cart, Models -->
6. FileOperations.fs  <!-- Depends on Models, Cart -->
7. MainViewModel.fs   <!-- Depends on all above -->
8. MainWindow.axaml.fs
9. App.axaml.fs
10. Program.fs        <!-- Entry point last -->


## 15.2 Key F# Concepts Used

| Concept | Usage |
|---------|-------|
| *Immutable Records* | All data types (Product, Cart, etc.) |
| *Discriminated Unions* | DiscountRule, SortBy |
| *Option Types* | Null-safe operations |
| *Result Types* | Error handling |
| *Pattern Matching* | Control flow |
| *Higher-Order Functions* | List.map, List.filter, List.fold |
| *Pipe Operator* | Function chaining (\|>) |
| *Pure Functions* | No side effects in business logic |

---

## Document Information

| Field | Value |
|-------|-------|
| *Project Name* | F# Store Simulator |
| *Version* | 1.0 |
| *Framework* | .NET 8.0 |
| *UI Framework* | Avalonia 11.3.8 |
| *Last Updated* | December 2025 |
| *Team Size* | 8 members |
| *Total Tests* | 90 |
| *Lines of Code* | ~2,500 |

---

*End of Documentation*

---

Would you like me to:
1. *Export this as a downloadable file format* (Markdown, PDF-ready)?
2. *Add any additional sections* (API examples, troubleshooting, etc.)?
3. *Create a shorter README.md version* for GitHub?
