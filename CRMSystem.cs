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

// Product & Order Implementation
public class Product : IProduct {
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }

    public override string ToString() => $"{Name} (${Price})";
}

public class Order : IOrder {
    public string Id { get; set; }
    public List<(string ProductId, int Quantity)> Products { get; set; } = new();
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public override string ToString() => $"Order {Id}, Status: {Status}";
}

// Inventory Item
public class InventoryItem {
    public Product Product { get; set; }
    public int Quantity { get; set; }

    public override string ToString() => $"{Product.Name} - Qty: {Quantity}";
}

// Generic Repository
public class Repository<T> where T : class {
    private readonly List<T> _items = new();

    public void Add(T item) {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _items.Add(item);
    }

    public IEnumerable<T> GetAll() => _items.ToList();

    public T Find(Func<T, bool> predicate) {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return _items.FirstOrDefault(predicate);
    }

    public void Remove(Func<T, bool> predicate) {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        var item = _items.FirstOrDefault(predicate);
        if (item != null) _items.Remove(item);
    }

    public int Count => _items.Count;
}

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Simple logger interface
/// </summary>
public interface ILogger {
    void Info(string message);
    void Warn(string message);
    void Error(string message);
}

public class ConsoleLogger : ILogger {
    public void Info(string message) => Console.WriteLine($"[INFO]: {message}");
    public void Warn(string message) => Console.WriteLine($"[WARN]: {message}");
    public void Error(string message) => Console.WriteLine($"[ERROR]: {message}");
}

/// <summary>
/// Represents a simple repository interface
/// </summary>
public interface IRepository<T> {
    void Add(T item);
    IEnumerable<T> GetAll();
}

