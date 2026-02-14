// Event.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace VolunteerManagementSystem.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public EventStatus Status { get; set; }

        public static int GetNextId(List<Event> events)
        {
            return events.Count > 0 ? events.Max(e => e.Id) + 1 : 1;
        }
    }

    public enum EventStatus
    {
        Planned,      // Запланировано
        InProgress,   // Идёт
        Completed,    // Завершено
        Cancelled     // Отменено
    }
}
