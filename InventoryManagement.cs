using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InventoryManagementSystem
{
    // Enum for product category
    public enum ProductCategory
    {
        Electronics,
        Clothing,
        Groceries,
        Furniture,
        Other
    }

    // Enum for product status
    public enum ProductStatus
    {
        InStock,
        OutOfStock,
        Discontinued
    }

    // Class to represent a product
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public ProductCategory Category { get; set; }
        public ProductStatus Status { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public Product(int id, string name, string description, decimal price, int quantity, ProductCategory category)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            Quantity = quantity;
            Category = category;
            Status = quantity > 0 ? ProductStatus.InStock : ProductStatus.OutOfStock;
            AddedDate = DateTime.Now;
        }
    }

    // Class to represent a supplier
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Address { get; set; }
        public DateTime RegisteredDate { get; set; }

        public Supplier(int id, string name, string contactEmail, string contactPhone, string address)
        {
            Id = id;
            Name = name;
            ContactEmail = contactEmail;
            ContactPhone = contactPhone;
            Address = address;
            RegisteredDate = DateTime.Now;
        }
    }

    // Class to represent a purchase order
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int SupplierId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public bool IsDelivered { get; set; }

        public PurchaseOrder(int id, int productId, int supplierId, int quantity, decimal totalCost, DateTime orderDate)
        {
            Id = id;
            ProductId = productId;
            SupplierId = supplierId;
            Quantity = quantity;
            TotalCost = totalCost;
            OrderDate = orderDate;
            IsDelivered = false;
        }
    }

    // Class to manage inventory
    public class InventoryManager
    {
        private List<Product> products;
        private List<Supplier> suppliers;
        private List<PurchaseOrder> purchaseOrders;
        private readonly string dataFilePath;

        public InventoryManager(string filePath)
        {
            dataFilePath = filePath;
            products = new List<Product>();
            suppliers = new List<Supplier>();
            purchaseOrders = new List<PurchaseOrder>();
            LoadData();
        }

        // Add a new product
        public void AddProduct(string name, string description, decimal price, int quantity, ProductCategory category)
        {
            int newId = products.Any() ? products.Max(p => p.Id) + 1 : 1;
            var product = new Product(newId, name, description, price, quantity, category);
            products.Add(product);
            SaveData();
            Console.WriteLine($"Product '{name}' added with ID {newId}.");
        }

        // Update an existing product
        public void UpdateProduct(int id, string name, string description, decimal price, int quantity, ProductCategory category)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                Console.WriteLine($"Product with ID {id} not found.");
                return;
            }

            product.Name = name;
            product.Description = description;
            product.Price = price;
            product.Quantity = quantity;
            product.Category = category;
            product.Status = quantity > 0 ? ProductStatus.InStock : ProductStatus.OutOfStock;
            product.UpdatedDate = DateTime.Now;
            SaveData();
            Console.WriteLine($"Product with ID {id} updated.");
        }

        // Delete a product
        public void DeleteProduct(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                Console.WriteLine($"Product with ID {id} not found.");
                return;
            }

            products.Remove(product);
            SaveData();
            Console.WriteLine($"Product with ID {id} deleted.");
        }

        // Add a new supplier
        public void AddSupplier(string name, string contactEmail, string contactPhone, string address)
        {
            int newId = suppliers.Any() ? suppliers.Max(s => s.Id) + 1 : 1;
            var supplier = new Supplier(newId, name, contactEmail, contactPhone, address);
            suppliers.Add(supplier);
            SaveData();
            Console.WriteLine($"Supplier '{name}' added with ID {newId}.");
        }

        // Update an existing supplier
        public void UpdateSupplier(int id, string name, string contactEmail, string contactPhone, string address)
        {
            var supplier = suppliers.FirstOrDefault(s => s.Id == id);
            if (supplier == null)
            {
                Console.WriteLine($"Supplier with ID {id} not found.");
                return;
            }

            supplier.Name = name;
            supplier.ContactEmail = contactEmail;
            supplier.ContactPhone = contactPhone;
            supplier.Address = address;
            SaveData();
            Console.WriteLine($"Supplier with ID {id} updated.");
        }

        // Delete a supplier
        public void DeleteSupplier(int id)
        {
            var supplier = suppliers.FirstOrDefault(s => s.Id == id);
            if (supplier == null)
            {
                Console.WriteLine($"Supplier with ID {id} not found.");
                return;
            }

            suppliers.Remove(supplier);
            SaveData();
            Console.WriteLine($"Supplier with ID {id} deleted.");
        }

        // Create a purchase order
        public void CreatePurchaseOrder(int productId, int supplierId, int quantity, decimal totalCost)
        {
            if (!products.Any(p => p.Id == productId))
            {
                Console.WriteLine($"Product with ID {productId} not found.");
                return;
            }
            if (!suppliers.Any(s => s.Id == supplierId))
            {
                Console.WriteLine($"Supplier with ID {supplierId} not found.");
                return;
            }

            int newId = purchaseOrders.Any() ? purchaseOrders.Max(po => po.Id) + 1 : 1;
            var order = new PurchaseOrder(newId, productId, supplierId, quantity, totalCost, DateTime.Now);
            purchaseOrders.Add(order);
            SaveData();
            Console.WriteLine($"Purchase order with ID {newId} created.");
        }

        // Mark purchase order as delivered
        public void MarkOrderDelivered(int orderId, int receivedQuantity)
        {
            var order = purchaseOrders.FirstOrDefault(po => po.Id == orderId);
            if (order == null)
            {
                Console.WriteLine($"Purchase order with ID {orderId} not found.");
                return;
            }

            order.IsDelivered = true;
            order.DeliveryDate = DateTime.Now;
            var product = products.FirstOrDefault(p => p.Id == order.ProductId);
            if (product != null)
            {
                product.Quantity += receivedQuantity;
                product.Status = product.Quantity > 0 ? ProductStatus.InStock : ProductStatus.OutOfStock;
                product.UpdatedDate = DateTime.Now;
            }
            SaveData();
            Console.WriteLine($"Purchase order with ID {orderId} marked as delivered.");
        }

        // List all products
        public void ListProducts()
        {
            if (!products.Any())
            {
                Console.WriteLine("No products available.");
                return;
            }

            Console.WriteLine("\nProduct List:");
            Console.WriteLine(new string('-', 60));
            foreach (var product in products)
            {
                Console.WriteLine($"ID: {product.Id}");
                Console.WriteLine($"Name: {product.Name}");
                Console.WriteLine($"Description: {product.Description}");
                Console.WriteLine($"Price: ${product.Price:F2}");
                Console.WriteLine($"Quantity: {product.Quantity}");
                Console.WriteLine($"Category: {product.Category}");
                Console.WriteLine($"Status: {product.Status}");
                Console.WriteLine($"Added: {product.AddedDate:yyyy-MM-dd}");
                Console.WriteLine($"Updated: {(product.UpdatedDate?.ToString("yyyy-MM-dd") ?? "N/A")}");
                Console.WriteLine(new string('-', 60));
            }
        }

        // List all suppliers
        public void ListSuppliers()
        {
            if (!suppliers.Any())
            {
                Console.WriteLine("No suppliers available.");
                return;
            }

            Console.WriteLine("\nSupplier List:");
            Console.WriteLine(new string('-', 60));
            foreach (var supplier in suppliers)
            {
                Console.WriteLine($"ID: {supplier.Id}");
                Console.WriteLine($"Name: {supplier.Name}");
                Console.WriteLine($"Email: {supplier.ContactEmail}");
                Console.WriteLine($"Phone: {supplier.ContactPhone}");
                Console.WriteLine($"Address: {supplier.Address}");
                Console.WriteLine($"Registered: {supplier.RegisteredDate:yyyy-MM-dd}");
                Console.WriteLine(new string('-', 60));
            }
        }

        // List all purchase orders
        public void ListPurchaseOrders()
        {
            if (!purchaseOrders.Any())
            {
                Console.WriteLine("No purchase orders available.");
                return;
            }

            Console.WriteLine("\nPurchase Order List:");
            Console.WriteLine(new string('-', 60));
            foreach (var order in purchaseOrders)
            {
                Console.WriteLine($"ID: {order.Id}");
                Console.WriteLine($"Product ID: {order.ProductId}");
                Console.WriteLine($"Supplier ID: {order.SupplierId}");
                Console.WriteLine($"Quantity: {order.Quantity}");
                Console.WriteLine($"Total Cost: ${order.TotalCost:F2}");
                Console.WriteLine($"Order Date: {order.OrderDate:yyyy-MM-dd}");
                Console.WriteLine($"Delivery Date: {(order.DeliveryDate?.ToString("yyyy-MM-dd") ?? "N/A")}");
                Console.WriteLine($"Delivered: {(order.IsDelivered ? "Yes" : "No")}");
                Console.WriteLine(new string('-', 60));
            }
        }

        // Search products by name
        public void SearchProducts(string keyword)
        {
            var results = products.Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!results.Any())
            {
                Console.WriteLine($"No products found with keyword '{keyword}'.");
                return;
            }

            Console.WriteLine($"\nProducts matching '{keyword}':");
            Console.WriteLine(new string('-', 60));
            foreach (var product in results)
            {
                Console.WriteLine($"ID: {product.Id}");
                Console.WriteLine($"Name: {product.Name}");
                Console.WriteLine($"Price: ${product.Price:F2}");
                Console.WriteLine($"Quantity: {product.Quantity}");
                Console.WriteLine(new string('-', 60));
            }
        }

        // Save data to CSV files
        private void SaveData()
        {
            try
            {
                SaveProducts();
                SaveSuppliers();
                SavePurchaseOrders();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }

        private void SaveProducts()
        {
            var lines = new List<string> { "Id,Name,Description,Price,Quantity,Category,Status,AddedDate,UpdatedDate" };
            foreach (var product in products)
            {
                lines.Add($"{product.Id},\"{product.Name.Replace("\"", "\"\"")}\"," +
                          $"\"{product.Description.Replace("\"", "\"\"")}\"," +
                          $"{product.Price},{product.Quantity},{product.Category},{product.Status}," +
                          $"{product.AddedDate:yyyy-MM-dd},{product.UpdatedDate?.ToString("yyyy-MM-dd") ?? ""}");
            }
            File.WriteAllLines(Path.Combine(dataFilePath, "products.csv"), lines);
        }

        private void SaveSuppliers()
        {
            var lines = new List<string> { "Id,Name,ContactEmail,ContactPhone,Address,RegisteredDate" };
            foreach (var supplier in suppliers)
            {
                lines.Add($"{supplier.Id},\"{supplier.Name.Replace("\"", "\"\"")}\"," +
                          $"\"{supplier.ContactEmail}\",\"{supplier.ContactPhone}\"," +
                          $"\"{supplier.Address.Replace("\"", "\"\"")}\"," +
                          $"{supplier.RegisteredDate:yyyy-MM-dd}");
            }
            File.WriteAllLines(Path.Combine(dataFilePath, "suppliers.csv"), lines);
        }

        private void SavePurchaseOrders()
        {
            var lines = new List<string> { "Id,ProductId,SupplierId,Quantity,TotalCost,OrderDate,DeliveryDate,IsDelivered" };
            foreach (var order in purchaseOrders)
            {
                lines.Add($"{order.Id},{order.ProductId},{order.SupplierId},{order.Quantity}," +
                          $"{order.TotalCost},{order.OrderDate:yyyy-MM-dd}," +
                          $"{(order.DeliveryDate?.ToString("yyyy-MM-dd") ?? "")},{order.IsDelivered}");
            }
            File.WriteAllLines(Path.Combine(dataFilePath, "purchase_orders.csv"), lines);
        }

        // Load data from CSV files
        private void LoadData()
        {
            try
            {
                LoadProducts();
                LoadSuppliers();
                LoadPurchaseOrders();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void LoadProducts()
        {
            string filePath = Path.Combine(dataFilePath, "products.csv");
            if (!File.Exists(filePath)) return;

            var lines = File.ReadAllLines(filePath).Skip(1);
            foreach (var line in lines)
            {
                var parts = ParseCsvLine(line);
                if (parts.Length < 9) continue;

                if (int.TryParse(parts[0], out int id) &&
                    decimal.TryParse(parts[3], out decimal price) &&
                    int.TryParse(parts[4], out int quantity) &&
                    Enum.TryParse(parts[5], out ProductCategory category) &&
                    Enum.TryParse(parts[6], out ProductStatus status) &&
                    DateTime.TryParse(parts[7], out DateTime addedDate))
                {
                    DateTime? updatedDate = string.IsNullOrEmpty(parts[8]) ? null : DateTime.Parse(parts[8]);
                    products.Add(new Product(id, parts[1], parts[2], price, quantity, category)
                    {
                        Status = status,
                        AddedDate = addedDate,
                        UpdatedDate = updatedDate
                    });
                }
            }
        }

        private void LoadSuppliers()
        {
            string filePath = Path.Combine(dataFilePath, "suppliers.csv");
            if (!File.Exists(filePath)) return;

            var lines = File.ReadAllLines(filePath).Skip(1);
            foreach (var line in lines)
            {
                var parts = ParseCsvLine(line);
                if (parts.Length < 6) continue;

                if (int.TryParse(parts[0], out int id) &&
                    DateTime.TryParse(parts[5], out DateTime registeredDate))
                {
                    suppliers.Add(new Supplier(id, parts[1], parts[2], parts[3], parts[4])
                    {
                        RegisteredDate = registeredDate
                    });
                }
            }
        }

        private void LoadPurchaseOrders()
        {
            string filePath = Path.Combine(dataFilePath, "purchase_orders.csv");
            if (!File.Exists(filePath)) return;

            var lines = File.ReadAllLines(filePath).Skip(1);
            foreach (var line in lines)
            {
                var parts = ParseCsvLine(line);
                if (parts.Length < 8) continue;

                if (int.TryParse(parts[0], out int id) &&
                    int.TryParse(parts[1], out int productId) &&
                    int.TryParse(parts[2], out int supplierId) &&
                    int.TryParse(parts[3], out int quantity) &&
                    decimal.TryParse(parts[4], out decimal totalCost) &&
                    DateTime.TryParse(parts[5], out DateTime orderDate) &&
                    bool.TryParse(parts[7], out bool isDelivered))
                {
                    DateTime? deliveryDate = string.IsNullOrEmpty(parts[6]) ? null : DateTime.Parse(parts[6]);
                    purchaseOrders.Add(new PurchaseOrder(id, productId, supplierId, quantity, totalCost, orderDate)
                    {
                        DeliveryDate = deliveryDate,
                        IsDelivered = isDelivered
                    });
                }
            }
        }

        // Parse CSV line with proper handling of quoted fields
        private string[] ParseCsvLine(string line)
        {
            var regex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            var parts = regex.Split(line);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim('"').Replace("\"\"", "\"");
            }
            return parts;
        }
    }

    // Class to handle user interface
    public class UserInterface
    {
        private readonly InventoryManager inventoryManager;

        public UserInterface(InventoryManager manager)
        {
            inventoryManager = manager;
        }

        // Display main menu
        public void ShowMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Inventory Management System");
                Console.WriteLine("==========================");
                Console.WriteLine("1. Add Product");
                Console.WriteLine("2. Update Product");
                Console.WriteLine("3. Delete Product");
                Console.WriteLine("4. List Products");
                Console.WriteLine("5. Add Supplier");
                Console.WriteLine("6. Update Supplier");
                Console.WriteLine("7. Delete Supplier");
                Console.WriteLine("8. List Suppliers");
                Console.WriteLine("9. Create Purchase Order");
                Console.WriteLine("10. Mark Order Delivered");
                Console.WriteLine("11. List Purchase Orders");
                Console.WriteLine("12. Search Products");
                Console.WriteLine("13. Exit");
                Console.WriteLine("==========================");
                Console.Write("Select an option (1-13): ");

                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        AddProductMenu();
                        break;
                    case "2":
                        UpdateProductMenu();
                        break;
                    case "3":
                        DeleteProductMenu();
                        break;
                    case "4":
                        inventoryManager.ListProducts();
                        Pause();
                        break;
                    case "5":
                        AddSupplierMenu();
                        break;
                    case "6":
                        UpdateSupplierMenu();
                        break;
                    case "7":
                        DeleteSupplierMenu();
                        break;
                    case "8":
                        inventoryManager.ListSuppliers();
                        Pause();
                        break;
                    case "9":
                        CreatePurchaseOrderMenu();
                        break;
                    case "10":
                        MarkOrderDeliveredMenu();
                        break;
                    case "11":
                        inventoryManager.ListPurchaseOrders();
                        Pause();
                        break;
                    case "12":
                        SearchProductsMenu();
                        break;
                    case "13":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        Pause();
                        break;
                }
            }
        }

        // Menu to add a product
        private void AddProductMenu()
        {
            Console.Clear();
            Console.WriteLine("Add New Product");
            Console.WriteLine("---------------");

            Console.Write("Name: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name cannot be empty.");
                Pause();
                return;
            }

            Console.Write("Description: ");
            string description = Console.ReadLine();

            Console.Write("Price: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price < 0)
            {
                Console.WriteLine("Invalid price.");
                Pause();
                return;
            }

            Console.Write("Quantity: ");
            if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity < 0)
            {
                Console.WriteLine("Invalid quantity.");
                Pause();
                return;
            }

            Console.WriteLine("Category (0=Electronics, 1=Clothing, 2=Groceries, 3=Furniture, 4=Other): ");
            if (!Enum.TryParse(Console.ReadLine(), out ProductCategory category) || !Enum.IsDefined(typeof(ProductCategory), category))
            {
                Console.WriteLine("Invalid category.");
                Pause();
                return;
            }

            inventoryManager.AddProduct(name, description, price, quantity, category);
            Pause();
        }

        // Menu to update a product
        private void UpdateProductMenu()
        {
            Console.Clear();
            Console.WriteLine("Update Product");
            Console.WriteLine("--------------");

            Console.Write("Product ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                Pause();
                return;
            }

            Console.Write("New Name: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name cannot be empty.");
                Pause();
                return;
            }

            Console.Write("New Description: ");
            string description = Console.ReadLine();

            Console.Write("New Price: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price < 0)
            {
                Console.WriteLine("Invalid price.");
                Pause();
                return;
            }

            Console.Write("New Quantity: ");
            if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity < 0)
            {
                Console.WriteLine("Invalid quantity.");
                Pause();
                return;
            }

            Console.WriteLine("New Category (0=Electronics, 1=Clothing, 2=Groceries, 3=Furniture, 4=Other): ");
            if (!Enum.TryParse(Console.ReadLine(), out ProductCategory category) || !Enum.IsDefined(typeof(ProductCategory), category))
            {
                Console.WriteLine("Invalid category.");
                Pause();
                return;
            }

            inventoryManager.UpdateProduct(id, name, description, price, quantity, category);
            Pause();
        }

        // Menu to delete a product
        private void DeleteProductMenu()
        {
            Console.Clear();
            Console.WriteLine("Delete Product");
            Console.WriteLine("--------------");

            Console.Write("Product ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                Pause();
                return;
            }

            inventoryManager.DeleteProduct(id);
            Pause();
        }

        // Menu to add a supplier
        private void AddSupplierMenu()
        {
            Console.Clear();
            Console.WriteLine("Add New Supplier");
            Console.WriteLine("----------------");

            Console.Write("Name: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name cannot be empty.");
                Pause();
                return;
            }

            Console.Write("Contact Email: ");
            string email = Console.ReadLine();
            if (!IsValidEmail(email))
            {
                Console.WriteLine("Invalid email format.");
                Pause();
                return;
            }

            Console.Write("Contact Phone: ");
            string phone = Console.ReadLine();

            Console.Write("Address: ");
            string address = Console.ReadLine();

            inventoryManager.AddSupplier(name, email, phone, address);
            Pause();
        }

        // Menu to update a supplier
        private void UpdateSupplierMenu()
        {
            Console.Clear();
            Console.WriteLine("Update Supplier");
            Console.WriteLine("---------------");

            Console.Write("Supplier ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                Pause();
                return;
            }

            Console.Write("New Name: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name cannot be empty.");
                Pause();
                return;
            }

            Console.Write("New Contact Email: ");
            string email = Console.ReadLine();
            if (!IsValidEmail(email))
            {
                Console.WriteLine("Invalid email format.");
                Pause();
                return;
            }

            Console.Write("New Contact Phone: ");
            string phone = Console.ReadLine();

            Console.Write("New Address: ");
            string address = Console.ReadLine();

            inventoryManager.UpdateSupplier(id, name, email, phone, address);
            Pause();
        }

        // Menu to delete a supplier
        private void DeleteSupplierMenu()
        {
            Console.Clear();
            Console.WriteLine("Delete Supplier");
            Console EDUCATOR.WriteLine("---------------");

            Console.Write("Supplier ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                Pause();
                return;
            }

            inventoryManager.DeleteSupplier(id);
            Pause();
        }

        // Menu to create a purchase order
        private void CreatePurchaseOrderMenu()
        {
            Console.Clear();
            Console.WriteLine("Create Purchase Order");
            Console.WriteLine("---------------------");

            Console.Write("Product ID: ");
            if (!int.TryParse(Console.ReadLine(), out int productId))
            {
                Console.WriteLine("Invalid product ID.");
                Pause();
                return;
            }

            Console.Write("Supplier ID: ");
            if (!int.TryParse(Console.ReadLine(), out int supplierId))
            {
                Console.WriteLine("Invalid supplier ID.");
                Pause();
                return;
            }

            Console.Write("Quantity: ");
            if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
            {
                Console.WriteLine("Invalid quantity.");
                Pause();
                return;
            }

            Console.Write("Total Cost: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal totalCost) || totalCost < 0)
            {
                Console.WriteLine("Invalid total cost.");
                Pause();
                return;
            }

            inventoryManager.CreatePurchaseOrder(productId, supplierId, quantity, totalCost);
            Pause();
        }

        // Menu to mark order as delivered
        private void MarkOrderDeliveredMenu()
        {
            Console.Clear();
            Console.WriteLine("Mark Order Delivered");
            Console.WriteLine("--------------------");

            Console.Write("Order ID: ");
            if (!int.TryParse(Console.ReadLine(), out int orderId))
            {
                Console.WriteLine("Invalid order ID.");
                Pause();
                return;
            }

            Console.Write("Received Quantity: ");
            if (!int.TryParse(Console.ReadLine(), out int receivedQuantity) || receivedQuantity < 0)
            {
                Console.WriteLine("Invalid quantity.");
                Pause();
                return;
            }

            inventoryManager.MarkOrderDelivered(orderId, receivedQuantity);
            Pause();
        }

        // Menu to search products
        private void SearchProductsMenu()
        {
            Console.Clear();
            Console.WriteLine("Search Products");
            Console.WriteLine("---------------");

            Console.Write("Enter keyword: ");
            string keyword = Console.ReadLine();
            inventoryManager.SearchProducts(keyword);
            Pause();
        }

        // Pause and wait for user input
        private void Pause()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        // Validate email format
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
    }

    // Utility classes for validation and formatting
    namespace InventoryManagementSystem.Utilities
    {
        public static class ValidationHelper
        {
            public static bool IsValidName(string name)
            {
                return !string.IsNullOrWhiteSpace(name) && name.Length <= 100;
            }

            public static bool IsValidPrice(decimal price)
            {
                return price >= 0;
            }

            public static bool IsValidQuantity(int quantity)
            {
                return quantity >= 0;
            }

            public static bool IsValidCategory(int category)
            {
                return Enum.IsDefined(typeof(ProductCategory), category);
            }

            public static bool IsValidPhone(string phone)
            {
                return string.IsNullOrEmpty(phone) || Regex.IsMatch(phone, @"^\+?\d{10,15}$");
            }
        }

        public static class DisplayHelper
        {
            public static void ShowHeader(string title)
            {
                Console.Clear();
                Console.WriteLine(new string('=', 50));
                Console.WriteLine(title.ToUpper());
                Console.WriteLine(new string('=', 50));
                Console.WriteLine();
            }

            public static void ShowError(string message)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {message}");
                Console.ResetColor();
            }

            public static void ShowSuccess(string message)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"SUCCESS: {message}");
                Console.ResetColor();
            }
        }

        public static class InputHelper
        {
            public static string GetStringInput(string prompt)
            {
                Console.Write(prompt);
                return Console.ReadLine();
            }

            public static int? GetIntInput(string prompt)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int result))
                    return result;
                return null;
            }

            public static decimal? GetDecimalInput(string prompt)
            {
                Console.Write(prompt);
                if (decimal.TryParse(Console.ReadLine(), out decimal result))
                    return result;
                return null;
            }
        }
    }

    // Utility classes for reporting
    namespace InventoryManagementSystem.Reports
    {
        public static class InventoryReports
        {
            public static void LowStockReport(List<Product> products, int threshold)
            {
                var lowStock = products.Where(p => p.Quantity <= threshold && p.Status == ProductStatus.InStock).ToList();
                if (!lowStock.Any())
                {
                    Console.WriteLine("No products with low stock.");
                    return;
                }

                Console.WriteLine($"\nLow Stock Report (Threshold: {threshold}):");
                Console.WriteLine(new string('-', 60));
                foreach (var product in lowStock)
                {
                    Console.WriteLine($"ID: {product.Id}");
                    Console.WriteLine($"Name: {product.Name}");
                    Console.WriteLine($"Quantity: {product.Quantity}");
                    Console.WriteLine($"Category: {product.Category}");
                    Console.WriteLine(new string('-', 60));
                }
            }

            public static void CategorySummary(List<Product> products)
            {
                var summary = products.GroupBy(p => p.Category)
                                     .Select(g => new { Category = g.Key, Count = g.Count(), TotalValue = g.Sum(p => p.Price * p.Quantity) })
                                     .ToList();

                Console.WriteLine("\nCategory Summary:");
                Console.WriteLine(new string('-', 60));
                foreach (var item in summary)
                {
                    Console.WriteLine($"Category: {item.Category}");
                    Console.WriteLine($"Product Count: {item.Count}");
                    Console.WriteLine($"Total Value: ${item.TotalValue:F2}");
                    Console.WriteLine(new string('-', 60));
                }
            }
        }
    }

    // Utility classes for sorting
    namespace InventoryManagementSystem.Sorting
    {
        public static class InventorySorter
        {
            public static List<Product> SortByPrice(List<Product> products, bool ascending = true)
            {
                return ascending ? products.OrderBy(p => p.Price).ToList() :
                                  products.OrderByDescending(p => p.Price).ToList();
            }

            public static List<Product> SortByQuantity(List<Product> products, bool ascending = true)
            {
                return ascending ? products.OrderBy(p => p.Quantity).ToList() :
                                  products.OrderByDescending(p => p.Quantity).ToList();
            }

            public static List<PurchaseOrder> SortByOrderDate(List<PurchaseOrder> orders, bool ascending = true)
            {
                return ascending ? orders.OrderBy(o => o.OrderDate).ToList() :
                                  orders.OrderByDescending(o => o.OrderDate).ToList();
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
    // Main program
    class Program
    {
        static void Main(string[] args)
        {
            string dataPath = Path.Combine(Environment.CurrentDirectory, "Data");
            Directory.CreateDirectory(dataPath);
            var inventoryManager = new InventoryManager(dataPath);
            var ui = new UserInterface(inventoryManager);
            ui.ShowMenu();
        }
    }
}