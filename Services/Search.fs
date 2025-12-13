module StoreSimulatorGUI.Search

open StoreSimulatorGUI.Models

// Filter by category
let filterByCategory (products: Product list) (category: string) : Product list =
    products 
    |> List.filter (fun p -> p.Category.ToLower() = category.ToLower())

// Filter by price range
let filterByPriceRange (products: Product list) (minPrice: decimal) (maxPrice: decimal) : Product list =
    products 
    |> List.filter (fun p -> p.Price >= minPrice && p.Price <= maxPrice)

// Search by name (case-insensitive, partial match)
let searchByName (products: Product list) (searchTerm: string) : Product list =
    products 
    |> List.filter (fun p -> p.Name.ToLower().Contains(searchTerm.ToLower()))

// Search by description
let searchByDescription (products: Product list) (searchTerm: string) : Product list =
    products 
    |> List.filter (fun p -> p.Description.ToLower().Contains(searchTerm.ToLower()))

// Search in name or description
let searchByNameOrDescription (products: Product list) (searchTerm: string) : Product list =
    products 
    |> List.filter (fun p -> 
        p.Name.ToLower().Contains(searchTerm.ToLower()) ||
        p.Description.ToLower().Contains(searchTerm.ToLower()))

// Filter by availability (in stock)
let filterInStock (products: Product list) : Product list =
    products 
    |> List.filter (fun p -> p.Stock > 0)

// Filter by low stock (less than threshold)
let filterLowStock (products: Product list) (threshold: int) : Product list =
    products 
    |> List.filter (fun p -> p.Stock > 0 && p.Stock < threshold)

// Sort products
type SortBy = 
    | PriceAsc 
    | PriceDesc 
    | NameAsc 
    | NameDesc
    | StockAsc
    | StockDesc

let sortProducts (products: Product list) (sortBy: SortBy) : Product list =
    match sortBy with
    | PriceAsc -> products |> List.sortBy (fun p -> p.Price)
    | PriceDesc -> products |> List.sortByDescending (fun p -> p.Price)
    | NameAsc -> products |> List.sortBy (fun p -> p.Name)
    | NameDesc -> products |> List.sortByDescending (fun p -> p.Name)
    | StockAsc -> products |> List.sortBy (fun p -> p.Stock)
    | StockDesc -> products |> List.sortByDescending (fun p -> p.Stock)

// Sort by price (simple version for compatibility with tests)
let sortByPrice (products: Product list) (ascending: bool) : Product list =
    if ascending then
        sortProducts products PriceAsc
    else
        sortProducts products PriceDesc

// Sort by name (simple version for compatibility with tests)
let sortByName (products: Product list) (ascending: bool) : Product list =
    if ascending then
        sortProducts products NameAsc
    else
        sortProducts products NameDesc

// Get unique categories
let getCategories (products: Product list) : string list =
    products 
    |> List.map (fun p -> p.Category)
    |> List.distinct
    |> List.sort

// Get products by multiple categories
let filterByCategories (products: Product list) (categories: string list) : Product list =
    let categoriesLower = categories |> List.map (fun c -> c.ToLower())
    products 
    |> List.filter (fun p -> categoriesLower |> List.contains (p.Category.ToLower()))

// Complex filter combining multiple criteria
let filterProducts (products: Product list) (criteria: FilterCriteria) : Product list =
    let mutable filtered = products
    
    // Apply category filter
    filtered <- 
        match criteria.Category with
        | Some cat -> filterByCategory filtered cat
        | None -> filtered
    
    // Apply price range filter
    filtered <- 
        match criteria.MinPrice, criteria.MaxPrice with
        | Some min, Some max -> filterByPriceRange filtered min max
        | Some min, None -> filtered |> List.filter (fun p -> p.Price >= min)
        | None, Some max -> filtered |> List.filter (fun p -> p.Price <= max)
        | None, None -> filtered
    
    // Apply search term
    filtered <- 
        match criteria.SearchTerm with
        | Some term -> searchByNameOrDescription filtered term
        | None -> filtered
    
    // Apply stock filter
    if criteria.InStockOnly then
        filterInStock filtered
    else
        filtered

// Search products with optional filters (for ViewModel compatibility)
let searchProducts (products: Product list) (searchTerm: string option) (category: string option) : Product list =
    let mutable result = products
    
    match searchTerm with
    | Some term when not (System.String.IsNullOrWhiteSpace(term)) ->
        result <- searchByName result term
    | _ -> ()
    
    match category with
    | Some cat when not (System.String.IsNullOrWhiteSpace(cat)) && cat <> "All" ->
        result <- filterByCategory result cat
    | _ -> ()
    
    result

// Get price statistics
let getPriceStatistics (products: Product list) : decimal * decimal * decimal =
    if products.IsEmpty then
        (0m, 0m, 0m)
    else
        let prices = products |> List.map (fun p -> p.Price)
        let minPrice = prices |> List.min
        let maxPrice = prices |> List.max
        let avgPrice = prices |> List.average
        (minPrice, maxPrice, avgPrice)

// Get total inventory value
let getTotalInventoryValue (products: Product list) : decimal =
    products 
    |> List.sumBy (fun p -> p.Price * decimal p.Stock)

// Find most expensive product
let findMostExpensive (products: Product list) : Product option =
    if products.IsEmpty then
        None
    else
        products |> List.maxBy (fun p -> p.Price) |> Some

// Find cheapest product
let findCheapest (products: Product list) : Product option =
    if products.IsEmpty then
        None
    else
        products |> List.minBy (fun p -> p.Price) |> Some