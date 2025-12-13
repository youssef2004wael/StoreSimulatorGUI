module StoreSimulatorGUI.Catalog

open StoreSimulatorGUI.Models

// Initialize product catalog as immutable Map
let initializeCatalog () : Map<int, Product> =
    [
        { Id = 1; Name = "Laptop"; Description = "High-performance laptop with 16GB RAM"; Price = 999.99m; Category = "Electronics"; Stock = 10 }
        { Id = 2; Name = "Mouse"; Description = "Wireless ergonomic mouse"; Price = 29.99m; Category = "Electronics"; Stock = 50 }
        { Id = 3; Name = "Keyboard"; Description = "Mechanical RGB keyboard"; Price = 79.99m; Category = "Electronics"; Stock = 30 }
        { Id = 4; Name = "Monitor"; Description = "27-inch 4K IPS monitor"; Price = 399.99m; Category = "Electronics"; Stock = 15 }
        { Id = 5; Name = "Headphones"; Description = "Noise-canceling wireless headphones"; Price = 199.99m; Category = "Electronics"; Stock = 25 }
        { Id = 6; Name = "Desk Chair"; Description = "Ergonomic office chair with lumbar support"; Price = 299.99m; Category = "Furniture"; Stock = 20 }
        { Id = 7; Name = "Desk Lamp"; Description = "LED desk lamp with adjustable brightness"; Price = 49.99m; Category = "Furniture"; Stock = 40 }
        { Id = 8; Name = "USB Cable"; Description = "USB-C to USB-C cable 2 meters"; Price = 14.99m; Category = "Accessories"; Stock = 100 }
        { Id = 9; Name = "Webcam"; Description = "1080p HD webcam with microphone"; Price = 89.99m; Category = "Electronics"; Stock = 35 }
        { Id = 10; Name = "Notebook"; Description = "Spiral notebook pack of 5"; Price = 9.99m; Category = "Stationery"; Stock = 60 }
        { Id = 11; Name = "Pen Set"; Description = "Professional pen set with case"; Price = 24.99m; Category = "Stationery"; Stock = 45 }
        { Id = 12; Name = "Backpack"; Description = "Laptop backpack with USB port"; Price = 59.99m; Category = "Accessories"; Stock = 30 }
        { Id = 13; Name = "Phone Stand"; Description = "Adjustable phone/tablet stand"; Price = 19.99m; Category = "Accessories"; Stock = 55 }
        { Id = 14; Name = "Coffee Mug"; Description = "Insulated coffee mug 16oz"; Price = 15.99m; Category = "Kitchen"; Stock = 70 }
        { Id = 15; Name = "Water Bottle"; Description = "Stainless steel water bottle 32oz"; Price = 22.99m; Category = "Kitchen"; Stock = 50 }
    ]
    |> List.map (fun p -> (p.Id, p))
    |> Map.ofList

// Get product by ID
let getProduct (catalog: Map<int, Product>) (productId: int) : Product option =
    catalog.TryFind productId

// Get all products as list
let getAllProducts (catalog: Map<int, Product>) : Product list =
    catalog |> Map.toList |> List.map snd

// Add new product to catalog (returns new catalog)
let addProduct (catalog: Map<int, Product>) (product: Product) : Map<int, Product> =
    catalog.Add(product.Id, product)

// Update product stock (immutable)
let updateStock (catalog: Map<int, Product>) (productId: int) (newStock: int) : Map<int, Product> =
    match catalog.TryFind productId with
    | Some product -> 
        let updatedProduct = { product with Stock = newStock }
        catalog.Add(productId, updatedProduct)
    | None -> catalog

// Decrease stock for multiple items (for checkout) - NEW FUNCTION
let decreaseStockBatch (catalog: Map<int, Product>) (items: CartItem list) : Map<int, Product> =
    items
    |> List.fold (fun currentCatalog item ->
        match currentCatalog.TryFind item.Product.Id with
        | Some product ->
            let newStock = max 0 (product.Stock - item.Quantity)
            let updatedProduct = { product with Stock = newStock }
            currentCatalog.Add(product.Id, updatedProduct)
        | None -> currentCatalog
    ) catalog

// Update product price
let updatePrice (catalog: Map<int, Product>) (productId: int) (newPrice: decimal) : Map<int, Product> =
    match catalog.TryFind productId with
    | Some product -> 
        let updatedProduct = { product with Price = newPrice }
        catalog.Add(productId, updatedProduct)
    | None -> catalog

// Remove product from catalog
let removeProduct (catalog: Map<int, Product>) (productId: int) : Map<int, Product> =
    catalog.Remove(productId)

// Display product details
let displayProduct (product: Product) : string =
    sprintf "[%d] %s - $%.2f\n    %s\n    Category: %s | Stock: %d" 
        product.Id product.Name product.Price product.Description product.Category product.Stock

// Display all products
let displayAllProducts (catalog: Map<int, Product>) : string =
    let products = getAllProducts catalog
    products
    |> List.map displayProduct
    |> String.concat "\n\n"

// Get product count
let getProductCount (catalog: Map<int, Product>) : int =
    catalog.Count

// Check if product exists
let productExists (catalog: Map<int, Product>) (productId: int) : bool =
    catalog.ContainsKey productId

// Check if product has sufficient stock
let hasStock (catalog: Map<int, Product>) (productId: int) (quantity: int) : bool =
    match catalog.TryFind productId with
    | Some product -> product.Stock >= quantity
    | None -> false

// Get low stock products (less than threshold)
let getLowStockProducts (catalog: Map<int, Product>) (threshold: int) : Product list =
    getAllProducts catalog
    |> List.filter (fun p -> p.Stock > 0 && p.Stock < threshold)

// Get out of stock products
let getOutOfStockProducts (catalog: Map<int, Product>) : Product list =
    getAllProducts catalog
    |> List.filter (fun p -> p.Stock = 0)
