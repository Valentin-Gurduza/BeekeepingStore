/**
 * Beekeeping Store - Main JavaScript file
 */

// Wait for the document to be ready
document.addEventListener('DOMContentLoaded', function () {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });

    // Initialize the cart
    initializeCart();

    // Initialize cart page if we're on the cart page
    if (document.querySelector('.cart-items')) {
        renderCartItems();

        // Clear cart button
        const clearCartButton = document.querySelector('.clear-cart-btn');
        if (clearCartButton) {
            clearCartButton.addEventListener('click', function () {
                if (confirm('Are you sure you want to empty your cart?')) {
                    clearCart();
                    renderCartItems();
                    showNotification('Your cart has been emptied.', 'info');
                }
            });
        }

        // Reset storage button
        const resetButton = document.querySelector('.reset-storage-btn');
        if (resetButton) {
            resetButton.addEventListener('click', function () {
                if (confirm('This will clear your cart data from browser storage. Continue?')) {
                    localStorage.clear();
                    window.location.reload();
                }
            });
        }
    }

    // Add to cart functionality
    const addToCartButtons = document.querySelectorAll('.add-to-cart-btn');
    if (addToCartButtons.length > 0) {
        addToCartButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();

                // Verifică dacă suntem în wishlist - în acest caz lăsăm jQuery să gestioneze
                if (window.location.href.includes('/Wishlist')) {
                    console.log('Suntem în pagina Wishlist, ignorăm click-ul standard');
                    return;
                }

                const productId = this.getAttribute('data-product-id');
                const productName = this.getAttribute('data-product-name');
                const productPrice = this.getAttribute('data-product-price') || '0';
                const promotionalPrice = this.getAttribute('data-product-promotional-price');
                const originalPrice = this.getAttribute('data-product-original-price') || productPrice;
                const productImage = this.getAttribute('data-product-image') || '';

                // Animation for the button
                this.innerHTML = '<i class="fas fa-check"></i> Added';
                this.classList.add('btn-success');

                // Use promotional price if available
                const finalPrice = promotionalPrice ? parseFloat(promotionalPrice) : parseFloat(productPrice);

                console.log('Adding to cart with prices:', {
                    finalPrice: finalPrice,
                    originalPrice: parseFloat(originalPrice),
                    promotionalPrice: promotionalPrice ? parseFloat(promotionalPrice) : null
                });

                // Add the product to cart - aici folosim incrementQuantity=true (implicit)
                // pentru a permite creșterea cantității pentru produsele adăugate din alte pagini
                addToCart({
                    id: productId || productName, // Fall back to name if ID not set
                    name: productName,
                    price: finalPrice,
                    originalPrice: parseFloat(originalPrice), // Store original price for reference
                    image: productImage,
                    quantity: 1
                });

                // Display notification only if not adding from wishlist
                // Wishlist has its own notification in Romanian
                if (!window.isAddingFromWishlist) {
                    showNotification(`${productName} has been added to your cart. <a href="/Home/Cart" class="text-white fw-bold"><u>View Cart</u></a>`, 'success', 5000);
                }

                // Update cart badge
                updateCartBadge();

                // Reset button after 2 seconds
                setTimeout(() => {
                    this.innerHTML = '<i class="fas fa-shopping-cart"></i> Add to Cart';
                    this.classList.remove('btn-success');
                }, 2000);
            });
        });
    }

    // Product image gallery functionality
    const productMainImage = document.getElementById('product-main-image');
    const productThumbnails = document.querySelectorAll('.product-thumbnail');

    if (productMainImage && productThumbnails.length > 0) {
        productThumbnails.forEach(thumbnail => {
            thumbnail.addEventListener('click', function () {
                // Remove active class from all thumbnails
                productThumbnails.forEach(t => t.classList.remove('active'));

                // Add active class to clicked thumbnail
                this.classList.add('active');

                // Update main image
                const imageUrl = this.getAttribute('data-image');
                productMainImage.src = imageUrl;

                // Animate image change
                productMainImage.style.opacity = '0';
                setTimeout(() => {
                    productMainImage.style.opacity = '1';
                }, 200);
            });
        });
    }

    // Newsletter subscription
    const newsletterForm = document.querySelector('.newsletter-form');
    if (newsletterForm) {
        newsletterForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const emailInput = this.querySelector('input[type="email"]');
            const email = emailInput.value.trim();

            if (isValidEmail(email)) {
                // Success case
                showNotification('Thank you for subscribing to our newsletter!', 'success');
                emailInput.value = '';

                // You would add AJAX call here to actually submit the form
                // subscribeNewsletter(email);
            } else {
                // Error case
                showNotification('Please enter a valid email address.', 'danger');
            }
        });
    }

    // Search functionality
    const searchForm = document.querySelector('.search-form');
    if (searchForm) {
        searchForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const searchInput = this.querySelector('input[type="search"]');
            const searchTerm = searchInput.value.trim();

            if (searchTerm.length > 0) {
                // Redirect to search results page
                window.location.href = `/Search?q=${encodeURIComponent(searchTerm)}`;
            }
        });
    }

    // Mobile menu improvements
    const navbarToggler = document.querySelector('.navbar-toggler');
    if (navbarToggler) {
        navbarToggler.addEventListener('click', function () {
            document.body.classList.toggle('menu-open');
        });
    }

    // Initialize AOS (Animate on Scroll) library if available
    if (typeof AOS !== 'undefined') {
        AOS.init({
            duration: 800,
            easing: 'ease-in-out',
            once: true
        });
    }

    // Initialize carousels
    const carousels = document.querySelectorAll('.carousel');
    if (carousels.length > 0) {
        carousels.forEach(carousel => {
            new bootstrap.Carousel(carousel, {
                interval: 5000
            });
        });
    }
});

