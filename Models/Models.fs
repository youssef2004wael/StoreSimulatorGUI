module StoreSimulatorGUI.Models

open System

type Product = {
    Id: int
    Name: string
    Description: string
    Price: decimal
    Category: string
    Stock: int
}

type CartItem = {
    Product: Product
    Quantity: int
}

type Cart = {
    Items: CartItem list
    TotalBeforeDiscount: decimal
    Discount: decimal
    FinalTotal: decimal
    AppliedCoupon: string option
}

type DiscountRule =
    | PercentageOff of decimal
    | FixedAmountOff of decimal
    | BuyXGetY of int * int
    | NoDiscount

type OrderSummary = {
    OrderId: string
    OrderDate: DateTime
    Items: CartItem list
    Subtotal: decimal
    Discount: decimal
    Total: decimal
    CouponUsed: string option
}

type FilterCriteria = {
    Category: string option
    MinPrice: decimal option
    MaxPrice: decimal option
    SearchTerm: string option
    InStockOnly: bool
}