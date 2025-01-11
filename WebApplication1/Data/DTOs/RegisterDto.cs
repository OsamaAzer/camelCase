namespace DefaultHRManagementSystem.Data.DTOs
{
    public class RegisterDto
    {
        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, StringLength(50)]
        public string UserName { get; set; }

        [Required, StringLength(70)]
        public string Email { get; set; }

        [Required, StringLength(256)]
        public string Password { get; set; }
    }
}
