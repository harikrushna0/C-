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

// Inventory Management System
public class OrderItem {
    public string ProductId { get; set; }
    public int Quantity { get; set; }
}
public class InventoryItem {
    public Product Product { get; set; }
    public int Quantity { get; set; }
}
public class Inventory {
    public Product Product { get; set; }
    public int Quantity { get; set; }
}
public class Order {
    public string Id { get; set; }
    public List<OrderItem> Products { get; set; } = new();
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class Product {
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}
public class InventoryItem {
    public Product Product { get; set; }
    public int Quantity { get; set; }
}
public class Order {
    public string Id { get; set; }
    public List<OrderItem> Products { get; set; } = new();
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class Product {
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}
public class InventoryItem {
    public Product Product { get; set; }
    public int Quantity { get; set; }
}
public class Order {
    public string Id { get; set; }
    public List<OrderItem> Products { get; set; } = new();
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class Product {
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}
public class InventoryItem {
    public Product Product { get; set; }
    public int Quantity { get; set; }
}
// Inventory Service
public class InventoryService
{
    private readonly Repository<Product> _productRepo = new();
    private readonly Repository<Order> _orderRepo = new();
    private readonly List<InventoryItem> _inventory = new();

    public void AddProduct(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product), "Product cannot be null");

        if (string.IsNullOrWhiteSpace(product.Id))
            throw new ArgumentException("Product ID cannot be empty", nameof(product.Id));

        if (product.Price <= 0)
            throw new ArgumentException("Product price must be greater than zero", nameof(product.Price));

        if (_productRepo.Find(p => p.Id == product.Id) != null)
            throw new InvalidOperationException($"A product with ID '{product.Id}' already exists");

        _productRepo.Add(product);
    }

    public bool AddStock(string productId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero");

        var product = _productRepo.Find(p => p.Id == productId);
        if (product == null) return false;

        var item = _inventory.FirstOrDefault(i => i.Product.Id == productId);
        if (item != null)
        {
            item.Quantity += quantity;
        }
        else
        {
            _inventory.Add(new InventoryItem
            {
                Product = product,
                Quantity = quantity
            });
        }

        return true;
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
    public bool RemoveStock(string productId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID is required", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero");

        var item = _inventory.FirstOrDefault(i => i.Product.Id == productId);
        if (item == null || item.Quantity < quantity) return false;

        item.Quantity -= quantity;

        if (item.Quantity == 0)
        {
            _inventory.RemoveAll(i => i.Product.Id == productId);
        }

        return true;
    }

    public Order CreateOrder(List<(string ProductId, int Quantity)> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Order must contain at least one item", nameof(items));

        foreach (var (pid, qty) in items)
        {
            if (string.IsNullOrWhiteSpace(pid) || qty <= 0)
                throw new ArgumentException($"Invalid order item: ProductId='{pid}', Quantity={qty}");

            var invItem = _inventory.FirstOrDefault(i => i.Product.Id == pid);
            if (invItem == null || invItem.Quantity < qty)
                return null;
        }

        var order = new Order
        {
            Id = $"ord-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Products = new List<(string ProductId, int Quantity)>(items)
        };

        foreach (var (pid, qty) in items)
        {
            RemoveStock(pid, qty);
        }

        _orderRepo.Add(order);
        return order;
    }

    public IEnumerable<InventoryItem> GetAllInventory()
    {
        return _inventory.Select(item => new InventoryItem
        {
            Product = item.Product,
            Quantity = item.Quantity
        }).ToList();
    }

    public Product GetProductById(string productId)
    {
        return _productRepo.Find(p => p.Id == productId);
    }

    public IEnumerable<Order> GetAllOrders()
    {
        return _orderRepo.GetAll();
    }
}

}

namespace TaskManagement
{
    public enum TaskPriority { Low, Medium, High, Critical }
    public enum TaskStatus { Created, InProgress, OnHold, Completed, Cancelled }

    public class TaskItem
    {
        public Guid Id { get; private set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public string AssignedTo { get; set; }
        public List<Comment> Comments { get; private set; }
        public List<TaskHistory> History { get; private set; }
        public List<Attachment> Attachments { get; private set; }

        public TaskItem(string title, string description)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            CreatedDate = DateTime.Now;
            Status = TaskStatus.Created;
            Priority = TaskPriority.Medium;
            Comments = new List<Comment>();
            History = new List<TaskHistory>();
            Attachments = new List<Attachment>();
            AddHistory("Task created");
        }

        public void AddComment(string author, string content)
        {
            Comments.Add(new Comment(author, content));
            AddHistory($"Comment added by {author}");
        }

        public void AddAttachment(string fileName, byte[] content)
        {
            Attachments.Add(new Attachment(fileName, content));
            AddHistory($"Attachment added: {fileName}");
        }

        private void AddHistory(string description)
        {
            History.Add(new TaskHistory(description));
        }
    }

    public class Comment
    {
        public Guid Id { get; private set; }
        public string Author { get; private set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime? ModifiedDate { get; private set; }

        public Comment(string author, string content)
        {
            Id = Guid.NewGuid();
            Author = author;
            Content = content;
            CreatedDate = DateTime.Now;
        }

        public void UpdateContent(string newContent)
        {
            Content = newContent;
            ModifiedDate = DateTime.Now;
        }
    }

    public class Attachment
    {
        public Guid Id { get; private set; }
        public string FileName { get; private set; }
        public byte[] Content { get; private set; }
        public DateTime UploadDate { get; private set; }
        public long FileSize { get; private set; }

        public Attachment(string fileName, byte[] content)
        {
            Id = Guid.NewGuid();
            FileName = fileName;
            Content = content;
            UploadDate = DateTime.Now;
            FileSize = content.Length;
        }
    }