/// <summary>
/// Product and Order DTOs
/// </summary>
public class Product {
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class Order {
    public string Id { get; set; }
    public List<(string ProductId, int Quantity)> Products { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InventoryItem {
    public Product Product { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Inventory Manager handling products and orders
/// </summary>
public class InventoryManager {
    private readonly List<InventoryItem> _inventory = new();
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Order> _orderRepo;
    private readonly ILogger _logger;

    public InventoryManager(IRepository<Product> productRepo, IRepository<Order> orderRepo, ILogger logger) {
        _productRepo = productRepo;
        _orderRepo = orderRepo;
        _logger = logger;
    }

    /// <summary>
    /// Attempts to remove stock for a given product
    /// </summary>
    public bool RemoveStock(string productId, int quantity) {
        if (string.IsNullOrEmpty(productId)) {
            _logger.Warn("RemoveStock failed: productId is null or empty.");
            return false;
        }

        if (quantity <= 0) {
            _logger.Warn("RemoveStock failed: quantity must be positive.");
            return false;
        }

        var item = _inventory.FirstOrDefault(i => i.Product.Id == productId);
        if (item == null) {
            _logger.Warn($"RemoveStock failed: product {productId} not found.");
            return false;
        }

        if (item.Quantity < quantity) {
            _logger.Warn($"RemoveStock failed: insufficient quantity for {productId}.");
            return false;
        }

        item.Quantity -= quantity;
        _logger.Info($"Removed {quantity} units of {productId}. Remaining: {item.Quantity}");

        if (item.Quantity == 0) {
            _inventory.RemoveAll(i => i.Product.Id == productId);
            _logger.Info($"Product {productId} removed from inventory as quantity reached zero.");
        }

        return true;
    }

    /// <summary>
    /// Creates a new order if all products are in stock
    /// </summary>
    public Order CreateOrder(List<(string ProductId, int Quantity)> productList) {
        if (productList == null || !productList.Any()) {
            _logger.Error("CreateOrder failed: product list is empty.");
            throw new ArgumentException("Empty product list");
        }

        var unavailableProducts = productList
            .Where(p => !_inventory.Any(i => i.Product.Id == p.ProductId && i.Quantity >= p.Quantity))
            .Select(p => p.ProductId)
            .ToList();

        if (unavailableProducts.Any()) {
            var message = $"Insufficient stock for: {string.Join(", ", unavailableProducts)}";
            _logger.Error(message);
            throw new InvalidOperationException(message);
        }

        var order = new Order {
            Id = $"ORD-{DateTime.UtcNow.Ticks}",
            Products = productList,
            CreatedAt = DateTime.UtcNow,
            Status = "processing"
        };

        foreach (var (productId, qty) in productList) {
            RemoveStock(productId, qty);
        }

        // Simulate shipping/delivery
        SimulateShipping(order);

        order.Status = "completed";
        _orderRepo.Add(order);
        _logger.Info($"Order {order.Id} created successfully with status: {order.Status}");

        return order;
    }

    private void SimulateShipping(Order order) {
        _logger.Info($"Shipping order {order.Id}...");
        // Simulate tracking/notification here
    }

    public IEnumerable<Product> GetAllProducts() {
        _logger.Info("Retrieving all products...");
        return _productRepo.GetAll();
    }

    public IEnumerable<InventoryItem> GetInventoryStatus() {
        _logger.Info("Retrieving current inventory status...");
        return _inventory.ToList();
    }

    public IEnumerable<Order> GetAllOrders() {
        _logger.Info("Retrieving all orders...");
        return _orderRepo.GetAll();
    }

    /// <summary>
    /// Adds new stock to inventory
    /// </summary>
    public void AddStock(Product product, int quantity) {
        if (quantity <= 0) {
            _logger.Warn("AddStock failed: Quantity must be greater than zero.");
            return;
        }

        var item = _inventory.FirstOrDefault(i => i.Product.Id == product.Id);
        if (item != null) {
            item.Quantity += quantity;
            _logger.Info($"Updated stock for {product.Name}: +{quantity} (Total: {item.Quantity})");
        } else {
            _inventory.Add(new InventoryItem { Product = product, Quantity = quantity });
            _logger.Info($"Added new product to inventory: {product.Name} with quantity {quantity}");
        }
    }

    public InventoryReport GenerateReport() {
        return new InventoryReport {
            TotalProducts = _inventory.Count,
            LowStockItems = _inventory.Where(i => i.Quantity < 5).ToList(),
            LastGenerated = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Report summary
/// </summary>
public class InventoryReport {
    public int TotalProducts { get; set; }
    public List<InventoryItem> LowStockItems { get; set; }
    public DateTime LastGenerated { get; set; }

    public void Print() {
        Console.WriteLine($"Inventory Report at {LastGenerated}");
        Console.WriteLine($"Total Products: {TotalProducts}");
        if (LowStockItems.Any()) {
            Console.WriteLine("Low Stock Items:");
            foreach (var item in LowStockItems) {
                Console.WriteLine($" - {item.Product.Name} (Qty: {item.Quantity})");
            }
        } else {
            Console.WriteLine("All products sufficiently stocked.");
        }
    }
}

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

namespace CRMSystem
{
    public class CRMSystem
    {
        private readonly CustomerManager customerManager;
        private readonly ContactManager contactManager;
        private readonly OpportunityManager opportunityManager;
        private readonly LeadManager leadManager;
        private readonly CampaignManager campaignManager;
        private readonly TaskManager taskManager;
        private readonly DocumentManager documentManager;
        private readonly ReportingEngine reportingEngine;
        private readonly NotificationService notificationService;
        private readonly WorkflowEngine workflowEngine;
        private readonly AuditLogger auditLogger;

        public CRMSystem()
        {
            customerManager = new CustomerManager();
            contactManager = new ContactManager();
            opportunityManager = new OpportunityManager();
            leadManager = new LeadManager();
            campaignManager = new CampaignManager();
            taskManager = new TaskManager();
            documentManager = new DocumentManager();
            reportingEngine = new ReportingEngine();
            notificationService = new NotificationService();
            workflowEngine = new WorkflowEngine();
            auditLogger = new AuditLogger();
        }

        // Many more methods and classes follow...
        // Due to length constraints, this is a shortened version.
        // The full implementation would include complete implementations of:
        // - Customer management
        // - Contact management
        // - Opportunity tracking
        // - Lead management
        // - Campaign management
        // - Task management
        // - Document management
        // - Reporting system
        // - Notification system
        // - Workflow engine
        // - Audit logging
        // - Integration services
        // - Analytics engine
        // - Email marketing
        // - Sales forecasting
        // - Territory management
        // - Product catalog
        // - Quote management
        // - Contract management
        // - Service level agreements
        // - Knowledge base
        // - Support ticket system
    }

    public class Customer
    {
        public Guid Id { get; }
        public string CompanyName { get; set; }
        public string Industry { get; set; }
        public int EmployeeCount { get; set; }
        public decimal AnnualRevenue { get; set; }
        public CustomerStatus Status { get; private set; }
        public CustomerType Type { get; set; }
        public List<Contact> Contacts { get; }
        public List<Opportunity> Opportunities { get; }
        public Address BillingAddress { get; set; }
        public Address ShippingAddress { get; set; }
        public DateTime CreatedDate { get; }
        public DateTime? LastModifiedDate { get; private set; }
        public string Website { get; set; }
        public string PhoneNumber { get; set; }
        public string Notes { get; set; }

        public Customer(string companyName, string industry)
        {
            Id = Guid.NewGuid();
            CompanyName = companyName;
            Industry = industry;
            Status = CustomerStatus.Active;
            Type = CustomerType.Prospect;
            Contacts = new List<Contact>();
            Opportunities = new List<Opportunity>();
            CreatedDate = DateTime.Now;
        }

        public void UpdateStatus(CustomerStatus newStatus)
        {
            Status = newStatus;
            LastModifiedDate = DateTime.Now;
        }
    }

    public class CustomerManager
    {
        private readonly Dictionary<Guid, Customer> customers;
        private readonly AuditLogger auditLogger;

        public CustomerManager()
        {
            customers = new Dictionary<Guid, Customer>();
            auditLogger = new AuditLogger();
        }

        public Customer CreateCustomer(string companyName, string industry)
        {
            var customer = new Customer(companyName, industry);
            customers.Add(customer.Id, customer);
            auditLogger.LogAction("Customer Created", customer.Id.ToString());
            return customer;
        }

        public List<Customer> SearchCustomers(string searchTerm)
        {
            return customers.Values
                .Where(c => c.CompanyName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           c.Industry.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<Customer> GetCustomersByIndustry(string industry)
        {
            return customers.Values
                .Where(c => c.Industry.Equals(industry, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    public class Contact
    {
        public Guid Id { get; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string JobTitle { get; set; }
        public Guid CustomerId { get; }
        public ContactStatus Status { get; private set; }
        public List<ContactInteraction> Interactions { get; }
        public DateTime CreatedDate { get; }
        public DateTime? LastModifiedDate { get; private set; }

        public Contact(string firstName, string lastName, string email, Guid customerId)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            CustomerId = customerId;
            Status = ContactStatus.Active;
            Interactions = new List<ContactInteraction>();
            CreatedDate = DateTime.Now;
        }

        public void AddInteraction(string type, string description)
        {
            Interactions.Add(new ContactInteraction(type, description));
            LastModifiedDate = DateTime.Now;
        }
    }

    public class ContactManager
    {
        private readonly Dictionary<Guid, Contact> contacts;
        private readonly AuditLogger auditLogger;

        public ContactManager()
        {
            contacts = new Dictionary<Guid, Contact>();
            auditLogger = new AuditLogger();
        }

        public Contact CreateContact(string firstName, string lastName, string email, Guid customerId)
        {
            var contact = new Contact(firstName, lastName, email, customerId);
            contacts.Add(contact.Id, contact);
            auditLogger.LogAction("Contact Created", contact.Id.ToString());
            return contact;
        }

        public List<Contact> GetContactsByCustomer(Guid customerId)
        {
            return contacts.Values
                .Where(c => c.CustomerId == customerId)
                .ToList();
        }
    }

    public class Opportunity
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public Guid CustomerId { get; }
        public decimal Value { get; set; }
        public OpportunityStage Stage { get; private set; }
        public decimal Probability { get; set; }
        public DateTime ExpectedCloseDate { get; set; }
        public List<OpportunityActivity> Activities { get; }
        public Contact PrimaryContact { get; set; }
        public string Source { get; set; }
        public DateTime CreatedDate { get; }
        public DateTime? LastModifiedDate { get; private set; }

        public Opportunity(string name, Guid customerId, decimal value)
        {
            Id = Guid.NewGuid();
            Name = name;
            CustomerId = customerId;
            Value = value;
            Stage = OpportunityStage.Prospecting;
            Activities = new List<OpportunityActivity>();
            CreatedDate = DateTime.Now;
        }

        public void UpdateStage(OpportunityStage newStage)
        {
            Stage = newStage;
            LastModifiedDate = DateTime.Now;
        }
    }

    public class OpportunityActivity
    {
        public Guid Id { get; }
        public string Type { get; }
        public string Description { get; }
        public DateTime ActivityDate { get; }
        public string Outcome { get; set; }

        public OpportunityActivity(string type, string description)
        {
            Id = Guid.NewGuid();
            Type = type;
            Description = description;
            ActivityDate = DateTime.Now;
        }
    }

    public enum CustomerStatus
    {
        Active,
        Inactive,
        Prospect,
        Lost
    }

    public enum CustomerType
    {
        Prospect,
        Customer,
        Partner,
        Competitor
    }

    public enum ContactStatus
    {
        Active,
        Inactive,
        OnHold
    }

    public enum OpportunityStage
    {
        Prospecting,
        Qualification,
        NeedsAnalysis,
        Proposal,
        Negotiation,
        ClosedWon,
        ClosedLost
    }

    public class AuditLogger
    {
        private readonly List<AuditLog> auditLogs;

        public AuditLogger()
        {
            auditLogs = new List<AuditLog>();
        }

        public void LogAction(string action, string entityId)
        {
            auditLogs.Add(new AuditLog(action, entityId));
        }

        public List<AuditLog> GetLogs(DateTime startDate, DateTime endDate)
        {
            return auditLogs
                .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate)
                .OrderByDescending(log => log.Timestamp)
                .ToList();
        }
    }

    public class AuditLog
    {
        public Guid Id { get; }
        public string Action { get; }
        public string EntityId { get; }
        public DateTime Timestamp { get; }

        public AuditLog(string action, string entityId)
        {
            Id = Guid.NewGuid();
            Action = action;
            EntityId = entityId;
            Timestamp = DateTime.Now;
        }
    }

    public class Lead
    {
        public Guid Id { get; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Source { get; set; }
        public LeadStatus Status { get; private set; }
        public decimal EstimatedValue { get; set; }
        public List<LeadActivity> Activities { get; }
        public DateTime CreatedDate { get; }
        public DateTime? LastModifiedDate { get; private set; }
        public string Notes { get; set; }

        public Lead(string firstName, string lastName, string companyName, string email)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            CompanyName = companyName;
            Email = email;
            Status = LeadStatus.New;
            Activities = new List<LeadActivity>();
            CreatedDate = DateTime.Now;
        }

        public void UpdateStatus(LeadStatus newStatus)
        {
            Status = newStatus;
            LastModifiedDate = DateTime.Now;
        }

        public void AddActivity(string type, string description)
        {
            Activities.Add(new LeadActivity(type, description));
            LastModifiedDate = DateTime.Now;
        }
    }

    public class LeadManager
    {
        private readonly Dictionary<Guid, Lead> leads;
        private readonly AuditLogger auditLogger;

        public LeadManager()
        {
            leads = new Dictionary<Guid, Lead>();
            auditLogger = new AuditLogger();
        }

        public Lead CreateLead(string firstName, string lastName, string companyName, string email)
        {
            var lead = new Lead(firstName, lastName, companyName, email);
            leads.Add(lead.Id, lead);
            auditLogger.LogAction("Lead Created", lead.Id.ToString());
            return lead;
        }

        public void QualifyLead(Guid leadId)
        {
            var lead = leads[leadId];
            lead.UpdateStatus(LeadStatus.Qualified);
            auditLogger.LogAction("Lead Qualified", leadId.ToString());
        }

        public List<Lead> GetLeadsByStatus(LeadStatus status)
        {
            return leads.Values
                .Where(l => l.Status == status)
                .ToList();
        }
    }

    public class Campaign
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CampaignType Type { get; set; }
        public CampaignStatus Status { get; private set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public decimal ActualCost { get; private set; }
        public List<CampaignActivity> Activities { get; }
        public List<Lead> GeneratedLeads { get; }
        public CampaignMetrics Metrics { get; }
        public DateTime CreatedDate { get; }
        public DateTime? LastModifiedDate { get; private set; }

        public Campaign(string name, CampaignType type, DateTime startDate, DateTime endDate, decimal budget)
        {
            Id = Guid.NewGuid();
            Name = name;
            Type = type;
            Status = CampaignStatus.Planning;
            StartDate = startDate;
            EndDate = endDate;
            Budget = budget;
            ActualCost = 0;
            Activities = new List<CampaignActivity>();
            GeneratedLeads = new List<Lead>();
            Metrics = new CampaignMetrics();
            CreatedDate = DateTime.Now;
        }

        public void UpdateStatus(CampaignStatus newStatus)
        {
            Status = newStatus;
            LastModifiedDate = DateTime.Now;
        }

        public void AddCost(decimal amount)
        {
            ActualCost += amount;
            LastModifiedDate = DateTime.Now;
        }

        public void AddLead(Lead lead)
        {
            GeneratedLeads.Add(lead);
            Metrics.UpdateLeadCount(GeneratedLeads.Count);
            LastModifiedDate = DateTime.Now;
        }
    }
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TicTacToe
{
    public enum PlayerSymbol
    {
        None,
        X,
        O
    }

    public class Board
    {
        private PlayerSymbol[,] grid;
        public const int Size = 3;

        public Board()
        {
            grid = new PlayerSymbol[Size, Size];
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    grid[i, j] = PlayerSymbol.None;
        }

        public bool IsCellEmpty(int row, int col)
        {
            return grid[row, col] == PlayerSymbol.None;
        }

        public bool MakeMove(int row, int col, PlayerSymbol symbol)
        {
            if (row < 0 || row >= Size || col < 0 || col >= Size || !IsCellEmpty(row, col))
                return false;
            grid[row, col] = symbol;
            return true;
        }

        public bool IsFull()
        {
            return grid.Cast<PlayerSymbol>().All(cell => cell != PlayerSymbol.None);
        }

        public bool CheckWin(PlayerSymbol symbol)
        {
            // Check rows for a win
            for (int i = 0; i < Size; i++)
                if (grid[i, 0] == symbol && grid[i, 1] == symbol && grid[i, 2] == symbol)
                    return true;

            // Check columns for a win
            for (int j = 0; j < Size; j++)
                if (grid[0, j] == symbol && grid[1, j] == symbol && grid[2, j] == symbol)
                    return true;

            // Check main diagonal
            if (grid[0, 0] == symbol && grid[1, 1] == symbol && grid[2, 2] == symbol)
                return true;

            // Check anti-diagonal
            if (grid[0, 2] == symbol && grid[1, 1] == symbol && grid[2, 0] == symbol)
                return true;

            return false;
        }

        public void Display()
        {
            Console.Clear();
            Console.WriteLine("\n   TIC-TAC-TOE");
            Console.WriteLine("  ===============");
            for (int i = 0; i < Size; i++)
            {
                Console.Write("  | ");
                for (int j = 0; j < Size; j++)
                {
                    char display = grid[i, j] == PlayerSymbol.None ? ' ' : grid[i, j].ToString()[0];
                    Console.Write($"{display} | ");
                }
                Console.WriteLine("\n  ===============");
            }
            Console.WriteLine();
        }
    }

    public class Player
    {
        public string Name { get; set; }
        public PlayerSymbol Symbol { get; set; }
        public bool IsHuman { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }

        public Player(string name, PlayerSymbol symbol, bool isHuman = true)
        {
            Name = name;
            Symbol = symbol;
            IsHuman = isHuman;
            Wins = 0;
            Draws = 0;
        }

        public string GetScore()
        {
            return $"{Name}: {Wins} Wins, {Draws} Draws";
        }
    }

    public class Game
    {
        private Board board;
        private Player[] players;
        private int currentPlayerIndex;
        private bool singlePlayer;
        private List<string> gameHistory;

        public Game(bool singlePlayer = false)
        {
            board = new Board();
            this.singlePlayer = singlePlayer;
            gameHistory = new List<string>();
            InitializePlayers();
        }

        private void InitializePlayers()
        {
            players = new Player[2];
            if (singlePlayer)
            {
                Console.Write("Enter your name (or press Enter for 'Player'): ");
                string playerName = Console.ReadLine()?.Trim();
                playerName = string.IsNullOrEmpty(playerName) ? "Player" : playerName;
                players[0] = new Player(playerName, PlayerSymbol.X);
                players[1] = new Player("AI", PlayerSymbol.O, false);
            }
            else
            {
                Console.Write("Enter Player 1 name (or press Enter for 'Player 1'): ");
                string player1Name = Console.ReadLine()?.Trim();
                player1Name = string.IsNullOrEmpty(player1Name) ? "Player 1" : player1Name;
                Console.Write("Enter Player 2 name (or press Enter for 'Player 2'): ");
                string player2Name = Console.ReadLine()?.Trim();
                player2Name = string.IsNullOrEmpty(player2Name) ? "Player 2" : player2Name;
                players[0] = new Player(player1Name, PlayerSymbol.X);
                players[1] = new Player(player2Name, PlayerSymbol.O);
            }
            currentPlayerIndex = 0;
        }

        public void Start()
        {
            DisplayTitleScreen();
            bool gameOver = false;
            board.Display();
            DisplayScores();

            while (!gameOver)
            {
                Player currentPlayer = players[currentPlayerIndex];
                Console.WriteLine($"{currentPlayer.Name}'s turn ({currentPlayer.Symbol})");

                if (currentPlayer.IsHuman)
                    HandleHumanMove(currentPlayer);
                else
                    HandleAIMove(currentPlayer);

                board.Display();
                DisplayScores();

                if (board.CheckWin(currentPlayer.Symbol))
                {
                    Console.WriteLine($"{currentPlayer.Name} wins!");
                    currentPlayer.Wins++;
                    gameHistory.Add($"{currentPlayer.Name} ({currentPlayer.Symbol}) won");
                    gameOver = true;
                }
                else if (board.IsFull())
                {
                    Console.WriteLine("It's a draw!");
                    players[0].Draws++;
                    players[1].Draws++;
                    gameHistory.Add("Draw");
                    gameOver = true;
                }
                else
                {
                    currentPlayerIndex = (currentPlayerIndex + 1) % 2;
                }
            }

            DisplayGameHistory();
            Console.Write("Play again? (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() == "y")
            {
                board = new Board();
                currentPlayerIndex = 0;
                Start();
            }
        }

        private void DisplayTitleScreen()
        {
            Console.Clear();
            Console.WriteLine("=================================");
            Console.WriteLine("       TIC-TAC-TOE GAME          ");
            Console.WriteLine("=================================");
            Console.WriteLine("Welcome to Tic-Tac-Toe!");
            Console.WriteLine("Enter moves as row (1-3) and column (1-3).");
            Console.WriteLine("Press any desire to start...");
            Console.ReadKey();
        }

        private void DisplayScores()
        {
            Console.WriteLine("--- Scores ---");
            Console.WriteLine(players[0].GetScore());
            Console.WriteLine(players[1].GetScore());
            Console.WriteLine("--------------");
        }

        private void DisplayGameHistory()
        {
            Console.WriteLine("\n--- Game History ---");
            if (gameHistory.Count == 0)
                Console.WriteLine("No games played yet.");
            else
                for (int i = 0; i < gameHistory.Count; i++)
                    Console.WriteLine($"Game {i + 1}: {gameHistory[i]}");
            Console.WriteLine("-------------------");
        }

        private void HandleHumanMove(Player player)
        {
            bool validMove = false;
            while (!validMove)
            {
                Console.Write("Enter row (1-3): ");
                string rowInput = Console.ReadLine()?.Trim();
                if (!int.TryParse(rowInput, out int row) || row < 1 || row > 3)
                {
                    Console.WriteLine("Invalid row. Please enter a number between 1 and 3.");
                    continue;
                }

                Console.Write("Enter column (1-3): ");
                string colInput = Console.ReadLine()?.Trim();
                if (!int.TryParse(colInput, out int col) || col < 1 || col > 3)
                {
                    Console.WriteLine("Invalid column. Please enter a number between 1 and 3.");
                    continue;
                }

                validMove = board.MakeMove(row - 1, col - 1, player.Symbol);
                if (!validMove)
                    Console.WriteLine("Cell already taken or invalid. Try again.");
            }
        }

        private void HandleAIMove(Player player)
        {
            Console.WriteLine("AI is thinking...");
            Thread.Sleep(1000); // Simulate AI decision-making

            // AI strategy: Win, block, or random
            (int row, int col) = FindBestMove(player.Symbol);
            if (row == -1 || col == -1)
            {
                // Fallback to random move
                Random rand = new Random();
                do
                {
                    row = rand.Next(0, Board.Size);
                    col = rand.Next(0, Board.Size);
                } while (!board.IsCellEmpty(row, col));
            }

            board.MakeMove(row, col, player.Symbol);
            Console.WriteLine($"AI placed {player.Symbol} at row {row + 1}, column {col + 1}");
        }

        private (int, int) FindBestMove(PlayerSymbol symbol)
        {
            // Look for a winning move
            for (int i = 0; i < Board.Size; i++)
                for (int j = 0; j < Board.Size; j++)
                    if (board.IsCellEmpty(i, j))
                    {
                        board.MakeMove(i, j, symbol);
                        if (board.CheckWin(symbol))
                        {
                            board.MakeMove(i, j, PlayerSymbol.None);
                            return (i, j);
                        }
                        board.MakeMove(i, j, PlayerSymbol.None);
                    }

            // Look for a blocking move
            PlayerSymbol opponent = symbol == PlayerSymbol.X ? PlayerSymbol.O : PlayerSymbol.X;
            for (int i = 0; i < Board.Size; i++)
                for (int j = 0; j < Board.Size; j++)
                    if (board.IsCellEmpty(i, j))
                    {
                        board.MakeMove(i, j, opponent);
                        if (board.CheckWin(opponent))
                        {
                            board.MakeMove(i, j, PlayerSymbol.None);
                            return (i, j);
                        }
                        board.MakeMove(i, j, PlayerSymbol.None);
                    }

            // Prefer center
            if (board.IsCellEmpty(1, 1))
                return (1, 1);

            // Prefer corners
            int[] corners = [0, 2];
            foreach (int i in corners)
                foreach (int j in corners)
                    if (board.IsCellEmpty(i, j))
                        return (i, j);

            return (-1, -1); // No strategic move
        }
    }

   // Display the history of played games
private void DisplayGameHistory()
{
    Console.WriteLine("\n--- Game History ---");

    if (gameHistory.Count == 0)
    {
        Console.WriteLine("No games played yet.");
    }
    else
    {
        Console.WriteLine($"Total games played: {gameHistory.Count}");

        // Loop through each recorded game
        for (int i = 0; i < gameHistory.Count; i++)
        {
            string result = gameHistory[i];
            Console.WriteLine($"Game {i + 1}: {result}");

            // Additional feedback based on result
            if (result.Contains("Win"))
            {
                Console.WriteLine("  Result: Victory detected.");
            }
            else if (result.Contains("Draw"))
            {
                Console.WriteLine("  Result: It was a draw. Close one!");
            }
            else
            {
                Console.WriteLine("  Result: Unclear - format mismatch.");
            }

            Console.WriteLine(); // Spacer
        }

        // Summary at the end
        int winCount = gameHistory.Count(g => g.Contains("Win"));
        int drawCount = gameHistory.Count(g => g.Contains("Draw"));
        int otherCount = gameHistory.Count - winCount - drawCount;

        Console.WriteLine("Summary of game results:");
        Console.WriteLine($"  Wins : {winCount}");
        Console.WriteLine($"  Draws: {drawCount}");
        Console.WriteLine($"  Others: {otherCount}");
    }

    Console.WriteLine("----------------------\n");
}

// Handle human player's move
private void HandleHumanMove(Player player)
{
    bool validMove = false;
    Console.WriteLine($"\n{player.Name}'s Turn ({player.Symbol})");

    while (!validMove)
    {
        Console.Write("Enter row (1-3): ");
        string rowInput = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(rowInput))
        {
            Console.WriteLine("Row input cannot be empty.");
            continue;
        }

        if (!int.TryParse(rowInput, out int row))
        {
            Console.WriteLine("Row must be a numeric value.");
            continue;
        }

        if (row < 1 || row > 3)
        {
            Console.WriteLine("Row must be between 1 and 3.");
            continue;
        }

        Console.Write("Enter column (1-3): ");
        string colInput = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(colInput))
        {
            Console.WriteLine("Column input cannot be empty.");
            continue;
        }

        if (!int.TryParse(colInput, out int col))
        {
            Console.WriteLine("Column must be a numeric value.");
            continue;
        }

        if (col < 1 || col > 3)
        {
            Console.WriteLine("Column must be between 1 and 3.");
            continue;
        }

        Console.WriteLine($"Attempting move at row {row}, column {col}...");

        validMove = board.MakeMove(row - 1, col - 1, player.Symbol);
        if (!validMove)
        {
            Console.WriteLine("Invalid move. Cell is already occupied or out of bounds.");
        }
        else
        {
            Console.WriteLine("Move successful.");
        }

        Console.WriteLine(); // Add spacing
    }

    Console.WriteLine("End of turn.\n");
}

// Handle AI move logic
private void HandleAIMove(Player player)
{
    Console.WriteLine("\nAI is making a move...");
    Thread.Sleep(1000); // Delay for effect

    (int row, int col) = FindBestMove(player.Symbol);

    if (row == -1 || col == -1)
    {
        Console.WriteLine("AI could not find a smart move. Picking randomly.");
        Random rand = new Random();
        int attempts = 0;

        do
        {
            row = rand.Next(0, Board.Size);
            col = rand.Next(0, Board.Size);
            attempts++;
        }
        while (!board.IsCellEmpty(row, col) && attempts < 100);

        if (attempts >= 100)
        {
            Console.WriteLine("AI failed to find any valid cell after 100 tries.");
            return;
        }
    }

    board.MakeMove(row, col, player.Symbol);
    Console.WriteLine($"AI placed {player.Symbol} at row {row + 1}, column {col + 1}");
    Console.WriteLine("AI turn complete.\n");
}

// AI decision logic
private (int, int) FindBestMove(PlayerSymbol symbol)
{
    Console.WriteLine("Evaluating best move...");

    // Step 1: Try to win
    for (int i = 0; i < Board.Size; i++)
    {
        for (int j = 0; j < Board.Size; j++)
        {
            if (board.IsCellEmpty(i, j))
            {
                board.MakeMove(i, j, symbol);

                if (board.CheckWin(symbol))
                {
                    board.MakeMove(i, j, PlayerSymbol.None);
                    Console.WriteLine($"Winning move found at {i},{j}");
                    return (i, j);
                }

                board.MakeMove(i, j, PlayerSymbol.None);
            }
        }
    }

    // Step 2: Block opponent's win
    PlayerSymbol opponent = symbol == PlayerSymbol.X ? PlayerSymbol.O : PlayerSymbol.X;

    for (int i = 0; i < Board.Size; i++)
    {
        for (int j = 0; j < Board.Size; j++)
        {
            if (board.IsCellEmpty(i, j))
            {
                board.MakeMove(i, j, opponent);

                if (board.CheckWin(opponent))
                {
                    board.MakeMove(i, j, PlayerSymbol.None);
                    Console.WriteLine($"Blocking opponent at {i},{j}");
                    return (i, j);
                }

                board.MakeMove(i, j, PlayerSymbol.None);
            }
        }
    }

    // Step 3: Take center
    if (board.IsCellEmpty(1, 1))
    {
        Console.WriteLine("Taking center cell.");
        return (1, 1);
    }

    // Step 4: Take a corner
    int[] corners = new int[] { 0, 2 };

    foreach (int i in corners)
    {
        foreach (int j in corners)
        {
            if (board.IsCellEmpty(i, j))
            {
                Console.WriteLine($"Taking corner at {i},{j}");
                return (i, j);
            }
        }
    }

    // Step 5: Fallback
    Console.WriteLine("No strategic move found.");
    return (-1, -1);
}

// Add these classes after the existing code

public class GameAnalytics
{
    private readonly List<GameRecord> gameRecords;
    private readonly Dictionary<PlayerSymbol, PlayerStats> playerStats;

    public GameAnalytics()
    {
        gameRecords = new List<GameRecord>();
        playerStats = new Dictionary<PlayerSymbol, PlayerStats>
        {
            { PlayerSymbol.X, new PlayerStats() },
            { PlayerSymbol.O, new PlayerStats() }
        };
    }

    public void RecordGame(GameRecord record)
    {
        gameRecords.Add(record);
        UpdatePlayerStats(record);
    }

    public void DisplayDetailedStats()
    {
        Console.WriteLine("\n=== Detailed Game Statistics ===");
        
        foreach (var player in playerStats)
        {
            Console.WriteLine($"\nPlayer {player.Key} Statistics:");
            Console.WriteLine($"Total Games: {player.Value.TotalGames}");
            Console.WriteLine($"Wins: {player.Value.Wins} ({CalculatePercentage(player.Value.Wins, player.Value.TotalGames)}%)");
            Console.WriteLine($"Losses: {player.Value.Losses} ({CalculatePercentage(player.Value.Losses, player.Value.TotalGames)}%)");
            Console.WriteLine($"Draws: {player.Value.Draws} ({CalculatePercentage(player.Value.Draws, player.Value.TotalGames)}%)");
            Console.WriteLine($"Average Moves Per Game: {player.Value.AverageMovesPerGame:F1}");
            Console.WriteLine($"Center Cell Usage: {CalculatePercentage(player.Value.CenterMoves, player.Value.TotalMoves)}%");
            Console.WriteLine($"Corner Cell Usage: {CalculatePercentage(player.Value.CornerMoves, player.Value.TotalMoves)}%");
        }

        DisplayWinningPatterns();
    }

    private void UpdatePlayerStats(GameRecord record)
    {
        var stats = playerStats[record.Winner ?? PlayerSymbol.None];
        stats.TotalGames++;

        if (record.Winner.HasValue)
            stats.Wins++;
        else
            stats.Draws++;

        foreach (var move in record.Moves)
        {
            var playerStats = playerStats[move.Player];
            playerStats.TotalMoves++;

            if (IsCenter(move.Row, move.Column))
                playerStats.CenterMoves++;
            else if (IsCorner(move.Row, move.Column))
                playerStats.CornerMoves++;
        }
    }

    private bool IsCenter(int row, int column)
    {
        return row == 1 && column == 1;
    }

    private bool IsCorner(int row, int column)
    {
        return (row == 0 || row == 2) && (column == 0 || column == 2);
    }

    private decimal CalculatePercentage(int part, int total)
    {
        return total == 0 ? 0 : (decimal)part / total * 100;
    }

    private void DisplayWinningPatterns()
    {
        var winningGames = gameRecords.Where(r => r.Winner.HasValue);
        
        Console.WriteLine("\nWinning Move Patterns:");
        foreach (var game in winningGames)
        {
            Console.WriteLine($"\nGame ID: {game.GameId}");
            Console.WriteLine($"Winner: Player {game.Winner}");
            Console.WriteLine("Moves:");
            foreach (var move in game.Moves)
            {
                Console.WriteLine($"- Player {move.Player}: ({move.Row + 1},{move.Column + 1})");
            }
        }
    }
}

public class GameRecord
{
    public Guid GameId { get; }
    public List<MoveRecord> Moves { get; }
    public PlayerSymbol? Winner { get; set; }
    public DateTime GameDate { get; }

    public GameRecord()
    {
        GameId = Guid.NewGuid();
        Moves = new List<MoveRecord>();
        GameDate = DateTime.Now;
    }
}

public class MoveRecord
{
    public PlayerSymbol Player { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public int MoveNumber { get; set; }
}

public class PlayerStats
{
    public int TotalGames { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public int TotalMoves { get; set; }
    public int CenterMoves { get; set; }
    public int CornerMoves { get; set; }
    public decimal AverageMovesPerGame => TotalGames == 0 ? 0 : (decimal)TotalMoves / TotalGames;
}

}
    public class CampaignActivity
    {
        public Guid Id { get; }
        public string Type { get; }
        public string Description { get; }
        public DateTime ActivityDate { get; }

        public CampaignActivity(string type, string description)
        {
            Id = Guid.NewGuid();
            Type = type;
            Description = description;
            ActivityDate = DateTime.Now;
        }
    }
    public class CampaignReport
    {
        public Guid CampaignId { get; set; }
        public int LeadCount { get; set; }
        public int ConversionCount { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal ROI { get; set; }
    }

    public class CampaignManager
    {
        private readonly Dictionary<Guid, Campaign> campaigns;
        private readonly AuditLogger auditLogger;

        public CampaignManager()
        {
            campaigns = new Dictionary<Guid, Campaign>();
            auditLogger = new AuditLogger();
        }

        public Campaign CreateCampaign(string name, CampaignType type, DateTime startDate, DateTime endDate, decimal budget)
        {
            var campaign = new Campaign(name, type, startDate, endDate, budget);
            campaigns.Add(campaign.Id, campaign);
            auditLogger.LogAction("Campaign Created", campaign.Id.ToString());
            return campaign;
        }

        public List<Campaign> GetActiveCampaigns()
        {
            return campaigns.Values
                .Where(c => c.Status == CampaignStatus.Active)
                .ToList();
        }

        public CampaignMetrics GetCampaignMetrics(Guid campaignId)
        {
            return campaigns[campaignId].Metrics;
        }
    }

    public class Document
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public string Type { get; }
        public byte[] Content { get; private set; }
        public string ContentType { get; }
        public long FileSize { get; }
        public Dictionary<string, string> Metadata { get; }
        public DateTime UploadDate { get; }
        public DateTime? LastModifiedDate { get; private set; }
        public string UploadedBy { get; }
        public List<DocumentVersion> Versions { get; }

        public Document(string name, string type, byte[] content, string contentType, string uploadedBy)
        {
            Id = Guid.NewGuid();
            Name = name;
            Type = type;
            Content = content;
            ContentType = contentType;
            FileSize = content.Length;
            Metadata = new Dictionary<string, string>();
            UploadDate = DateTime.Now;
            UploadedBy = uploadedBy;
            Versions = new List<DocumentVersion>();
        }

        public void UpdateContent(byte[] newContent, string updatedBy)
        {
            var version = new DocumentVersion(Content, UploadedBy, LastModifiedDate ?? UploadDate);
            Versions.Add(version);
            
            Content = newContent;
            LastModifiedDate = DateTime.Now;
            FileSize = newContent.Length;
        }

        public void AddMetadata(string key, string value)
        {
            Metadata[key] = value;
            LastModifiedDate = DateTime.Now;
        }
    }

    public class DocumentManager
    {
        private readonly Dictionary<Guid, Document> documents;
        private readonly AuditLogger auditLogger;

        public DocumentManager()
        {
            documents = new Dictionary<Guid, Document>();
            auditLogger = new AuditLogger();
        }

        public Document UploadDocument(string name, string type, byte[] content, string contentType, string uploadedBy)
        {
            var document = new Document(name, type, content, contentType, uploadedBy);
            documents.Add(document.Id, document);
            auditLogger.LogAction("Document Uploaded", document.Id.ToString());
            return document;
        }

        public List<Document> SearchDocuments(string searchTerm)
        {
            return documents.Values
                .Where(d => d.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           d.Type.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public Document GetDocumentVersion(Guid documentId, int version)
        {
            var document = documents[documentId];
            if (version >= document.Versions.Count)
                throw new ArgumentException("Version not found");

            var historicalVersion = document.Versions[version];
            return new Document(
                document.Name,
                document.Type,
                historicalVersion.Content,
                document.ContentType,
                historicalVersion.ModifiedBy);
        }
    }

    public class DocumentVersion
    {
        public Guid Id { get; }
        public byte[] Content { get; }
        public string ModifiedBy { get; }
        public DateTime ModifiedDate { get; }

        public DocumentVersion(byte[] content, string modifiedBy, DateTime modifiedDate)
        {
            Id = Guid.NewGuid();
            Content = content;
            ModifiedBy = modifiedBy;
            ModifiedDate = modifiedDate;
        }
    }

    public enum LeadStatus
    {
        New,
        Contacted,
        Qualified,
        Converted,
        Lost
    }

    public enum CampaignStatus
    {
        Planning,
        Active,
        Paused,
        Completed,
        Cancelled
    }

    public enum CampaignType
    {
        Email,
        Social,
        Event,
        Webinar,
        PPC,
        Direct
    }

    public class CampaignMetrics
    {
        public int LeadCount { get; private set; }
        public int ConversionCount { get; private set; }
        public decimal ConversionRate => LeadCount > 0 ? (decimal)ConversionCount / LeadCount : 0;
        public decimal ROI { get; private set; }

        public void UpdateLeadCount(int count)
        {
            LeadCount = count;
        }

        public void UpdateConversionCount(int count)
        {
            ConversionCount = count;
        }

        public void UpdateROI(decimal revenue, decimal cost)
        {
            ROI = cost > 0 ? (revenue - cost) / cost : 0;
        }
    }

    public class ReportingEngine
    {
        private readonly CustomerManager customerManager;
        private readonly LeadManager leadManager;
        private readonly CampaignManager campaignManager;
        private readonly OpportunityManager opportunityManager;

        public ReportingEngine()
        {
            customerManager = new CustomerManager();
            leadManager = new LeadManager();
            campaignManager = new CampaignManager();
            opportunityManager = new OpportunityManager();
        }

        public SalesReport GenerateSalesReport(DateTime startDate, DateTime endDate)
        {
            return new SalesReport
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = CalculateTotalRevenue(startDate, endDate),
                OpportunityCount = GetOpportunityCount(startDate, endDate),
                WinRate = CalculateWinRate(startDate, endDate),
                AverageDeaSize = CalculateAverageDealSize(startDate, endDate)
            };
        }

        public LeadReport GenerateLeadReport(DateTime startDate, DateTime endDate)
        {
            return new LeadReport
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalLeads = GetTotalLeads(startDate, endDate),
                QualifiedLeads = GetQualifiedLeads(startDate, endDate),
                ConversionRate = CalculateLeadConversionRate(startDate, endDate),
                AverageQualificationTime = CalculateAverageQualificationTime(startDate, endDate)
            };
        }

        public CampaignReport GenerateCampaignReport(Guid campaignId)
        {
            var campaign = campaignManager.GetCampaignMetrics(campaignId);
            return new CampaignReport
            {
                CampaignId = campaignId,
                LeadCount = campaign.LeadCount,
                ConversionCount = campaign.ConversionCount,
                ConversionRate = campaign.ConversionRate,
                ROI = campaign.ROI
            };
        }

        private decimal CalculateTotalRevenue(DateTime startDate, DateTime endDate)
        {
            // Implementation for calculating total revenue
            return 0;
        }

        private int GetOpportunityCount(DateTime startDate, DateTime endDate)
        {
            // Implementation for counting opportunities
            return 0;
        }

        private decimal CalculateWinRate(DateTime startDate, DateTime endDate)
        {
            // Implementation for calculating win rate
            return 0;
        }

        private decimal CalculateAverageDealSize(DateTime startDate, DateTime endDate)
        {
            // Implementation for calculating average deal size
            return 0;
        }

        private int GetTotalLeads(DateTime startDate, DateTime endDate)
        {
            // Implementation for counting total leads
            return 0;
        }

        private int GetQualifiedLeads(DateTime startDate, DateTime endDate)
        {
            // Implementation for counting qualified leads
            return 0;
        }

        private decimal CalculateLeadConversionRate(DateTime startDate, DateTime endDate)
        {
            // Implementation for calculating lead conversion rate
            return 0;
        }

        private TimeSpan CalculateAverageQualificationTime(DateTime startDate, DateTime endDate)
        {
            // Implementation for calculating average qualification time
            return TimeSpan.Zero;
        }
    }

    public class NotificationService
    {
        private readonly Dictionary<Guid, List<NotificationSubscription>> subscriptions;
        private readonly Queue<Notification> notificationQueue;
        private readonly INotificationSender emailSender;
        private readonly INotificationSender smsSender;

        public NotificationService()
        {
            subscriptions = new Dictionary<Guid, List<NotificationSubscription>>();
            notificationQueue = new Queue<Notification>();
            emailSender = new EmailNotificationSender();
            smsSender = new SMSNotificationSender();
        }

        public void Subscribe(Guid userId, NotificationType type, string channel)
        {
            if (!subscriptions.ContainsKey(userId))
                subscriptions[userId] = new List<NotificationSubscription>();

            subscriptions[userId].Add(new NotificationSubscription(type, channel));
        }

        public void Notify(NotificationType type, string message, Guid? userId = null)
        {
            var notification = new Notification(type, message);
            
            if (userId.HasValue)
            {
                SendNotificationToUser(userId.Value, notification);
            }
            else
            {
                SendNotificationToAllSubscribers(notification);
            }
        }

        private void SendNotificationToUser(Guid userId, Notification notification)
        {
            if (!subscriptions.ContainsKey(userId))
                return;

            foreach (var subscription in subscriptions[userId])
            {
                if (subscription.Type == notification.Type)
                {
                    SendNotification(notification, subscription.Channel);
                }
            }
        }

        private void SendNotificationToAllSubscribers(Notification notification)
        {
            foreach (var userSubscriptions in subscriptions.Values)
            {
                foreach (var subscription in userSubscriptions)
                {
                    if (subscription.Type == notification.Type)
                    {
                        SendNotification(notification, subscription.Channel);
                    }
                }
            }
        }

        private void SendNotification(Notification notification, string channel)
        {
            switch (channel.ToLower())
            {
                case "email":
                    emailSender.Send(notification);
                    break;
                case "sms":
                    smsSender.Send(notification);
                    break;
            }
        }
    }

    public class WorkflowEngine
    {
        private readonly Dictionary<string, Workflow> workflows;
        private readonly NotificationService notificationService;
        private readonly AuditLogger auditLogger;

        public WorkflowEngine()
        {
            workflows = new Dictionary<string, Workflow>();
            notificationService = new NotificationService();
            auditLogger = new AuditLogger();
        }

        public void RegisterWorkflow(string name, Workflow workflow)
        {
            workflows[name] = workflow;
            auditLogger.LogAction("Workflow Registered", name);
        }

        public void ExecuteWorkflow(string name, WorkflowContext context)
        {
            if (!workflows.ContainsKey(name))
                throw new ArgumentException($"Workflow '{name}' not found");

            var workflow = workflows[name];
            try
            {
                workflow.Execute(context);
                auditLogger.LogAction("Workflow Executed", name);
            }
            catch (Exception ex)
            {
                auditLogger.LogAction("Workflow Failed", $"{name}: {ex.Message}");
                notificationService.Notify(NotificationType.Error, 
                    $"Workflow '{name}' failed: {ex.Message}");
                throw;
            }
        }
    }

    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Models and Context
public class WorkflowContext
{
    public Dictionary<string, object> Data { get; }
    public List<WorkflowAction> Actions { get; }
    public WorkflowStatus Status { get; private set; }
    public DateTime StartTime { get; }
    public DateTime? EndTime { get; private set; }
    public string WorkflowName { get; set; }
    public string Initiator { get; set; }
    public List<string> Logs { get; }

    public WorkflowContext(string workflowName, string initiator)
    {
        Data = new Dictionary<string, object>();
        Actions = new List<WorkflowAction>();
        Logs = new List<string>();
        WorkflowName = workflowName;
        Initiator = initiator;
        Status = WorkflowStatus.Pending;
        StartTime = DateTime.Now;
    }

    public void Complete()
    {
        Status = WorkflowStatus.Completed;
        EndTime = DateTime.Now;
        Logs.Add("Workflow completed.");
    }

    public void Fail(string reason)
    {
        Status = WorkflowStatus.Failed;
        EndTime = DateTime.Now;
        Data["FailureReason"] = reason;
        Logs.Add($"Workflow failed: {reason}");
    }

    public void Log(string message)
    {
        Logs.Add($"{DateTime.Now}: {message}");
    }

    public void SetData(string key, object value)
    {
        if (Data.ContainsKey(key))
            Data[key] = value;
        else
            Data.Add(key, value);
    }

    public T GetData<T>(string key)
    {
        if (Data.TryGetValue(key, out var value) && value is T typed)
            return typed;
        return default;
    }

    public void AddAction(WorkflowAction action)
    {
        Actions.Add(action);
        Log($"Action added: {action.Name}");
    }
}

// Workflow Base
public abstract class Workflow
{
    protected readonly List<WorkflowStep> steps;

    public string Name { get; set; }
    public string Description { get; set; }
    public TimeSpan Timeout { get; set; }

    protected Workflow(string name)
    {
        Name = name;
        Description = string.Empty;
        steps = new List<WorkflowStep>();
        Timeout = TimeSpan.FromMinutes(10);
    }

    public void AddStep(WorkflowStep step)
    {
        steps.Add(step);
    }

    public virtual void Execute(WorkflowContext context)
    {
        context.Log($"Executing workflow: {Name}");
        context.Status = WorkflowStatus.InProgress;

        foreach (var step in steps)
        {
            try
            {
                context.Log($"Starting step: {step.Name}");
                step.Execute(context);
                context.Log($"Finished step: {step.Name}");
            }
            catch (Exception ex)
            {
                context.Log($"Exception in step '{step.Name}': {ex.Message}");
                context.Fail(ex.Message);
                return;
            }
        }

        context.Complete();
    }

    public async Task ExecuteAsync(WorkflowContext context)
    {
        context.Log($"Async execution started for: {Name}");
        context.Status = WorkflowStatus.InProgress;

        foreach (var step in steps)
        {
            try
            {
                context.Log($"Starting async step: {step.Name}");
                await step.ExecuteAsync(context);
                context.Log($"Completed async step: {step.Name}");
            }
            catch (Exception ex)
            {
                context.Log($"Async step '{step.Name}' failed: {ex.Message}");
                context.Fail(ex.Message);
                return;
            }
        }

        context.Complete();
    }
}

// Step Base
public abstract class WorkflowStep
{
    public string Name { get; }
    public bool IsRequired { get; }
    public TimeSpan? DelayAfter { get; set; }

    protected WorkflowStep(string name, bool isRequired = true)
    {
        Name = name;
        IsRequired = isRequired;
    }

    public abstract void Execute(WorkflowContext context);

    public virtual Task ExecuteAsync(WorkflowContext context)
    {
        Execute(context);
        return Task.CompletedTask;
    }
}

// Supporting Structures
public enum WorkflowStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success
}

public class WorkflowAction
{
    public string Name { get; set; }
    public DateTime PerformedAt { get; set; }
    public string PerformedBy { get; set; }
    public string Description { get; set; }

    public WorkflowAction(string name, string performedBy, string description)
    {
        Name = name;
        PerformedBy = performedBy;
        Description = description;
        PerformedAt = DateTime.Now;
    }
}

// Example Step
public class NotifyStep : WorkflowStep
{
    public NotificationType Type { get; }

    public NotifyStep(string name, NotificationType type) : base(name)
    {
        Type = type;
    }

    public override void Execute(WorkflowContext context)
    {
        context.Log($"Notification sent: {Type}");
    }
}

// Another Example Step
public class DataUpdateStep : WorkflowStep
{
    private readonly string _key;
    private readonly object _value;

    public DataUpdateStep(string name, string key, object value) : base(name)
    {
        _key = key;
        _value = value;
    }

    public override void Execute(WorkflowContext context)
    {
        context.SetData(_key, _value);
        context.Log($"Data key '{_key}' updated with value: {_value}");
    }
}
