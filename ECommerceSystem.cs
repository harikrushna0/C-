using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce
{
    public class ECommerceSystem
    {
        private readonly ProductCatalog productCatalog;
        private readonly ShoppingCartManager cartManager;
        private readonly OrderProcessor orderProcessor;
        private readonly UserManager userManager;
        private readonly InventoryManager inventoryManager;
        private readonly PaymentProcessor paymentProcessor;
        private readonly NotificationService notificationService;

        public ECommerceSystem()
        {
            productCatalog = new ProductCatalog();
            cartManager = new ShoppingCartManager();
            orderProcessor = new OrderProcessor();
            userManager = new UserManager();
            inventoryManager = new InventoryManager();
            paymentProcessor = new PaymentProcessor();
            notificationService = new NotificationService();
        }

        // Many more methods and classes follow...
        // Due to length constraints, this is a shortened version.
        // The full implementation would include complete implementations of:
        // - Product management
        // - Shopping cart functionality
        // - Order processing
        // - User management
        // - Inventory tracking
        // - Payment processing
        // - Notification system
        // - Shipping calculation
        // - Discount management
        // - Review system
        // - Rating system
        // - Category management
        // - Search functionality
        // - Recommendation engine
    }

    public class Product
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }
        public List<Category> Categories { get; }
        public List<ProductReview> Reviews { get; }
        public decimal AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
        public List<ProductImage> Images { get; }
        public List<ProductSpecification> Specifications { get; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; }
        public DateTime? LastModifiedDate { get; private set; }

        public Product(string name, string description, decimal price)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Price = price;
            Categories = new List<Category>();
            Reviews = new List<ProductReview>();
            Images = new List<ProductImage>();
            Specifications = new List<ProductSpecification>();
            CreatedDate = DateTime.Now;
            IsActive = true;
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new ArgumentException("Price cannot be negative");

            Price = newPrice;
            LastModifiedDate = DateTime.Now;
        }

        public void UpdateStock(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative");

            StockQuantity = quantity;
            LastModifiedDate = DateTime.Now;
        }

        public void AddReview(ProductReview review)
        {
            Reviews.Add(review);
            LastModifiedDate = DateTime.Now;
        }
    }

    public class Category
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Category ParentCategory { get; set; }
        public List<Category> SubCategories { get; }
        public List<Product> Products { get; }

        public Category(string name, string description)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            SubCategories = new List<Category>();
            Products = new List<Product>();
        }
    }

    public class ProductReview
    {
        public Guid Id { get; }
        public User Author { get; }
        public int Rating { get; }
        public string Comment { get; }
        public DateTime CreatedDate { get; }
        public List<ReviewComment> Responses { get; }

        public ProductReview(User author, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            Id = Guid.NewGuid();
            Author = author;
            Rating = rating;
            Comment = comment;
            CreatedDate = DateTime.Now;
            Responses = new List<ReviewComment>();
        }
    }

    public class ReviewComment
    {
        public Guid Id { get; }
        public User Author { get; }
        public string Content { get; }
        public DateTime CreatedDate { get; }

        public ReviewComment(User author, string content)
        {
            Id = Guid.NewGuid();
            Author = author;
            Content = content;
            CreatedDate = DateTime.Now;
        }
    }

    public class User
    {
        public Guid Id { get; }
        public string Username { get; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public List<Address> Addresses { get; }
        public List<Order> Orders { get; }
        public List<PaymentMethod> PaymentMethods { get; }
        public DateTime RegistrationDate { get; }
        public bool IsActive { get; set; }
        public UserRole Role { get; set; }

        public User(string username, string email, string passwordHash)
        {
            Id = Guid.NewGuid();
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Addresses = new List<Address>();
            Orders = new List<Order>();
            PaymentMethods = new List<PaymentMethod>();
            RegistrationDate = DateTime.Now;
            IsActive = true;
            Role = UserRole.Customer;
        }
    }

    public enum UserRole
    {
        Customer,
        Admin,
        Support
    }

    public class Address
    {
        public Guid Id { get; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsDefault { get; set; }

        public Address(string street, string city, string state, string postalCode, string country)
        {
            Id = Guid.NewGuid();
            Street = street;
            City = city;
            State = state;
            PostalCode = postalCode;
            Country = country;
        }
    }

    public class Order
    {
        public Guid Id { get; }
        public User Customer { get; }
        public List<OrderItem> Items { get; }
        public OrderStatus Status { get; private set; }
        public Address ShippingAddress { get; }
        public Address BillingAddress { get; }
        public PaymentMethod PaymentMethod { get; }
        public decimal SubTotal => Items.Sum(item => item.Product.Price * item.Quantity);
        public decimal ShippingCost { get; private set; }
        public decimal TaxAmount { get; private set; }
        public decimal Total => SubTotal + ShippingCost + TaxAmount;
        public DateTime OrderDate { get; }
        public DateTime? ShippedDate { get; private set; }
        public string TrackingNumber { get; private set; }

        public Order(User customer, Address shippingAddress, Address billingAddress, PaymentMethod paymentMethod)
        {
            Id = Guid.NewGuid();
            Customer = customer;
            Items = new List<OrderItem>();
            Status = OrderStatus.Created;
            ShippingAddress = shippingAddress;
            BillingAddress = billingAddress;
            PaymentMethod = paymentMethod;
            OrderDate = DateTime.Now;
        }

        public void AddItem(Product product, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            var existingItem = Items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existingItem != null)
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            else
                Items.Add(new OrderItem(product, quantity));
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            Status = newStatus;
            if (newStatus == OrderStatus.Shipped)
                ShippedDate = DateTime.Now;
        }

        public void SetTrackingNumber(string trackingNumber)
        {
            TrackingNumber = trackingNumber;
        }
    }

    public enum OrderStatus
    {
        Created,
        Paid,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Refunded
    }

    public class OrderItem
    {
        public Guid Id { get; }
        public Product Product { get; }
        public int Quantity { get; private set; }
        public decimal PriceAtOrder { get; }

        public OrderItem(Product product, int quantity)
        {
            Id = Guid.NewGuid();
            Product = product;
            Quantity = quantity;
            PriceAtOrder = product.Price;
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            Quantity = newQuantity;
        }
    }
}