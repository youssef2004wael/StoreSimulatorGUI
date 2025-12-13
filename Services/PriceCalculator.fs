module StoreSimulatorGUI.PriceCalculator

open StoreSimulatorGUI.Models
open StoreSimulatorGUI.Cart

// Calculate subtotal (before discounts)
let calculateSubtotal (cart: Cart) : decimal =
    cart.Items
    |> List.sumBy (fun item -> item.Product.Price * decimal item.Quantity)

// Calculate line item total
let calculateLineItemTotal (item: CartItem) : decimal =
    item.Product.Price * decimal item.Quantity

// Apply discount rule to cart
let applyDiscount (subtotal: decimal) (rule: DiscountRule) : decimal =
    match rule with
    | NoDiscount -> 0m
    | PercentageOff percent -> 
        subtotal * (percent / 100m)
    | FixedAmountOff amount -> 
        min amount subtotal
    | BuyXGetY (buy, free) -> 
        0m

// Discount rules based on cart value (automatic discounts)
let getAutomaticDiscount (subtotal: decimal) : DiscountRule =
    if subtotal >= 1000m then
        PercentageOff 15m
    elif subtotal >= 500m then
        PercentageOff 10m
    elif subtotal >= 200m then
        PercentageOff 5m
    else
        NoDiscount

// Apply coupon code (MOVED UP - MUST BE BEFORE recalculateCart)
let applyCoupon (couponCode: string) : Result<DiscountRule, string> =
    match couponCode.ToUpper() with
    | "SAVE10" -> Ok (PercentageOff 10m)
    | "SAVE20" -> Ok (PercentageOff 20m)
    | "SAVE25" -> Ok (PercentageOff 25m)
    | "FLAT50" -> Ok (FixedAmountOff 50m)
    | "FLAT100" -> Ok (FixedAmountOff 100m)
    | "WELCOME15" -> Ok (PercentageOff 15m)
    | _ -> Error "Invalid coupon code"

// Validate coupon minimum purchase requirement (MOVED UP)
let validateCouponMinimum (subtotal: decimal) (couponCode: string) : bool =
    match couponCode.ToUpper() with
    | "SAVE20" -> subtotal >= 100m
    | "SAVE25" -> subtotal >= 200m
    | "FLAT100" -> subtotal >= 500m
    | _ -> true

// Calculate total with discount (preserves coupon)
let calculateTotal (cart: Cart) (discountRule: DiscountRule option) : Cart =
    let subtotal = calculateSubtotal cart
    
    let discount =
        match discountRule with
        | Some rule -> applyDiscount subtotal rule
        | None -> 
            // Check if there's already a coupon applied
            match cart.AppliedCoupon with
            | Some _ -> cart.Discount  // Keep existing coupon discount
            | None ->
                let autoRule = getAutomaticDiscount subtotal
                applyDiscount subtotal autoRule
    
    let finalTotal = subtotal - discount
    
    { cart with 
        TotalBeforeDiscount = subtotal
        Discount = discount
        FinalTotal = finalTotal }

// Recalculate cart totals (preserves coupon) - NOW applyCoupon is defined above
let recalculateCart (cart: Cart) : Cart =
    match cart.AppliedCoupon with
    | Some couponCode ->
        // If coupon is applied, recalculate with that coupon
        let subtotal = calculateSubtotal cart
        match applyCoupon couponCode with
        | Ok rule ->
            let discount = applyDiscount subtotal rule
            let finalTotal = subtotal - discount
            { cart with 
                TotalBeforeDiscount = subtotal
                Discount = discount
                FinalTotal = finalTotal }
        | Error _ ->
            calculateTotal cart None
    | None ->
        calculateTotal cart None

// Get savings amount
let getSavings (cart: Cart) : decimal =
    cart.Discount

// Get savings percentage
let getSavingsPercentage (cart: Cart) : decimal =
    if cart.TotalBeforeDiscount > 0m then
        (cart.Discount / cart.TotalBeforeDiscount) * 100m
    else
        0m


// Calculate tax
let calculateTax (amount: decimal) (taxRate: decimal) : decimal =
    amount * taxRate

// Calculate shipping
let calculateShipping (cart: Cart) (freeShippingThreshold: decimal) : decimal =
    if cart.FinalTotal >= freeShippingThreshold then
        0m
    else
        9.99m

// Calculate grand total with tax and shipping
let calculateGrandTotal (cart: Cart) (taxRate: decimal) (freeShippingThreshold: decimal) : decimal * decimal * decimal =
    let tax = calculateTax cart.FinalTotal taxRate
    let shipping = calculateShipping cart freeShippingThreshold
    let grandTotal = cart.FinalTotal + tax + shipping
    (tax, shipping, grandTotal)

// Apply coupon to cart (stores coupon code)
let applyCouponToCart (cart: Cart) (couponCode: string) : Result<Cart, string> =
    let subtotal = calculateSubtotal cart
    
    if not (validateCouponMinimum subtotal couponCode) then
        Error "Cart total doesn't meet minimum requirement for this coupon"
    else
        match applyCoupon couponCode with
        | Ok rule ->
            let discount = applyDiscount subtotal rule
            let finalTotal = subtotal - discount
            Ok { cart with 
                    TotalBeforeDiscount = subtotal
                    Discount = discount
                    FinalTotal = finalTotal
                    AppliedCoupon = Some (couponCode.ToUpper()) }
        | Error msg -> Error msg

// Remove coupon from cart
let removeCoupon (cart: Cart) : Cart =
    let recalculated = calculateTotal { cart with AppliedCoupon = None } None
    recalculated
