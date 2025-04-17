using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

// Interfaces
public interface IProduct {
    string Id { get; }
    string Name { get; }
    decimal Price { get; }
    string Category { get; }
}

public interface IOrder {
    string Id { get; }
    List<(string ProductId, int Quantity)> Products { get; }
    string Status { get; set; }
    DateTime CreatedAt { get; }
}

public class Product : IProduct {
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}

public class Order : IOrder {
    public string Id { get; set; }
    public List<(string ProductId, int Quantity)> Products { get; set; } = new();
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class InventoryItem {
    public Product Product { get; set; }
    public int Quantity { get; set; }
}

// Generic Repository
public class Repository<T> where T : class {
    private readonly List<T> _items = new();

    public void Add(T item) => _items.Add(item);
    public IEnumerable<T> GetAll() => _items.ToList();
    public T Find(Func<T, bool> predicate) => _items.FirstOrDefault(predicate);
    public void Remove(Func<T, bool> predicate) {
        var item = _items.FirstOrDefault(predicate);
        if (item != null) _items.Remove(item);
    }
}

// Inventory Service
public class InventoryService {
    private readonly Repository<Product> _productRepo = new();
    private readonly Repository<Order> _orderRepo = new();
    private readonly List<InventoryItem> _inventory = new();

    public void AddProduct(Product product) {
        if (string.IsNullOrWhiteSpace(product.Id) || product.Price <= 0)
            throw new ArgumentException("Invalid product data");
        _productRepo.Add(product);
    }

    public bool AddStock(string productId, int quantity) {
        var product = _productRepo.Find(p => p.Id == productId);
        if (product == null) return false;

        var item = _inventory.FirstOrDefault(i => i.Product.Id == productId);
        if (item != null)
            item.Quantity += quantity;
        else
            _inventory.Add(new InventoryItem { Product = product, Quantity = quantity });

        return true;
    }

    public bool RemoveStock(string productId, int quantity) {
        var item = _inventory.FirstOrDefault(i => i.Product.Id == productId);
        if (item == null || item.Quantity < quantity) return false;

        item.Quantity -= quantity;
        if (item.Quantity == 0)
            _inventory.RemoveAll(i => i.Product.Id == productId);

        return true;
    }

    public Order CreateOrder(List<(string ProductId, int Quantity)> items) {
        foreach (var (pid, qty) in items) {
            var invItem = _inventory.FirstOrDefault(i => i.Product.Id == pid);
            if (invItem == null || invItem.Quantity < qty) return null;
        }

        var order = new Order {
            Id = $"ord-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Products = items
        };

        _orderRepo.Add(order);
        return order;
    }
}

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
    public string Notes { get; private set; }
    public bool IsGift { get; private set; }
    public string CouponCode { get; private set; }
    public decimal DiscountAmount { get; private set; }

    public Order(User customer, Address shippingAddress, Address billingAddress, PaymentMethod paymentMethod)
    {
        Id = Guid.NewGuid();
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        BillingAddress = billingAddress ?? throw new ArgumentNullException(nameof(billingAddress));
        PaymentMethod = paymentMethod ?? throw new ArgumentNullException(nameof(paymentMethod));
        Items = new List<OrderItem>();
        Status = OrderStatus.Created;
        OrderDate = DateTime.Now;
        DiscountAmount = 0;
    }

    public void AddItem(Product product, int quantity)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");

