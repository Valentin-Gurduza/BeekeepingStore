﻿/**
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

                const productId = this.getAttribute('data-product-id');
                const productName = this.getAttribute('data-product-name');
                const productPrice = this.getAttribute('data-product-price') || '0';
                const productImage = this.getAttribute('data-product-image') || '';

                // Animation for the button
                this.innerHTML = '<i class="fas fa-check"></i> Added';
                this.classList.add('btn-success');

                // Add the product to cart
                addToCart({
                    id: productId || productName, // Fall back to name if ID not set
                    name: productName,
                    price: parseFloat(productPrice),
                    image: productImage,
                    quantity: 1
                });

                // Display notification
                showNotification(`${productName} has been added to your cart. <a href="/Home/Cart" class="text-white fw-bold"><u>View Cart</u></a>`, 'success', 5000);

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
 */
function addToCart(product) {
    const cart = JSON.parse(localStorage.getItem('cart')) || [];

    // Asigură-te că produsul are un ID unic
    if (!product.id && product.name) {
        product.id = product.name; // Folosește numele ca ID dacă nu există
    }

    // Generează un ID bazat pe timestamp dacă nici ID și nici nume nu există
    if (!product.id) {
        product.id = 'product_' + Date.now();
    }

    // Log product data for debugging
    console.log('Adding product to cart:', product);
    console.log('Image URL:', product.image);
    console.log('Product ID:', product.id);

    // Găsește produsul în coș după ID
    const existingProductIndex = cart.findIndex(item => item.id === product.id);

    if (existingProductIndex > -1) {
        // Dacă produsul există deja, crește cantitatea
        cart[existingProductIndex].quantity += product.quantity;
        console.log('Increased quantity for existing product to', cart[existingProductIndex].quantity);
    } else {
        // Make sure image path is correct
        if (product.image && !product.image.startsWith('http') && !product.image.startsWith('/')) {
            // If the image path is relative but doesn't start with a slash, add one
            product.image = '/' + product.image;
        }

        // Adaugă produsul nou în coș
        cart.push(product);
        console.log('Added new product to cart');
    }

    // Save the cart
    localStorage.setItem('cart', JSON.stringify(cart));

    // Log for debugging
    console.log('Cart updated:', cart);
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

    const cart = JSON.parse(localStorage.getItem('cart')) || [];

    console.log('Rendering cart items:', cart);

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

        // Hide summary
        if (cartSummaryContainer) {
            cartSummaryContainer.classList.add('d-none');
        }
    } else {
        // Show cart items
        let cartItemsHTML = '';

        cart.forEach(item => {
            // Asigură-te că fiecare produs are un ID valid
            if (!item.id && item.name) {
                item.id = item.name;
            }

            if (!item.id) {
                item.id = 'product_' + Date.now();
            }

            console.log('Rendering product:', item.name, 'with ID:', item.id);

            // Ensure price is displayed with MDL currency
            const formattedPrice = formatCurrency(item.price);
            const formattedTotal = formatCurrency(item.price * item.quantity);

            cartItemsHTML += `
                <div class="card mb-3 cart-item" data-product-id="${item.id}">
                    <div class="card-body">
                        <div class="row align-items-center">
                            <div class="col-lg-2 col-md-3 col-sm-3">
                                <img src="${item.image || '/Content/Images/products/default-product.png'}" 
                                     class="img-fluid rounded" 
                                     alt="${item.name}"
                                     onerror="this.src='/Content/Images/products/default-product.png'"
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
            const subtotal = cart.reduce((total, item) => total + (item.price * item.quantity), 0);

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
