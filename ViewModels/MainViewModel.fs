namespace StoreSimulatorGUI.ViewModels

open System
open System.Collections.ObjectModel
open System.ComponentModel
open System.Windows.Input
open StoreSimulatorGUI.Models
open StoreSimulatorGUI.Catalog
open StoreSimulatorGUI.Cart
open StoreSimulatorGUI.Search
open StoreSimulatorGUI.PriceCalculator
open StoreSimulatorGUI.FileOperations

// ═══════════════════════════════════════════════════════════════════════════════
// RELAY COMMAND - Implementation of ICommand for MVVM pattern
// ═══════════════════════════════════════════════════════════════════════════════
type RelayCommand(execute: obj -> unit, canExecute: obj -> bool) =
    let canExecuteChanged = Event<EventHandler, EventArgs>()
    
    interface ICommand with
        member _.Execute(parameter) = execute parameter
        member _.CanExecute(parameter) = canExecute parameter
        [<CLIEvent>]
        member _.CanExecuteChanged = canExecuteChanged.Publish
    
    member _.RaiseCanExecuteChanged() = 
        canExecuteChanged.Trigger(null, EventArgs.Empty)

// ═══════════════════════════════════════════════════════════════════════════════
// PRODUCT VIEW MODEL - Wrapper for Product display in UI
// ═══════════════════════════════════════════════════════════════════════════════
[<AllowNullLiteral>]
type ProductViewModel(product: Product) =
    member _.Id = product.Id
    member _.Name = product.Name
    member _.Description = product.Description
    member _.Price = product.Price
    member _.PriceFormatted = sprintf "$%.2f" product.Price
    member _.Category = product.Category
    member _.Stock = product.Stock
    member _.StockInfo = sprintf "Stock: %d" product.Stock
    member _.Product = product
    member _.IsInStock = product.Stock > 0
    member _.StockColor = 
        if product.Stock > 10 then "Green" 
        elif product.Stock > 0 then "Orange" 
        else "Red"

// ═══════════════════════════════════════════════════════════════════════════════
// CART ITEM VIEW MODEL - Wrapper for CartItem display in UI
// ═══════════════════════════════════════════════════════════════════════════════
type CartItemViewModel(item: CartItem, removeCommand: ICommand) =
    member _.ProductName = item.Product.Name
    member _.Price = item.Product.Price
    member _.Quantity = item.Quantity
    member _.Subtotal = item.Product.Price * decimal item.Quantity
    member _.SubtotalFormatted = sprintf "$%.2f" (item.Product.Price * decimal item.Quantity)
    member _.ProductId = item.Product.Id
    member _.RemoveCommand = removeCommand

// ═══════════════════════════════════════════════════════════════════════════════
// ORDER VIEW MODEL - Wrapper for Order display in UI (NEW)
// ═══════════════════════════════════════════════════════════════════════════════
[<AllowNullLiteral>]
type OrderViewModel(order: OrderDisplayInfo) =
    member _.FileName = order.FileName
    member _.OrderId = order.OrderId
    member _.ShortOrderId = 
        if order.OrderId.Length > 8 then 
            order.OrderId.Substring(0, 8) + "..." 
        else 
            order.OrderId
    member _.OrderDate = order.OrderDate
    member _.DateFormatted = order.OrderDate.ToString("yyyy-MM-dd HH:mm")
    member _.ItemCount = order.ItemCount
    member _.ItemCountText = sprintf "%d items" order.ItemCount
    member _.Total = order.Total
    member _.TotalFormatted = sprintf "$%.2f" order.Total
    member _.HasCoupon = order.CouponUsed.IsSome
    member _.CouponText = 
        match order.CouponUsed with
        | Some code -> sprintf "🎟️ %s" code
        | None -> ""
    member _.Items = order.Items
    member _.Order = order
    member _.Summary = formatOrderSummary order
    member _.Details = formatOrderDetails order

