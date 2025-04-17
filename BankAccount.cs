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

namespace BankingSystem
{
    public class BankAccount
    {
        private string accountNumber;
        private string accountHolder;
        private decimal balance;
        private readonly decimal minimumBalance = 100.00m;

        public BankAccount(string accountNumber, string accountHolder, decimal initialDeposit)
        {
            if (initialDeposit < minimumBalance)
                throw new ArgumentException($"Initial deposit must be at least {minimumBalance:C}");

            this.accountNumber = accountNumber;
            this.accountHolder = accountHolder;
            this.balance = initialDeposit;
        }

        public decimal Balance => balance;

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive");

            balance += amount;
            LogTransaction("Deposit", amount);
        }

        public bool Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive");

            if (balance - amount < minimumBalance)
            {
                Console.WriteLine("Insufficient funds!");
                return false;
            }

            balance -= amount;
            LogTransaction("Withdrawal", amount);
            return true;
        }

        private void LogTransaction(string type, decimal amount)
        {
            Console.WriteLine($"Transaction: {type}");
            Console.WriteLine($"Amount: {amount:C}");
            Console.WriteLine($"New Balance: {balance:C}");
            Console.WriteLine($"Date: {DateTime.Now}");
            Console.WriteLine("------------------------");
        }

        public override string ToString()
        {
            return $"Account: {accountNumber}\n" +
                   $"Holder: {accountHolder}\n" +
                   $"Balance: {balance:C}";
        }
    }
}