// Cart Functions

/**
 * Initialize the shopping cart
 */
function initializeCart() {
    // Create the cart if it doesn't exist
    if (!localStorage.getItem('cart')) {
        localStorage.setItem('cart', JSON.stringify([]));
    }

    // Update the cart badge
    updateCartBadge();
}

/**
 * Add a product to the cart
 * @param {Object} product - Product to add
 * @param {boolean} incrementQuantity - Whether to increment quantity if product exists (default: true)
 */
function addToCart(product, incrementQuantity = true) {
    // Validate input
    if (!product || !product.name) {
        console.error('Invalid product added to cart:', product);
        return;
    }

    console.log('Adding product to cart:', {
        id: product.id,
        name: product.name,
        price: product.price,
        quantity: product.quantity,
        incrementQuantity: incrementQuantity
    });

    // Ensure quantity is valid
    if (!product.quantity || isNaN(product.quantity) || product.quantity <= 0) {
        console.log('Invalid quantity, setting to 1:', product.quantity);
        product.quantity = 1;
    }

    // Convert ID to string for consistent comparison
    const productId = String(product.id);

    // Prevent multiple additions by disabling the button
    if (window.preventDuplicateAdd && window.preventDuplicateAdd[productId]) {
        console.log('Preventing multiple additions for:', productId);
        return;
    }

    // Mark product as being added
    if (!window.preventDuplicateAdd) window.preventDuplicateAdd = {};
    window.preventDuplicateAdd[productId] = true;

    // Release lock after a second
    setTimeout(() => {
        if (window.preventDuplicateAdd) {
            window.preventDuplicateAdd[productId] = false;
        }
    }, 1000);

    // Create a new product object to avoid references
    const newProduct = {
        id: productId,
        name: product.name,
        price: parseFloat(parseFloat(product.price).toFixed(2)) || 0,
        originalPrice: parseFloat(parseFloat(product.originalPrice || product.price).toFixed(2)) || 0,
        image: product.image || '',
        quantity: parseInt(product.quantity) || 1
    };

    // Verificăm dacă produsul are reducere și corectăm prețul dacă e necesar
    if (newProduct.originalPrice > newProduct.price) {
        // Calculăm reducerea actuală
        const currentDiscount = 1 - (newProduct.price / newProduct.originalPrice);
        console.log(`Produsul ${newProduct.name} are o reducere de ${(currentDiscount * 100).toFixed(2)}%`);

        // Verificăm dacă e reducere de aproximativ 5%
        if (Math.abs(currentDiscount - 0.05) < 0.01) {
            // Asigurăm-ne că prețul are valoarea exactă cu reducere de 5%
            const expectedPrice = parseFloat((newProduct.originalPrice * 0.95).toFixed(2));
            if (Math.abs(expectedPrice - newProduct.price) > 0.01) {
                console.log(`Corectăm prețul pentru ${newProduct.name} de la ${newProduct.price} la ${expectedPrice} (reducere exactă 5%)`);
                newProduct.price = expectedPrice;
            }
        }
    }

    // Log exact values to ensure we're capturing decimal places
    console.log('Product prices:', {
        rawPrice: product.price,
        formattedPrice: newProduct.price,
        rawOriginalPrice: product.originalPrice || product.price,
        formattedOriginalPrice: newProduct.originalPrice
    });

    // Get cart from localStorage
    let cart;
    try {
        cart = JSON.parse(localStorage.getItem('cart') || '[]');

        if (!Array.isArray(cart) || cart.length === 0) {
            console.error('Cart in localStorage is not a valid array!');
            cart = [];
        }
    } catch (e) {
        console.error('Error reading cart from localStorage:', e);
        cart = [];
    }

    console.log('Current cart contents:', cart.map(x => ({ id: x.id, name: x.name, qty: x.quantity })));

    // Find existing product
    const existingIndex = cart.findIndex(item => String(item.id) === productId);

    if (existingIndex > -1) {
        console.log(`Product found in cart at index ${existingIndex}. Current quantity:`, cart[existingIndex].quantity);

        if (incrementQuantity) {
            // If incrementQuantity is true, add to existing quantity
            cart[existingIndex].quantity += newProduct.quantity;
            console.log('After incrementing, new quantity:', cart[existingIndex].quantity);

            // Make sure we preserve/update the price data (important for promotions)
            // Only update price if the new price is different and lower (promotional)
            if (newProduct.price < cart[existingIndex].price) {
                console.log('Updating price from', cart[existingIndex].price, 'to promotional price', newProduct.price);
                cart[existingIndex].price = newProduct.price;
                cart[existingIndex].originalPrice = newProduct.originalPrice;
            }
        } else {
            // Otherwise, either replace product or set quantity to 1
            console.log('Setting quantity to 1 (incrementQuantity = false)');

            // Replace all product properties, including the price and originalPrice
            cart[existingIndex] = {
                ...newProduct,
                quantity: 1
            };
        }
    } else {
        // Product doesn't exist in cart
        console.log('Product does not exist in cart, adding with quantity:', newProduct.quantity);

        // Fix image path if necessary
        if (newProduct.image && !newProduct.image.startsWith('http') && !newProduct.image.startsWith('/')) {
            newProduct.image = '/' + newProduct.image;
        }

        // Set fallback for image
        if (!newProduct.image || newProduct.image === 'undefined' || newProduct.image === '') {
            newProduct.image = '/Content/Images/products/beehive.jpg';
        }

        // Add product to cart
        cart.push(newProduct);
    }

    // Save updated cart
    try {
        localStorage.setItem('cart', JSON.stringify(cart));
        console.log('Cart saved successfully:', cart.map(x => ({ id: x.id, name: x.name, qty: x.quantity, price: x.price, originalPrice: x.originalPrice })));
    } catch (e) {
        console.error('Error saving cart to localStorage:', e);
    }

    // Update the cart badge
    updateCartBadge();

    // Return updated cart
    return cart;
}

