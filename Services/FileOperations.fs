module StoreSimulatorGUI.FileOperations

open System
open System.IO
open Newtonsoft.Json
open StoreSimulatorGUI.Models


let private jsonSettings = 
    JsonSerializerSettings(
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    )


let private ordersDirectory = "Orders"
let private catalogDirectory = "Catalog"


let private ensureDirectoryExists (directory: string) =
    if not (Directory.Exists(directory)) then
        Directory.CreateDirectory(directory) |> ignore


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


let createOrderSummary (cart: Cart) : OrderSummary =
    {
        OrderId = Guid.NewGuid().ToString()
        OrderDate = DateTime.Now
        Items = cart.Items
        Subtotal = cart.TotalBeforeDiscount
        Discount = cart.Discount
        Total = cart.FinalTotal
        CouponUsed = cart.AppliedCoupon  
    }


let generateOrderFileName () : string =
    let timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
    sprintf "order_%s.json" timestamp


let saveOrder (cart: Cart) : Result<string, string> =
    try
        ensureDirectoryExists ordersDirectory
        
        let summary = createOrderSummary cart
        let fileName = generateOrderFileName()
        let fullPath = Path.Combine(ordersDirectory, fileName)
        
        saveOrderToJson summary fullPath
    with
    | ex -> Error (sprintf "Failed to save order: %s" ex.Message)



let listSavedOrders () : string list =
    ensureDirectoryExists ordersDirectory
    if Directory.Exists(ordersDirectory) then
        Directory.GetFiles(ordersDirectory, "order_*.json")
        |> Array.toList
        |> List.map Path.GetFileName
        |> List.sort
        |> List.rev 
    else
        []


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



type OrderDisplayInfo = {
    FileName: string
    OrderId: string
    OrderDate: DateTime
    ItemCount: int
    Total: decimal
    CouponUsed: string option
    Items: CartItem list
}

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


let loadAllOrdersAsDisplayInfo () : OrderDisplayInfo list =
    listSavedOrders()
    |> List.choose (fun fileName ->
        match loadOrderAsDisplayInfo fileName with
        | Ok info -> Some info
        | Error _ -> None)
    |> List.sortByDescending (fun o -> o.OrderDate)


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
        (order.OrderId.Substring(0, 8) + "...")  
        (order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"))
        itemsText
        order.Total
        couponText


let formatOrderSummary (order: OrderDisplayInfo) : string =
    let couponIndicator = if order.CouponUsed.IsSome then " 🎟️" else ""
    sprintf "%s | %d items | $%.2f%s" 
        (order.OrderDate.ToString("yyyy-MM-dd HH:mm"))
        order.ItemCount
        order.Total
        couponIndicator
