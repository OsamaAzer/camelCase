namespace DefaultHRManagementSystem.Data.DTOs
{
    public class OfficialHolidayDto
    {
        [Required]
        public string Name { get; set; } // Corresponds to the "Name" column in the Excel file

        [Required]
        public DateOnly Date { get; set; } // Corresponds to the "Date" column in the Excel file
    }
}
