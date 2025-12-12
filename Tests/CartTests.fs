module StoreSimulatorGUI.Tests.CartTests

open Xunit
open FsUnit.Xunit
open StoreSimulatorGUI.Models
open StoreSimulatorGUI.Cart
open StoreSimulatorGUI.Catalog

let testProduct = {
    Id = 1
    Name = "Test Product"
    Description = "Test Description"
    Price = 100.00m
    Category = "Test"
    Stock = 10
}

let testProduct2 = {
    Id = 2
    Name = "Test Product 2"
    Description = "Test Description 2"
    Price = 50.00m
    Category = "Test"
    Stock = 20
}

[<Fact>]
let ``emptyCart should create empty cart`` () =
    // Act
    let cart = emptyCart
    
    // Assert
    cart.Items.Length |> should equal 0
    cart.TotalBeforeDiscount |> should equal 0m
    cart.Discount |> should equal 0m
    cart.FinalTotal |> should equal 0m
    cart.AppliedCoupon |> should equal None

[<Fact>]
let ``isEmpty should return true for empty cart`` () =
    // Arrange
    let cart = emptyCart
    
    // Act & Assert
    isEmpty cart |> should equal true

[<Fact>]
let ``isEmpty should return false for non-empty cart`` () =
    // Arrange
    let cart = emptyCart
    let cartWithItem = (addToCart cart testProduct 1)
    
    // Act & Assert
    match cartWithItem with
    | Ok c -> isEmpty c |> should equal false
    | Error _ -> failwith "Should not error"

[<Fact>]
let ``addToCart should add item to empty cart`` () =
    // Arrange
    let cart = emptyCart
    
    // Act
    let result = addToCart cart testProduct 2
    
    // Assert
    match result with
    | Ok newCart ->
        newCart.Items.Length |> should equal 1
        newCart.Items.[0].Product.Id |> should equal testProduct.Id
        newCart.Items.[0].Quantity |> should equal 2
    | Error _ -> failwith "Should not return error"

[<Fact>]
let ``addToCart should return error for invalid quantity`` () =
    // Arrange
    let cart = emptyCart
    
    // Act
    let result = addToCart cart testProduct 0
    
    // Assert
    match result with
    | Ok _ -> failwith "Should return error"
    | Error msg -> msg |> should equal "Quantity must be positive"

[<Fact>]
let ``addToCart should return error when exceeding stock`` () =
    // Arrange
    let cart = emptyCart
    
    // Act
    let result = addToCart cart testProduct 100
    
    // Assert
    match result with
    | Ok _ -> failwith "Should return error"
    | Error msg -> 
        msg.Contains("Not enough stock") |> should equal true

[<Fact>]
let ``addToCart should merge quantities for same product`` () =
    // Arrange
    let cart = emptyCart
    let cart1 = addToCart cart testProduct 2
    
    // Act
    let result = 
        match cart1 with
        | Ok c -> addToCart c testProduct 3
        | Error _ -> failwith "First add failed"
    
    // Assert
    match result with
    | Ok newCart ->
        newCart.Items.Length |> should equal 1
        newCart.Items.[0].Quantity |> should equal 5
    | Error _ -> failwith "Should not return error"

[<Fact>]
let ``addToCart should add different products separately`` () =
    // Arrange
    let cart = emptyCart
    let cart1 = addToCart cart testProduct 1
    
    // Act
    let result =
        match cart1 with
        | Ok c -> addToCart c testProduct2 1
        | Error _ -> failwith "First add failed"
    
    // Assert
    match result with
    | Ok newCart ->
        newCart.Items.Length |> should equal 2
    | Error _ -> failwith "Should not return error"

[<Fact>]
let ``removeFromCart should remove item from cart`` () =
    // Arrange
    let cart = emptyCart
    let cartWithItem =
        match addToCart cart testProduct 2 with
        | Ok c -> c
        | Error _ -> failwith "Add failed"
    
    // Act
    let result = removeFromCart cartWithItem testProduct.Id
    
    // Assert
    result.Items.Length |> should equal 0

[<Fact>]
let ``removeFromCart should only remove specified item`` () =
    // Arrange
    let cart = emptyCart
    let cart1 =
        match addToCart cart testProduct 1 with
        | Ok c -> c
        | Error _ -> failwith "Add 1 failed"
    let cart2 =
        match addToCart cart1 testProduct2 1 with
        | Ok c -> c
        | Error _ -> failwith "Add 2 failed"
    
    // Act
    let result = removeFromCart cart2 testProduct.Id
    
    // Assert
    result.Items.Length |> should equal 1
    result.Items.[0].Product.Id |> should equal testProduct2.Id

