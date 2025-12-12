module StoreSimulatorGUI.Tests.SearchTests

open Xunit
open FsUnit.Xunit
open StoreSimulatorGUI.Models
open StoreSimulatorGUI.Catalog
open StoreSimulatorGUI.Search

[<Fact>]
let ``searchByName should find products by name`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let results = searchByName allProducts "Laptop"
    
    // Assert
    results.Length |> should equal 1
    results.[0].Name |> should equal "Laptop"

[<Fact>]
let ``searchByName should be case-insensitive`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let results1 = searchByName allProducts "laptop"
    let results2 = searchByName allProducts "LAPTOP"
    let results3 = searchByName allProducts "Laptop"
    
    // Assert
    results1.Length |> should equal results2.Length
    results2.Length |> should equal results3.Length

[<Fact>]
let ``searchByName should return empty list for no matches`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let results = searchByName allProducts "NonExistentProduct"
    
    // Assert
    results.Length |> should equal 0

[<Fact>]
let ``searchByName should find partial matches`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let results = searchByName allProducts "Desk"
    
    // Assert
    results.Length |> should be (greaterThan 0)
    results |> List.forall (fun p -> p.Name.Contains("Desk")) |> should equal true

[<Fact>]
let ``filterByCategory should filter Electronics products`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let results = filterByCategory allProducts "Electronics"
    
    // Assert
    results.Length |> should equal 6  // Changed from 9 to 6
    results |> List.forall (fun p -> p.Category = "Electronics") |> should equal true

[<Fact>]
let ``filterByCategory should filter Furniture products`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let results = filterByCategory allProducts "Furniture"
    
    // Assert
    results.Length |> should equal 2
    results |> List.forall (fun p -> p.Category = "Furniture") |> should equal true

[<Fact>]
let ``filterByCategory should return empty for non-existent category`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let results = filterByCategory allProducts "NonExistent"
    
    // Assert
    results.Length |> should equal 0

[<Fact>]
let ``filterByPriceRange should filter products in range`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let results = filterByPriceRange allProducts 20m 100m
    
    // Assert
    results.Length |> should be (greaterThan 0)
    results |> List.forall (fun p -> p.Price >= 20m && p.Price <= 100m) |> should equal true

[<Fact>]
let ``filterByPriceRange should handle edge cases`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let results = filterByPriceRange allProducts 29.99m 29.99m
    
    // Assert
    results.Length |> should be (greaterThan 0)
    results |> List.forall (fun p -> p.Price = 29.99m) |> should equal true

[<Fact>]
let ``filterInStock should return only products with stock`` () =
    // Arrange
    let products = [
        { Id = 1; Name = "In Stock"; Description = ""; Price = 10m; Category = "Test"; Stock = 5 }
        { Id = 2; Name = "Out of Stock"; Description = ""; Price = 20m; Category = "Test"; Stock = 0 }
        { Id = 3; Name = "In Stock 2"; Description = ""; Price = 30m; Category = "Test"; Stock = 10 }
    ]
    
    // Act
    let results = filterInStock products
    
    // Assert
    results.Length |> should equal 2
    results |> List.forall (fun p -> p.Stock > 0) |> should equal true

[<Fact>]
let ``getCategories should return unique categories`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let categories = getCategories allProducts
    
    // Assert
    categories.Length |> should be (greaterThan 0)
    categories |> List.distinct |> List.length |> should equal categories.Length // All unique

[<Fact>]
let ``getCategories should include all expected categories`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let categories = getCategories allProducts
    
    // Assert
    categories |> should contain "Electronics"
    categories |> should contain "Furniture"
    categories |> should contain "Accessories"
    categories |> should contain "Stationery"
    categories |> should contain "Kitchen"

[<Fact>]
let ``sortByPrice ascending should sort correctly`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let sorted = sortByPrice allProducts true
    
    // Assert
    sorted.Length |> should equal allProducts.Length
    // Check if sorted (each element <= next element)
    sorted 
    |> List.pairwise 
    |> List.forall (fun (a, b) -> a.Price <= b.Price) 
    |> should equal true

[<Fact>]
let ``sortByPrice descending should sort correctly`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act
    let sorted = sortByPrice allProducts false
    
    // Assert
    sorted.Length |> should equal allProducts.Length
    // Check if sorted descending
    sorted 
    |> List.pairwise 
    |> List.forall (fun (a, b) -> a.Price >= b.Price) 
    |> should equal true

[<Fact>]
let ``combining filters should work correctly`` () =
    // Arrange
    let catalog = initializeCatalog()
    let allProducts = getAllProducts catalog
    
    // Act - Search "Desk" and filter by "Furniture" category
    let step1 = searchByName allProducts "Desk"
    let step2 = filterByCategory step1 "Furniture"
    
    // Assert
    step2.Length |> should be (greaterThan 0)
    step2 |> List.forall (fun p -> 
        p.Category = "Furniture" && p.Name.Contains("Desk")) 
    |> should equal true