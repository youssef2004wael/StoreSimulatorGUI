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

// Simple Command Implementation
type RelayCommand(execute: obj -> unit, canExecute: obj -> bool) =
    let canExecuteChanged = Event<EventHandler, EventArgs>()
    
    interface ICommand with
        member _.Execute(parameter) = execute parameter
        member _.CanExecute(parameter) = canExecute parameter
        [<CLIEvent>]
        member _.CanExecuteChanged = canExecuteChanged.Publish
    
    member _.RaiseCanExecuteChanged() = 
        canExecuteChanged.Trigger(null, EventArgs.Empty)

// Product ViewModel
// Product ViewModel
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

// Cart Item ViewModel
type CartItemViewModel(item: CartItem, removeCommand: ICommand) =
    member _.ProductName = item.Product.Name
    member _.Price = item.Product.Price
    member _.Quantity = item.Quantity
    member _.Subtotal = item.Product.Price * decimal item.Quantity
    member _.SubtotalFormatted = sprintf "$%.2f" (item.Product.Price * decimal item.Quantity)
    member _.ProductId = item.Product.Id
    member _.RemoveCommand = removeCommand

// Main ViewModel
type MainViewModel() as this =
    let mutable catalog = initializeCatalog()
    let mutable cart = emptyCart
    let mutable selectedProduct: ProductViewModel = null
    let mutable quantityToAdd = 1
    let mutable statusMessage = "Welcome to F# Store Simulator!"
    let mutable couponCode = ""
    let mutable searchText = ""
    let mutable selectedCategory = "All"
    
    let products = ObservableCollection<ProductViewModel>()
    let cartItems = ObservableCollection<CartItemViewModel>()
    let categories = ObservableCollection<string>()
    
    let propertyChanged = Event<PropertyChangedEventHandler, PropertyChangedEventArgs>()
    
    // Commands
    let mutable addToCartCommand: ICommand = null
    let mutable clearCartCommand: ICommand = null
    let mutable applyCouponCommand: ICommand = null
    let mutable removeCouponCommand: ICommand = null
    let mutable checkoutCommand: ICommand = null
    let mutable removeFromCartCommand: ICommand = null
    
    // Initialize
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
            
            // Initialize commands
            Console.WriteLine("Initializing commands...")
            addToCartCommand <- RelayCommand((fun _ -> this.AddToCartExecute()), (fun _ -> this.CanAddToCart))
            clearCartCommand <- RelayCommand((fun _ -> this.ClearCartExecute()), (fun _ -> this.HasItems))
            applyCouponCommand <- RelayCommand((fun _ -> this.ApplyCouponExecute()), (fun _ -> true))
            removeCouponCommand <- RelayCommand((fun _ -> this.RemoveCouponExecute()), (fun _ -> this.HasCoupon))
            checkoutCommand <- RelayCommand((fun _ -> this.CheckoutExecute()), (fun _ -> this.HasItems))
            removeFromCartCommand <- RelayCommand((fun param -> 
                match param with
                | :? int as id -> this.RemoveFromCart(id)
                | _ -> ()), (fun _ -> true))
            
            Console.WriteLine("MainViewModel initialized successfully")
        with
        | ex ->
            Console.WriteLine($"ERROR initializing MainViewModel: {ex.Message}")
            Console.WriteLine($"Stack: {ex.StackTrace}")
            reraise()
    
    // INotifyPropertyChanged implementation
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member _.PropertyChanged = propertyChanged.Publish
    
    member private _.RaisePropertyChanged(propertyName: string) =
        propertyChanged.Trigger(this, PropertyChangedEventArgs(propertyName))
    
    // Properties
    member _.Products = products
    member _.CartItems = cartItems
    member _.Categories = categories
    
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
    
    member _.StatusMessage
        with get() = statusMessage
        and set(value) = 
            statusMessage <- value
            this.RaisePropertyChanged("StatusMessage")
    
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
    
    member _.CouponCode
        with get() = couponCode
        and set(value) = 
            couponCode <- value
            this.RaisePropertyChanged("CouponCode")
    
    // Cart properties
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
    
    // Command Properties
    member _.AddToCartCommand = addToCartCommand
    member _.ClearCartCommand = clearCartCommand
    member _.ApplyCouponCommand = applyCouponCommand
    member _.RemoveCouponCommand = removeCouponCommand
    member _.CheckoutCommand = checkoutCommand
    member _.RemoveFromCartCommand = removeFromCartCommand
    
    // Methods
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
    
    // Command Execute Methods
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
            catalog <- decreaseStockBatch catalog cart.Items
            match saveOrder cart with
            | Ok _ ->
                statusMessage <- "✓ Order completed successfully!"
                cart <- emptyCart
                this.UpdateCart()
                this.FilterProducts()
                this.RaisePropertyChanged("StatusMessage")
            | Error msg ->
                statusMessage <- sprintf "Order saved but: %s" msg
                this.RaisePropertyChanged("StatusMessage")