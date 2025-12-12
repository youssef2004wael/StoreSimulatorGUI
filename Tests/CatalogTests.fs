module StoreSimulatorGUI.Tests.CatalogTests

open Xunit
open FsUnit.Xunit
open StoreSimulatorGUI.Models
open StoreSimulatorGUI.Catalog

[<Fact>]
let ``initializeCatalog should return 15 products`` () =
    // Arrange & Act
    let catalog = initializeCatalog()
    
    // Assert
    catalog.Count |> should equal 15

[<Fact>]
let ``initializeCatalog should use Map structure`` () =
    // Arrange & Act
    let catalog = initializeCatalog()
    
    // Assert
    catalog |> should be instanceOfType<Map<int, Product>>

[<Fact>]
let ``getProduct should return Some when product exists`` () =
    // Arrange
    let catalog = initializeCatalog()
    
    // Act
    let result = getProduct catalog 1
    
    // Assert
    result |> should not' (equal None)
    match result with
    | Some product -> 
        product.Name |> should equal "Laptop"
        product.Price |> should equal 999.99m
    | None -> failwith "Product should exist"

[<Fact>]
let ``getProduct should return None when product does not exist`` () =
    // Arrange
    let catalog = initializeCatalog()
    
    // Act
    let result = getProduct catalog 999
    
    // Assert
    result |> should equal None

[<Fact>]
let ``getAllProducts should return list of 15 products`` () =
    // Arrange
    let catalog = initializeCatalog()
    
    // Act
    let products = getAllProducts catalog
    
    // Assert
    products.Length |> should equal 15
    products |> should be instanceOfType<Product list>

[<Fact>]
let ``addProduct should add new product to catalog`` () =
    // Arrange
    let catalog = initializeCatalog()
    let newProduct = {
        Id = 100
        Name = "Test Product"
        Description = "Test Description"
        Price = 50.00m
        Category = "Test"
        Stock = 10
    }
    
    // Act
    let updatedCatalog = addProduct catalog newProduct
    
    // Assert
    updatedCatalog.Count |> should equal 16
    let retrieved = getProduct updatedCatalog 100
    retrieved |> should not' (equal None)

[<Fact>]
let ``updateStock should update product stock immutably`` () =
    // Arrange
    let catalog = initializeCatalog()
    let originalStock = (getProduct catalog 1).Value.Stock
    
    // Act
    let updatedCatalog = updateStock catalog 1 50
    
    // Assert
    (getProduct updatedCatalog 1).Value.Stock |> should equal 50
    (getProduct catalog 1).Value.Stock |> should equal originalStock // Original unchanged

[<Fact>]
let ``updateStock should return unchanged catalog if product not found`` () =
    // Arrange
    let catalog = initializeCatalog()
    
    // Act
    let result = updateStock catalog 999 50
    
    // Assert
    result.Count |> should equal catalog.Count

[<Fact>]
let ``hasStock should return true when sufficient stock available`` () =
    // Arrange
    let catalog = initializeCatalog()
    
    // Act & Assert
    hasStock catalog 1 5 |> should equal true

[<Fact>]
let ``hasStock should return false when insufficient stock`` () =
    // Arrange
    let catalog = initializeCatalog()
    
    // Act & Assert
    hasStock catalog 1 100 |> should equal false

[<Fact>]
let ``hasStock should return false for non-existent product`` () =
    // Arrange
    let catalog = initializeCatalog()
    
    // Act & Assert
    hasStock catalog 999 1 |> should equal false

[<Fact>]
let ``removeProduct should remove product from catalog`` () =
    // Arrange
    let catalog = initializeCatalog()
    
    // Act
    let updatedCatalog = removeProduct catalog 1
    
    // Assert
    updatedCatalog.Count |> should equal 14
    getProduct updatedCatalog 1 |> should equal None

[<Fact>]
let ``productExists should return true for existing product`` () =
    // Arrange
    let catalog = initializeCatalog()
    
    // Act & Assert
    productExists catalog 1 |> should equal true

[<Fact>]
let ``productExists should return false for non-existing product`` () =
    // Arrange
    let catalog = initializeCatalog()
    
    // Act & Assert
    productExists catalog 999 |> should equal false

[<Fact>]
let ``decreaseStockBatch should decrease stock for multiple items`` () =
    // Arrange
    let catalog = initializeCatalog()
    let product1 = (getProduct catalog 1).Value
    let product2 = (getProduct catalog 2).Value
    let cartItems = [
        { Product = product1; Quantity = 2 }
        { Product = product2; Quantity = 3 }
    ]
    
    // Act
    let updatedCatalog = decreaseStockBatch catalog cartItems
    
    // Assert
    (getProduct updatedCatalog 1).Value.Stock |> should equal (product1.Stock - 2)
    (getProduct updatedCatalog 2).Value.Stock |> should equal (product2.Stock - 3)

[<Fact>]
let ``getLowStockProducts should return products below threshold`` () =
    // Arrange
    let catalog = initializeCatalog()
    let threshold = 20
    
    // Act
    let lowStock = getLowStockProducts catalog threshold
    
    // Assert
    lowStock |> List.forall (fun p -> p.Stock < threshold && p.Stock > 0) |> should equal true

[<Fact>]
let ``getOutOfStockProducts should return empty list when all in stock`` () =
    // Arrange
    let catalog = initializeCatalog()
    
    // Act
    let outOfStock = getOutOfStockProducts catalog
    
    // Assert
    outOfStock.Length |> should equal 0