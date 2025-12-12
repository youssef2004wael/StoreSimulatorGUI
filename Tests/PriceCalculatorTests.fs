module StoreSimulatorGUI.Tests.PriceCalculatorTests

open Xunit
open FsUnit.Xunit
open StoreSimulatorGUI.Models
open StoreSimulatorGUI.Cart
open StoreSimulatorGUI.PriceCalculator

let testProduct1 = {
    Id = 1
    Name = "Product 1"
    Description = "Description 1"
    Price = 100.00m
    Category = "Test"
    Stock = 10
}

let testProduct2 = {
    Id = 2
    Name = "Product 2"
    Description = "Description 2"
    Price = 50.00m
    Category = "Test"
    Stock = 10
}

let createCartWithItems items =
    let mutable cart = emptyCart
    for (product, quantity) in items do
        match addToCart cart product quantity with
        | Ok c -> cart <- c
        | Error _ -> failwith "Failed to add item"
    cart

[<Fact>]
let ``calculateSubtotal should return correct total`` () =
    // Arrange
    let cart = createCartWithItems [(testProduct1, 2); (testProduct2, 3)]
    
    // Act
    let subtotal = calculateSubtotal cart
    
    // Assert
    subtotal |> should equal 350.00m // (100*2) + (50*3)

[<Fact>]
let ``calculateSubtotal should return 0 for empty cart`` () =
    // Arrange
    let cart = emptyCart
    
    // Act
    let subtotal = calculateSubtotal cart
    
    // Assert
    subtotal |> should equal 0m

[<Fact>]
let ``calculateLineItemTotal should return correct total`` () =
    // Arrange
    let item = { Product = testProduct1; Quantity = 3 }
    
    // Act
    let total = calculateLineItemTotal item
    
    // Assert
    total |> should equal 300.00m

[<Fact>]
let ``applyDiscount PercentageOff should calculate correct discount`` () =
    // Arrange
    let subtotal = 1000.00m
    let rule = PercentageOff 10m
    
    // Act
    let discount = applyDiscount subtotal rule
    
    // Assert
    discount |> should equal 100.00m

[<Fact>]
let ``applyDiscount FixedAmountOff should return fixed amount`` () =
    // Arrange
    let subtotal = 1000.00m
    let rule = FixedAmountOff 50m
    
    // Act
    let discount = applyDiscount subtotal rule
    
    // Assert
    discount |> should equal 50.00m

[<Fact>]
let ``applyDiscount FixedAmountOff should not exceed subtotal`` () =
    // Arrange
    let subtotal = 30.00m
    let rule = FixedAmountOff 50m
    
    // Act
    let discount = applyDiscount subtotal rule
    
    // Assert
    discount |> should equal 30.00m // Limited to subtotal

[<Fact>]
let ``applyDiscount NoDiscount should return 0`` () =
    // Arrange
    let subtotal = 1000.00m
    let rule = NoDiscount
    
    // Act
    let discount = applyDiscount subtotal rule
    
    // Assert
    discount |> should equal 0m

[<Fact>]
let ``getAutomaticDiscount should return 5% for $200+`` () =
    // Act
    let rule = getAutomaticDiscount 250.00m
    
    // Assert
    match rule with
    | PercentageOff percent -> percent |> should equal 5m
    | _ -> failwith "Wrong discount type"

[<Fact>]
let ``getAutomaticDiscount should return 10% for $500+`` () =
    // Act
    let rule = getAutomaticDiscount 750.00m
    
    // Assert
    match rule with
    | PercentageOff percent -> percent |> should equal 10m
    | _ -> failwith "Wrong discount type"

[<Fact>]
let ``getAutomaticDiscount should return 15% for $1000+`` () =
    // Act
    let rule = getAutomaticDiscount 1500.00m
    
    // Assert
    match rule with
    | PercentageOff percent -> percent |> should equal 15m
    | _ -> failwith "Wrong discount type"

[<Fact>]
let ``getAutomaticDiscount should return NoDiscount for < $200`` () =
    // Act
    let rule = getAutomaticDiscount 150.00m
    
    // Assert
    rule |> should equal NoDiscount

[<Fact>]
let ``applyCoupon SAVE10 should return 10% discount`` () =
    // Act
    let result = applyCoupon "SAVE10"
    
    // Assert
    match result with
    | Ok (PercentageOff percent) -> percent |> should equal 10m
    | _ -> failwith "Wrong result"

[<Fact>]
let ``applyCoupon SAVE20 should return 20% discount`` () =
    // Act
    let result = applyCoupon "SAVE20"
    
    // Assert
    match result with
    | Ok (PercentageOff percent) -> percent |> should equal 20m
    | _ -> failwith "Wrong result"

