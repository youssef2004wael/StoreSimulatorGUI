module StoreSimulatorGUI.PriceCalculator

open StoreSimulatorGUI.Models
open StoreSimulatorGUI.Cart


let calculateSubtotal (cart: Cart) : decimal =
    cart.Items
    |> List.sumBy (fun item -> item.Product.Price * decimal item.Quantity)


let calculateLineItemTotal (item: CartItem) : decimal =
    item.Product.Price * decimal item.Quantity


let applyDiscount (subtotal: decimal) (rule: DiscountRule) : decimal =
    match rule with
    | NoDiscount -> 0m
    | PercentageOff percent -> 
        subtotal * (percent / 100m)
    | FixedAmountOff amount -> 
        min amount subtotal
    | BuyXGetY (buy, free) -> 
        0m


let getAutomaticDiscount (subtotal: decimal) : DiscountRule =
    if subtotal >= 1000m then
        PercentageOff 15m
    elif subtotal >= 500m then
        PercentageOff 10m
    elif subtotal >= 200m then
        PercentageOff 5m
    else
        NoDiscount

let getDiscountDescription (rule: DiscountRule) : string =
    match rule with
    | NoDiscount -> "No discount"
    | PercentageOff percent -> sprintf "%.0f%% off" percent
    | FixedAmountOff amount -> sprintf "$%.2f off" amount
    | BuyXGetY (buy, free) -> sprintf "Buy %d get %d free" buy free


let applyCoupon (couponCode: string) : Result<DiscountRule, string> =
    match couponCode.ToUpper() with
    | "SAVE10" -> Ok (PercentageOff 10m)
    | "SAVE20" -> Ok (PercentageOff 20m)
    | "SAVE25" -> Ok (PercentageOff 25m)
    | "FLAT50" -> Ok (FixedAmountOff 50m)
    | "FLAT100" -> Ok (FixedAmountOff 100m)
    | "WELCOME15" -> Ok (PercentageOff 15m)
    | _ -> Error "Invalid coupon code"


let validateCouponMinimum (subtotal: decimal) (couponCode: string) : bool =
    match couponCode.ToUpper() with
    | "SAVE20" -> subtotal >= 100m
    | "SAVE25" -> subtotal >= 200m
    | "FLAT100" -> subtotal >= 500m
    | _ -> true

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


let recalculateCart (cart: Cart) : Cart =
    match cart.AppliedCoupon with
    | Some couponCode ->
       
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

let getSavings (cart: Cart) : decimal =
    cart.Discount


let getSavingsPercentage (cart: Cart) : decimal =
    if cart.TotalBeforeDiscount > 0m then
        (cart.Discount / cart.TotalBeforeDiscount) * 100m
    else
        0m

let displayPriceBreakdown (cart: Cart) : string =
    let lines = [
        "--- Price Breakdown ---"
        sprintf "Subtotal:        $%.2f" cart.TotalBeforeDiscount
        if cart.Discount > 0m then
            match cart.AppliedCoupon with
            | Some coupon ->
                sprintf "Coupon (%s):    -$%.2f" coupon cart.Discount
            | None ->
                sprintf "Discount:       -$%.2f" cart.Discount
        "------------------------"
        sprintf "Final Total:     $%.2f" cart.FinalTotal
        "========================"
    ]
    String.concat "\n" (lines |> List.filter (fun s -> s <> ""))


let calculateTax (amount: decimal) (taxRate: decimal) : decimal =
    amount * taxRate


let calculateShipping (cart: Cart) (freeShippingThreshold: decimal) : decimal =
    if cart.FinalTotal >= freeShippingThreshold then
        0m
    else
        9.99m


let calculateGrandTotal (cart: Cart) (taxRate: decimal) (freeShippingThreshold: decimal) : decimal * decimal * decimal =
    let tax = calculateTax cart.FinalTotal taxRate
    let shipping = calculateShipping cart freeShippingThreshold
    let grandTotal = cart.FinalTotal + tax + shipping
    (tax, shipping, grandTotal)


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

let removeCoupon (cart: Cart) : Cart =
    let recalculated = calculateTotal { cart with AppliedCoupon = None } None
    recalculated

let qualifiesForFreeShipping (cart: Cart) (threshold: decimal) : bool =
    cart.FinalTotal >= threshold

let amountNeededForFreeShipping (cart: Cart) (threshold: decimal) : decimal =
    if qualifiesForFreeShipping cart threshold then
        0m
    else
        threshold - cart.FinalTotal


let displayDetailedPriceSummary (cart: Cart) (taxRate: decimal) (freeShippingThreshold: decimal) : string =
    let (tax, shipping, grandTotal) = calculateGrandTotal cart taxRate freeShippingThreshold
    let lines = [
        "================================"
        "      PRICE SUMMARY"
        "================================"
        sprintf "Subtotal:          $%.2f" cart.TotalBeforeDiscount
        if cart.Discount > 0m then
            match cart.AppliedCoupon with
            | Some coupon ->
                sprintf "Coupon (%s):      -$%.2f (%.1f%% off)" coupon cart.Discount (getSavingsPercentage cart)
            | None ->
                sprintf "Discount:         -$%.2f (%.1f%% off)" cart.Discount (getSavingsPercentage cart)
        "--------------------------------"
        sprintf "Subtotal after discount: $%.2f" cart.FinalTotal
        sprintf "Tax (%.1f%%):        $%.2f" (taxRate * 100m) tax
        if shipping = 0m then
            sprintf "Shipping:          FREE ✓"
        else
            sprintf "Shipping:          $%.2f" shipping
        "================================"
        sprintf "GRAND TOTAL:       $%.2f" grandTotal
        "================================"
    ]
    String.concat "\n" (lines |> List.filter (fun s -> s <> ""))


let getAverageItemPrice (cart: Cart) : decimal =
    if isEmpty cart then
        0m
    else
        cart.TotalBeforeDiscount / decimal (getItemCount cart)

let getMostExpensiveItem (cart: Cart) : CartItem option =
    if isEmpty cart then
        None
    else
        cart.Items 
        |> List.maxBy (fun item -> item.Product.Price)
        |> Some


let getCheapestItem (cart: Cart) : CartItem option =
    if isEmpty cart then
        None
    else
        cart.Items 
        |> List.minBy (fun item -> item.Product.Price)
        |> Some
