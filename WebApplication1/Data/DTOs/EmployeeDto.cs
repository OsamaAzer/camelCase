namespace DefaultHRManagementSystem.Data.DTOs
{
    public class EmployeeDto
    {
        public string FullName { get; set; }

        public string Gender { get; set; }

        public string Address { get; set; }

        public string Nationality { get; set; }

        public string JobTitle { get; set; }

        public string PhoneNumber { get; set; }

        public DateOnly ContractDate { get; set; }

        public TimeOnly ArrivalTime { get; set; }

        public TimeOnly DepartureTime { get; set; }

        public decimal Salary { get; set; }

        public string DepartmentName { get; set; }
    }
}
