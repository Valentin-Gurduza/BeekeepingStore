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
                const productImage = this.getAttribute('data-product-image') || '';

                // Animation for the button
                this.innerHTML = '<i class="fas fa-check"></i> Added';
                this.classList.add('btn-success');

                // Add the product to cart - aici folosim incrementQuantity=true (implicit)
                // pentru a permite creșterea cantității pentru produsele adăugate din alte pagini
                addToCart({
                    id: productId || productName, // Fall back to name if ID not set
                    name: productName,
                    price: parseFloat(productPrice),
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
        price: parseFloat(product.price) || 0,
        image: product.image || '',
        quantity: parseInt(product.quantity) || 1
    };

    // Get cart from localStorage
    let cart;
    try {
        cart = JSON.parse(localStorage.getItem('cart') || '[]');

        if (!Array.isArray(cart)) {
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
        } else {
            // Otherwise, either replace product or set quantity to 1
            console.log('Setting quantity to 1 (incrementQuantity = false)');

            // Replace all product properties, but keep the ID
            // and set quantity to 1 exactly
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
        console.log('Cart saved successfully:', cart.map(x => ({ id: x.id, name: x.name, qty: x.quantity })));
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
    const existingProductIndex = cart.findIndex(item => item.id === productId);

    console.log('Found product at index:', existingProductIndex);

    if (existingProductIndex > -1) {
        // Salvează cantitatea veche pentru comparație
        const oldQuantity = cart[existingProductIndex].quantity;
        console.log('Old quantity:', oldQuantity, 'New quantity:', quantity);

        // Actualizează cantitatea
        cart[existingProductIndex].quantity = quantity;

        // Elimină produsul dacă cantitatea este 0
        if (quantity <= 0) {
            cart.splice(existingProductIndex, 1);

            // Doar în acest caz este nevoie de un re-render complet
            localStorage.setItem('cart', JSON.stringify(cart));
            updateCartBadge();
            renderCartItems();
            return;
        }

        // Salvează coșul
        localStorage.setItem('cart', JSON.stringify(cart));

        // Actualizează badge-ul coșului
        updateCartBadge();

        // Actualizează prețul total pentru acest element fără a face un re-render complet
        const cartItem = document.querySelector(`.cart-item[data-product-id="${productId}"]`);
        if (cartItem) {
            const pricePerItem = cart[existingProductIndex].price;
            const totalPriceElement = cartItem.querySelector('.col-lg-2.col-md-2.col-sm-4.mt-3.mt-sm-0.text-end .fw-bold');
            if (totalPriceElement) {
                totalPriceElement.textContent = formatCurrency(pricePerItem * quantity);
            }
        }

        // Actualizează totalurile coșului
        const cartSummaryContainer = document.querySelector('.cart-summary');
        if (cartSummaryContainer) {
            // Calculează subtotalul
            const subtotal = cart.reduce((total, item) => total + (item.price * item.quantity), 0);

            // Calculează transportul (gratuit dacă subtotalul depășește 900 MDL)
            const shipping = subtotal > 900 ? 0 : 150;

            // Calculează totalul
            const total = subtotal + shipping;

            // Actualizează sumarul
            const subtotalEl = cartSummaryContainer.querySelector('.cart-subtotal');
            const shippingEl = cartSummaryContainer.querySelector('.cart-shipping');
            const totalEl = cartSummaryContainer.querySelector('.cart-total');

            if (subtotalEl) subtotalEl.textContent = formatCurrency(subtotal);
            if (shippingEl) shippingEl.textContent = shipping === 0 ? 'FREE' : formatCurrency(shipping);
            if (totalEl) totalEl.textContent = formatCurrency(total);
        }

        console.log('Cart updated successfully');
    } else {
        console.error('Product not found in cart! ID:', productId);
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
 * Render cart items on the cart page
 */
function renderCartItems() {
    const cartItemsContainer = document.querySelector('.cart-items');
    const cartSummaryContainer = document.querySelector('.cart-summary');

    if (!cartItemsContainer) return;

    // Obține coșul actualizat
    const cart = JSON.parse(localStorage.getItem('cart')) || [];

    // Abordare îmbunătățită: detectăm și corectăm doar duplicatele,
    // dar nu forțăm toate cantitățile la 1
    console.log('[RENDERFIX] Procesare coș pentru afișare, verificare duplicate');

    // Standardizează toate ID-urile pentru a preveni duplicatele
    const uniqueCart = [];
    const processedIds = new Set();

    // Consolidează produsele cu același ID, dar păstrează cantitățile setate de utilizator
    cart.forEach(item => {
        const itemId = String(item.id || '');

        // Verifică dacă acest ID a fost deja procesat
        if (itemId && processedIds.has(itemId)) {
            // Găsește indexul în coșul unic
            const existingIndex = uniqueCart.findIndex(x => String(x.id) === itemId);
            if (existingIndex !== -1) {
                // Păstrăm cantitatea mai mare dintre cele două
                // Acest lucru permite utilizatorului să seteze cantități mai mari de 1
                if (item.quantity > uniqueCart[existingIndex].quantity) {
                    console.log(`[RENDERFIX] Actualizare cantitate pentru ${item.name} de la ${uniqueCart[existingIndex].quantity} la ${item.quantity}`);
                    uniqueCart[existingIndex].quantity = item.quantity;
                }
            }
        } else if (itemId) {
            // Marchează ID-ul ca procesat
            processedIds.add(itemId);
            // Adaugă în coșul unic cu cantitatea originală
            uniqueCart.push({ ...item, id: itemId });
        } else {
            // Generează un ID pentru produsele fără ID
            const newId = 'product_' + Date.now() + '_' + Math.random().toString(36).substring(2, 11);
            uniqueCart.push({ ...item, id: newId });
        }
    });

    // Salvează coșul consolidat înapoi în localStorage doar dacă e nevoie
    if (uniqueCart.length !== cart.length) {
        console.log('Coșul a fost consolidat: de la', cart.length, 'la', uniqueCart.length, 'produse');
        localStorage.setItem('cart', JSON.stringify(uniqueCart));
    }

    console.log('Rendering cart items:', uniqueCart);

    if (uniqueCart.length === 0) {
        // Show empty cart message
        cartItemsContainer.innerHTML = `
            <div class="text-center py-5">
                <i class="fas fa-shopping-cart fa-4x text-muted mb-3"></i>
                <h3>Your cart is empty</h3>
                <p class="mb-4">Looks like you haven't added any products to your cart yet.</p>
                <a href="/Home/Products" class="btn btn-primary">Continue Shopping</a>
            </div>
        `;

        // Hide summary
        if (cartSummaryContainer) {
            cartSummaryContainer.classList.add('d-none');
        }
    } else {
        // Show cart items
        let cartItemsHTML = '';

        uniqueCart.forEach(item => {
            // Verifică imaginea și folosește una implicită dacă e nevoie
            if (!item.image || item.image === 'undefined' || item.image === '') {
                item.image = '/Content/Images/products/beehive.jpg';
            }

            console.log('Rendering product:', item.name, 'with ID:', item.id, 'and quantity:', item.quantity);

            // Ensure price is displayed with MDL currency
            const formattedPrice = formatCurrency(item.price);
            const formattedTotal = formatCurrency(item.price * item.quantity);

            cartItemsHTML += `
                <div class="card mb-3 cart-item" data-product-id="${item.id}">
                    <div class="card-body">
                        <div class="row align-items-center">
                            <div class="col-lg-2 col-md-3 col-sm-3">
                                <img src="${item.image}" 
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
                                           value="${item.quantity}" 
                                           readonly 
                                           style="background-color: #fff; pointer-events: none;">
                                    <button class="btn btn-sm btn-outline-secondary cart-plus-btn" type="button">+</button>
                                </div>
                            </div>
                            <div class="col-lg-2 col-md-2 col-sm-4 mt-3 mt-sm-0">
                                <div class="fw-bold">${formattedPrice}</div>
                            </div>
                            <div class="col-lg-2 col-md-2 col-sm-4 mt-3 mt-sm-0 text-end">
                                <div class="fw-bold">${formattedTotal}</div>
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

        // Show summary
        if (cartSummaryContainer) {
            cartSummaryContainer.classList.remove('d-none');

            // Calculate subtotal
            const subtotal = uniqueCart.reduce((total, item) => total + (item.price * item.quantity), 0);

            // Calculate shipping (free if over 900 MDL)
            const shipping = subtotal > 900 ? 0 : 150;

            // Calculate total
            const total = subtotal + shipping;

            // Update summary
            const subtotalEl = cartSummaryContainer.querySelector('.cart-subtotal');
            const shippingEl = cartSummaryContainer.querySelector('.cart-shipping');
            const totalEl = cartSummaryContainer.querySelector('.cart-total');

            if (subtotalEl) subtotalEl.textContent = formatCurrency(subtotal);
            if (shippingEl) shippingEl.textContent = shipping === 0 ? 'FREE' : formatCurrency(shipping);
            if (totalEl) totalEl.textContent = formatCurrency(total);
        }
    }

    // If there's a custom updateCartTotals function on the page, call it
    if (typeof window.updateCartTotals === 'function') {
        setTimeout(window.updateCartTotals, 300);
    }

    // Emit an event that cart has been rendered
    const cartRenderedEvent = new CustomEvent('cartRendered');
    document.dispatchEvent(cartRenderedEvent);

    // If there's a fix function, call it
    if (typeof window.fixCartButtons === 'function') {
        setTimeout(window.fixCartButtons, 100);
    }
}

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
    // Format the price using Intl
    let formatted = new Intl.NumberFormat(locale, {
        style: 'currency',
        currency: currency
    }).format(price);

    // Ensure the currency is MDL, even if the browser changes it
    if (!formatted.includes('MDL')) {
        // If the browser ignores our locale/currency settings,
        // manually format it by replacing the currency symbol with MDL
        formatted = formatted.replace(/[$€£¥]/g, '') + ' MDL';
    }

    return formatted;
}