[<Fact>]
let ``updateQuantity should update item quantity`` () =
    // Arrange
    let cart = emptyCart
    let cartWithItem =
        match addToCart cart testProduct 2 with
        | Ok c -> c
        | Error _ -> failwith "Add failed"
    
    // Act
    let result = updateQuantity cartWithItem testProduct.Id 5
    
    // Assert
    match result with
    | Ok newCart ->
        newCart.Items.[0].Quantity |> should equal 5
    | Error _ -> failwith "Should not error"

[<Fact>]
let ``updateQuantity should remove item when quantity is 0`` () =
    // Arrange
    let cart = emptyCart
    let cartWithItem =
        match addToCart cart testProduct 2 with
        | Ok c -> c
        | Error _ -> failwith "Add failed"
    
    // Act
    let result = updateQuantity cartWithItem testProduct.Id 0
    
    // Assert
    match result with
    | Ok newCart ->
        newCart.Items.Length |> should equal 0
    | Error _ -> failwith "Should not error"

[<Fact>]
let ``updateQuantity should return error when exceeding stock`` () =
    // Arrange
    let cart = emptyCart
    let cartWithItem =
        match addToCart cart testProduct 2 with
        | Ok c -> c
        | Error _ -> failwith "Add failed"
    
    // Act
    let result = updateQuantity cartWithItem testProduct.Id 100
    
    // Assert
    match result with
    | Ok _ -> failwith "Should return error"
    | Error msg -> 
        msg.Contains("Not enough stock") |> should equal true

[<Fact>]
let ``getItemCount should return total quantity of all items`` () =
    // Arrange
    let cart = emptyCart
    let cart1 =
        match addToCart cart testProduct 3 with
        | Ok c -> c
        | Error _ -> failwith "Add 1 failed"
    let cart2 =
        match addToCart cart1 testProduct2 2 with
        | Ok c -> c
        | Error _ -> failwith "Add 2 failed"
    
    // Act
    let count = getItemCount cart2
    
    // Assert
    count |> should equal 5

[<Fact>]
let ``getUniqueProductCount should return number of different products`` () =
    // Arrange
    let cart = emptyCart
    let cart1 =
        match addToCart cart testProduct 3 with
        | Ok c -> c
        | Error _ -> failwith "Add 1 failed"
    let cart2 =
        match addToCart cart1 testProduct2 2 with
        | Ok c -> c
        | Error _ -> failwith "Add 2 failed"
    
    // Act
    let count = getUniqueProductCount cart2
    
    // Assert
    count |> should equal 2

[<Fact>]
let ``clearCart should return empty cart`` () =
    // Arrange
    let cart = emptyCart
    let cartWithItems =
        match addToCart cart testProduct 5 with
        | Ok c -> c
        | Error _ -> failwith "Add failed"
    
    // Act
    let result = clearCart cartWithItems
    
    // Assert
    result.Items.Length |> should equal 0
    isEmpty result |> should equal true

[<Fact>]
let ``findCartItem should return Some when item exists`` () =
    // Arrange
    let cart = emptyCart
    let cartWithItem =
        match addToCart cart testProduct 2 with
        | Ok c -> c
        | Error _ -> failwith "Add failed"
    
    // Act
    let result = findCartItem cartWithItem testProduct.Id
    
    // Assert
    result |> should not' (equal None)
    match result with
    | Some item -> item.Product.Id |> should equal testProduct.Id
    | None -> failwith "Should find item"

[<Fact>]
let ``findCartItem should return None when item does not exist`` () =
    // Arrange
    let cart = emptyCart
    
    // Act
    let result = findCartItem cart 999
    
    // Assert
    result |> should equal None

[<Fact>]
let ``isInCart should return true when product is in cart`` () =
    // Arrange
    let cart = emptyCart
    let cartWithItem =
        match addToCart cart testProduct 1 with
        | Ok c -> c
        | Error _ -> failwith "Add failed"
    
    // Act & Assert
    isInCart cartWithItem testProduct.Id |> should equal true

[<Fact>]
let ``isInCart should return false when product is not in cart`` () =
    // Arrange
    let cart = emptyCart
    
    // Act & Assert
    isInCart cart testProduct.Id |> should equal false