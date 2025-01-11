namespace DefaultHRManagementSystem.Data.DTOs
{
    public class UpdateUserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string CurrentPassword { get; set; } // Required for password change
        public string NewPassword { get; set; }     // Optional new password
    }
}
