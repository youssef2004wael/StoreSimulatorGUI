module StoreSimulatorGUI.FileOperations

open System
open System.IO
open Newtonsoft.Json
open StoreSimulatorGUI.Models

// JSON serialization settings
let private jsonSettings = 
    JsonSerializerSettings(
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    )

// Default directories
let private ordersDirectory = "Orders"
let private catalogDirectory = "Catalog"

// Ensure directory exists
let private ensureDirectoryExists (directory: string) =
    if not (Directory.Exists(directory)) then
        Directory.CreateDirectory(directory) |> ignore

// Save order summary to JSON
let saveOrderToJson (summary: OrderSummary) (filePath: string) : Result<string, string> =
    try
        let directory = Path.GetDirectoryName(filePath)
        if not (String.IsNullOrEmpty(directory)) then
            ensureDirectoryExists directory
        
        let json = JsonConvert.SerializeObject(summary, jsonSettings)
        File.WriteAllText(filePath, json)
        Ok filePath
    with
    | ex -> Error (sprintf "Failed to save order: %s" ex.Message)

// Load order from JSON
let loadOrderFromJson (filePath: string) : Result<OrderSummary, string> =
    try
        if File.Exists(filePath) then
            let json = File.ReadAllText(filePath)
            let summary = JsonConvert.DeserializeObject<OrderSummary>(json)
            Ok summary
        else
            Error "File not found"
    with
    | ex -> Error (sprintf "Failed to load order: %s" ex.Message)

// Save catalog to JSON
let saveCatalogToJson (catalog: Map<int, Product>) (filePath: string) : Result<string, string> =
    try
        let directory = Path.GetDirectoryName(filePath)
        if not (String.IsNullOrEmpty(directory)) then
            ensureDirectoryExists directory
        
        let products = catalog |> Map.toList |> List.map snd
        let json = JsonConvert.SerializeObject(products, jsonSettings)
        File.WriteAllText(filePath, json)
        Ok filePath
    with
    | ex -> Error (sprintf "Failed to save catalog: %s" ex.Message)

// Load catalog from JSON
let loadCatalogFromJson (filePath: string) : Result<Map<int, Product>, string> =
    try
        if File.Exists(filePath) then
            let json = File.ReadAllText(filePath)
            let products = JsonConvert.DeserializeObject<Product list>(json)
            let catalog = products |> List.map (fun p -> (p.Id, p)) |> Map.ofList
            Ok catalog
        else
            Error "File not found"
    with
    | ex -> Error (sprintf "Failed to load catalog: %s" ex.Message)

// Create order summary from cart
let createOrderSummary (cart: Cart) : OrderSummary =
    {
        OrderId = Guid.NewGuid().ToString()
        OrderDate = DateTime.Now
        Items = cart.Items
        Subtotal = cart.TotalBeforeDiscount
        Discount = cart.Discount
        Total = cart.FinalTotal
        CouponUsed = cart.AppliedCoupon  // ← ADD THIS LINE
    }

// Generate filename with timestamp
let generateOrderFileName () : string =
    let timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
    sprintf "order_%s.json" timestamp

// Save order with auto-generated filename
let saveOrder (cart: Cart) : Result<string, string> =
    try
        ensureDirectoryExists ordersDirectory
        
        let summary = createOrderSummary cart
        let fileName = generateOrderFileName()
        let fullPath = Path.Combine(ordersDirectory, fileName)
        
        saveOrderToJson summary fullPath
    with
    | ex -> Error (sprintf "Failed to save order: %s" ex.Message)

// Save order to specific directory
let saveOrderToDirectory (cart: Cart) (directory: string) : Result<string, string> =
    try
        ensureDirectoryExists directory
        
        let summary = createOrderSummary cart
        let fileName = generateOrderFileName()
        let fullPath = Path.Combine(directory, fileName)
        
        saveOrderToJson summary fullPath
    with
    | ex -> Error (sprintf "Failed to save order: %s" ex.Message)

// List all saved orders
let listSavedOrders () : string list =
    ensureDirectoryExists ordersDirectory
    if Directory.Exists(ordersDirectory) then
        Directory.GetFiles(ordersDirectory, "order_*.json")
        |> Array.toList
        |> List.map Path.GetFileName
        |> List.sort
        |> List.rev // Most recent first
    else
        []

