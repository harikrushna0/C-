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