// Volunteer.cs
using System.Collections.Generic;
using System.Linq;

namespace VolunteerManagementSystem.Models
{
    public enum VolunteerStatus
    {
        Active,     // Действующий
        Inactive    // Не действующий
    }

    public class Volunteer
    {
        public int Id { get; set; }
        public string LastName { get; set; }   // Фамилия
        public string FirstName { get; set; }  // Имя
        public string MiddleName { get; set; } // Отчество
        public VolunteerStatus Status { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        public static int GetNextId(List<Volunteer> volunteers)
        {
            return volunteers.Count > 0 ? volunteers.Max(v => v.Id) + 1 : 1;
        }
    }
}
