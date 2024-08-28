using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class Todo
    {
        public int Id { get; set; }

        // Required property
        [Required]
        public string Task { get; set; }

        public bool Done { get; set; }

        // Nullable properties
        public DateTime? DeadlineDate { get; set; }
        public TimeSpan? DeadlineTime { get; set; }
        public string? AssignedTo { get; internal set; }

        // Constructor with parameters to ensure Task is set
        public Todo(string task)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));
        }

        // Parameterless constructor for scenarios where initialization is handled later
        public Todo()
        {
        }
    }
}
