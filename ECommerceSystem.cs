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

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents player symbols in the game.
/// </summary>
public enum PlayerSymbol
{
    X,
    O
}

/// <summary>
/// Represents the result of a game
/// </summary>
public enum GameResult
{
    Win,
    Loss,
    Draw,
    Unknown
}

/// <summary>
/// Holds information about a single game
/// </summary>
public class GameRecord
{
    public Guid GameId { get; }
    public List<MoveRecord> Moves { get; }
    public PlayerSymbol? Winner { get; set; }
    public DateTime GameDate { get; }
    public TimeSpan GameDuration { get; set; }
    public string Description { get; set; }
    public string GameMode { get; set; }

    public GameRecord()
    {
        GameId = Guid.NewGuid();
        Moves = new List<MoveRecord>();
        GameDate = DateTime.Now;
        GameDuration = TimeSpan.Zero;
        Description = string.Empty;
        GameMode = "Classic";
    }

    public void AddMove(MoveRecord move)
    {
        Moves.Add(move);
        UpdateGameDuration();
    }

    private void UpdateGameDuration()
    {
        if (Moves.Count < 2) return;
        GameDuration = Moves.Last().Timestamp - Moves.First().Timestamp;
    }

    public string GetSummary()
    {
        return $"Game ID: {GameId}\nDate: {GameDate}\nWinner: {Winner?.ToString() ?? "None"}\nMoves: {Moves.Count}\nDuration: {GameDuration.TotalSeconds:F2} sec\n";
    }

    public int GetMoveCountByPlayer(PlayerSymbol player) => Moves.Count(m => m.Player == player);

    public IEnumerable<MoveRecord> GetMovesByTurnOrder()
    {
        return Moves.OrderBy(m => m.MoveNumber);
    }

    public bool WasDraw() => Winner == null && Moves.Count >= 9;

    public bool IsValidGame()
    {
        return Moves.Count >= 5 && (Winner != null || WasDraw());
    }
}

/// <summary>
/// Represents a single move made during a game
/// </summary>
public class MoveRecord
{
    public PlayerSymbol Player { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public int MoveNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public string MoveEvaluation { get; set; }

    public MoveRecord()
    {
        Timestamp = DateTime.Now;
        MoveEvaluation = "Neutral";
    }

    public bool IsCenterMove() => Row == 1 && Column == 1;

    public bool IsCornerMove() => (Row == 0 || Row == 2) && (Column == 0 || Column == 2);

    public string FormatMove()
    {
        return $"#{MoveNumber}: Player {Player} moved to ({Row}, {Column}) at {Timestamp:T}";
    }
}

/// <summary>
/// Tracks statistical data about a player's performance
/// </summary>
public class PlayerStats
{
    public string PlayerName { get; set; }
    public int TotalGames { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public int TotalMoves { get; set; }
    public int CenterMoves { get; set; }
    public int CornerMoves { get; set; }

    public decimal AverageMovesPerGame => TotalGames == 0 ? 0 : (decimal)TotalMoves / TotalGames;

    public double WinRate => TotalGames == 0 ? 0 : (double)Wins / TotalGames;

    public void RecordGame(GameResult result, List<MoveRecord> moves)
    {
        TotalGames++;
        switch (result)
        {
            case GameResult.Win: Wins++; break;
            case GameResult.Loss: Losses++; break;
            case GameResult.Draw: Draws++; break;
        }

        TotalMoves += moves.Count;
        CenterMoves += moves.Count(m => m.IsCenterMove());
        CornerMoves += moves.Count(m => m.IsCornerMove());
    }

    public void PrintSummary()
    {
        Console.WriteLine($"Player: {PlayerName}");
        Console.WriteLine($"Games: {TotalGames}, Wins: {Wins}, Losses: {Losses}, Draws: {Draws}");
        Console.WriteLine($"Avg. Moves/Game: {AverageMovesPerGame:F2}, Win Rate: {WinRate:P}");
        Console.WriteLine($"Center Moves: {CenterMoves}, Corner Moves: {CornerMoves}");
    }
}

/// <summary>
/// Tracks the game board state and validates moves
/// </summary>
public class GameBoardState
{
    private readonly PlayerSymbol?[,] _board;

    public GameBoardState()
    {
        _board = new PlayerSymbol?[3, 3];
    }

    public bool ApplyMove(MoveRecord move)
    {
        if (_board[move.Row, move.Column] != null)
            return false;

        _board[move.Row, move.Column] = move.Player;
        return true;
    }

    public PlayerSymbol? CheckWinner()
    {
        for (int i = 0; i < 3; i++)
        {
            // Rows and columns
            if (_board[i, 0] != null && _board[i, 0] == _board[i, 1] && _board[i, 1] == _board[i, 2])
                return _board[i, 0];

            if (_board[0, i] != null && _board[0, i] == _board[1, i] && _board[1, i] == _board[2, i])
                return _board[0, i];
        }

        // Diagonals
        if (_board[0, 0] != null && _board[0, 0] == _board[1, 1] && _board[1, 1] == _board[2, 2])
            return _board[0, 0];

        if (_board[0, 2] != null && _board[0, 2] == _board[1, 1] && _board[1, 1] == _board[2, 0])
            return _board[0, 2];

        return null;
    }

    public void PrintBoard()
    {
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                var symbol = _board[r, c]?.ToString() ?? ".";
                Console.Write($"{symbol} ");
            }
            Console.WriteLine();
        }
    }
}

/// <summary>
/// Logs game events such as start, moves, and results
/// </summary>
public class GameEventLog
{
    private readonly List<string> _logs = new();

    public void Log(string message)
    {
        _logs.Add($"{DateTime.Now:T} - {message}");
    }

    public void Print()
    {
        Console.WriteLine("=== Game Log ===");
        foreach (var entry in _logs)
        {
            Console.WriteLine(entry);
        }
    }

    public void Clear() => _logs.Clear();
}

/// <summary>
/// Aggregates and analyzes multiple game records
/// </summary>
public class GameAnalytics
{
    public List<GameRecord> Games { get; set; } = new();

    public void AddGame(GameRecord game)
    {
        if (game.IsValidGame())
            Games.Add(game);
    }

    public int TotalGames => Games.Count;

    public double AverageMoves => Games.Count == 0 ? 0 : Games.Average(g => g.Moves.Count);

    public double AverageDurationSeconds => Games.Count == 0 ? 0 : Games.Average(g => g.GameDuration.TotalSeconds);

    public void PrintStats()
    {
        Console.WriteLine($"Total Games: {TotalGames}");
        Console.WriteLine($"Avg. Moves: {AverageMoves:F2}");
        Console.WriteLine($"Avg. Duration: {AverageDurationSeconds:F1} sec");

        var mostMoves = Games.OrderByDescending(g => g.Moves.Count).FirstOrDefault();
        if (mostMoves != null)
        {
            Console.WriteLine($"Game with most moves: {mostMoves.GameId} ({mostMoves.Moves.Count} moves)");
        }
    }
}


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