namespace DefaultHRManagementSystem.Data.Models
{
    public class Department
    {
        public int Id {  get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
