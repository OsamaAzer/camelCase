
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DefaultHRManagementSystem.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Department> Departments { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Attendance> Attendances { get; set; }

        public DbSet<OfficialHoliday> OfficialHolidays { get; set; }

        public DbSet<SpecialLeave> SpecialLeaves { get; set; }

        public DbSet<Permission> Permissions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Permission>()
            .HasKey(p => p.Id); // Set the primary key

            modelBuilder.Entity<Permission>()
                .HasIndex(p => new { p.RoleName, p.PermissionName })
                .IsUnique(); // Ensure unique role-permission combinations

            //modelBuilder.Entity<Employee>()
            //    .Property(e => e.Gender)
            //    .HasConversion(
            //        v => v.ToString(),
            //        v => (Gender)Enum.Parse(typeof(Gender), v)
            //    );

            //modelBuilder.Entity<Attendance>()
            //    .Property(a => a.Status)
            //    .HasConversion(
            //        v => v.ToString(), // Enum to string for database storage
            //        v => (AttendanceStatus)Enum.Parse(typeof(AttendanceStatus), v) // String to enum for entity
            //    );

        }
    }
}
