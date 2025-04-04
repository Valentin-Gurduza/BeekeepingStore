/**
 * Beekeeping Store - Main JavaScript file
 */

// Wait for the document to be ready
document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });

    // Add to cart functionality
    const addToCartButtons = document.querySelectorAll('.add-to-cart-btn');
    if (addToCartButtons.length > 0) {
        addToCartButtons.forEach(button => {
            button.addEventListener('click', function(e) {
                e.preventDefault();
                
                const productId = this.getAttribute('data-product-id');
                const productName = this.getAttribute('data-product-name');
                
                // Animation for the button
                this.innerHTML = '<i class="fas fa-check"></i> Added';
                this.classList.add('btn-success');
                
                // Display notification
                showNotification(`${productName} has been added to your cart.`);
                
                // Reset button after 2 seconds
                setTimeout(() => {
                    this.innerHTML = '<i class="fas fa-shopping-cart"></i> Add to Cart';
                    this.classList.remove('btn-success');
                }, 2000);
                
                // You would add AJAX call here to actually add the item to the cart
                // addToCart(productId);
            });
        });
    }
    
    // Product image gallery functionality
    const productMainImage = document.getElementById('product-main-image');
    const productThumbnails = document.querySelectorAll('.product-thumbnail');
    
    if (productMainImage && productThumbnails.length > 0) {
        productThumbnails.forEach(thumbnail => {
            thumbnail.addEventListener('click', function() {
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
        newsletterForm.addEventListener('submit', function(e) {
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
        searchForm.addEventListener('submit', function(e) {
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
        navbarToggler.addEventListener('click', function() {
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

// Helper Functions

/**
 * Show a notification toast
 * @param {string} message - Message to display
 * @param {string} type - Type of notification (success, danger, warning, info)
 */
function showNotification(message, type = 'success') {
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
        delay: 3000
    });
    
    // Show toast
    toast.show();
    
    // Remove the toast element after it's hidden
    toastEl.addEventListener('hidden.bs.toast', function() {
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
 * @param {string} locale - Locale for formatting (default: 'en-US')
 * @param {string} currency - Currency code (default: 'USD')
 * @returns {string} - Formatted price
 */
function formatCurrency(price, locale = 'en-US', currency = 'USD') {
    return new Intl.NumberFormat(locale, {
        style: 'currency',
        currency: currency
    }).format(price);
}