    public class TaskHistory
    {
        public Guid Id { get; private set; }
        public string Description { get; private set; }
        public DateTime Timestamp { get; private set; }

        public TaskHistory(string description)
        {
            Id = Guid.NewGuid();
            Description = description;
            Timestamp = DateTime.Now;
        }
    }

   public class TaskAnalytics
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public Dictionary<TaskPriority, int> TasksByPriority { get; set; }
    public TimeSpan? AverageCompletionTime { get; set; }
    public string MostActiveUser { get; set; }

    public override string ToString()
    {
        var avgTime = AverageCompletionTime.HasValue ? AverageCompletionTime.Value.ToString(@"hh\:mm\:ss") : "N/A";
        return $"Total Tasks: {TotalTasks}\nCompleted: {CompletedTasks}\nOverdue: {OverdueTasks}\n" +
               $"Average Completion: {avgTime}\nMost Active User: {MostActiveUser}";
    }
}

public class TaskManager
{
    private readonly List<TaskItem> tasks;
    private readonly Dictionary<string, List<TaskItem>> userTasks;

    public TaskManager()
    {
        tasks = new List<TaskItem>();
        userTasks = new Dictionary<string, List<TaskItem>>();
    }

    public TaskItem CreateTask(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty");

        var task = new TaskItem(title, description);
        tasks.Add(task);
        return task;
    }

    public void AssignTask(Guid taskId, string assignee)
    {
        if (string.IsNullOrWhiteSpace(assignee))
            throw new ArgumentException("Assignee cannot be empty");

        var task = GetTaskById(taskId);
        if (task == null)
            throw new ArgumentException("Task not found");

        task.AssignedTo = assignee;
        if (!userTasks.ContainsKey(assignee))
            userTasks[assignee] = new List<TaskItem>();

        if (!userTasks[assignee].Contains(task))
            userTasks[assignee].Add(task);

        task.History.Add(new TaskHistory($"Task assigned to {assignee}"));
    }

    public List<TaskItem> GetUserTasks(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return new List<TaskItem>();

        return userTasks.TryGetValue(username, out var userTaskList) ? userTaskList : new List<TaskItem>();
    }

    public List<TaskItem> GetTasksByStatus(TaskStatus status)
    {
        return tasks.Where(t => t.Status == status).ToList();
    }

    public List<TaskItem> GetTasksByPriority(TaskPriority priority)
    {
        return tasks.Where(t => t.Priority == priority).ToList();
    }

    public List<TaskItem> GetOverdueTasks()
    {
        var now = DateTime.Now;
        return tasks.Where(t =>
            t.DueDate.HasValue &&
            t.DueDate.Value < now &&
            t.Status != TaskStatus.Completed &&
            t.Status != TaskStatus.Cancelled
        ).ToList();
    }

    public void UpdateTaskStatus(Guid taskId, TaskStatus newStatus)
    {
        var task = GetTaskById(taskId);
        if (task == null)
            throw new ArgumentException("Task not found");

        var previousStatus = task.Status;
        if (previousStatus == newStatus)
            return;

        task.Status = newStatus;
        task.History.Add(new TaskHistory($"Status changed from {previousStatus} to {newStatus}"));

        if (newStatus == TaskStatus.Completed)
            task.CompletedDate = DateTime.Now;
    }

    private TaskItem GetTaskById(Guid taskId)
    {
        return tasks.FirstOrDefault(t => t.Id == taskId);
    }

    public TaskAnalytics GetAnalytics()
    {
        var analytics = new TaskAnalytics();
        analytics.TotalTasks = tasks.Count;
        analytics.CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Completed);
        analytics.OverdueTasks = GetOverdueTasks().Count;

        analytics.TasksByPriority = tasks
            .GroupBy(t => t.Priority)
            .ToDictionary(g => g.Key, g => g.Count());

        var completedTasks = tasks.Where(t => t.CompletedDate.HasValue && t.CreatedDate.HasValue).ToList();
        if (completedTasks.Count > 0)
        {
            var avgTicks = completedTasks
                .Average(t => (t.CompletedDate.Value - t.CreatedDate.Value).Ticks);

            analytics.AverageCompletionTime = TimeSpan.FromTicks((long)avgTicks);
        }

        analytics.MostActiveUser = userTasks
            .OrderByDescending(kv => kv.Value.Count)
            .Select(kv => kv.Key)
            .FirstOrDefault();

        return analytics;
    }
}

        public TaskAnalytics GetTaskAnalytics()
        {
            return new TaskAnalytics
            {
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Completed),
                OverdueTasks = GetOverdueTasks().Count,
                TasksByPriority = new Dictionary<TaskPriority, int>
                {
                    { TaskPriority.Low, tasks.Count(t => t.Priority == TaskPriority.Low) },
                    { TaskPriority.Medium, tasks.Count(t => t.Priority == TaskPriority.Medium) },
                    { TaskPriority.High, tasks.Count(t => t.Priority == TaskPriority.High) },
                    { TaskPriority.Critical, tasks.Count(t => t.Priority == TaskPriority.Critical) }
                },
                AverageCompletionTime = CalculateAverageCompletionTime(),
                MostActiveUser = GetMostActiveUser()
            };
        }

        private TimeSpan? CalculateAverageCompletionTime()
        {
            var completedTasks = tasks.Where(t => t.CompletedDate.HasValue);
            if (!completedTasks.Any()) return null;

            var totalTime = completedTasks.Sum(t => (t.CompletedDate.Value - t.CreatedDate).TotalMinutes);
            return TimeSpan.FromMinutes(totalTime / completedTasks.Count());
        }

        private string GetMostActiveUser()
        {
            return userTasks
                .OrderByDescending(ut => ut.Value.Count)
                .FirstOrDefault().Key;
        }
    }
}