// ═══════════════════════════════════════════════════════════════════════════════
// MAIN VIEW MODEL - The main ViewModel for the application
// ═══════════════════════════════════════════════════════════════════════════════
type MainViewModel() as this =
    
    // ───────────────────────────────────────────────────────────────────────────
    // STATE - Mutable state for the application
    // ───────────────────────────────────────────────────────────────────────────
    let mutable catalog = initializeCatalog()
    let mutable cart = emptyCart
    let mutable selectedProduct: ProductViewModel = null
    let mutable quantityToAdd = 1
    let mutable statusMessage = "Welcome to F# Store Simulator!"
    let mutable couponCode = ""
    let mutable searchText = ""
    let mutable selectedCategory = "All"
    
    // NEW: Order history state
    let mutable isOrderHistoryVisible = false
    let mutable selectedOrder: OrderViewModel = null
    let mutable orderDetails = ""
    
    // Observable collections for UI binding
    let products = ObservableCollection<ProductViewModel>()
    let cartItems = ObservableCollection<CartItemViewModel>()
    let categories = ObservableCollection<string>()
    let orders = ObservableCollection<OrderViewModel>()  // NEW
    
    // Property changed event for INotifyPropertyChanged
    let propertyChanged = Event<PropertyChangedEventHandler, PropertyChangedEventArgs>()
    
    // ───────────────────────────────────────────────────────────────────────────
    // COMMANDS - Mutable command references
    // ───────────────────────────────────────────────────────────────────────────
    let mutable addToCartCommand: ICommand = null
    let mutable clearCartCommand: ICommand = null
    let mutable applyCouponCommand: ICommand = null
    let mutable removeCouponCommand: ICommand = null
    let mutable checkoutCommand: ICommand = null
    let mutable removeFromCartCommand: ICommand = null
    
    // NEW: Order history commands
    let mutable loadOrdersCommand: ICommand = null
    let mutable closeOrderHistoryCommand: ICommand = null
    let mutable viewOrderDetailsCommand: ICommand = null
    let mutable deleteOrderCommand: ICommand = null
    
    // ───────────────────────────────────────────────────────────────────────────
    // INITIALIZATION
    // ───────────────────────────────────────────────────────────────────────────
    do
        try
            Console.WriteLine("Initializing MainViewModel...")
            
            // Load categories
            categories.Add("All")
            Console.WriteLine("Loading catalog...")
            let allProducts = getAllProducts catalog
            Console.WriteLine($"Found {List.length allProducts} products")
            
            let cats = getCategories allProducts
            cats |> List.iter categories.Add
            Console.WriteLine($"Loaded {categories.Count} categories")
            
            // Load products
            allProducts |> List.iter (fun p -> products.Add(ProductViewModel(p)))
            Console.WriteLine($"Added {products.Count} products to observable collection")
            
            // Initialize cart commands
            Console.WriteLine("Initializing commands...")
            addToCartCommand <- RelayCommand(
                (fun _ -> this.AddToCartExecute()), 
                (fun _ -> this.CanAddToCart))
            
            clearCartCommand <- RelayCommand(
                (fun _ -> this.ClearCartExecute()), 
                (fun _ -> this.HasItems))
            
            applyCouponCommand <- RelayCommand(
                (fun _ -> this.ApplyCouponExecute()), 
                (fun _ -> true))
            
            removeCouponCommand <- RelayCommand(
                (fun _ -> this.RemoveCouponExecute()), 
                (fun _ -> this.HasCoupon))
            
            checkoutCommand <- RelayCommand(
                (fun _ -> this.CheckoutExecute()), 
                (fun _ -> this.HasItems))
            
            removeFromCartCommand <- RelayCommand(
                (fun param -> 
                    match param with
                    | :? int as id -> this.RemoveFromCart(id)
                    | _ -> ()), 
                (fun _ -> true))
            
            // NEW: Initialize order history commands
            loadOrdersCommand <- RelayCommand(
                (fun _ -> this.LoadOrdersExecute()), 
                (fun _ -> true))
            
            closeOrderHistoryCommand <- RelayCommand(
                (fun _ -> this.CloseOrderHistoryExecute()), 
                (fun _ -> true))
            
            viewOrderDetailsCommand <- RelayCommand(
                (fun param -> 
                    match param with
                    | :? OrderViewModel as order -> this.ViewOrderDetailsExecute(order)
                    | _ -> ()), 
                (fun _ -> true))
            
            deleteOrderCommand <- RelayCommand(
                (fun param ->
                    match param with
                    | :? string as fileName -> this.DeleteOrderExecute(fileName)
                    | _ -> ()),
                (fun _ -> true))
            
            Console.WriteLine("MainViewModel initialized successfully")
        with
        | ex ->
            Console.WriteLine($"ERROR initializing MainViewModel: {ex.Message}")
            Console.WriteLine($"Stack: {ex.StackTrace}")
            reraise()
    
    // ───────────────────────────────────────────────────────────────────────────
    // INOTIFYPROPERTYCHANGED IMPLEMENTATION
    // ───────────────────────────────────────────────────────────────────────────
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member _.PropertyChanged = propertyChanged.Publish
    
    member private _.RaisePropertyChanged(propertyName: string) =
        propertyChanged.Trigger(this, PropertyChangedEventArgs(propertyName))
    
    // ───────────────────────────────────────────────────────────────────────────
    // COLLECTION PROPERTIES
    // ───────────────────────────────────────────────────────────────────────────
    member _.Products = products
    member _.CartItems = cartItems
    member _.Categories = categories
    member _.Orders = orders  // NEW
    
    // ───────────────────────────────────────────────────────────────────────────
    // PRODUCT PROPERTIES
    // ───────────────────────────────────────────────────────────────────────────
    member _.SelectedProduct
        with get() = selectedProduct
        and set(value) = 
            selectedProduct <- value
            Console.WriteLine $"""=== SelectedProduct changed to: {if obj.ReferenceEquals(value, null) then "None" else value.Name} ==="""
            this.RaisePropertyChanged("SelectedProduct")
            this.RaisePropertyChanged("CanAddToCart")
            (addToCartCommand :?> RelayCommand).RaiseCanExecuteChanged()
    
    member _.QuantityToAdd
        with get() = quantityToAdd
        and set(value) = 
            quantityToAdd <- max 1 value
            this.RaisePropertyChanged("QuantityToAdd")
    
    member _.SearchText
        with get() = searchText
        and set(value) = 
            searchText <- value
            this.RaisePropertyChanged("SearchText")
            this.FilterProducts()
    
    member _.SelectedCategory
        with get() = selectedCategory
        and set(value) = 
            selectedCategory <- value
            this.RaisePropertyChanged("SelectedCategory")
            this.FilterProducts()
    
    // ───────────────────────────────────────────────────────────────────────────
    // CART PROPERTIES
    // ───────────────────────────────────────────────────────────────────────────
    member _.CouponCode
        with get() = couponCode
        and set(value) = 
            couponCode <- value
            this.RaisePropertyChanged("CouponCode")
    
    member _.CartSubtotal = sprintf "$%.2f" cart.TotalBeforeDiscount
    member _.CartDiscount = sprintf "-$%.2f" cart.Discount
    member _.CartTotal = sprintf "$%.2f" cart.FinalTotal
    member _.CartItemCount = sprintf "%d items" (getItemCount cart)
    member _.HasItems = not (isEmpty cart)
    member _.HasCoupon = cart.AppliedCoupon.IsSome
    
    member _.AppliedCouponText = 
        match cart.AppliedCoupon with
        | Some code -> sprintf "Applied: %s" code
        | None -> "No coupon"
    
    member _.CanAddToCart = not (obj.ReferenceEquals(selectedProduct, null))
    
    // ───────────────────────────────────────────────────────────────────────────
    // STATUS MESSAGE PROPERTY
    // ───────────────────────────────────────────────────────────────────────────
    member _.StatusMessage
        with get() = statusMessage
        and set(value) = 
            statusMessage <- value
            this.RaisePropertyChanged("StatusMessage")
    
    // ───────────────────────────────────────────────────────────────────────────
    // ORDER HISTORY PROPERTIES (NEW)
    // ───────────────────────────────────────────────────────────────────────────
    member _.IsOrderHistoryVisible
        with get() = isOrderHistoryVisible
        and set(value) = 
            isOrderHistoryVisible <- value
            this.RaisePropertyChanged("IsOrderHistoryVisible")
            this.RaisePropertyChanged("IsOrderHistoryHidden")
    
    member _.IsOrderHistoryHidden = not isOrderHistoryVisible
    
    member _.SelectedOrder
        with get() = selectedOrder
        and set(value) = 
            selectedOrder <- value
            this.RaisePropertyChanged("SelectedOrder")
            this.RaisePropertyChanged("HasSelectedOrder")
            if not (obj.ReferenceEquals(value, null)) then
                orderDetails <- value.Details
                this.RaisePropertyChanged("OrderDetails")
    
    member _.HasSelectedOrder = not (obj.ReferenceEquals(selectedOrder, null))
    
    member _.OrderDetails
        with get() = orderDetails
        and set(value) =
            orderDetails <- value
            this.RaisePropertyChanged("OrderDetails")
    
    member _.OrderCount = orders.Count
    
    member _.OrderCountText = 
        let count = orders.Count
        if count = 0 then "No orders yet"
        elif count = 1 then "1 order"
        else sprintf "%d orders" count
    
    member _.TotalSpentText =
        let total = orders |> Seq.sumBy (fun o -> o.Total)
        sprintf "Total Spent: $%.2f" total
    
    // ───────────────────────────────────────────────────────────────────────────
    // COMMAND PROPERTIES
    // ───────────────────────────────────────────────────────────────────────────
    member _.AddToCartCommand = addToCartCommand
    member _.ClearCartCommand = clearCartCommand
    member _.ApplyCouponCommand = applyCouponCommand
    member _.RemoveCouponCommand = removeCouponCommand
    member _.CheckoutCommand = checkoutCommand
    member _.RemoveFromCartCommand = removeFromCartCommand
    
    // NEW: Order history command properties
    member _.LoadOrdersCommand = loadOrdersCommand
    member _.CloseOrderHistoryCommand = closeOrderHistoryCommand
    member _.ViewOrderDetailsCommand = viewOrderDetailsCommand
    member _.DeleteOrderCommand = deleteOrderCommand
    
    // ───────────────────────────────────────────────────────────────────────────
    // PRIVATE HELPER METHODS
    // ───────────────────────────────────────────────────────────────────────────
    member private this.FilterProducts() =
        products.Clear()
        let mutable filtered = getAllProducts catalog
        
        if selectedCategory <> "All" then
            filtered <- filterByCategory filtered selectedCategory
        
        if not (String.IsNullOrWhiteSpace(searchText)) then
            filtered <- searchByName filtered searchText
        
        filtered |> List.iter (fun p -> products.Add(ProductViewModel(p)))
    
    member private this.UpdateCart() =
        cart <- recalculateCart cart
        cartItems.Clear()
        cart.Items |> List.iter (fun item -> cartItems.Add(CartItemViewModel(item, removeFromCartCommand)))
        
        this.RaisePropertyChanged("CartSubtotal")
        this.RaisePropertyChanged("CartDiscount")
        this.RaisePropertyChanged("CartTotal")
        this.RaisePropertyChanged("CartItemCount")
        this.RaisePropertyChanged("HasItems")
        this.RaisePropertyChanged("HasCoupon")
        this.RaisePropertyChanged("AppliedCouponText")
        
        (clearCartCommand :?> RelayCommand).RaiseCanExecuteChanged()
        (checkoutCommand :?> RelayCommand).RaiseCanExecuteChanged()
        (removeCouponCommand :?> RelayCommand).RaiseCanExecuteChanged()
    
    member private this.RefreshOrdersIfVisible() =
        if isOrderHistoryVisible then
            this.LoadOrdersExecute()
    
    // ───────────────────────────────────────────────────────────────────────────
    // CART COMMAND IMPLEMENTATIONS
    // ───────────────────────────────────────────────────────────────────────────
    member private this.AddToCartExecute() =
        Console.WriteLine("=== AddToCartExecute called ===")
        
        if obj.ReferenceEquals(selectedProduct, null) then
            statusMessage <- "Please select a product"
            this.RaisePropertyChanged("StatusMessage")
            Console.WriteLine("=== No product selected ===")
        else
            Console.WriteLine($"Attempting to add {quantityToAdd} x {selectedProduct.Name} to cart")
            match addToCart cart selectedProduct.Product quantityToAdd with
            | Ok newCart ->
                cart <- newCart
                this.UpdateCart()
                statusMessage <- sprintf "✓ Added %d x %s" quantityToAdd selectedProduct.Name
                this.RaisePropertyChanged("StatusMessage")
                Console.WriteLine("=== Item added successfully ===")
            | Error msg ->
                statusMessage <- sprintf "✗ %s" msg
                this.RaisePropertyChanged("StatusMessage")
                Console.WriteLine($"=== Error adding item: {msg} ===")
    
    member this.RemoveFromCart(productId: int) =
        cart <- removeFromCart cart productId
        this.UpdateCart()
        statusMessage <- "Item removed from cart"
        this.RaisePropertyChanged("StatusMessage")
    
    member private this.ClearCartExecute() =
        cart <- emptyCart
        this.UpdateCart()
        statusMessage <- "Cart cleared"
        this.RaisePropertyChanged("StatusMessage")
    
    member private this.ApplyCouponExecute() =
        if String.IsNullOrWhiteSpace(couponCode) then
            statusMessage <- "Please enter a coupon code"
            this.RaisePropertyChanged("StatusMessage")
        else
            match applyCouponToCart cart couponCode with
            | Ok newCart ->
                cart <- newCart
                this.UpdateCart()
                statusMessage <- sprintf "✓ Coupon applied! Saved $%.2f" cart.Discount
                couponCode <- ""
                this.RaisePropertyChanged("CouponCode")
                this.RaisePropertyChanged("StatusMessage")
            | Error msg ->
                statusMessage <- sprintf "✗ %s" msg
                this.RaisePropertyChanged("StatusMessage")
    
    member private this.RemoveCouponExecute() =
        cart <- removeCoupon cart
        this.UpdateCart()
        statusMessage <- "Coupon removed"
        this.RaisePropertyChanged("StatusMessage")
    
    member private this.CheckoutExecute() =
        if isEmpty cart then
            statusMessage <- "Cart is empty"
            this.RaisePropertyChanged("StatusMessage")
        else
            // Decrease stock in catalog
            catalog <- decreaseStockBatch catalog cart.Items
            
            // Save order
            match saveOrder cart with
            | Ok filePath ->
                statusMessage <- "✓ Order completed successfully!"
                cart <- emptyCart
                this.UpdateCart()
                this.FilterProducts()  // Refresh products to show updated stock
                this.RefreshOrdersIfVisible()  // NEW: Refresh order history if visible
                this.RaisePropertyChanged("StatusMessage")
                Console.WriteLine($"Order saved to: {filePath}")
            | Error msg ->
                statusMessage <- sprintf "Order saved but: %s" msg
                this.RaisePropertyChanged("StatusMessage")
    
    // ───────────────────────────────────────────────────────────────────────────
    // ORDER HISTORY COMMAND IMPLEMENTATIONS (NEW)
    // ───────────────────────────────────────────────────────────────────────────
    member private this.LoadOrdersExecute() =
        Console.WriteLine("=== Loading orders ===")
        
        // Clear existing data
        orders.Clear()
        selectedOrder <- null
        orderDetails <- ""
        
        // Load orders from file system
        let loadedOrders = loadAllOrdersAsDisplayInfo()
        loadedOrders |> List.iter (fun o -> orders.Add(OrderViewModel(o)))
        
        // Show order history panel
        isOrderHistoryVisible <- true
        
        // Notify UI of all changes
        this.RaisePropertyChanged("IsOrderHistoryVisible")
        this.RaisePropertyChanged("IsOrderHistoryHidden")
        this.RaisePropertyChanged("OrderCount")
        this.RaisePropertyChanged("OrderCountText")
        this.RaisePropertyChanged("TotalSpentText")
        this.RaisePropertyChanged("SelectedOrder")
        this.RaisePropertyChanged("HasSelectedOrder")
        this.RaisePropertyChanged("OrderDetails")
        
        // Update status message
        statusMessage <- sprintf "📋 Loaded %d orders" orders.Count
        this.RaisePropertyChanged("StatusMessage")
        
        Console.WriteLine($"=== Loaded {orders.Count} orders ===")
    
    member private this.CloseOrderHistoryExecute() =
        Console.WriteLine("=== Closing order history ===")
        
        // Hide order history panel
        isOrderHistoryVisible <- false
        selectedOrder <- null
        orderDetails <- ""
        
        // Notify UI
        this.RaisePropertyChanged("IsOrderHistoryVisible")
        this.RaisePropertyChanged("IsOrderHistoryHidden")
        this.RaisePropertyChanged("SelectedOrder")
        this.RaisePropertyChanged("HasSelectedOrder")
        this.RaisePropertyChanged("OrderDetails")
        
        // Reset status message
        statusMessage <- "Welcome to F# Store Simulator!"
        this.RaisePropertyChanged("StatusMessage")
    
    member private this.ViewOrderDetailsExecute(order: OrderViewModel) =
        Console.WriteLine($"=== Viewing order: {order.ShortOrderId} ===")
        
        // Set selected order
        selectedOrder <- order
        orderDetails <- order.Details
        
        // Notify UI
        this.RaisePropertyChanged("SelectedOrder")
        this.RaisePropertyChanged("HasSelectedOrder")
        this.RaisePropertyChanged("OrderDetails")
        
        // Update status message
        statusMessage <- sprintf "📄 Viewing order from %s" order.DateFormatted
        this.RaisePropertyChanged("StatusMessage")
    
    member private this.DeleteOrderExecute(fileName: string) =
        Console.WriteLine($"=== Deleting order: {fileName} ===")
        
        match deleteOrder fileName with
        | Ok () ->
            // Refresh the orders list
            this.LoadOrdersExecute()
            statusMessage <- "🗑️ Order deleted successfully"
            this.RaisePropertyChanged("StatusMessage")
            Console.WriteLine("=== Order deleted successfully ===")
        | Error msg ->
            statusMessage <- sprintf "❌ Failed to delete: %s" msg
            this.RaisePropertyChanged("StatusMessage")
            Console.WriteLine($"=== Error deleting order: {msg} ===")