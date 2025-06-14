// MarketPlace Pro - Site JavaScript

// Initialize when DOM is loaded
document.addEventListener("DOMContentLoaded", () => {
    initializeComponents()
    initializeFormValidation()
    initializeTooltips()
    initializeConfirmDialogs()
})

// Initialize all components
function initializeComponents() {
    // Initialize Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map((tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl))

    // Initialize Bootstrap popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
    var popoverList = popoverTriggerList.map((popoverTriggerEl) => new bootstrap.Popover(popoverTriggerEl))

    // Auto-hide alerts after 5 seconds
    setTimeout(() => {
        var alerts = document.querySelectorAll(".alert-dismissible")
        alerts.forEach((alert) => {
            var bsAlert = new bootstrap.Alert(alert)
            bsAlert.close()
        })
    }, 5000)
}

// Form validation enhancements
function initializeFormValidation() {
    // Add custom validation styles
    var forms = document.querySelectorAll(".needs-validation")
    Array.prototype.slice.call(forms).forEach((form) => {
        form.addEventListener(
            "submit",
            (event) => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                }
                form.classList.add("was-validated")
            },
            false,
        )
    })

    // Password strength indicator
    var passwordInputs = document.querySelectorAll('input[type="password"]')
    passwordInputs.forEach((input) => {
        if (input.name === "Password") {
            input.addEventListener("input", function () {
                checkPasswordStrength(this)
            })
        }
    })
}

// Initialize tooltips
function initializeTooltips() {
    // Add tooltips to buttons and icons
    var elements = document.querySelectorAll("[title]")
    elements.forEach((element) => {
        element.setAttribute("data-bs-toggle", "tooltip")
        element.setAttribute("data-bs-placement", "top")
    })
}

// Initialize confirmation dialogs
function initializeConfirmDialogs() {
    // Add confirmation to delete buttons
    var deleteButtons = document.querySelectorAll(".btn-danger, [data-confirm]")
    deleteButtons.forEach((button) => {
        button.addEventListener("click", function (e) {
            var message = this.getAttribute("data-confirm") || "Are you sure you want to delete this item?"
            if (!confirm(message)) {
                e.preventDefault()
                return false
            }
        })
    })
}

// Password strength checker
function checkPasswordStrength(input) {
    var password = input.value
    var strength = 0
    var feedback = ""

    // Check password criteria
    if (password.length >= 8) strength++
    if (password.match(/[a-z]/)) strength++
    if (password.match(/[A-Z]/)) strength++
    if (password.match(/[0-9]/)) strength++
    if (password.match(/[^a-zA-Z0-9]/)) strength++

    // Create or update strength indicator
    var strengthIndicator = document.getElementById("password-strength")
    if (!strengthIndicator) {
        strengthIndicator = document.createElement("div")
        strengthIndicator.id = "password-strength"
        strengthIndicator.className = "password-strength mt-2"
        input.parentNode.appendChild(strengthIndicator)
    }

    // Update strength display
    switch (strength) {
        case 0:
        case 1:
            strengthIndicator.innerHTML =
                '<div class="progress"><div class="progress-bar bg-danger" style="width: 20%"></div></div><small class="text-danger">Weak password</small>'
            break
        case 2:
        case 3:
            strengthIndicator.innerHTML =
                '<div class="progress"><div class="progress-bar bg-warning" style="width: 60%"></div></div><small class="text-warning">Medium password</small>'
            break
        case 4:
        case 5:
            strengthIndicator.innerHTML =
                '<div class="progress"><div class="progress-bar bg-success" style="width: 100%"></div></div><small class="text-success">Strong password</small>'
            break
    }
}

// Shopping cart functionality
var Cart = {
    // Add item to cart with animation
    addItem: (productId, quantity) => {
        var button = event.target
        var originalText = button.innerHTML

        // Show loading state
        button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Adding...'
        button.disabled = true

        // Simulate API call delay
        setTimeout(() => {
            button.innerHTML = '<i class="fas fa-check"></i> Added!'
            button.classList.remove("btn-primary")
            button.classList.add("btn-success")

            // Reset button after 2 seconds
            setTimeout(() => {
                button.innerHTML = originalText
                button.classList.remove("btn-success")
                button.classList.add("btn-primary")
                button.disabled = false
            }, 2000)
        }, 500)
    },

    // Update cart item quantity
    updateQuantity: function (productId, quantity) {
        if (quantity <= 0) {
            this.removeItem(productId)
            return
        }

        // Update quantity in UI
        var quantityInput = document.querySelector(`input[data-product-id="${productId}"]`)
        if (quantityInput) {
            quantityInput.value = quantity
        }

        // Recalculate totals
        this.calculateTotals()
    },

    // Remove item from cart
    removeItem: (productId) => {
        var row = document.querySelector(`tr[data-product-id="${productId}"]`)
        if (row) {
            row.style.transition = "opacity 0.3s ease"
            row.style.opacity = "0"
            setTimeout(() => {
                row.remove()
                Cart.calculateTotals()
            }, 300)
        }
    },

    // Calculate cart totals
    calculateTotals: () => {
        var subtotal = 0
        var rows = document.querySelectorAll(".cart-item-row")

        rows.forEach((row) => {
            var price = Number.parseFloat(row.querySelector(".item-price").textContent.replace("$", ""))
            var quantity = Number.parseInt(row.querySelector(".quantity-input").value)
            var itemTotal = price * quantity

            row.querySelector(".item-total").textContent = "$" + itemTotal.toFixed(2)
            subtotal += itemTotal
        })

        var tax = subtotal * 0.08 // 8% tax
        var total = subtotal + tax

        // Update totals in UI
        document.getElementById("cart-subtotal").textContent = "$" + subtotal.toFixed(2)
        document.getElementById("cart-tax").textContent = "$" + tax.toFixed(2)
        document.getElementById("cart-total").textContent = "$" + total.toFixed(2)
    },
}

