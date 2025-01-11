using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DefaultHRManagementSystem.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required,MaxLength(50)]
        public string FullName { get; set; }
    }
}
