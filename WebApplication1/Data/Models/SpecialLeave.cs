namespace DefaultHRManagementSystem.Data.Models
{
    public class SpecialLeave
    {
        public int Id { get; set; }

        public string ReasonForLeaving { get; set; }

        public DateOnly Date { get; set; }

        public int? EmployeeId { get; set; }

        [JsonIgnore]
        public virtual Employee? Employee { get; set; }
    }
}