/**
 * Remove a product from the cart
 * @param {string} productId - ID of product to remove
 */
function removeFromCart(productId) {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];

    console.log('Removing product with ID:', productId);

    // Remove product by ID
    cart = cart.filter(item => {
        console.log('Comparing product ID:', item.id, 'with', productId);
        return item.id !== productId;
    });

    console.log('Cart after removal:', cart);

    // Save the cart
    localStorage.setItem('cart', JSON.stringify(cart));

    // Update the cart badge
    updateCartBadge();
}

/**
 * Clear the entire cart
 */
function clearCart() {
    // Clear the cart in localStorage
    localStorage.setItem('cart', JSON.stringify([]));

    // Update the cart badge
    updateCartBadge();
}

/**
 * Update the quantity of a product in the cart
 * @param {string} productId - ID of product to update
 * @param {number} quantity - New quantity
 */
function updateCartItemQuantity(productId, quantity) {
    // Asigură-te că cantitatea este un număr valid
    quantity = parseInt(quantity);
    if (isNaN(quantity) || quantity < 1) {
        quantity = 1;
    }

    console.log('Updating quantity for product ID:', productId, 'to', quantity);

    // Obține coșul din localStorage
    const cart = JSON.parse(localStorage.getItem('cart')) || [];

    // Găsește produsul după ID
    const index = cart.findIndex(item => String(item.id) === String(productId));

    if (index > -1) {
        // Actualizează cantitatea
        cart[index].quantity = quantity;

        // Asigură-te că prețul are întotdeauna două zecimale
        cart[index].price = parseFloat(parseFloat(cart[index].price).toFixed(2));
        if (cart[index].originalPrice) {
            cart[index].originalPrice = parseFloat(parseFloat(cart[index].originalPrice).toFixed(2));
        }

        // Salvează coșul actualizat
        localStorage.setItem('cart', JSON.stringify(cart));

        // Actualizează badge-ul coșului
        updateCartBadge();

        // Actualizează sumarul din pagina coșului dacă există
        if (document.querySelector('.cart-subtotal')) {
            renderCartItems();
        }
    }
}

