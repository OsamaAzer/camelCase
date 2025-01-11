using DefaultHRManagementSystem.Enums;

namespace DefaultHRManagementSystem.Data.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        public string AttendanceStatus { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly ArrivingTime { get; set; }

        public TimeOnly LeavingTime { get; set; }

        public int OverTimeHours { get; set; } = 0;

        public int LateTimeHours { get; set; } = 0;

        public int? EmployeeId { get; set; }
        
        [JsonIgnore]
        public virtual Employee? Employee { get; set; }
    }
}