[<Fact>]
let ``applyCoupon FLAT50 should return $50 discount`` () =
    // Act
    let result = applyCoupon "FLAT50"
    
    // Assert
    match result with
    | Ok (FixedAmountOff amount) -> amount |> should equal 50m
    | _ -> failwith "Wrong result"

[<Fact>]
let ``applyCoupon should be case-insensitive`` () =
    // Act
    let result1 = applyCoupon "save10"
    let result2 = applyCoupon "SAVE10"
    let result3 = applyCoupon "Save10"
    
    // Assert
    result1 |> should equal result2
    result2 |> should equal result3

[<Fact>]
let ``applyCoupon should return error for invalid code`` () =
    // Act
    let result = applyCoupon "INVALID"
    
    // Assert
    match result with
    | Ok _ -> failwith "Should return error"
    | Error msg -> msg |> should equal "Invalid coupon code"

[<Fact>]
let ``applyCouponToCart should apply discount correctly`` () =
    // Arrange
    let cart = createCartWithItems [(testProduct1, 10)] // $1000
    
    // Act
    let result = applyCouponToCart cart "SAVE10"
    
    // Assert
    match result with
    | Ok newCart ->
        newCart.TotalBeforeDiscount |> should equal 1000.00m
        newCart.Discount |> should equal 100.00m
        newCart.FinalTotal |> should equal 900.00m
        newCart.AppliedCoupon |> should equal (Some "SAVE10")
    | Error _ -> failwith "Should not error"

[<Fact>]
let ``applyCouponToCart should validate minimum purchase`` () =
    // Arrange
    let cart = createCartWithItems [(testProduct2, 1)] // $50
    
    // Act
    let result = applyCouponToCart cart "SAVE20" // Requires $100
    
    // Assert
    match result with
    | Ok _ -> failwith "Should return error"
    | Error msg -> 
        msg.Contains("minimum") |> should equal true

[<Fact>]
let ``removeCoupon should remove applied coupon`` () =
    // Arrange
    let cart = createCartWithItems [(testProduct1, 10)]
    let cartWithCoupon =
        match applyCouponToCart cart "SAVE10" with
        | Ok c -> c
        | Error _ -> failwith "Coupon apply failed"
    
    // Act
    let result = removeCoupon cartWithCoupon
    
    // Assert
    result.AppliedCoupon |> should equal None

[<Fact>]
let ``recalculateCart should preserve coupon`` () =
    // Arrange
    let cart = createCartWithItems [(testProduct1, 5)]
    let cartWithCoupon =
        match applyCouponToCart cart "SAVE10" with
        | Ok c -> c
        | Error _ -> failwith "Coupon apply failed"
    
    // Act
    let result = recalculateCart cartWithCoupon
    
    // Assert
    result.AppliedCoupon |> should equal (Some "SAVE10")
    result.Discount |> should equal 50.00m

[<Fact>]
let ``calculateTax should calculate correct tax amount`` () =
    // Act
    let tax = calculateTax 100.00m 0.08m
    
    // Assert
    tax |> should equal 8.00m

[<Fact>]
let ``calculateShipping should return 0 for orders above threshold`` () =
    // Arrange
    let cart = createCartWithItems [(testProduct1, 10)]
    let recalculated = recalculateCart cart
    
    // Act
    let shipping = calculateShipping recalculated 50.00m
    
    // Assert
    shipping |> should equal 0m

[<Fact>]
let ``calculateShipping should return fee for orders below threshold`` () =
    // Arrange
    let cart = createCartWithItems [(testProduct2, 1)]
    let recalculated = recalculateCart cart
    
    // Act
    let shipping = calculateShipping recalculated 100.00m
    
    // Assert
    shipping |> should equal 9.99m

[<Fact>]
let ``getSavings should return discount amount`` () =
    // Arrange
    let cart = createCartWithItems [(testProduct1, 10)]
    let cartWithCoupon =
        match applyCouponToCart cart "SAVE10" with
        | Ok c -> c
        | Error _ -> failwith "Coupon apply failed"
    
    // Act
    let savings = getSavings cartWithCoupon
    
    // Assert
    savings |> should equal 100.00m

[<Fact>]
let ``getSavingsPercentage should calculate correct percentage`` () =
    // Arrange
    let cart = createCartWithItems [(testProduct1, 10)]
    let cartWithCoupon =
        match applyCouponToCart cart "SAVE10" with
        | Ok c -> c
        | Error _ -> failwith "Coupon apply failed"
    
    // Act
    let percentage = getSavingsPercentage cartWithCoupon
    
    // Assert
    percentage |> should equal 10.00m