/**
 * Update the cart badge with the current number of items
 */
function updateCartBadge() {
    const cart = JSON.parse(localStorage.getItem('cart')) || [];
    const cartBadge = document.querySelector('.cart-badge');

    if (cartBadge) {
        // Calculate total items
        const totalItems = cart.reduce((total, item) => total + item.quantity, 0);

        // Update badge
        cartBadge.textContent = totalItems.toString();

        // Show or hide badge
        if (totalItems > 0) {
            cartBadge.classList.remove('d-none');
        } else {
            cartBadge.classList.add('d-none');
        }
    }
}

/**
 * Render cart items in the cart page
 */
function renderCartItems() {
    const cartItemsContainer = document.querySelector('.cart-items');
    if (!cartItemsContainer) return;

    // Get cart from localStorage
    const cart = JSON.parse(localStorage.getItem('cart')) || [];

    // Check if cart is empty
    if (cart.length === 0) {
        cartItemsContainer.innerHTML = `
            <div class="alert alert-info text-center">
                <i class="fas fa-shopping-cart fa-3x mb-3"></i>
                <h4>Your cart is empty</h4>
                <p>Looks like you haven't added any products to your cart yet.</p>
                <a href="/Home/Products" class="btn btn-primary mt-3">Continue Shopping</a>
            </div>
        `;
        return;
    }

    // Generate cart items HTML
    let cartItemsHTML = '';

    cart.forEach(item => {
        // Ensure we have the correct price values with proper decimal places
        const itemPrice = parseFloat(parseFloat(item.price).toFixed(2)) || 0;
        const itemOriginalPrice = item.originalPrice ? parseFloat(parseFloat(item.originalPrice).toFixed(2)) : null;
        const itemQuantity = parseInt(item.quantity) || 1;
        const itemTotal = parseFloat((itemPrice * itemQuantity).toFixed(2));
        const hasPromotionalPrice = itemOriginalPrice && itemOriginalPrice > itemPrice;

        console.log('Rendering cart item:', {
            id: item.id,
            name: item.name,
            price: itemPrice,
            originalPrice: itemOriginalPrice,
            hasPromotion: hasPromotionalPrice,
            total: itemTotal
        });

        cartItemsHTML += `
            <div class="card mb-3 cart-item" data-product-id="${item.id}">
                <div class="card-body">
                    <div class="row align-items-center">
                        <div class="col-lg-2 col-md-2 col-sm-4 text-center">
                            <img src="${item.image || '/Content/Images/products/beehive.jpg'}" 
                                 alt="${item.name}" 
                                 class="img-fluid rounded" 
                                 style="max-height: 80px; max-width: 80px;">
                        </div>
                        <div class="col-lg-4 col-md-4 col-sm-8">
                            <h5 class="mb-1">${item.name}</h5>
                            <p class="text-muted small mb-0">Product ID: ${item.id}</p>
                        </div>
                        <div class="col-lg-2 col-md-2 col-sm-4 mt-3 mt-sm-0">
                            <div class="quantity-control">
                                <button class="btn btn-sm btn-outline-secondary decrease-btn">-</button>
                                <input type="number" class="form-control mx-2 quantity-input" value="${itemQuantity}" min="1" max="99">
                                <button class="btn btn-sm btn-outline-secondary increase-btn">+</button>
                            </div>
                        </div>
                        <div class="col-lg-2 col-md-2 col-sm-4 mt-3 mt-sm-0 text-center">
                            <p class="mb-0">Price</p>
                            <p class="mb-0 fw-bold">${formatCurrency(itemPrice)}</p>
                            ${hasPromotionalPrice ?
                `<small class="text-muted text-decoration-line-through">${formatCurrency(itemOriginalPrice)}</small>` : ''}
                        </div>
                        <div class="col-lg-2 col-md-2 col-sm-4 mt-3 mt-sm-0 text-end">
                            <p class="mb-0">Total</p>
                            <p class="mb-0 fw-bold">${formatCurrency(itemTotal)}</p>
                            ${hasPromotionalPrice ?
                `<small class="text-muted text-decoration-line-through">${formatCurrency(itemOriginalPrice * itemQuantity)}</small>` : ''}
                            <button class="btn btn-link text-danger p-0 mt-2 delete-item">
                                <i class="fas fa-trash me-1"></i> Remove
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    });

    // Update cart items container
    cartItemsContainer.innerHTML = cartItemsHTML;

    // Update quantity input event listeners
    const quantityInputs = document.querySelectorAll('.quantity-input');
    quantityInputs.forEach(input => {
        input.addEventListener('change', function () {
            const cartItem = this.closest('.cart-item');
            if (!cartItem) return;

            const productId = cartItem.getAttribute('data-product-id');
            const quantity = parseInt(this.value);

            if (isNaN(quantity) || quantity < 1) {
                this.value = 1;
                updateCartItemQuantity(productId, 1);
            } else {
                updateCartItemQuantity(productId, quantity);
            }
        });
    });

    // Update increment buttons
    const increaseButtons = document.querySelectorAll('.increase-btn');
    increaseButtons.forEach(button => {
        button.addEventListener('click', function () {
            const cartItem = this.closest('.cart-item');
            if (!cartItem) return;

            const productId = cartItem.getAttribute('data-product-id');
            const quantityInput = cartItem.querySelector('.quantity-input');
            const currentQuantity = parseInt(quantityInput.value);
            const newQuantity = currentQuantity + 1;

            quantityInput.value = newQuantity;
            updateCartItemQuantity(productId, newQuantity);
        });
    });

    // Update decrement buttons
    const decreaseButtons = document.querySelectorAll('.decrease-btn');
    decreaseButtons.forEach(button => {
        button.addEventListener('click', function () {
            const cartItem = this.closest('.cart-item');
            if (!cartItem) return;

            const productId = cartItem.getAttribute('data-product-id');
            const quantityInput = cartItem.querySelector('.quantity-input');
            const currentQuantity = parseInt(quantityInput.value);

            if (currentQuantity > 1) {
                const newQuantity = currentQuantity - 1;
                quantityInput.value = newQuantity;
                updateCartItemQuantity(productId, newQuantity);
            }
        });
    });

    // Update delete buttons
    const deleteButtons = document.querySelectorAll('.delete-item');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function () {
            const cartItem = this.closest('.cart-item');
            if (!cartItem) return;

            const productId = cartItem.getAttribute('data-product-id');
            const productName = cartItem.querySelector('h5').textContent;

            if (confirm(`Remove ${productName} from your cart?`)) {
                removeFromCart(productId);
                renderCartItems();
            }
        });
    });

    // Calculate totals
    const subtotal = cart.reduce((total, item) => total + (item.price * item.quantity), 0);
    const shipping = subtotal > 500 ? 0 : 50;
    const total = subtotal + shipping;

    // Update totals
    const subtotalEl = document.querySelector('.cart-subtotal');
    const shippingEl = document.querySelector('.cart-shipping');
    const totalEl = document.querySelector('.cart-total');

    if (subtotalEl) subtotalEl.textContent = formatCurrency(subtotal);
    if (shippingEl) shippingEl.textContent = shipping === 0 ? 'FREE' : formatCurrency(shipping);
    if (totalEl) totalEl.textContent = formatCurrency(total);

    // Dispatch an event indicating that the cart was rendered
    // This is useful for other scripts that need to hook into cart rendering
    const event = new CustomEvent('cartRendered');
    document.dispatchEvent(event);
}

/**
 * Synchronize cart items with current promotional prices from the server
 * This ensures that if a promotional price changes on the server, the cart reflects it
 */
function syncCartPromotionalPrices() {
    console.log('Synchronizing cart with current promotional prices...');

    // Only run this on the cart page
    if (!document.querySelector('.cart-items')) {
        return;
    }

    // Make an AJAX call to get current promotional prices
    fetch('/api/promotions/current', {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to fetch promotional prices');
            }
            return response.json();
        })
        .then(data => {
            console.log('Received promotional data:', data);

            // Get cart from localStorage
            const cart = JSON.parse(localStorage.getItem('cart')) || [];
            let hasChanges = false;

            // Update cart items with current promotional prices
            cart.forEach(item => {
                const productId = parseInt(item.id);
                if (isNaN(productId)) return;

                const promotionInfo = data.find(p => p.productId === productId);
                if (promotionInfo) {
                    // Debugging: Log exact values from API and localStorage
                    console.log('DEBUGGING PROMOTIONAL PRICES:');
                    console.log('Item in cart:', JSON.stringify(item));
                    console.log('Promotion info from API:', JSON.stringify(promotionInfo));
                    console.log('Actual values to compare:');
                    console.log('Current price in cart (stored):', typeof item.price, item.price);
                    console.log('Current price as parsed float:', typeof parseFloat(item.price), parseFloat(item.price));
                    console.log('Current price with 2 decimals:', typeof parseFloat(parseFloat(item.price).toFixed(2)), parseFloat(parseFloat(item.price).toFixed(2)));
                    console.log('New promotional price from API:', typeof promotionInfo.promotionalPrice, promotionInfo.promotionalPrice);
                    console.log('New promotional price as parsed float:', typeof parseFloat(promotionInfo.promotionalPrice), parseFloat(promotionInfo.promotionalPrice));
                    console.log('New promotional price with 2 decimals:', typeof parseFloat(parseFloat(promotionInfo.promotionalPrice).toFixed(2)), parseFloat(parseFloat(promotionInfo.promotionalPrice).toFixed(2)));

                    // Asigură-te că prețul este format ca decimal corect (pentru comparații precise)
                    const currentPrice = parseFloat(parseFloat(item.price).toFixed(2));
                    const newPromotionalPrice = parseFloat(parseFloat(promotionInfo.promotionalPrice).toFixed(2));

                    console.log('Comparison result:', newPromotionalPrice < currentPrice);
                    console.log('Prices are different:', Math.abs(newPromotionalPrice - currentPrice) > 0.01);

                    // Verifică dacă prețurile sunt diferite folosind o toleranță mică pentru comparații cu virgulă mobilă
                    // Am eliminat verificarea specifică pentru "Miere"
                    if (Math.abs(newPromotionalPrice - currentPrice) > 0.01) {
                        console.log(`Updating price for ${item.name} from ${currentPrice} to ${newPromotionalPrice}`);

                        // Update with promotional price
                        item.originalPrice = parseFloat(promotionInfo.originalPrice);
                        item.price = newPromotionalPrice;
                        hasChanges = true;
                    } else {
                        console.log(`Prețurile pentru ${item.name} sunt aproximativ egale (diferență: ${Math.abs(newPromotionalPrice - currentPrice)}), nu actualizăm`);
                    }
                }
            });

            // If changes were made, save cart and re-render
            if (hasChanges) {
                // Salvăm modificările în localStorage
                localStorage.setItem('cart', JSON.stringify(cart));
                console.log('[API SYNC] Prețurile au fost actualizate, renderizăm coșul');

                // Reafișăm coșul fără a reîncărca pagina
                renderCartItems();
            } else {
                console.log('[API SYNC] Nu au fost făcute modificări la prețuri');
                // Chiar dacă nu au fost făcute modificări, renderizăm coșul pentru a fi siguri
                renderCartItems();
            }
        })
        .catch(error => {
            console.error('Error fetching promotional prices:', error);
        });
}

// Add event listener to synchronize cart on page load
document.addEventListener('DOMContentLoaded', function () {
    // Only try to sync if we're on the cart page
    if (document.querySelector('.cart-items')) {
        // Call syncCartPromotionalPrices after a short delay to ensure the page is loaded
        setTimeout(syncCartPromotionalPrices, 500);
    }
});

// Helper Functions

/**
 * Show a notification toast
 * @param {string} message - Message to display
 * @param {string} type - Type of notification (success, danger, warning, info)
 * @param {number} duration - How long to show the toast in milliseconds
 */
function showNotification(message, type = 'success', duration = 3000) {
    // Create toast container if it doesn't exist
    let toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }

    // Create the toast element
    const toastEl = document.createElement('div');
    toastEl.className = `toast align-items-center text-white bg-${type} border-0`;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');

    // Allow HTML in messages
    // Create toast content
    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    // Add toast to container
    toastContainer.appendChild(toastEl);

    // Initialize toast
    const toast = new bootstrap.Toast(toastEl, {
        autohide: true,
        delay: duration
    });

    // Show toast
    toast.show();

    // Remove the toast element after it's hidden
    toastEl.addEventListener('hidden.bs.toast', function () {
        toastEl.remove();
    });
}

/**
 * Validate an email address
 * @param {string} email - Email to validate
 * @returns {boolean} - Whether the email is valid
 */
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

/**
 * Format currency based on locale
 * @param {number} price - Price to format
 * @param {string} locale - Locale for formatting (default: 'ro-MD')
 * @param {string} currency - Currency code (default: 'MDL')
 * @returns {string} - Formatted price
 */
function formatCurrency(price, locale = 'ro-MD', currency = 'MDL') {
    // Asigură-te că prețul are întotdeauna două zecimale
    price = parseFloat(parseFloat(price).toFixed(2));

    // Format the price using Intl
    let formatted = new Intl.NumberFormat(locale, {
        style: 'currency',
        currency: currency,
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(price);

    // Ensure the currency is MDL, even if the browser changes it
    if (!formatted.includes('MDL')) {
        // If the browser ignores our locale/currency settings,
        // manually format it by replacing the currency symbol with MDL
        formatted = formatted.replace(/[$€£¥]/g, '') + ' MDL';
    }

    return formatted;
}

/**
 * Debug function to inspect the cart state
 */
function debugCartPrices() {
    try {
        console.group('CART PRICE DEBUG');
        const cart = JSON.parse(localStorage.getItem('cart')) || [];

        if (cart.length === 0) {
            console.log('Cart is empty');
            console.groupEnd();
            return;
        }

        console.log('Cart items found:', cart.length);

        cart.forEach(item => {
            console.group(`Item: ${item.name} (ID: ${item.id})`);

            // Inspect price values
            console.log('Raw price value:', item.price, typeof item.price);
            console.log('Parsed price:', parseFloat(item.price), typeof parseFloat(item.price));
            console.log('Formatted price (2 decimals):', parseFloat(parseFloat(item.price).toFixed(2)));

            if (item.originalPrice) {
                console.log('Raw original price:', item.originalPrice, typeof item.originalPrice);
                console.log('Parsed original price:', parseFloat(item.originalPrice), typeof parseFloat(item.originalPrice));
                console.log('Formatted original price (2 decimals):', parseFloat(parseFloat(item.originalPrice).toFixed(2)));

                // Check discount calculation
                const percentDiscount = (1 - (parseFloat(item.price) / parseFloat(item.originalPrice))) * 100;
                console.log('Calculated discount:', percentDiscount.toFixed(2) + '%');
            }

            console.groupEnd();
        });

        console.groupEnd();
    } catch (error) {
        console.error('Error in debugCartPrices:', error);
    }
}

// Run the debug function when the page loads
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(debugCartPrices, 1000);
});
