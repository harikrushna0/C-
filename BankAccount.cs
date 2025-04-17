using System;
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
using System;
using System.Collections.Generic;
using System.Linq;

// Domain Interfaces
public interface IEntity {
    string Id { get; }
}

public interface ILogService {
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception ex = null);
}

// Domain Models
public class Product : IEntity {
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}

public class Order : IEntity {
    public string Id { get; set; }
    public List<(string ProductId, int Quantity)> Products { get; set; } = new();
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class InventoryItem {
    public Product Product { get; set; }
    public int Quantity { get; set; }
}

// Simple Console Logger
public class ConsoleLogger : ILogService {
    public void Info(string message) => Console.WriteLine($"[INFO] {message}");
    public void Warn(string message) => Console.WriteLine($"[WARN] {message}");
    public void Error(string message, Exception ex = null) {
        Console.WriteLine($"[ERROR] {message}");
        if (ex != null) Console.WriteLine(ex);
    }
}

// Generic Repository
public class Repository<T> where T : class, IEntity {
    private readonly List<T> _items = new();

    public void Add(T item) {
        if (_items.Any(x => x.Id == item.Id)) {
            throw new InvalidOperationException("Duplicate item ID.");
        }
        _items.Add(item);
    }

    public T GetById(string id) => _items.FirstOrDefault(i => i.Id == id);
    public IEnumerable<T> GetAll() => _items.ToList();

    public T Find(Func<T, bool> predicate) => _items.FirstOrDefault(predicate);

    public void Update(string id, Action<T> updateAction) {
        var item = _items.FirstOrDefault(x => x.Id == id);
        if (item != null) {
            updateAction(item);
        }
    }

    public void Remove(string id) {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null) _items.Remove(item);
    }

    public int Count() => _items.Count;
}

// Inventory Service
public class InventoryService {
    private readonly Repository<Product> _productRepo = new();
    private readonly Repository<Order> _orderRepo = new();
    private readonly List<InventoryItem> _inventory = new();
    private readonly ILogService _logger;

    public InventoryService(ILogService logger) {
        _logger = logger;
    }

    public void AddProduct(Product product) {
        if (product == null) {
            _logger.Warn("Attempted to add null product.");
            return;
        }

        if (string.IsNullOrWhiteSpace(product.Id) || product.Price <= 0) {
            _logger.Error("Invalid product data");
            throw new ArgumentException("Invalid product data");
        }

        try {
            _productRepo.Add(product);
            _logger.Info($"Product added: {product.Id}");
        } catch (Exception ex) {
            _logger.Error("Failed to add product", ex);
        }
    }

    public bool AddStock(string productId, int quantity) {
        if (quantity <= 0) {
            _logger.Warn("Attempted to add zero or negative quantity.");
            return false;
        }

        var product = _productRepo.GetById(productId);
        if (product == null) {
            _logger.Warn($"Product {productId} not found.");
            return false;
        }

        var item = _inventory.FirstOrDefault(i => i.Product.Id == productId);
        if (item != null) {
            item.Quantity += quantity;
        } else {
            _inventory.Add(new InventoryItem { Product = product, Quantity = quantity });
        }

        _logger.Info($"Stock added for product {productId}: {quantity}");
        return true;
    }

    public bool RemoveStock(string productId, int quantity) {
        if (quantity <= 0) {
            _logger.Warn("Attempted to remove zero or negative quantity.");
            return false;
        }

        var item = _inventory.FirstOrDefault(i => i.Product.Id == productId);
        if (item == null || item.Quantity < quantity) {
            _logger.Warn($"Insufficient stock for product {productId}");
            return false;
        }

        item.Quantity -= quantity;
        if (item.Quantity == 0)
            _inventory.RemoveAll(i => i.Product.Id == productId);

        _logger.Info($"Stock removed for product {productId}: {quantity}");
        return true;
    }

    public Order CreateOrder(List<(string ProductId, int Quantity)> items) {
        if (items == null || !items.Any()) {
            _logger.Warn("Attempted to create order with no items.");
            return null;
        }

        foreach (var (pid, qty) in items) {
            var invItem = _inventory.FirstOrDefault(i => i.Product.Id == pid);
            if (invItem == null || invItem.Quantity < qty) {
                _logger.Warn($"Order creation failed: insufficient stock for {pid}");
                return null;
            }
        }

        var order = new Order {
            Id = $"ord-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Products = items
        };

        _orderRepo.Add(order);
        _logger.Info($"Order created: {order.Id}");
        return order;
    }

    public bool ProcessOrder(string orderId) {
        var order = _orderRepo.GetById(orderId);
        if (order == null || order.Status != "pending") {
            _logger.Warn($"Order not found or invalid status: {orderId}");
            return false;
        }

        foreach (var (pid, qty) in order.Products) {
            if (!RemoveStock(pid, qty)) {
                _logger.Warn($"Failed to process order: insufficient stock for {pid}");
                return false;
            }
        }

        _orderRepo.Update(orderId, o => o.Status = "completed");
        _logger.Info($"Order processed: {orderId}");
        return true;
    }

    public void PrintInventoryStatus() {
        _logger.Info("=== Inventory Status ===");
        foreach (var item in _inventory) {
            Console.WriteLine($"Product: {item.Product.Name}, Quantity: {item.Quantity}");
        }
    }

