using System.ComponentModel;

namespace DefaultHRManagementSystem.Data.DTOs
{
    public class AddUserDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }
        
        [Required]
        [PasswordPropertyText]
        public string Password { get; set; }
    }
}