// List orders in specific directory
let listOrdersInDirectory (directory: string) : string list =
    if Directory.Exists(directory) then
        Directory.GetFiles(directory, "order_*.json")
        |> Array.toList
        |> List.map Path.GetFileName
        |> List.sort
        |> List.rev
    else
        []

// Get order count
let getOrderCount () : int =
    listSavedOrders().Length

// Delete order file
let deleteOrder (fileName: string) : Result<unit, string> =
    try
        let fullPath = Path.Combine(ordersDirectory, fileName)
        if File.Exists(fullPath) then
            File.Delete(fullPath)
            Ok ()
        else
            Error "Order file not found"
    with
    | ex -> Error (sprintf "Failed to delete order: %s" ex.Message)

// Export cart to readable text format
let exportCartToText (cart: Cart) (filePath: string) : Result<string, string> =
    try
        let directory = Path.GetDirectoryName(filePath)
        if not (String.IsNullOrEmpty(directory)) then
            ensureDirectoryExists directory
        
        let lines = [
            "========================================="
            "         STORE CART SUMMARY"
            "========================================="
            ""
            "Items:"
            yield! cart.Items |> List.map (fun item ->
                sprintf "  • %s (x%d) - $%.2f each = $%.2f" 
                    item.Product.Name 
                    item.Quantity 
                    item.Product.Price 
                    (item.Product.Price * decimal item.Quantity))
            ""
            "========================================="
            sprintf "Subtotal:        $%.2f" cart.TotalBeforeDiscount
            sprintf "Discount:       -$%.2f" cart.Discount
            "----------------------------------------"
            sprintf "TOTAL:           $%.2f" cart.FinalTotal
            "========================================="
            sprintf "Date: %s" (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
        ]
        
        let content = String.concat "\n" lines
        File.WriteAllText(filePath, content)
        Ok filePath
    with
    | ex -> Error (sprintf "Failed to export cart: %s" ex.Message)

// Export catalog to CSV
let exportCatalogToCsv (catalog: Map<int, Product>) (filePath: string) : Result<string, string> =
    try
        let directory = Path.GetDirectoryName(filePath)
        if not (String.IsNullOrEmpty(directory)) then
            ensureDirectoryExists directory
        
        let products = catalog |> Map.toList |> List.map snd
        let header = "ID,Name,Description,Price,Category,Stock"
        let rows = 
            products 
            |> List.map (fun p -> 
                sprintf "%d,\"%s\",\"%s\",%.2f,%s,%d" 
                    p.Id p.Name p.Description p.Price p.Category p.Stock)
        
        let content = header :: rows |> String.concat "\n"
        File.WriteAllText(filePath, content)
        Ok filePath
    with
    | ex -> Error (sprintf "Failed to export catalog: %s" ex.Message)

// Save default catalog
let saveDefaultCatalog (catalog: Map<int, Product>) : Result<string, string> =
    try
        ensureDirectoryExists catalogDirectory
        let filePath = Path.Combine(catalogDirectory, "catalog.json")
        saveCatalogToJson catalog filePath
    with
    | ex -> Error (sprintf "Failed to save catalog: %s" ex.Message)

// Load default catalog
let loadDefaultCatalog () : Result<Map<int, Product>, string> =
    let filePath = Path.Combine(catalogDirectory, "catalog.json")
    loadCatalogFromJson filePath

// Display order summary from file
let displayOrderFromFile (fileName: string) : Result<string, string> =
    let fullPath = Path.Combine(ordersDirectory, fileName)
    match loadOrderFromJson fullPath with
    | Ok summary ->
        let lines = [
            "========================================="
            sprintf "Order ID: %s" summary.OrderId
            sprintf "Date: %s" (summary.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"))
            "========================================="
            ""
            "Items:"
            yield! summary.Items |> List.map (fun item ->
                sprintf "  • %s x%d @ $%.2f = $%.2f" 
                    item.Product.Name 
                    item.Quantity 
                    item.Product.Price 
                    (item.Product.Price * decimal item.Quantity))
            ""
            "========================================="
            sprintf "Subtotal:  $%.2f" summary.Subtotal
            sprintf "Discount: -$%.2f" summary.Discount
            "----------------------------------------"
            sprintf "TOTAL:     $%.2f" summary.Total
            "========================================="
        ]
        Ok (String.concat "\n" lines)
    | Error msg -> Error msg

// Get total sales from all orders
let getTotalSales () : decimal =
    listSavedOrders()
    |> List.choose (fun fileName ->
        let fullPath = Path.Combine(ordersDirectory, fileName)
        match loadOrderFromJson fullPath with
        | Ok summary -> Some summary.Total
        | Error _ -> None)
    |> List.sum

// Backup orders to directory
let backupOrders (backupDirectory: string) : Result<int, string> =
    try
        ensureDirectoryExists backupDirectory
        let orders = listSavedOrders()
        
        orders |> List.iter (fun fileName ->
            let sourcePath = Path.Combine(ordersDirectory, fileName)
            let destPath = Path.Combine(backupDirectory, fileName)
            File.Copy(sourcePath, destPath, true))
        
        Ok orders.Length
    with
    | ex -> Error (sprintf "Failed to backup orders: %s" ex.Message)
// ═══════════════════════════════════════════════════════════════
// NEW: LOAD ORDERS FEATURE
// ═══════════════════════════════════════════════════════════════

// Type to represent an order for display purposes
type OrderDisplayInfo = {
    FileName: string
    OrderId: string
    OrderDate: DateTime
    ItemCount: int
    Total: decimal
    CouponUsed: string option
    Items: CartItem list
}

// Load a single order and convert to display format
let loadOrderAsDisplayInfo (fileName: string) : Result<OrderDisplayInfo, string> =
    let fullPath = Path.Combine(ordersDirectory, fileName)
    match loadOrderFromJson fullPath with
    | Ok summary ->
        Ok {
            FileName = fileName
            OrderId = summary.OrderId
            OrderDate = summary.OrderDate
            ItemCount = summary.Items |> List.sumBy (fun i -> i.Quantity)
            Total = summary.Total
            CouponUsed = summary.CouponUsed
            Items = summary.Items
        }
    | Error msg -> Error msg

// Load all orders as display info (sorted by date, newest first)
let loadAllOrdersAsDisplayInfo () : OrderDisplayInfo list =
    listSavedOrders()
    |> List.choose (fun fileName ->
        match loadOrderAsDisplayInfo fileName with
        | Ok info -> Some info
        | Error _ -> None)
    |> List.sortByDescending (fun o -> o.OrderDate)

// Format order for detailed display
let formatOrderDetails (order: OrderDisplayInfo) : string =
    let itemsText = 
        order.Items
        |> List.map (fun item ->
            sprintf "  • %s x%d @ $%.2f = $%.2f" 
                item.Product.Name 
                item.Quantity 
                item.Product.Price 
                (item.Product.Price * decimal item.Quantity))
        |> String.concat "\n"
    
    let couponText = 
        match order.CouponUsed with
        | Some code -> sprintf "\nCoupon Used: %s" code
        | None -> ""
    
    sprintf """═══════════════════════════════════════
Order ID: %s
Date: %s
═══════════════════════════════════════

Items:
%s

───────────────────────────────────────
Total: $%.2f%s
═══════════════════════════════════════"""
        (order.OrderId.Substring(0, 8) + "...")  // Shortened ID
        (order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"))
        itemsText
        order.Total
        couponText

// Get order summary for list display
let formatOrderSummary (order: OrderDisplayInfo) : string =
    let couponIndicator = if order.CouponUsed.IsSome then " 🎟️" else ""
    sprintf "%s | %d items | $%.2f%s" 
        (order.OrderDate.ToString("yyyy-MM-dd HH:mm"))
        order.ItemCount
        order.Total
        couponIndicator

// Get total number of orders
let getOrderStatistics () : int * decimal =
    let orders = loadAllOrdersAsDisplayInfo()
    let count = orders.Length
    let totalSpent = orders |> List.sumBy (fun o -> o.Total)
    (count, totalSpent)