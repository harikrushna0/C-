using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var task = new TaskItem(title, description);
            tasks.Add(task);
            return task;
        }

        public void AssignTask(Guid taskId, string assignee)
        {
            var task = GetTaskById(taskId);
            if (task == null)
                throw new ArgumentException("Task not found");

            task.AssignedTo = assignee;
            if (!userTasks.ContainsKey(assignee))
                userTasks[assignee] = new List<TaskItem>();

            userTasks[assignee].Add(task);
            task.History.Add(new TaskHistory($"Task assigned to {assignee}"));
        }

        public List<TaskItem> GetUserTasks(string username)
        {
            return userTasks.ContainsKey(username) ? userTasks[username] : new List<TaskItem>();
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
            return tasks.Where(t => t.DueDate.HasValue && 
                                  t.DueDate.Value < DateTime.Now &&
                                  t.Status != TaskStatus.Completed &&
                                  t.Status != TaskStatus.Cancelled)
                       .ToList();
        }

        public void UpdateTaskStatus(Guid taskId, TaskStatus newStatus)
        {
            var task = GetTaskById(taskId);
            if (task == null)
                throw new ArgumentException("Task not found");

            var oldStatus = task.Status;
            task.Status = newStatus;
            task.History.Add(new TaskHistory($"Status changed from {oldStatus} to {newStatus}"));

            if (newStatus == TaskStatus.Completed)
                task.CompletedDate = DateTime.Now;
        }

        private TaskItem GetTaskById(Guid taskId)
        {
            return tasks.FirstOrDefault(t => t.Id == taskId);
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