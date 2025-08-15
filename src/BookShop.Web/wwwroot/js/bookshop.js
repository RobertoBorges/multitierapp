// bookshop.js - Client-side functionality for BookShop application

// Wait for the DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function () {
    // Initialize search functionality
    initializeSearch();
    
    // Initialize category filter
    initializeCategoryFilter();
    
    // Initialize add to cart buttons
    initializeAddToCart();
});

// Search functionality
function initializeSearch() {
    const searchForm = document.getElementById('search-form');
    const searchInput = document.getElementById('search-input');
    
    if (searchForm) {
        searchForm.addEventListener('submit', function (e) {
            if (searchInput.value.trim() === '') {
                e.preventDefault();
            }
        });
    }
}

// Category filter functionality
function initializeCategoryFilter() {
    const categoryLinks = document.querySelectorAll('.category-filter');
    
    if (categoryLinks) {
        categoryLinks.forEach(link => {
            link.addEventListener('click', function () {
                document.querySelectorAll('.category-filter').forEach(item => {
                    item.classList.remove('active');
                });
                this.classList.add('active');
            });
        });
    }
}

// Add to cart functionality
function initializeAddToCart() {
    const addToCartButtons = document.querySelectorAll('.add-to-cart');
    
    if (addToCartButtons) {
        addToCartButtons.forEach(button => {
            button.addEventListener('click', async function (e) {
                e.preventDefault();
                const bookId = this.getAttribute('data-book-id');
                
                try {
                    const response = await fetch('/api/cart', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({ bookId: bookId, quantity: 1 })
                    });
                    
                    if (response.ok) {
                        showNotification('Book added to cart successfully!', 'success');
                        updateCartCount();
                    } else {
                        showNotification('Failed to add book to cart', 'error');
                    }
                } catch (error) {
                    showNotification('An error occurred', 'error');
                    console.error('Error:', error);
                }
            });
        });
    }
}

// Show notification
function showNotification(message, type) {
    const notificationContainer = document.getElementById('notification-container');
    
    if (!notificationContainer) {
        // Create notification container if it doesn't exist
        const container = document.createElement('div');
        container.id = 'notification-container';
        container.style.position = 'fixed';
        container.style.top = '20px';
        container.style.right = '20px';
        container.style.zIndex = '1000';
        document.body.appendChild(container);
    }
    
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show`;
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    // Add to container
    document.getElementById('notification-container').appendChild(notification);
    
    // Auto dismiss after 3 seconds
    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => {
            notification.remove();
        }, 300);
    }, 3000);
}

// Update cart count in the navigation
function updateCartCount() {
    fetch('/api/cart/count')
        .then(response => response.json())
        .then(data => {
            const cartCountElement = document.getElementById('cart-count');
            if (cartCountElement) {
                cartCountElement.textContent = data.count;
                
                if (data.count > 0) {
                    cartCountElement.classList.remove('d-none');
                } else {
                    cartCountElement.classList.add('d-none');
                }
            }
        })
        .catch(error => console.error('Error fetching cart count:', error));
}
