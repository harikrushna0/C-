using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a product in the store.
/// </summary>
public interface IProduct {
    string Id { get; }
    string Name { get; }
    decimal Price { get; }
    string Category { get; }
    string Description { get; }
    decimal Weight { get; }
}

/// <summary>
/// Represents a customer's order.
/// </summary>
public interface IOrder {
    string Id { get; }
    List<(string ProductId, int Quantity)> Products { get; }
    string Status { get; set; }
    DateTime CreatedAt { get; }
    decimal TotalPrice { get; }
    string CustomerId { get; }
}

/// <summary>
/// Represents a product discount.
/// </summary>
public interface IDiscount {
    string Id { get; }
    string ProductId { get; }
    decimal Percentage { get; }
    bool IsActive { get; }
}

/// <summary>
/// Represents shipping information.
/// </summary>
public interface IShippingInfo {
    string Address { get; set; }
    string City { get; set; }
    string Country { get; set; }
    string PostalCode { get; set; }
}

/// <summary>
/// The base product class.
/// </summary>
public class Product : IProduct {
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public decimal Weight { get; set; }

    public override string ToString() =>
        $"{Name} ({Category}): {Price:C2}";
}

/// <summary>
/// Represents an order containing multiple products.
/// </summary>
public class Order : IOrder {
    public string Id { get; set; }
    public List<(string ProductId, int Quantity)> Products { get; set; } = new();
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CustomerId { get; set; }

    public decimal TotalPrice { get; private set; }

    public void CalculateTotal(IEnumerable<Product> allProducts) {
        TotalPrice = Products.Sum(p => {
            var prod = allProducts.FirstOrDefault(x => x.Id == p.ProductId);
            return prod != null ? prod.Price * p.Quantity : 0;
        });
    }

    public void AddProduct(string productId, int quantity) {
        Products.Add((productId, quantity));
    }

    public void RemoveProduct(string productId) {
        Products.RemoveAll(p => p.ProductId == productId);
    }
}

/// <summary>
/// Represents an inventory item.
/// </summary>
public class InventoryItem {
    public Product Product { get; set; }
    public int Quantity { get; set; }

    public bool IsInStock => Quantity > 0;

    public void ReduceStock(int amount) {
        if (Quantity >= amount) {
            Quantity -= amount;
        } else {
            throw new InvalidOperationException("Insufficient stock.");
        }
    }

    public void Restock(int amount) {
        Quantity += amount;
    }
}

/// <summary>
/// A concrete discount model.
/// </summary>
public class Discount : IDiscount {
    public string Id { get; set; }
    public string ProductId { get; set; }
    public decimal Percentage { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Customer shipping info.
/// </summary>
public class ShippingInfo : IShippingInfo {
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }

    public override string ToString() =>
        $"{Address}, {City}, {Country}, {PostalCode}";
}

/// <summary>
/// Represents a customer.
/// </summary>
public class Customer {
    public string Id { get; set; }
    public string FullName { get; set; }
    public IShippingInfo ShippingInfo { get; set; }
    public List<Order> Orders { get; set; } = new();

    public void PlaceOrder(Order order) {
        Orders.Add(order);
    }
}

/// <summary>
/// Order statuses.
/// </summary>
public enum OrderStatus {
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>
/// Inventory manager service.
/// </summary>
public class InventoryService {
    private readonly Dictionary<string, InventoryItem> _items = new();

    public void AddInventoryItem(InventoryItem item) {
        _items[item.Product.Id] = item;
    }

    public InventoryItem GetItem(string productId) {
        return _items.TryGetValue(productId, out var item) ? item : null;
    }

    public bool HasStock(string productId, int quantity) {
        var item = GetItem(productId);
        return item != null && item.Quantity >= quantity;
    }

    public void DeductStock(string productId, int quantity) {
        var item = GetItem(productId);
        if (item == null || item.Quantity < quantity)
            throw new InvalidOperationException("Not enough stock.");
        item.ReduceStock(quantity);
    }

    public List<InventoryItem> GetAllInventory() => _items.Values.ToList();
}

/// <summary>
/// Handles order processing.
/// </summary>
public class OrderService {
    private readonly InventoryService _inventoryService;

    public OrderService(InventoryService inventoryService) {
        _inventoryService = inventoryService;
    }

    public void ProcessOrder(Order order, List<Product> productCatalog) {
        foreach (var (productId, qty) in order.Products) {
            _inventoryService.DeductStock(productId, qty);
        }

        order.CalculateTotal(productCatalog);
        order.Status = OrderStatus.Confirmed.ToString();
    }
}

/// <summary>
/// Factory for creating products.
/// </summary>
public static class ProductFactory {
    public static Product Create(string name, decimal price, string category, string description, decimal weight) {
        return new Product {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Price = price,
            Category = category,
            Description = description,
            Weight = weight
        };
    }
}

/// <summary>
/// Validator utility.
/// </summary>
public static class Validator {
    public static bool ValidateProduct(Product p) =>
        !string.IsNullOrEmpty(p.Name) &&
        p.Price > 0 &&
        !string.IsNullOrEmpty(p.Category);
}

/// <summary>
/// Demo program usage.
/// </summary>
public static class Program {
    public static void Main() {
        var catalog = new List<Product> {
            ProductFactory.Create("Phone", 999.99m, "Electronics", "Smartphone with OLED display", 0.5m),
            ProductFactory.Create("Laptop", 1599.99m, "Computers", "Powerful laptop for work", 2.5m),
        };

        var inventory = new InventoryService();
        foreach (var product in catalog) {
            inventory.AddInventoryItem(new InventoryItem { Product = product, Quantity = 10 });
        }

        var order = new Order {
            Id = Guid.NewGuid().ToString(),
            CustomerId = "cust-001"
        };
        order.AddProduct(catalog[0].Id, 1);
        order.AddProduct(catalog[1].Id, 1);

        var orderService = new OrderService(inventory);
        orderService.ProcessOrder(order, catalog);

        Console.WriteLine($"Order processed. Total: {order.TotalPrice:C2} | Status: {order.Status}");
    }
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

    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== TIC-TAC-TOE ===");
            Console.Write("Play against AI? (y/n): ");
            bool singlePlayer = Console.ReadLine()?.Trim().ToLower() == "y";

            Game game = new Game(singlePlayer);
            game.Start();

            Console.WriteLine("Thanks for playing!");
        }
    }
}