    public void PrintOrders() {
        _logger.Info("=== Orders ===");
        foreach (var order in _orderRepo.GetAll()) {
            Console.WriteLine($"Order: {order.Id}, Status: {order.Status}, Items: {order.Products.Count}");
        }
    }
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
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BankingSystem
{
    public enum AccountType
    {
        Savings,
        Checking,
        Business
    }

    public class Transaction
    {
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime Timestamp { get; set; }
        public string Note { get; set; }

        public override string ToString()
        {
            return $"{Timestamp:G} - {Type}: {Amount:C}, Balance: {BalanceAfter:C} - {Note}";
        }
    }

    public class BankAccount
    {
        private string accountNumber;
        private string accountHolder;
        private decimal balance;
        private readonly decimal minimumBalance;
        private List<Transaction> transactions;
        private AccountType accountType;
        private bool isFrozen;

        public string AccountNumber => accountNumber;
        public string AccountHolder => accountHolder;
        public decimal Balance => balance;
        public IReadOnlyList<Transaction> Transactions => transactions.AsReadOnly();
        public AccountType Type => accountType;
        public bool IsFrozen => isFrozen;

        public BankAccount(string accountNumber, string accountHolder, decimal initialDeposit, AccountType type = AccountType.Savings)
        {
            minimumBalance = 100.00m;
            if (initialDeposit < minimumBalance)
                throw new ArgumentException($"Initial deposit must be at least {minimumBalance:C}");

            this.accountNumber = accountNumber;
            this.accountHolder = accountHolder;
            this.balance = initialDeposit;
            this.accountType = type;
            this.transactions = new List<Transaction>();
            this.isFrozen = false;

            LogTransaction("Account Created", initialDeposit, "Initial deposit");
        }

        public void Deposit(decimal amount, string note = "Deposit")
        {
            if (isFrozen)
            {
                Notify("Account is frozen. Cannot deposit.");
                return;
            }

            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive");

            balance += amount;
            LogTransaction("Deposit", amount, note);
        }

        public bool Withdraw(decimal amount, string note = "Withdrawal")
        {
            if (isFrozen)
            {
                Notify("Account is frozen. Cannot withdraw.");
                return false;
            }

            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive");

            if (balance - amount < minimumBalance)
            {
                Notify("Insufficient funds after maintaining minimum balance.");
                return false;
            }

            balance -= amount;
            LogTransaction("Withdrawal", amount, note);
            return true;
        }

        public void FreezeAccount()
        {
            isFrozen = true;
            Notify("Account has been frozen.");
        }

        public void UnfreezeAccount()
        {
            isFrozen = false;
            Notify("Account has been unfrozen.");
        }

        public void ChangeAccountHolder(string newHolder)
        {
            if (string.IsNullOrWhiteSpace(newHolder))
                throw new ArgumentException("Account holder name cannot be empty.");
            accountHolder = newHolder;
            LogTransaction("Account Holder Updated", 0, $"Changed to {newHolder}");
        }

        public void PrintStatement()
        {
            Console.WriteLine($"--- Statement for Account {accountNumber} ---");
            foreach (var transaction in transactions)
                Console.WriteLine(transaction);
            Console.WriteLine($"Current Balance: {balance:C}");
            Console.WriteLine("---------------------------------------------");
        }

        private void LogTransaction(string type, decimal amount, string note)
        {
            transactions.Add(new Transaction
            {
                Type = type,
                Amount = amount,
                BalanceAfter = balance,
                Timestamp = DateTime.Now,
                Note = note
            });

            Console.WriteLine($"{type} of {amount:C} completed. New balance: {balance:C}");
        }

        private void Notify(string message)
        {
            Console.WriteLine($"[Notification] {message}");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Account Number: {accountNumber}");
            sb.AppendLine($"Account Holder: {accountHolder}");
            sb.AppendLine($"Account Type: {accountType}");
            sb.AppendLine($"Balance: {balance:C}");
            sb.AppendLine($"Frozen: {isFrozen}");
            return sb.ToString();
        }

        // Simulated external service integration
        public void ExportStatementToCsv(string path)
        {
            try
            {
                var csv = new StringBuilder();
                csv.AppendLine("Type,Amount,BalanceAfter,Timestamp,Note");

                foreach (var t in transactions)
                {
                    csv.AppendLine($"{t.Type},{t.Amount},{t.BalanceAfter},{t.Timestamp},{t.Note}");
                }

                System.IO.File.WriteAllText(path, csv.ToString());
                Notify($"Statement exported to {path}");
            }
            catch (Exception ex)
            {
                Notify($"Failed to export statement: {ex.Message}");
            }
        }

        // Simulated monthly interest application
        public void ApplyMonthlyInterest(decimal annualRatePercent)
        {
            if (annualRatePercent <= 0) return;

            var monthlyRate = annualRatePercent / 12 / 100;
            var interest = balance * (decimal)monthlyRate;
            balance += interest;

            LogTransaction("Interest", interest, $"Monthly interest applied ({annualRatePercent}% annual)");
        }

        public void CloseAccount()
        {
            LogTransaction("Account Closed", balance, "Closing account and withdrawing all funds");
            balance = 0;
            isFrozen = true;
        }

        public void RenameAccount(string newHolder)
        {
            ChangeAccountHolder(newHolder);
        }

        public void ResetTransactions()
        {
            transactions.Clear();
            LogTransaction("Transaction History Cleared", 0, "All transaction logs deleted");
        }

        public bool IsHighValueAccount(decimal threshold = 10000)
        {
            return balance >= threshold;
        }

        public void SetCustomMinimumBalance(decimal newMin)
        {
            if (newMin < 0) throw new ArgumentException("Minimum balance must be non-negative.");
            LogTransaction("Minimum Balance Changed", 0, $"Changed from {minimumBalance:C} to {newMin:C}");
        }
    }
}
