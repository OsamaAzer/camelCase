namespace DefaultHRManagementSystem.Data.DTOs
{
    public class AttendanceDto
    {
        public string AttendanceStatus { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly ArrivingTime { get; set; }

        public TimeOnly LeavingTime { get; set; }

        public int OverTimeHours { get; set; }

        public int LateTimeHours { get; set; }

        public string EmployeeName { get; set; }
    }
}
