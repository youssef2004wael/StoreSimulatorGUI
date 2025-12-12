module StoreSimulatorGUI.Tests.FileOperationsTests

open System
open System.IO
open Xunit
open FsUnit.Xunit
open StoreSimulatorGUI.Models
open StoreSimulatorGUI.Cart
open StoreSimulatorGUI.FileOperations
open StoreSimulatorGUI.Catalog

let testDirectory = "TestOrders"

let cleanupTestDirectory() =
    if Directory.Exists(testDirectory) then
        Directory.Delete(testDirectory, true)

let testProduct = {
    Id = 1
    Name = "Test Product"
    Description = "Test Description"
    Price = 100.00m
    Category = "Test"
    Stock = 10
}

let createTestCart() =
    let cart = emptyCart
    match addToCart cart testProduct 2 with
    | Ok c -> c
    | Error _ -> failwith "Failed to create test cart"

[<Fact>]
let ``saveOrder should create Orders directory`` () =
    // Arrange
    cleanupTestDirectory()
    let cart = createTestCart()
    
    // Act
    let result = saveOrder cart
    
    // Assert
    match result with
    | Ok filePath ->
        Directory.Exists("Orders") |> should equal true
        File.Exists(filePath) |> should equal true
    | Error msg -> failwith $"Save failed: {msg}"

[<Fact>]
let ``saveOrder should create file with timestamp`` () =
    // Arrange
    let cart = createTestCart()
    
    // Act
    let result = saveOrder cart
    
    // Assert
    match result with
    | Ok filePath ->
        let filename = Path.GetFileName(filePath)
        filename |> should startWith "order_"
        filename |> should endWith ".json"
    | Error _ -> failwith "Should succeed"

[<Fact>]
let ``createOrderSummary should create valid summary`` () =
    // Arrange
    let cart = createTestCart()
    
    // Act
    let summary = createOrderSummary cart
    
    // Assert
    summary.OrderId |> should not' (equal "")
    summary.Items.Length |> should equal cart.Items.Length
    summary.Subtotal |> should equal cart.TotalBeforeDiscount
    summary.Total |> should equal cart.FinalTotal

[<Fact>]
let ``saveOrderToJson and loadOrderFromJson should round-trip`` () =
    // Arrange
    cleanupTestDirectory()
    Directory.CreateDirectory(testDirectory) |> ignore
    let cart = createTestCart()
    let summary = createOrderSummary cart
    let testFile = Path.Combine(testDirectory, "test_order.json")
    
    // Act
    let saveResult = saveOrderToJson summary testFile
    let loadResult = loadOrderFromJson testFile
    
    // Assert
    match saveResult, loadResult with
    | Ok _, Ok loadedSummary ->
        loadedSummary.OrderId |> should equal summary.OrderId
        loadedSummary.Items.Length |> should equal summary.Items.Length
        loadedSummary.Total |> should equal summary.Total
    | Error msg, _ -> failwith $"Save failed: {msg}"
    | _, Error msg -> failwith $"Load failed: {msg}"
    
    // Cleanup
    cleanupTestDirectory()

[<Fact>]
let ``loadOrderFromJson should return error for non-existent file`` () =
    // Act
    let result = loadOrderFromJson "nonexistent.json"
    
    // Assert
    match result with
    | Ok _ -> failwith "Should return error"
    | Error msg -> 
        let isValidError = msg.Contains("not found") || msg.Contains("File")
        isValidError |> should equal true
        
[<Fact>]
let ``listSavedOrders should return list of order files`` () =
    // Arrange
    let cart = createTestCart()
    saveOrder cart |> ignore
    
    // Act
    let orders = listSavedOrders()
    
    // Assert
    orders.Length |> should be (greaterThan 0)
    orders |> List.forall (fun f -> f.Contains("order_") && f.EndsWith(".json")) 
    |> should equal true

[<Fact>]
let ``saveCatalogToJson and loadCatalogFromJson should round-trip`` () =
    // Arrange
    cleanupTestDirectory()
    Directory.CreateDirectory(testDirectory) |> ignore
    let catalog = initializeCatalog()
    let testFile = Path.Combine(testDirectory, "test_catalog.json")
    
    // Act
    let saveResult = saveCatalogToJson catalog testFile
    let loadResult = loadCatalogFromJson testFile
    
    // Assert
    match saveResult, loadResult with
    | Ok _, Ok loadedCatalog ->
        loadedCatalog.Count |> should equal catalog.Count
        let originalProducts = getAllProducts catalog
        let loadedProducts = getAllProducts loadedCatalog
        loadedProducts.Length |> should equal originalProducts.Length
    | Error msg, _ -> failwith $"Save failed: {msg}"
    | _, Error msg -> failwith $"Load failed: {msg}"
    
    // Cleanup
    cleanupTestDirectory()

[<Fact>]
let ``exportCartToText should create readable text file`` () =
    // Arrange
    cleanupTestDirectory()
    Directory.CreateDirectory(testDirectory) |> ignore
    let cart = createTestCart()
    let testFile = Path.Combine(testDirectory, "test_cart.txt")
    
    // Act
    let result = exportCartToText cart testFile
    
    // Assert
    match result with
    | Ok filePath ->
        File.Exists(filePath) |> should equal true
        let content = File.ReadAllText(filePath)
        content.Contains("STORE CART SUMMARY") |> should equal true
        content.Contains(testProduct.Name) |> should equal true
    | Error msg -> failwith $"Export failed: {msg}"
    
    // Cleanup
    cleanupTestDirectory()

[<Fact>]
let ``exportCatalogToCsv should create valid CSV`` () =
    // Arrange
    cleanupTestDirectory()
    Directory.CreateDirectory(testDirectory) |> ignore
    let catalog = initializeCatalog()
    let testFile = Path.Combine(testDirectory, "test_catalog.csv")
    
    // Act
    let result = exportCatalogToCsv catalog testFile
    
    // Assert
    match result with
    | Ok filePath ->
        File.Exists(filePath) |> should equal true
        let lines = File.ReadAllLines(filePath)
        lines.Length |> should be (greaterThan 1) // Header + data
        lines.[0].Contains("ID") |> should equal true
        lines.[0].Contains("Name") |> should equal true
    | Error msg -> failwith $"Export failed: {msg}"
    
    // Cleanup
    cleanupTestDirectory()
    
[<Fact>]
let ``generateOrderFileName should include timestamp`` () =
    // Act
    let filename = generateOrderFileName()
    
    // Assert
    filename |> should startWith "order_"
    filename |> should endWith ".json"
    filename.Length |> should be (greaterThan 20) // order_ + timestamp + .json