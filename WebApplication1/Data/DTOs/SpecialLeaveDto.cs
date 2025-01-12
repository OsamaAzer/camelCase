namespace DefaultHRManagementSystem.Data.DTOs
{
    public class SpecialLeaveDto
    {
        public string ReasonForLeaving { get; set; }

        public DateOnly Date { get; set; }

        public int EmployeeId { get; set; }
    }
}
