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

// Get unique categories
let getCategories (products: Product list) : string list =
    products 
    |> List.map (fun p -> p.Category)
    |> List.distinct
    |> List.sort