        var existingItem = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            Items.Add(new OrderItem(product, quantity));
        }
    }

    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(i => i.Product.Id == productId);
        if (item != null) Items.Remove(item);
    }

    public void ApplyCoupon(string couponCode, decimal discount)
    {
        if (string.IsNullOrWhiteSpace(couponCode)) throw new ArgumentException("Invalid coupon code");
        if (discount <= 0) throw new ArgumentException("Invalid discount amount");

        CouponCode = couponCode;
        DiscountAmount = discount;
    }

    public void MarkAsGift(string note = null)
    {
        IsGift = true;
        Notes = note;
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
            throw new ArgumentException("Invalid status");

        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot change status of a cancelled order");

        Status = newStatus;

        if (newStatus == OrderStatus.Shipped)
        {
            ShippedDate = DateTime.Now;
        }
        else if (newStatus == OrderStatus.Delivered && string.IsNullOrEmpty(TrackingNumber))
        {
            throw new InvalidOperationException("Cannot mark as delivered without tracking info");
        }
    }

    public void SetShippingAddress(Address address)
    {
        if (address == null) throw new ArgumentNullException(nameof(address));
        ShippingAddress = address;
    }
    public void SetBillingAddress(Address address)
    {
        if (address == null) throw new ArgumentNullException(nameof(address));
        BillingAddress = address;
    }
    public void SetPaymentMethod(PaymentMethod method)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));
        PaymentMethod = method;
    }
    public void SetNotes(string notes)
    {
        Notes = notes;
    }
    public void SetGift(bool isGift)
    {
        IsGift = isGift;
    }
    public void SetTrackingInfo(string trackingNumber, string carrier)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber)) throw new ArgumentException("Invalid tracking number");
        if (string.IsNullOrWhiteSpace(carrier)) throw new ArgumentException("Invalid carrier name");

        TrackingNumber = trackingNumber;
        // Carrier can be stored or used for shipping label generation
    }
    public void SetShippingCarrier(string carrier)
    {
        if (string.IsNullOrWhiteSpace(carrier)) throw new ArgumentException("Carrier name cannot be empty");
        // Carrier can be stored or used for shipping label generation
    }
    public void SetShippingDate(DateTime date)
    {
        if (date < OrderDate) throw new ArgumentOutOfRangeException(nameof(date), "Shipping date cannot be before order date");
        ShippedDate = date;
    }

   public void SetTrackingNumber(string trackingNumber)
{
    if (string.IsNullOrWhiteSpace(trackingNumber))
        throw new ArgumentException("Tracking number cannot be empty or whitespace", nameof(trackingNumber));

    if (trackingNumber.Length < 5)
        throw new ArgumentException("Tracking number must be at least 5 characters long", nameof(trackingNumber));

    TrackingNumber = trackingNumber;
    // Log tracking assignment or notify shipping system
    // Log($"Tracking number set: {trackingNumber}");
}

public void SetShippingCost(decimal cost)
{
    if (cost < 0)
        throw new ArgumentOutOfRangeException(nameof(cost), "Shipping cost cannot be negative");

    if (cost > 1000)
        throw new ArgumentException("Shipping cost seems unusually high", nameof(cost));

    ShippingCost = Math.Round(cost, 2);
    // Log($"Shipping cost set to {ShippingCost:C2}");
}

public void SetTaxAmount(decimal tax)
{
    if (tax < 0)
        throw new ArgumentOutOfRangeException(nameof(tax), "Tax amount cannot be negative");

    if (tax > SubTotal)
        throw new InvalidOperationException("Tax amount cannot exceed subtotal");

    TaxAmount = Math.Round(tax, 2);
    // Log($"Tax amount set to {TaxAmount:C2}");
}

public decimal GetFinalTotal()
{
    var totalBeforeDiscount = SubTotal + ShippingCost + TaxAmount;
    var discounted = totalBeforeDiscount - DiscountAmount;

    if (DiscountAmount > totalBeforeDiscount)
        discounted = 0;

    return Math.Round(discounted, 2);
}

public override string ToString()
{
    var details = new StringBuilder();
    details.AppendLine($"Order ID     : {Id}");
    details.AppendLine($"Customer     : {Customer?.Name ?? "N/A"}");
    details.AppendLine($"Status       : {Status}");
    details.AppendLine($"Items        : {Items.Count}");
    details.AppendLine($"Subtotal     : {SubTotal:C2}");
    details.AppendLine($"Shipping     : {ShippingCost:C2}");
    details.AppendLine($"Tax          : {TaxAmount:C2}");
    details.AppendLine($"Discount     : {DiscountAmount:C2}");
    details.AppendLine($"Final Total  : {GetFinalTotal():C2}");
    details.AppendLine($"Order Date   : {OrderDate:yyyy-MM-dd}");
    if (!string.IsNullOrEmpty(TrackingNumber))
        details.AppendLine($"Tracking No. : {TrackingNumber}");

    return details.ToString();
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