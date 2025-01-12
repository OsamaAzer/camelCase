
namespace DefaultHRManagementSystem.Services
{
    public class SeedService(RoleManager<IdentityRole> _roleManager, AppDbContext _context, ILogger<SeedService> _logger, UserManager<ApplicationUser> userManager)
    {
        public async Task Initialize()
        {
            await SeedRoles();

            await SeedPermissions();

            await SeedAdminUser();
        }

        private async Task SeedRoles()
        {
            string[] roles = { Role.Admin, Role.User };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task SeedPermissions()
        {
            // Generate all permissions dynamically
            var permissions = PermissionGenerator.GeneratePermissions();

            // Assign permissions to roles based on predefined rules
            foreach (var roleRule in RolePermissionMapping.RolePermissionRules)
            {
                var roleName = roleRule.Key;
                var permissionRule = roleRule.Value;

                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    foreach (var permission in permissions)
                    {
                        if (permissionRule(permission)) // Check if the permission matches the rule
                        {
                            // Check if the permission already exists for the role
                            var existingPermission = await _context.Permissions
                                .FirstOrDefaultAsync(p => p.RoleName == roleName && p.PermissionName == permission);

                            if (existingPermission == null)
                            {
                                _logger.LogInformation($"Adding permission '{permission}' to role '{roleName}'.");

                                // Add the permission as a claim to the role
                                await _roleManager.AddClaimAsync(role, new Claim("permission", permission));

                                // Save the permission to the database
                                _context.Permissions.Add(new Permission
                                {
                                    RoleName = roleName,
                                    PermissionName = permission
                                });
                            }
                            else
                            {
                                _logger.LogInformation($"Permission '{permission}' already exists for role '{roleName}'. Skipping.");
                            }
                        }
                    }
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        private async Task SeedAdminUser()
        {
            if (userManager is not null)
            {
                var adminUser = await userManager.FindByEmailAsync("admin@example.com");

                //var count = userManager.Users.Count();
                if (adminUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = "admin",
                        Email = "admin@example.com",
                        FullName = "Admin User"
                    };

                    var result = await userManager.CreateAsync(user, "Admin@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                }
            }
        }
    }
}