# Marketplace-Pro

A full-stack online marketplace platform developed as a self learning project, designed to connect vendors and customers in a secure, role-based e-commerce environment.

## 🎯 Project Overview

This marketplace platform enables three distinct user roles:
- **Customers**: Browse products, manage cart, place orders, and track purchases
- **Vendors**: Manage product listings, inventory, and order fulfillment
- **Administrators**: Oversee users, vendors, products, and platform operations

## 🛠️ Technologies & Frameworks

### Backend
- **Framework**: ASP.NET Core 8.0+ MVC
- **Language**: C# 10+
- **Database**: MySQL 8.0+
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Identity (Custom Implementation)
- **Architecture**: MVC Pattern with Service Layer

### Frontend
- **Template Engine**: Razor Views (.cshtml)
- **Styling**: CSS3 with Flexbox/Grid
- **JavaScript**: Vanilla
- **UI Framework**: Bootstrap 5 (integrated)
- **Icons**: Font Awesome
- **Responsive Design**: Mobile-first approach

### Development Tools
- **IDE**: Visual Studio 2022
- **Package Manager**: NuGet
- **Version Control**: Git
- **Database Tools**: MySQL Workbench

## 📁 Project Structure

\`\`\`
MarketplaceProject/
├── Controllers/           # MVC Controllers
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── CustomerController.cs
│   ├── VendorController.cs
│   ├── AdminController.cs
│   └── CartController.cs
├── Models/               # Data Models
│   ├── User.cs
│   ├── Product.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── Cart.cs
├── Services/             # Business Logic Layer
│   ├── IUserService.cs
│   ├── UserService.cs
│   ├── IProductService.cs
│   ├── ProductService.cs
│   ├── IOrderService.cs
│   ├── OrderService.cs
│   ├── ICartService.cs
│   └── CartService.cs
├── ViewModels/           # View Models
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   └── CheckoutViewModel.cs
├── Views/                # Razor Views
│   ├── Home/
│   ├── Account/
│   ├── Customer/
│   ├── Vendor/
│   ├── Admin/
│   ├── Cart/
│   └── Shared/
├── Data/                 # Database Context
│   └── ApplicationDbContext.cs
├── wwwroot/              # Static Files
│   ├── css/
│   ├── js/
│   └── 
└── 
\`\`\`

## 🗄️ Database Schema

### Core Tables
- **Users**: User authentication and profile information
- **Products**: Product catalog with vendor relationships
- **Orders**: Order management and tracking
- **OrderItems**: Individual items within orders
- **Carts**: Shopping cart functionality

### Key Relationships
- Users → Products (One-to-Many: Vendor products)
- Users → Orders (One-to-Many: Customer orders)
- Orders → OrderItems (One-to-Many: Order details)
- Products → OrderItems (One-to-Many: Product sales)

## ✅ Implemented Features

### Authentication & Authorization
- [x] User registration and login
- [x] Role-based access control (Customer, Vendor, Admin)
- [x] Session management
- [x] Password hashing and security

### Product Management
- [x] Product CRUD operations
- [x] Category-based organization
- [x] Inventory tracking
- [x] Product search and filtering
- [x] Image upload support (backend ready)

### Order Management
- [x] Shopping cart functionality
- [x] Order placement and processing
- [x] Order status tracking
- [x] Order history for customers and vendors

### User Management
- [x] Customer dashboard
- [x] Vendor dashboard
- [x] Admin panel for user management
- [x] Profile management

### Database Operations
- [x] Entity Framework migrations
- [x] Seed data for testing
- [x] Database relationships and constraints

## 🚧 In Development

### Frontend Integration
- [ ] Product display on homepage (backend ready)
- [ ] Real-time cart updates
- [ ] Advanced search filters
- [ ] Product image display

### Additional Features
- [ ] Email notifications
- [ ] Payment gateway integration
- [ ] Advanced reporting for admin
- [ ] Vendor approval workflow
- [ ] Product reviews and ratings

## 🚀 Getting Started

### Prerequisites
- .NET 6.0 SDK or later
- MySQL Server 8.0+
- Visual Studio 2022 or VS Code

### Test Accounts
The application includes seeded test accounts:

**Admin Account:**
- Username: admin
- Password: Admin123

**Vendor Account:**
- Username: vendor1
- Password: Vendor123

**Customer Account:**
- Username: customer1
- Password: Customer123

## 📊 Current Status

### Completed Components
- ✅ Backend architecture and services
- ✅ Database schema and relationships
- ✅ Authentication system
- ✅ Role-based authorization
- ✅ Core business logic
- ✅ Razor views and UI structure

### Known Issues
- Some frontend features need backend integration
- Product images require file upload implementation

## 🔧 Configuration

### Database Configuration
- Ensure MySQL server is running
- Create database: `MarketplaceDB`
- Run migrations to create tables
- Seed data will be automatically inserted

## 📝 Development Notes

This project follows ASP.NET Core MVC best practices:
- Separation of concerns with service layer
- Repository pattern implementation
- Dependency injection
- Model validation
- Error handling and logging

## 🤝 Contributing

This is a self-initiated project for personal learning and growth in full-stack development. Contributions are welcome to improve features or enhance architecture.

## 📄 License

This project is developed for educational and personal learning purposes as part of a self-learning initiative.

## 📞 Support

For questions about this implementation, please refer to:
- ASP.NET Core Documentation
- Entity Framework Core Documentation
- MySQL Documentation

---

**Note**: This project is actively under development. Some features may not be fully integrated between frontend and backend components. The core architecture and business logic are complete and functional.
