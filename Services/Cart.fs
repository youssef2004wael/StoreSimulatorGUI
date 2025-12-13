module StoreSimulatorGUI.Cart

open StoreSimulatorGUI.Models

// Create empty cart (UPDATED)
let emptyCart : Cart = {
    Items = []
    TotalBeforeDiscount = 0m
    Discount = 0m
    FinalTotal = 0m
    AppliedCoupon = None  // ← ADD THIS LINE
}

// Find cart item by product ID
let findCartItem (cart: Cart) (productId: int) : CartItem option =
    cart.Items 
    |> List.tryFind (fun item -> item.Product.Id = productId)

// Check if product is in cart
let isInCart (cart: Cart) (productId: int) : bool =
    cart.Items 
    |> List.exists (fun item -> item.Product.Id = productId)

// Add product to cart (immutable operation)
let addToCart (cart: Cart) (product: Product) (quantity: int) : Result<Cart, string> =
    if quantity <= 0 then
        Error "Quantity must be positive"
    elif quantity > product.Stock then
        Error (sprintf "Not enough stock. Available: %d" product.Stock)
    else
        let updatedItems =
            match findCartItem cart product.Id with
            | Some existingItem ->
                let newQuantity = existingItem.Quantity + quantity
                if newQuantity > product.Stock then
                    cart.Items // Return unchanged if exceeds stock
                else
                    cart.Items 
                    |> List.map (fun item -> 
                        if item.Product.Id = product.Id then
                            { item with Quantity = newQuantity }
                        else
                            item)
            | None ->
                { Product = product; Quantity = quantity } :: cart.Items
        
        Ok { cart with Items = updatedItems }

// Remove product from cart completely
let removeFromCart (cart: Cart) (productId: int) : Cart =
    let updatedItems = 
        cart.Items 
        |> List.filter (fun item -> item.Product.Id <> productId)
    
    { cart with Items = updatedItems }

// Update quantity of product in cart
let updateQuantity (cart: Cart) (productId: int) (newQuantity: int) : Result<Cart, string> =
    if newQuantity <= 0 then
        Ok (removeFromCart cart productId)
    else
        match findCartItem cart productId with
        | Some item ->
            if newQuantity > item.Product.Stock then
                Error (sprintf "Not enough stock. Available: %d" item.Product.Stock)
            else
                let updatedItems =
                    cart.Items 
                    |> List.map (fun i -> 
                        if i.Product.Id = productId then
                            { i with Quantity = newQuantity }
                        else
                            i)
                Ok { cart with Items = updatedItems }
        | None ->
            Error "Product not in cart"



// Clear entire cart
let clearCart (cart: Cart) : Cart =
    emptyCart

// Get cart item count (total items)
let getItemCount (cart: Cart) : int =
    cart.Items |> List.sumBy (fun item -> item.Quantity)

// Get unique product count
let getUniqueProductCount (cart: Cart) : int =
    cart.Items.Length

// Check if cart is empty
let isEmpty (cart: Cart) : bool =
    cart.Items.IsEmpty


// Display cart item
let displayCartItem (item: CartItem) : string =
    let subtotal = item.Product.Price * decimal item.Quantity
    sprintf "%s x %d @ $%.2f = $%.2f" 
        item.Product.Name item.Quantity item.Product.Price subtotal


