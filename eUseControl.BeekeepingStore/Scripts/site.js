@{
    ViewBag.Title = "Shopping Cart";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<div class="container py-4">
    <div class="row">
        <div class="col-12">
            <div class="d-flex justify-content-between align-items-center mb-4">
                <h1 class="mb-0">Shopping Cart</h1>
                <div>
                    <button class="btn btn-outline-danger me-2 clear-cart-btn">
                        <i class="fas fa-trash me-2"></i>Clear Cart
                    </button>
                    <button class="btn btn-outline-warning me-2 reset-storage-btn">
                        <i class="fas fa-sync me-2"></i>Reset Cache
                    </button>
                    <a href="@Url.Action("Products", "Home")" class="btn btn-outline-primary">
                        <i class="fas fa-arrow-left me-2"></i>Continue Shopping
                    </a>
                </div>
            </div>
            <hr>
        </div>
    </div>

    <div class="row">
        <div class="col-lg-8">
            <!-- Cart items will be injected here by JavaScript -->
            <div class="cart-items"></div>
        </div>

        <div class="col-lg-4">
            <div class="card cart-summary">
                <div class="card-body">
                    <h4 class="card-title mb-4">Order Summary</h4>

                    <div class="d-flex justify-content-between mb-3">
                        <span>Subtotal</span>
                        <span class="cart-subtotal">0,00 MDL</span>
                    </div>

                    <div class="d-flex justify-content-between mb-3">
                        <span>Shipping</span>
                        <span class="cart-shipping">0,00 MDL</span>
                    </div>

                    <hr>

                    <div class="d-flex justify-content-between mb-4">
                        <span class="fw-bold">Total</span>
                        <span class="fw-bold cart-total">0,00 MDL</span>
                    </div>

                    <div class="mb-3">
                        <label for="promo-code" class="form-label">Promo Code</label>
                        <div class="input-group">
                            <input type="text" class="form-control" id="promo-code" placeholder="Enter promo code">
                            <button class="btn btn-outline-secondary" type="button">Apply</button>
                        </div>
                    </div>

                    <a href="@Url.Action("Checkout", "Order")" class="btn btn-primary w-100">
                        Proceed to Checkout
                    </a>
                </div>

                <div class="card-footer">
                    <div class="text-center mb-2">
                        <span class="text-muted">Secure Checkout</span>
                    </div>
                    <div class="d-flex justify-content-center">
                        <i class="fab fa-cc-visa mx-1 fa-2x text-muted"></i>
                        <i class="fab fa-cc-mastercard mx-1 fa-2x text-muted"></i>
                        <i class="fab fa-cc-amex mx-1 fa-2x text-muted"></i>
                        <i class="fab fa-cc-paypal mx-1 fa-2x text-muted"></i>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div >

    @section scripts {
    <script>
        /**
        * Cart cleanup function - runs on page load to prevent duplicate items
        */
        (function() {
            console.log("Cart cleanup: Initializing duplicate prevention");

        function cleanupCart() {
                try {
                    // Get the cart from localStorage
                    const cart = JSON.parse(localStorage.getItem('cart') || '[]');
        if (!Array.isArray(cart) || cart.length === 0) return;

        console.log("Cart cleanup: Original cart items:", cart.length);

        // Use a map to track unique items by ID
        const uniqueItems = new Map();
        let duplicateFound = false;

                    // Process each item in the cart
                    cart.forEach(item => {
                        const itemId = String(item.id || '');
        if (!itemId) return;

        // If this ID already exists in our map, it's a duplicate
        if (uniqueItems.has(itemId)) {
            duplicateFound = true;
        console.log(`Cart cleanup: Found duplicate item ${item.name} (ID: ${itemId})`);

        // Keep the item with the highest quantity if both exist
        const existingItem = uniqueItems.get(itemId);
                            if (item.quantity > existingItem.quantity) {
            console.log(`Cart cleanup: Keeping newer item with quantity ${item.quantity}`);
        uniqueItems.set(itemId, item);
                            }
                        } else {
            // First time seeing this ID, add it to our map
            uniqueItems.set(itemId, item);
                        }
                    });

        // If we found duplicates, update the cart
        if (duplicateFound) {
                        const cleanCart = Array.from(uniqueItems.values());
        console.log(`Cart cleanup: Updated cart from ${cart.length} to ${cleanCart.length} items`);
        localStorage.setItem('cart', JSON.stringify(cleanCart));

        // Force page reload if we're on the cart page
        if (window.location.href.includes('/Cart')) {
            console.log("Cart cleanup: Reloading page to update UI");
                            setTimeout(() => window.location.reload(), 100);
                        }
                    } else {
            console.log("Cart cleanup: No duplicates found");
                    }
                } catch (e) {
            console.error("Cart cleanup error:", e);
                }
            }

        // Run on initialization
        cleanupCart();

        // Add event listeners to ensure it runs properly
        window.addEventListener('load', cleanupCart);
        document.addEventListener('DOMContentLoaded', cleanupCart);
        })();

        // The previous duplicate handling code has been consolidated into a single cleanupCart function above
        // No additional cleanup functions are needed

        /**
         * Format currency based on locale
         */
        function cartFormatCurrency(price, locale = 'ro-MD', currency = 'MDL') {
            // Create a better currency formatter that ensures correct display
            try {
                // Format using Intl API
                const formatter = new Intl.NumberFormat(locale, {
            style: 'currency',
        currency: currency,
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
                });

        // Get the formatted result
        let formatted = formatter.format(price);

        // Fix common formatting issues
        if (formatted.includes('MDL MDL') || formatted.includes('L MDL')) {
            // Clean up the duplication
            formatted = formatted.replace(' MDL', '');
                }

        // Handle any other odd formatting
        if (!formatted.includes('MDL') && !formatted.includes('L')) {
            formatted = formatted + ' MDL';
                }

        console.log(`Formatted price: ${price} -> ${formatted}`);
        return formatted;

            } catch (error) {
            // Fallback formatting
            console.error('Currency formatting error:', error);
        return price.toFixed(2) + ' MDL';
            }
        }

        /**
         * Function that completely overrides the quantity buttons
         */
        function fixCartQuantityButtons() {
            console.log("[CART FIX] Starting complete cart button override...");

        // Wait for the DOM to be fully loaded
        if (!document.querySelector('.cart-items')) {
            console.log("[CART FIX] Cart items container not found");
        return;
            }

        // Get all increase buttons USING ANY POSSIBLE CLASS NAME (including our own fixed class)
        const plusButtons = document.querySelectorAll('.increase-btn, .cart-plus-btn, .cart-plus-fixed');
        console.log("[CART FIX] Found", plusButtons.length, "plus buttons");

        if (plusButtons.length === 0) {
            console.log("[CART FIX] Problema detectată: Nu s-au găsit butoane plus. Așteptăm...");
        // Reîncercăm mai târziu
        setTimeout(fixCartQuantityButtons, 500);
        return;
            }

            plusButtons.forEach(btn => {
                // Skip buttons that are already fixed
                if (btn.classList.contains('cart-plus-fixed')) {
                    return;
                }

        // Completely replace the button with a new one
        const newBtn = document.createElement('button');
        newBtn.className = "btn btn-sm btn-outline-secondary cart-plus-fixed";
        newBtn.type = "button";
        newBtn.innerHTML = "+";

        // Replace the old button
        if (btn.parentNode) {
            btn.parentNode.replaceChild(newBtn, btn);
                }

        // Add direct event handler
        newBtn.onclick = function (e) {
            e.preventDefault();
        e.stopPropagation();

        // Get product details
        const item = this.closest('.cart-item');
        if (!item) return;

        const productId = item.getAttribute('data-product-id');
        if (!productId) {
            console.error("[CART FIX] Product ID not found");
        return;
                    }

        // Direct cart manipulation
        const cart = JSON.parse(localStorage.getItem('cart')) || [];
                    const itemIndex = cart.findIndex(x => x.id === productId);

        if (itemIndex === -1) {
            console.error("[CART FIX] Product not found in cart:", productId);
        return;
                    }

        const oldQty = cart[itemIndex].quantity;
        const newQty = oldQty + 1;

        // Log changes
        console.log("[CART FIX] Increment:", productId, "from", oldQty, "to", newQty);

        // Update quantity in localStorage
        cart[itemIndex].quantity = newQty;
        localStorage.setItem('cart', JSON.stringify(cart));

        // Update UI
        const qtyInput = item.querySelector('.quantity-input');
        if (qtyInput) qtyInput.value = newQty;

        const totalPrice = cart[itemIndex].price * newQty;
        const priceEl = item.querySelector('.col-lg-2.col-md-2.col-sm-4.mt-3.mt-sm-0.text-end .fw-bold');
        if (priceEl) priceEl.textContent = cartFormatCurrency(totalPrice);

        // Update cart badge
        const cartBadge = document.querySelector('.cart-badge');
        if (cartBadge) {
                        const totalItems = cart.reduce((total, item) => total + item.quantity, 0);
        cartBadge.textContent = totalItems;

                        if (totalItems > 0) {
            cartBadge.classList.remove('d-none');
                        }
                    }

        // Update totals
        updateCartTotals();

        return false;
                };
            });

        // Get all decrease buttons USING ANY POSSIBLE CLASS NAME (including our own fixed class)
        const minusButtons = document.querySelectorAll('.decrease-btn, .cart-minus-btn, .cart-minus-fixed');
        console.log("[CART FIX] Found", minusButtons.length, "minus buttons");

            minusButtons.forEach(btn => {
                // Skip buttons that are already fixed
                if (btn.classList.contains('cart-minus-fixed')) {
                    return;
                }

        // Completely replace the button with a new one
        const newBtn = document.createElement('button');
        newBtn.className = "btn btn-sm btn-outline-secondary cart-minus-fixed";
        newBtn.type = "button";
        newBtn.innerHTML = "-";

        // Replace the old button
        if (btn.parentNode) {
            btn.parentNode.replaceChild(newBtn, btn);
                }

        // Add direct event handler
        newBtn.onclick = function (e) {
            e.preventDefault();
        e.stopPropagation();

        // Get product details
        const item = this.closest('.cart-item');
        if (!item) return;

        const productId = item.getAttribute('data-product-id');
        if (!productId) {
            console.error("[CART FIX] Product ID not found");
        return;
                    }

        // Direct cart manipulation
        const cart = JSON.parse(localStorage.getItem('cart')) || [];
                    const itemIndex = cart.findIndex(x => x.id === productId);

        if (itemIndex === -1) {
            console.error("[CART FIX] Product not found in cart:", productId);
        return;
                    }

        const oldQty = cart[itemIndex].quantity;
        const newQty = Math.max(1, oldQty - 1);

        // Log changes
        console.log("[CART FIX] Decrement:", productId, "from", oldQty, "to", newQty);

        // Update quantity in localStorage
        cart[itemIndex].quantity = newQty;
        localStorage.setItem('cart', JSON.stringify(cart));

        // Update UI
        const qtyInput = item.querySelector('.quantity-input');
        if (qtyInput) qtyInput.value = newQty;

        const totalPrice = cart[itemIndex].price * newQty;
        const priceEl = item.querySelector('.col-lg-2.col-md-2.col-sm-4.mt-3.mt-sm-0.text-end .fw-bold');
        if (priceEl) priceEl.textContent = cartFormatCurrency(totalPrice);

        // Update cart badge
        const cartBadge = document.querySelector('.cart-badge');
        if (cartBadge) {
                        const totalItems = cart.reduce((total, item) => total + item.quantity, 0);
        cartBadge.textContent = totalItems;

                        if (totalItems > 0) {
            cartBadge.classList.remove('d-none');
                        }
                    }

        // Update totals
        updateCartTotals();

        return false;
                };
            });

        console.log("[CART FIX] Cart button override complete!");
        }

        /**
         * Update cart totals
         */
        function updateCartTotals() {
            const cart = JSON.parse(localStorage.getItem('cart')) || [];

            // Calculate values
            const subtotal = cart.reduce((total, item) => total + (item.price * item.quantity), 0);
            const shipping = subtotal > 900 ? 0 : 150;
        const total = subtotal + shipping;

        // Update display elements
        const subtotalEl = document.querySelector('.cart-subtotal');
        const shippingEl = document.querySelector('.cart-shipping');
        const totalEl = document.querySelector('.cart-total');

        if (subtotalEl) subtotalEl.textContent = cartFormatCurrency(subtotal);
        if (shippingEl) shippingEl.textContent = shipping === 0 ? 'FREE' : cartFormatCurrency(shipping);
        if (totalEl) totalEl.textContent = cartFormatCurrency(total);

        console.log("[CART FIX] Updated totals. Subtotal:", subtotal, "Shipping:", shipping, "Total:", total);
        }

        /**
         * Add event handlers to delete buttons
         */
        function addDeleteButtonHandlers() {
            const deleteButtons = document.querySelectorAll('.delete-item');
        console.log("Adding handlers to", deleteButtons.length, "delete buttons");
            
            deleteButtons.forEach(button => {
                // Verificăm dacă butonul are deja un handler atașat
                if (button.hasAttribute('data-has-delete-handler')) {
                    return; // Sărim peste acest buton, are deja un handler
                }

        // Marcăm butonul ca având un handler atașat
        button.setAttribute('data-has-delete-handler', 'true');

        button.addEventListener('click', function(e) {
            e.preventDefault();
        e.stopPropagation();

        // Find parent cart item to get the product ID
        const cartItem = this.closest('.cart-item');
        if (!cartItem) return;

        const productId = cartItem.getAttribute('data-product-id');
        if (!productId) {
            console.error("Product ID not found for delete operation");
        return;
                    }

        // Get product name for notification
        const productName = cartItem.querySelector('h5')?.textContent || 'Product';

        // Confirm deletion
        if (confirm(`Remove ${productName} from your cart?`)) {
            console.log("Removing product:", productId);

        // Get the cart
        let cart = JSON.parse(localStorage.getItem('cart') || '[]');

                        // Filter out the product
                        cart = cart.filter(item => String(item.id) !== String(productId));

        // Save the updated cart
        localStorage.setItem('cart', JSON.stringify(cart));

        // Remove the item from UI with animation
        cartItem.style.transition = 'all 0.3s ease-out';
        cartItem.style.opacity = '0';
        cartItem.style.transform = 'translateX(30px)';
                        
                        setTimeout(() => {
            // Re-render the cart or remove the item
            renderCartItems();

        // Show notification
        showNotification(`${productName} has been removed from your cart.`, 'info');

        // Update cart badge
        const cartBadge = document.querySelector('.cart-badge');
        if (cartBadge) {
                                const totalItems = cart.reduce((total, item) => total + item.quantity, 0);
        cartBadge.textContent = totalItems;

        if (totalItems === 0) {
            cartBadge.classList.add('d-none');
                                }
                            }
                        }, 300);
                    }
                });
            });
        }

        /**
         * BRUTAL FIX: Completely replace the renderCartItems function
         */
        function hijackRenderCartItems() {
            console.log("[CART FIX] Attempting to override renderCartItems...");

        // Save the original function
        if (!window._originalRenderCartItems && typeof window.renderCartItems === 'function') {
            window._originalRenderCartItems = window.renderCartItems;

        // Replace with our own
        window.renderCartItems = function () {
            console.log("[CART FIX] Using hijacked renderCartItems");

        // Call original function to build the cart
        window._originalRenderCartItems();

        // Aplicăm toate fix-urile într-un singur setTimeout
        // pentru a le executa într-o singură operație
        setTimeout(function() {
            addDeleteButtonHandlers();
        fixCartQuantityButtons();

        // Procesăm și inputurile de cantitate
        const quantityInputs = document.querySelectorAll('.quantity-input');
                        quantityInputs.forEach(input => {
            // Make the input wider and more visible
            input.style.minWidth = "50px";
        input.style.width = "50px";
        input.style.textAlign = "center";
        input.style.fontWeight = "bold";
        input.style.fontSize = "1.1rem";
        input.style.padding = "0.2rem";
        input.style.margin = "0 0.3rem";

        // Make the parent control more spacious
        const controlDiv = input.closest('.quantity-control');
        if (controlDiv) {
            controlDiv.style.display = "flex";
        controlDiv.style.justifyContent = "center";
        controlDiv.style.alignItems = "center";
        controlDiv.style.padding = "0.5rem 0";
                            }
                        });
                    }, 50);
                };

        console.log("[CART FIX] Successfully overrode renderCartItems");
            } else {
            console.log("[CART FIX] Could not override renderCartItems");
            }
        }

        /**
         * Function to fix all price displays in the cart
         */
        function fixPriceDisplays() {
            // Find all price elements in the cart
            const priceElements = document.querySelectorAll('.cart-item .fw-bold');

            priceElements.forEach(el => {
                // Get the current text
                const currentText = el.textContent.trim();

        // Check if it contains the problematic format
        if (currentText.includes('L MDL')) {
                    // Fix the format by removing the duplicate
                    const fixedText = currentText.replace(' MDL', '');
        el.textContent = fixedText;
        console.log(`[CART FIX] Fixed price display: ${currentText} -> ${fixedText}`);
                }
            });

        // Also fix summary prices
        const summaryPrices = document.querySelectorAll('.cart-subtotal, .cart-shipping, .cart-total');
            summaryPrices.forEach(el => {
                if (el.textContent.includes('L MDL')) {
            el.textContent = el.textContent.replace(' MDL', '');
                }
            });
        }

        // Run the fix once the cart is loaded
        document.addEventListener('DOMContentLoaded', function () {
            console.log("[CART FIX] DOM loaded, installing fixes...");

        // Try to hijack the renderCartItems function
        hijackRenderCartItems();

        // Executăm funcțiile într-un mod mai eficient și evităm suprapunerile
        // Reducem numărul de apeluri la setInterval
        setTimeout(function() {
            fixCartQuantityButtons();
        addDeleteButtonHandlers();
        fixPriceDisplays();
            }, 300);

        // Registrăm evenimentul pentru randarea coșului
        document.addEventListener('cartRendered', function () {
            console.log("[CART FIX] Cart rendered event detected");
        // Adăugăm un singur timeout pentru toate operațiunile de fix
        setTimeout(function() {
            addDeleteButtonHandlers(); // Această funcție nu va mai adăuga handlere duplicate
        fixCartQuantityButtons();
        fixPriceDisplays();
                }, 50);
            });

        // Add reset storage button event
        const resetButton = document.querySelector('.reset-storage-btn');
        if (resetButton) {
            resetButton.addEventListener('click', function () {
                if (confirm('This will clear your cart data from browser storage. Continue?')) {
                    localStorage.clear();
                    window.location.reload();
                }
            });
            }
        });

        // Run fix on complete page load - doar o singură dată
        window.addEventListener('load', function () {
            console.log("[CART FIX] Window fully loaded, running final fix...");
        setTimeout(function() {
            addDeleteButtonHandlers();
        fixCartQuantityButtons();
        fixPriceDisplays();
            }, 500);
        });

        // Re-run the fix whenever needed
        window.fixCartButtons = fixCartQuantityButtons;
        window.fixPrices = fixPriceDisplays;
        window.fixDeleteButtons = addDeleteButtonHandlers;

        // Add custom styles for cart quantity inputs
        document.addEventListener('DOMContentLoaded', function () {
            // Add CSS rules for quantity inputs
            const style = document.createElement('style');
        style.textContent = `
        .quantity-input {
            min - width: 50px !important;
        width: 50px !important;
        text-align: center !important;
        font-weight: bold !important;
        font-size: 1.1rem !important;
        padding: 0.2rem !important;
        margin: 0 0.3rem !important;
        border-radius: 4px !important;
                    }
        .quantity-control {
            display: flex !important;
        justify-content: center !important;
        align-items: center !important;
        padding: 0.5rem 0 !important;
                    }
        .cart-plus-fixed, .cart-minus-fixed {
            min - width: 32px !important;
        height: 32px !important;
        font-weight: bold !important;
        display: flex !important;
        align-items: center !important;
        justify-content: center !important;
                    }
        `;
        document.head.appendChild(style);
        });

        /**
         * Fallback implementation of renderCartItems to ensure delete functionality works
         * This will only be used if the site.js version is not available
         */
        if (typeof window.renderCartItems !== 'function') {
            console.log("No renderCartItems found, implementing fallback version");

        window.renderCartItems = function() {
                const cartItemsContainer = document.querySelector('.cart-items');
        if (!cartItemsContainer) return;

        // Get cart from localStorage
        const cart = JSON.parse(localStorage.getItem('cart') || '[]');
        console.log("Fallback renderCartItems with", cart.length, "items");

        if (cart.length === 0) {
            // Show empty cart message
            cartItemsContainer.innerHTML = `
                        <div class="text-center py-5">
                            <i class="fas fa-shopping-cart fa-4x text-muted mb-3"></i>
                            <h3>Your cart is empty</h3>
                            <p class="mb-4">Looks like you haven't added any products to your cart yet.</p>
                            <a href="/Home/Products" class="btn btn-primary">Continue Shopping</a>
                        </div>
                    `;
        return;
                }

        // Render cart items
        let cartItemsHTML = '';
                cart.forEach(item => {
                    const itemPrice = parseFloat(item.price) || 0;
        const itemQuantity = parseInt(item.quantity) || 1;
        const totalPrice = itemPrice * itemQuantity;

        // Ensure image path is valid
        let imagePath = item.image || '/Content/Images/products/beehive.jpg';
        if (!imagePath.startsWith('/') && !imagePath.startsWith('http')) {
            imagePath = '/' + imagePath;
                    }

        cartItemsHTML += `
        <div class="card mb-3 cart-item" data-product-id="${item.id}">
            <div class="card-body">
                <div class="row align-items-center">
                    <div class="col-lg-2 col-md-3 col-sm-3">
                        <img src="${imagePath}"
                            class="img-fluid rounded"
                            alt="${item.name}"
                            onerror="this.src='/Content/Images/products/beehive.jpg'"
                            style="max-height: 100px; object-fit: cover;">
                    </div>
                    <div class="col-lg-4 col-md-3 col-sm-9">
                        <h5 class="mb-0">${item.name}</h5>
                    </div>
                    <div class="col-lg-2 col-md-2 col-sm-4 mt-3 mt-sm-0">
                        <div class="quantity-control d-flex align-items-center">
                            <button class="btn btn-sm btn-outline-secondary cart-minus-btn" type="button">-</button>
                            <input type="text"
                                class="form-control text-center quantity-input"
                                value="${itemQuantity}"
                                readonly
                                style="background-color: #fff; pointer-events: none;">
                                <button class="btn btn-sm btn-outline-secondary cart-plus-btn" type="button">+</button>
                        </div>
                    </div>
                    <div class="col-lg-2 col-md-2 col-sm-4 mt-3 mt-sm-0">
                        <div class="fw-bold">${cartFormatCurrency(itemPrice)}</div>
                    </div>
                    <div class="col-lg-2 col-md-2 col-sm-4 mt-3 mt-sm-0 text-end">
                        <div class="fw-bold">${cartFormatCurrency(totalPrice)}</div>
                        <button class="btn btn-sm text-danger delete-item" type="button">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
        `;
                });

        cartItemsContainer.innerHTML = cartItemsHTML;

                // Calculate and update totals
                const subtotal = cart.reduce((total, item) => total + (parseFloat(item.price) * parseInt(item.quantity)), 0);
                const shipping = subtotal > 900 ? 0 : 150;
        const total = subtotal + shipping;

        // Update summary if it exists
        const subtotalEl = document.querySelector('.cart-subtotal');
        const shippingEl = document.querySelector('.cart-shipping');
        const totalEl = document.querySelector('.cart-total');

        if (subtotalEl) subtotalEl.textContent = cartFormatCurrency(subtotal);
        if (shippingEl) shippingEl.textContent = shipping === 0 ? 'FREE' : cartFormatCurrency(shipping);
        if (totalEl) totalEl.textContent = cartFormatCurrency(total);

        // Trigger event for other handlers
        const event = new CustomEvent('cartRendered');
        document.dispatchEvent(event);
            };
        }
    </script>
}