// Product search and filtering
var ProductSearch = {
    // Initialize search functionality
    init: function () {
        var searchInput = document.getElementById("product-search")
        var categorySelect = document.getElementById("category-filter")

        if (searchInput) {
            searchInput.addEventListener("input", this.debounce(this.performSearch, 300))
        }

        if (categorySelect) {
            categorySelect.addEventListener("change", this.performSearch)
        }
    },

    // Debounce function to limit API calls
    debounce: (func, wait) => {
        var timeout
        return function executedFunction(...args) {
            var later = () => {
                clearTimeout(timeout)
                func(...args)
            }
            clearTimeout(timeout)
            timeout = setTimeout(later, wait)
        }
    },

    // Perform search
    performSearch: () => {
        var searchTerm = document.getElementById("product-search")?.value || ""
        var categoryId = document.getElementById("category-filter")?.value || ""

        // Update URL with search parameters
        var url = new URL(window.location)
        url.searchParams.set("searchTerm", searchTerm)
        url.searchParams.set("categoryId", categoryId)

        // Reload page with new parameters
        window.location.href = url.toString()
    },
}

// Dashboard functionality
var Dashboard = {
    // Initialize dashboard components
    init: function () {
        this.initializeCharts()
        this.initializeDataTables()
        this.setupRealTimeUpdates()
    },

    // Initialize charts (placeholder for chart library integration)
    initializeCharts: () => {
        // This would integrate with Chart.js or similar library
        console.log("Charts initialized")
    },

    // Initialize data tables with sorting and pagination
    initializeDataTables: () => {
        var tables = document.querySelectorAll(".data-table")
        tables.forEach((table) => {
            // Add sorting functionality
            var headers = table.querySelectorAll("th[data-sortable]")
            headers.forEach((header) => {
                header.style.cursor = "pointer"
                header.addEventListener("click", function () {
                    Dashboard.sortTable(table, this)
                })
            })
        })
    },

    // Sort table by column
    sortTable: (table, header) => {
        var column = Array.from(header.parentNode.children).indexOf(header)
        var rows = Array.from(table.querySelectorAll("tbody tr"))
        var ascending = !header.classList.contains("sort-asc")

        // Remove existing sort classes
        table.querySelectorAll("th").forEach((th) => {
            th.classList.remove("sort-asc", "sort-desc")
        })

        // Add sort class to current header
        header.classList.add(ascending ? "sort-asc" : "sort-desc")

        // Sort rows
        rows.sort((a, b) => {
            var aVal = a.children[column].textContent.trim()
            var bVal = b.children[column].textContent.trim()

            // Try to parse as numbers
            var aNum = Number.parseFloat(aVal.replace(/[^0-9.-]/g, ""))
            var bNum = Number.parseFloat(bVal.replace(/[^0-9.-]/g, ""))

            if (!isNaN(aNum) && !isNaN(bNum)) {
                return ascending ? aNum - bNum : bNum - aNum
            } else {
                return ascending ? aVal.localeCompare(bVal) : bVal.localeCompare(aVal)
            }
        })

        // Reorder rows in table
        var tbody = table.querySelector("tbody")
        rows.forEach((row) => {
            tbody.appendChild(row)
        })
    },

    // Setup real-time updates (placeholder for SignalR or WebSocket integration)
    setupRealTimeUpdates: () => {
        // This would integrate with SignalR for real-time notifications
        console.log("Real-time updates initialized")
    },
}

// Utility functions
var Utils = {
    // Format currency
    formatCurrency: (amount) =>
        new Intl.NumberFormat("en-US", {
            style: "currency",
            currency: "USD",
        }).format(amount),

    // Format date
    formatDate: (date) =>
        new Intl.DateTimeFormat("en-US", {
            year: "numeric",
            month: "short",
            day: "numeric",
        }).format(new Date(date)),

    // Show loading spinner
    showLoading: (element) => {
        element.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Loading...'
        element.disabled = true
    },

    // Hide loading spinner
    hideLoading: (element, originalText) => {
        element.innerHTML = originalText
        element.disabled = false
    },

    // Show toast notification
    showToast: (message, type = "info") => {
        var toast = document.createElement("div")
        toast.className = `alert alert-${type} alert-dismissible fade show position-fixed`
        toast.style.top = "20px"
        toast.style.right = "20px"
        toast.style.zIndex = "9999"
        toast.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `

        document.body.appendChild(toast)

        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast)
            }
        }, 5000)
    },
}

// Initialize components when page loads
document.addEventListener("DOMContentLoaded", () => {
    ProductSearch.init()
    Dashboard.init()
})

// Export functions for global use
window.Cart = Cart
window.Dashboard = Dashboard
window.Utils = Utils
