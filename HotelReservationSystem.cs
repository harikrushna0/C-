using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelReservationSystem
{
    // Enum for room type
    public enum RoomType
    {
        Single,
        Double,
        Suite
    }

    // Enum for room status
    public enum RoomStatus
    {
        Available,
        Occupied,
        Maintenance
    }

    // Enum for payment status
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }

    // Room class to represent a hotel room
    public class Room
    {
        public string RoomNumber { get; private set; }
        public RoomType Type { get; private set; }
        public decimal RatePerNight { get; private set; }
        public RoomStatus Status { get; private set; }
        public int MaxOccupancy { get; private set; }
        public string Description { get; private set; }

        public Room(string roomNumber, RoomType type, decimal ratePerNight, int maxOccupancy, string description)
        {
            RoomNumber = roomNumber;
            Type = type;
            RatePerNight = ratePerNight;
            Status = RoomStatus.Available;
            MaxOccupancy = maxOccupancy;
            Description = description;
        }

        public void UpdateStatus(RoomStatus status)
        {
            Status = status;
        }

        public void UpdateRate(decimal rate)
        {
            if (rate <= 0)
                throw new ArgumentException("Rate must be greater than zero.");
            RatePerNight = rate;
        }

        public override string ToString()
        {
            return $"Room: {RoomNumber}, Type: {Type}, Rate: ${RatePerNight}/night, Status: {Status}, Max Occupancy: {MaxOccupancy}, Description: {Description}";
        }
    }

    // Guest class to represent a hotel guest
    public class Guest
    {
        public string GuestId { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public List<Reservation> Reservations { get; private set; }

        public Guest(string guestId, string name, string email, string phone)
        {
            GuestId = guestId;
            Name = name;
            Email = email;
            Phone = phone;
            Reservations = new List<Reservation>();
        }

        public void UpdateContactInfo(string email, string phone)
        {
            Email = email;
            Phone = phone;
        }

        public override string ToString()
        {
            return $"ID: {GuestId}, Name: {Name}, Email: {Email}, Phone: {Phone}, Reservations: {Reservations.Count}";
        }
    }

    // Reservation class to represent a booking
    public class Reservation
    {
        public string ReservationId { get; private set; }
        public Room Room { get; private set; }
        public Guest Guest { get; private set; }
        public DateTime CheckInDate { get; private set; }
        public DateTime CheckOutDate { get; private set; }
        public int NumberOfGuests { get; private set; }
        public decimal TotalCost { get; private set; }
        public bool IsCancelled { get; private set; }

        public Reservation(string reservationId, Room room, Guest guest, DateTime checkInDate, DateTime checkOutDate, int numberOfGuests)
        {
            if (checkInDate >= checkOutDate)
                throw new ArgumentException("Check-out date must be after check-in date.");
            if (numberOfGuests > room.MaxOccupancy)
                throw new ArgumentException($"Number of guests exceeds room capacity of {room.MaxOccupancy}.");
            if (numberOfGuests <= 0)
                throw new ArgumentException("Number of guests must be greater than zero.");

            ReservationId = reservationId;
            Room = room;
            Guest = guest;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            NumberOfGuests = numberOfGuests;
            TotalCost = CalculateTotalCost(room, checkInDate, checkOutDate);
            IsCancelled = false;
        }

        private decimal CalculateTotalCost(Room room, DateTime checkIn, DateTime checkOut)
        {
            int nights = (checkOut - checkIn).Days;
            return nights * room.RatePerNight;
        }

        public void Cancel()
        {
            if (IsCancelled)
                throw new InvalidOperationException("Reservation is already cancelled.");
            IsCancelled = true;
        }

        public override string ToString()
        {
            return $"Reservation ID: {ReservationId}, Room: {Room.RoomNumber}, Guest: {Guest.Name}, Check-In: {CheckInDate:yyyy-MM-dd}, Check-Out: {CheckOutDate:yyyy-MM-dd}, Guests: {NumberOfGuests}, Total: ${TotalCost}, Status: {(IsCancelled ? "Cancelled" : "Active")}";
        }
    }

    // Payment class to represent a payment transaction
    public class Payment
    {
        public string PaymentId { get; private set; }
        public Reservation Reservation { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime PaymentDate { get; private set; }
        public PaymentStatus Status { get; private set; }

        public Payment(string paymentId, Reservation reservation, decimal amount)
        {
            PaymentId = paymentId;
            Reservation = reservation;
            Amount = amount;
            PaymentDate = DateTime.Now;
            Status = PaymentStatus.Pending;
        }

        public void ProcessPayment()
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException("Payment is already processed or failed.");
            Status = PaymentStatus.Completed;
        }

        public void FailPayment()
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException("Payment is already processed or failed.");
            Status = PaymentStatus.Failed;
        }

        public override string ToString()
        {
            return $"Payment ID: {PaymentId}, Reservation: {Reservation.ReservationId}, Amount: ${Amount}, Date: {PaymentDate:yyyy-MM-dd}, Status: {Status}";
        }
    }

    // Hotel class to manage rooms, guests, reservations, and payments
    public class Hotel
    {
        private List<Room> rooms;
        private List<Guest> guests;
        private List<Reservation> reservations;
        private List<Payment> payments;

        public Hotel()
        {
            rooms = new List<Room>();
            guests = new List<Guest>();
            reservations = new List<Reservation>();
            payments = new List<Payment>();
        }

        // Room management methods
        public void AddRoom(Room room)
        {
            if (rooms.Any(r => r.RoomNumber == room.RoomNumber))
                throw new InvalidOperationException("Room with this number already exists.");
            rooms.Add(room);
        }

        public void RemoveRoom(string roomNumber)
        {
            var room = rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
            if (room == null)
                throw new InvalidOperationException("Room not found.");
            if (room.Status == RoomStatus.Occupied)
                throw new InvalidOperationException("Cannot remove an occupied room.");
            rooms.Remove(room);
        }

        public void UpdateRoomStatus(string roomNumber, RoomStatus status)
        {
            var room = rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
            if (room == null)
                throw new InvalidOperationException("Room not found.");
            room.UpdateStatus(status);
        }

        public List<Room> SearchRooms(string query)
        {
            query = query.ToLower();
            return rooms.Where(r => r.RoomNumber.ToLower().Contains(query) ||
                                   r.Type.ToString().ToLower().Contains(query) ||
                                   r.Description.ToLower().Contains(query))
                        .ToList();
        }

        public List<Room> GetAvailableRooms(DateTime checkIn, DateTime checkOut)
        {
            return rooms.Where(r => r.Status == RoomStatus.Available &&
                                   !reservations.Any(res => res.Room == r &&
                                                           !res.IsCancelled &&
                                                           checkIn < res.CheckOutDate &&
                                                           checkOut > res.CheckInDate))
                        .ToList();
        }

        public List<Room> GetAllRooms()
        {
            return rooms;
        }

        public List<Room> GetOccupiedRooms()
        {
            return rooms.Where(r => r.Status == RoomStatus.Occupied).ToList();
        }

        public List<Room> GetRoomsInMaintenance()
        {
            return rooms.Where(r => r.Status == RoomStatus.Maintenance).ToList();
        }

        // Guest management methods
        public void RegisterGuest(Guest guest)
        {
            if (guests.Any(g => g.GuestId == guest.GuestId))
                throw new InvalidOperationException("Guest with this ID already exists.");
            guests.Add(guest);
        }

        public void RemoveGuest(string guestId)
        {
            var guest = guests.FirstOrDefault(g => g.GuestId == guestId);
            if (guest == null)
                throw new InvalidOperationException("Guest not found.");
            if (guest.Reservations.Any(r => !r.IsCancelled))
                throw new InvalidOperationException("Cannot remove guest with active reservations.");
            guests.Remove(guest);
        }

        public List<Guest> SearchGuests(string query)
        {
            query = query.ToLower();
            return guests.Where(g => g.Name.ToLower().Contains(query) ||
                                    g.GuestId.ToLower().Contains(query) ||
                                    g.Email.ToLower().Contains(query))
                         .ToList();
        }

        // Reservation management methods
        public void MakeReservation(string roomNumber, string guestId, DateTime checkIn, DateTime checkOut, int numberOfGuests)
        {
            var room = rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
            if (room == null)
                throw new InvalidOperationException("Room not found.");
            if (room.Status != RoomStatus.Available)
                throw new InvalidOperationException("Room is not available.");
            if (GetAvailableRooms(checkIn, checkOut).All(r => r.RoomNumber != roomNumber))
                throw new InvalidOperationException("Room is not available for the selected dates.");

            var guest = guests.FirstOrDefault(g => g.GuestId == guestId);
            if (guest == null)
                throw new InvalidOperationException("Guest not found.");

            string reservationId = Guid.NewGuid().ToString();
            var reservation = new Reservation(reservationId, room, guest, checkIn, checkOut, numberOfGuests);
            room.UpdateStatus(RoomStatus.Occupied);
            guest.Reservations.Add(reservation);
            reservations.Add(reservation);
        }

        public void CancelReservation(string reservationId)
        {
            var reservation = reservations.FirstOrDefault(r => r.ReservationId == reservationId && !r.IsCancelled);
            if (reservation == null)
                throw new InvalidOperationException("Active reservation not found.");
            reservation.Cancel();
            reservation.Room.UpdateStatus(RoomStatus.Available);
        }

        // Payment management methods
        public void ProcessPayment(string reservationId, decimal amount)
        {
            var reservation = reservations.FirstOrDefault(r => r.ReservationId == reservationId && !r.IsCancelled);
            if (reservation == null)
                throw new InvalidOperationException("Active reservation not found.");
            if (amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.");

            string paymentId = Guid.NewGuid().ToString();
            var payment = new Payment(paymentId, reservation, amount);
            payment.ProcessPayment();
            payments.Add(payment);
        }

        // Reporting methods
        public List<Reservation> GetUpcomingReservations()
        {
            var today = DateTime.Today;
            return reservations.Where(r => !r.IsCancelled && r.CheckInDate >= today)
                               .OrderBy(r => r.CheckInDate)
                               .ToList();
        }

        public List<Reservation> GetGuestReservationHistory(string guestId)
        {
            return reservations.Where(r => r.Guest.GuestId == guestId)
                               .OrderBy(r => r.CheckInDate)
                               .ToList();
        }

        public List<Payment> GetPaymentHistory()
        {
            return payments.OrderBy(p => p.PaymentDate).ToList();
        }

        public decimal GetTotalRevenue()
        {
            return payments.Where(p => p.Status == PaymentStatus.Completed)
                           .Sum(p => p.Amount);
        }
    }

    public class ReportingSystem
    {
        private readonly Hotel hotel;
        private readonly List<Report> reportHistory;

        public ReportingSystem(Hotel hotel)
        {
            this.hotel = hotel;
            this.reportHistory = new List<Report>();
        }

        public OccupancyReport GenerateOccupancyReport(DateTime startDate, DateTime endDate)
        {
            var report = new OccupancyReport
            {
                StartDate = startDate,
                EndDate = endDate,
                GeneratedDate = DateTime.Now,
                TotalRooms = hotel.GetAllRooms().Count,
                OccupiedRooms = hotel.GetOccupiedRooms().Count,
                AvailableRooms = hotel.GetAvailableRooms(startDate, endDate).Count,
                MaintenanceRooms = hotel.GetRoomsInMaintenance().Count
            };

            reportHistory.Add(report);
            return report;
        }

        public RevenueReport GenerateRevenueReport(DateTime startDate, DateTime endDate)
        {
            var payments = hotel.GetPaymentHistory()
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate);

            var report = new RevenueReport
            {
                StartDate = startDate,
                EndDate = endDate,
                GeneratedDate = DateTime.Now,
                TotalRevenue = payments.Sum(p => p.Amount),
                PaymentsByType = payments.GroupBy(p => p.PaymentType)
                                       .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
                RevenueByRoomType = payments.GroupBy(p => p.Reservation.Room.Type)
                                          .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount))
            };

            reportHistory.Add(report);
            return report;
        }

        public List<Report> GetReportHistory(DateTime startDate, DateTime endDate)
        {
            return reportHistory
                .Where(r => r.GeneratedDate >= startDate && r.GeneratedDate <= endDate)
                .OrderByDescending(r => r.GeneratedDate)
                .ToList();
        }
    }

    public abstract class Report
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedDate { get; set; }
    }

    public class OccupancyReport : Report
    {
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public decimal OccupancyRate => TotalRooms > 0 ? (decimal)OccupiedRooms / TotalRooms : 0;
    }

    public class RevenueReport : Report
    {
        public decimal TotalRevenue { get; set; }
        public Dictionary<PaymentType, decimal> PaymentsByType { get; set; }
        public Dictionary<RoomType, decimal> RevenueByRoomType { get; set; }
    }

    public class HousekeepingSystem
    {
        private readonly Hotel hotel;
        private readonly List<HousekeepingTask> tasks;
        private readonly List<HousekeepingStaff> staff;

        public HousekeepingSystem(Hotel hotel)
        {
            this.hotel = hotel;
            this.tasks = new List<HousekeepingTask>();
            this.staff = new List<HousekeepingStaff>();
        }

        public void CreateTask(string roomNumber, TaskType type, string description)
        {
            var room = hotel.GetRoom(roomNumber);
            if (room == null)
                throw new ArgumentException("Room not found");

            var task = new HousekeepingTask
            {
                Id = Guid.NewGuid(),
                RoomNumber = roomNumber,
                Type = type,
                Description = description,
                Status = TaskStatus.Pending,
                CreatedDate = DateTime.Now
            };

            tasks.Add(task);
        }

        public void AssignTask(Guid taskId, string staffId)
        {
            var task = tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                throw new ArgumentException("Task not found");

            var staff = this.staff.FirstOrDefault(s => s.Id == staffId);
            if (staff == null)
                throw new ArgumentException("Staff member not found");

            task.AssignedTo = staff;
            task.Status = TaskStatus.Assigned;
            task.AssignedDate = DateTime.Now;
        }

        public void CompleteTask(Guid taskId, string notes)
        {
            var task = tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                throw new ArgumentException("Task not found");

            task.Status = TaskStatus.Completed;
            task.CompletionDate = DateTime.Now;
            task.CompletionNotes = notes;
        }

        public List<HousekeepingTask> GetPendingTasks()
        {
            return tasks.Where(t => t.Status != TaskStatus.Completed)
                       .OrderBy(t => t.CreatedDate)
                       .ToList();
        }

        public List<HousekeepingTask> GetStaffTasks(string staffId)
        {
            return tasks.Where(t => t.AssignedTo?.Id == staffId)
                       .OrderByDescending(t => t.CreatedDate)
                       .ToList();
        }
    }

    public class HousekeepingTask
    {
        public Guid Id { get; set; }
        public string RoomNumber { get; set; }
        public TaskType Type { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public HousekeepingStaff AssignedTo { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string CompletionNotes { get; set; }
    }

    public class HousekeepingStaff
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public StaffStatus Status { get; set; }
        public List<HousekeepingTask> AssignedTasks { get; set; } = new List<HousekeepingTask>();
    }

    public enum TaskType
    {
        RoomCleaning,
        Maintenance,
        Supplies,
        Special
    }

    public enum TaskStatus
    {
        Pending,
        Assigned,
        InProgress,
        Completed,
        Cancelled
    }

    public enum StaffStatus
    {
        Available,
        Busy,
        OnBreak,
        OffDuty
    }

    public enum PaymentType
    {
        Cash,
        CreditCard,
        DebitCard,
        BankTransfer
    }

    // Console UI for interacting with the hotel system
    public class HotelConsoleUI
    {
        private Hotel hotel;

        public HotelConsoleUI()
        {
            hotel = new Hotel();
        }

        public void Run()
        {
            while (true)
            {
                DisplayMenu();
                string choice = Console.ReadLine();
                try
                {
                    switch (choice)
                    {
                        case "1":
                            AddRoom();
                            break;
                        case "2":
                            RemoveRoom();
                            break;
                        case "3":
                            UpdateRoomStatus();
                            break;
                        case "4":
                            SearchRooms();
                            break;
                        case "5":
                            ViewAvailableRooms();
                            break;
                        case "6":
                            RegisterGuest();
                            break;
                        case "7":
                            RemoveGuest();
                            break;
                        case "8":
                            SearchGuests();
                            break;
                        case "9":
                            MakeReservation();
                            break;
                        case "10":
                            CancelReservation();
                            break;
                        case "11":
                            ProcessPayment();
                            break;
                        case "12":
                            ViewUpcomingReservations();
                            break;
                        case "13":
                            ViewGuestReservationHistory();
                            break;
                        case "14":
                            ViewPaymentHistory();
                            break;
                        case "15":
                            ViewTotalRevenue();
                            break;
                        case "16":
                            Console.WriteLine("Exiting...");
                            return;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private void DisplayMenu()
        {
            Console.WriteLine("=== Hotel Reservation System ===");
            Console.WriteLine("1. Add Room");
            Console.WriteLine("2. Remove Room");
            Console.WriteLine("3. Update Room Status");
            Console.WriteLine("4. Search Rooms");
            Console.WriteLine("5. View Available Rooms");
            Console.WriteLine("6. Register Guest");
            Console.WriteLine("7. Remove Guest");
            Console.WriteLine("8. Search Guests");
            Console.WriteLine("9. Make Reservation");
            Console.WriteLine("10. Cancel Reservation");
            Console.WriteLine("11. Process Payment");
            Console.WriteLine("12. View Upcoming Reservations");
            Console.WriteLine("13. View Guest Reservation History");
            Console.WriteLine("14. View Payment History");
            Console.WriteLine("15. View Total Revenue");
            Console.WriteLine("16. Exit");
            Console.Write("Enter your choice: ");
        }

        private void AddRoom()
        {
            Console.Write("Enter Room Number: ");
            string roomNumber = Console.ReadLine();
            Console.Write("Enter Room Type (Single/Double/Suite): ");
            if (!Enum.TryParse(Console.ReadLine(), true, out RoomType type))
                throw new InvalidOperationException("Invalid room type.");

            Console.Write("Enter Rate per Night: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal rate) || rate <= 0)
                throw new InvalidOperationException("Invalid rate format.");

            Console.Write("Enter Max Occupancy: ");
            if (!int.TryParse(Console.ReadLine(), out int maxOccupancy) || maxOccupancy <= 0)
                throw new InvalidOperationException("Invalid max occupancy.");

            Console.Write("Enter Room Description: ");
            string description = Console.ReadLine();

            var room = new Room(roomNumber, type, rate, maxOccupancy, description);
            hotel.AddRoom(room);
            Console.WriteLine("Room added successfully.");
        }

        private void RemoveRoom()
        {
            Console.Write("Enter Room Number: ");
            string roomNumber = Console.ReadLine();
            hotel.RemoveRoom(roomNumber);
            Console.WriteLine("Room removed successfully.");
        }

        private void UpdateRoomStatus()
        {
            Console.Write("Enter Room Number: ");
            string roomNumber = Console.ReadLine();
            Console.Write("Enter New Status (Available/Occupied/Maintenance): ");
            if (!Enum.TryParse(Console.ReadLine(), true, out RoomStatus status))
                throw new InvalidOperationException("Invalid status.");
            hotel.UpdateRoomStatus(roomNumber, status);
            Console.WriteLine("Room status updated successfully.");
        }

        private void SearchRooms()
        {
            Console.Write("Enter search query: ");
            string query = Console.ReadLine();
            var results = hotel.SearchRooms(query);
            if (results.Count == 0)
                Console.WriteLine("No rooms found.");
            else
            {
                Console.WriteLine("\nSearch Results:");
                foreach (var room in results)
                    Console.WriteLine(room);
            }
        }

        private void ViewAvailableRooms()
        {
            Console.Write("Enter Check-In Date (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime checkIn))
                throw new InvalidOperationException("Invalid date format.");

            Console.Write("Enter Check-Out Date (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime checkOut))
                throw new InvalidOperationException("Invalid date format.");

            var availableRooms = hotel.GetAvailableRooms(checkIn, checkOut);
            if (availableRooms.Count == 0)
                Console.WriteLine("No available rooms for the selected dates.");
            else
            {
                Console.WriteLine("\nAvailable Rooms:");
                foreach (var room in availableRooms)
                    Console.WriteLine(room);
            }
        }

        private void RegisterGuest()
        {
            Console.Write("Enter Guest ID: ");
            string guestId = Console.ReadLine();
            Console.Write("Enter Name: ");
            string name = Console.ReadLine();
            Console.Write("Enter Email: ");
            string email = Console.ReadLine();
            Console.Write("Enter Phone: ");
            string phone = Console.ReadLine();

            var guest = new Guest(guestId, name, email, phone);
            hotel.RegisterGuest(guest);
            Console.WriteLine("Guest registered successfully.");
        }

        private void RemoveGuest()
        {
            Console.Write("Enter Guest ID: ");
            string guestId = Console.ReadLine();
            hotel.RemoveGuest(guestId);
            Console.WriteLine("Guest removed successfully.");
        }

        private void SearchGuests()
        {
            Console.Write("Enter search query: ");
            string query = Console.ReadLine();
            var results = hotel.SearchGuests(query);
            if (results.Count == 0)
                Console.WriteLine("No guests found.");
            else
            {
                Console.WriteLine("\nSearch Results:");
                foreach (var guest in results)
                    Console.WriteLine(guest);
            }
        }

        private void MakeReservation()
        {
            Console.Write("Enter Room Number: ");
            string roomNumber = Console.ReadLine();
            Console.Write("Enter Guest ID: ");
            string guestId = Console.ReadLine();
            Console.Write("Enter Check-In Date (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime checkIn))
                throw new InvalidOperationException("Invalid date format.");

            Console.Write("Enter Check-Out Date (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime checkOut))
                throw new InvalidOperationException("Invalid date format.");

            Console.Write("Enter Number of Guests: ");
            if (!int.TryParse(Console.ReadLine(), out int numberOfGuests))
                throw new InvalidOperationException("Invalid number of guests.");

            hotel.MakeReservation(roomNumber, guestId, checkIn, checkOut, numberOfGuests);
            Console.WriteLine("Reservation made successfully.");
        }

        private void CancelReservation()
        {
            Console.Write("Enter Reservation ID: ");
            string reservationId = Console.ReadLine();
            hotel.CancelReservation(reservationId);
            Console.WriteLine("Reservation cancelled successfully.");
        }

        private void ProcessPayment()
        {
            Console.Write("Enter Reservation ID: ");
            string reservationId = Console.ReadLine();
            Console.Write("Enter Payment Amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
                throw new InvalidOperationException("Invalid amount format.");

            hotel.ProcessPayment(reservationId, amount);
            Console.WriteLine("Payment processed successfully.");
        }

        private void ViewUpcomingReservations()
        {
            var upcomingReservations = hotel.GetUpcomingReservations();
            if (upcomingReservations.Count == 0)
                Console.WriteLine("No upcoming reservations.");
            else
            {
                Console.WriteLine("\nUpcoming Reservations:");
                foreach (var reservation in upcomingReservations)
                    Console.WriteLine(reservation);
            }
        }

        private void ViewGuestReservationHistory()
        {
            Console.Write("Enter Guest ID: ");
            string guestId = Console.ReadLine();
            var history = hotel.GetGuestReservationHistory(guestId);
            if (history.Count == 0)
                Console.WriteLine("No reservation history found.");
            else
            {
                Console.WriteLine("\nReservation History:");
                foreach (var reservation in history)
                    Console.WriteLine(reservation);
            }
        }

        private void ViewTotalRevenue()
        {
            var totalRevenue = hotel.GetTotalRevenue();
            Console.WriteLine($"Total Revenue: ${totalRevenue}");
        }
    }

    // Main program
    class Program
    {
        static void Main(string[] args)
        {
            var ui = new HotelConsoleUI();
            ui.Run();
        }
    }
}

namespace TaskManagement 
{
    public class TaskReporting 
    {
        private readonly TaskManager taskManager;
        private readonly IReportStorage reportStorage;

        public TaskReporting(TaskManager taskManager, IReportStorage reportStorage)
        {
            this.taskManager = taskManager;
            this.reportStorage = reportStorage;
        }

        public async Task<PerformanceReport> GenerateTeamPerformanceReport(string teamId, DateTime startDate, DateTime endDate)
        {
            var report = new PerformanceReport
            {
                TeamId = teamId,
                StartDate = startDate,
                EndDate = endDate,
                GeneratedDate = DateTime.Now,
                Metrics = await CalculateTeamMetrics(teamId, startDate, endDate),
                UserMetrics = await CalculateUserMetrics(teamId, startDate, endDate)
            };

            await reportStorage.StoreReport(report);
            return report;
        }

        private async Task<TeamMetrics> CalculateTeamMetrics(string teamId, DateTime startDate, DateTime endDate)
        {
            // Implementation
            return new TeamMetrics();
        }

        private async Task<List<UserMetrics>> CalculateUserMetrics(string teamId, DateTime startDate, DateTime endDate)
        {
            // Implementation
            return new List<UserMetrics>();
        }

        public async Task<PerformanceReport> GetReport(string reportId)
        {
            return await reportStorage.RetrieveReport(reportId);
        }

        public async Task<List<PerformanceReport>> GetAllReports(string teamId)
        {
            return await reportStorage.RetrieveAllReports(teamId);
        }
        
        public async Task DeleteReport(string reportId)
        {
            await reportStorage.DeleteReport(reportId);
        }

        public async Task DeleteAllReports(string teamId)
        {
            await reportStorage.DeleteAllReports(teamId);
        }

        public async Task<List<PerformanceReport>> GetReportsByDateRange(string teamId, DateTime startDate, DateTime endDate)
        {
            return await reportStorage.RetrieveReportsByDateRange(teamId, startDate, endDate);
        }

        public async Task<List<PerformanceReport>> GetReportsByUser(string teamId, string userId)
        {
            return await reportStorage.RetrieveReportsByUser(teamId, userId);
        }

        public async Task<List<PerformanceReport>> GetReportsByTask(string teamId, string taskId)
        {
            return await reportStorage.RetrieveReportsByTask(teamId, taskId);
        }

        public async Task<List<PerformanceReport>> GetReportsByStatus(string teamId, string status)
        {
            return await reportStorage.RetrieveReportsByStatus(teamId, status);
        }

        public async Task<List<PerformanceReport>> GetReportsByPriority(string teamId, string priority)
        {
            return await reportStorage.RetrieveReportsByPriority(teamId, priority);
        }

        public async Task<List<PerformanceReport>> GetReportsByAssignee(string teamId, string assigneeId)
        {
            return await reportStorage.RetrieveReports

            ByAssignee(teamId, assigneeId); 
        }

        public async Task<List<PerformanceReport>> GetReportsByDueDate(string teamId, DateTime dueDate)
        {
            return await reportStorage.RetrieveReportsByDueDate(teamId, dueDate);
        }

        public async Task<List<PerformanceReport>> GetReportsByCompletionDate(string teamId, DateTime completionDate)
        {
            return await reportStorage.RetrieveReportsByCompletionDate(teamId, completionDate);
        }

        public async Task<List<PerformanceReport>> GetReportsByCreatedDate(string teamId, DateTime createdDate)
        {
            return await reportStorage.RetrieveReportsByCreatedDate(teamId, createdDate);
        }

        public async Task<List<PerformanceReport>> GetReportsByUpdatedDate(string teamId, DateTime updatedDate)
        {
            return await reportStorage.RetrieveReportsByUpdatedDate(teamId, updatedDate);
        }

        public async Task<List<PerformanceReport>> GetReportsByCreatedBy(string teamId, string createdById)
        {
            return await reportStorage.RetrieveReportsByCreatedBy(teamId, createdById);
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