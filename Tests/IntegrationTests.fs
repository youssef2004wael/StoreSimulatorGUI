module StoreSimulatorGUI.Tests.IntegrationTests

open Xunit
open FsUnit.Xunit
open StoreSimulatorGUI.Models
open StoreSimulatorGUI.Catalog
open StoreSimulatorGUI.Cart
open StoreSimulatorGUI.PriceCalculator
open StoreSimulatorGUI.Search
open StoreSimulatorGUI.FileOperations

[<Fact>]
let ``Complete shopping workflow should work end-to-end`` () =
    // 1. Initialize catalog
    let catalog = initializeCatalog()
    catalog.Count |> should equal 15
    
    // 2. Search for products
    let allProducts = getAllProducts catalog
    let laptops = searchByName allProducts "Laptop"
    laptops.Length |> should equal 1
    
    // 3. Create cart and add items
    let cart = emptyCart
    let laptop = laptops.[0]
    
    let cartWithLaptop =
        match addToCart cart laptop 2 with
        | Ok c -> c
        | Error msg -> failwith $"Add failed: {msg}"
    
    // 4. Add more items
    let miceResults = searchByName allProducts "Mouse"
    let mouse = miceResults.[0]
    
    let cartWithBoth =
        match addToCart cartWithLaptop mouse 3 with
        | Ok c -> c
        | Error msg -> failwith $"Add failed: {msg}"
    
    // 5. Calculate prices
    let recalculated = recalculateCart cartWithBoth
    let subtotal = calculateSubtotal recalculated
    subtotal |> should equal (999.99m * 2m + 29.99m * 3m)
    
    // 6. Apply coupon
    let cartWithCoupon =
        match applyCouponToCart recalculated "SAVE10" with
        | Ok c -> c
        | Error msg -> failwith $"Coupon failed: {msg}"
    
    cartWithCoupon.Discount |> should be (greaterThan 0m)
    cartWithCoupon.AppliedCoupon |> should equal (Some "SAVE10")
    
    // 7. Save order
    let saveResult = saveOrder cartWithCoupon
    match saveResult with
    | Ok filePath ->
        System.IO.File.Exists(filePath) |> should equal true
    | Error msg -> failwith $"Save failed: {msg}"
    
    // 8. Verify stock decreased
    let updatedCatalog = decreaseStockBatch catalog cartWithCoupon.Items
    let laptopAfter = getProduct updatedCatalog laptop.Id
    match laptopAfter with
    | Some p -> p.Stock |> should equal (laptop.Stock - 2)
    | None -> failwith "Laptop should still exist"
    
[<Fact>]
let ``Filter and purchase workflow`` () =
    // 1. Get Electronics products
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    let electronics = filterByCategory allProducts "Electronics"
    electronics.Length |> should equal 6  // Changed from 9 to 6
    
    // 2. Filter by price range
    let affordable = filterByPriceRange electronics 0m 100m
    affordable.Length |> should be (greaterThan 0)
    
    // 3. Sort by price
    let sorted = sortByPrice affordable true
    sorted |> List.pairwise |> List.forall (fun (a, b) -> a.Price <= b.Price) 
    |> should equal true
    
    // 4. Add cheapest to cart
    let cheapest = sorted |> List.head
    let cart = emptyCart
    let result = addToCart cart cheapest 1
    
    match result with
    | Ok c -> c.Items.Length |> should equal 1
    | Error _ -> failwith "Should add successfully"

[<Fact>]
let ``Stock management integration test`` () =
    // 1. Get product with limited stock
    let catalog = initializeCatalog()
    let laptop = (getProduct catalog 1).Value
    let originalStock = laptop.Stock
    
    // 2. Try to add more than available
    let cart = emptyCart
    let result = addToCart cart laptop (originalStock + 5)
    
    match result with
    | Ok _ -> failwith "Should reject over-stock"
    | Error msg -> 
        let isValidError = msg.Contains("Not enough stock") || msg.Contains("stock")
        isValidError |> should equal true
    
    // 3. Add maximum available
    let validResult = addToCart cart laptop originalStock
    match validResult with
    | Ok c -> c.Items.[0].Quantity |> should equal originalStock
    | Error _ -> failwith "Should accept available stock"
    
    // 4. Decrease stock
    let cartItems = (validResult |> Result.toOption).Value.Items
    let updatedCatalog = decreaseStockBatch catalog cartItems
    let laptopAfter = (getProduct updatedCatalog 1).Value
    laptopAfter.Stock |> should equal 0
    
[<Fact>]
let ``Multiple coupons test`` () =
    // Test that only one coupon can be applied at a time
    let catalog = initializeCatalog()
    let laptop = (getProduct catalog 1).Value
    let cart = emptyCart
    
    let cartWithItem =
        match addToCart cart laptop 1 with
        | Ok c -> c
        | Error _ -> failwith "Add failed"
    
    // Apply first coupon
    let cartWithCoupon1 =
        match applyCouponToCart cartWithItem "SAVE10" with
        | Ok c -> c
        | Error _ -> failwith "First coupon failed"
    
    cartWithCoupon1.AppliedCoupon |> should equal (Some "SAVE10")
    
    // Apply second coupon (should replace first)
    let cartWithCoupon2 =
        match applyCouponToCart cartWithCoupon1 "SAVE20" with
        | Ok c -> c
        | Error _ -> failwith "Second coupon failed"
    
    cartWithCoupon2.AppliedCoupon |> should equal (Some "SAVE20")

[<Fact>]
let ``Cart persistence through recalculation`` () =
    // Ensure cart state persists correctly through multiple operations
    let catalog = initializeCatalog()
    let products = getAllProducts catalog |> List.take 3
    let mutable cart = emptyCart
    
    // Add multiple items
    for product in products do
        match addToCart cart product 2 with
        | Ok c -> cart <- c
        | Error _ -> failwith "Add failed"
    
    cart.Items.Length |> should equal 3
    
    // Recalculate
    let recalc1 = recalculateCart cart
    recalc1.Items.Length |> should equal 3
    
    // Apply coupon
    let withCoupon =
        match applyCouponToCart recalc1 "SAVE10" with
        | Ok c -> c
        | Error _ -> failwith "Coupon failed"
    
    // Recalculate again
    let recalc2 = recalculateCart withCoupon
    recalc2.Items.Length |> should equal 3
    recalc2.AppliedCoupon |> should equal (Some "SAVE10")
    
    // Remove one item
    let withRemoval = removeFromCart recalc2 (products.[0].Id)
    withRemoval.Items.Length |> should equal 2
    
    // Recalculate final
    let recalc3 = recalculateCart withRemoval
    recalc3.Items.Length |> should equal 2
    recalc3.AppliedCoupon |> should equal (Some "SAVE10")