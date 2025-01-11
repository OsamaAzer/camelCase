
namespace DefaultHRManagementSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly JWT _jwt;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AuthService(IOptions<JWT> jwt, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            _jwt = jwt.Value;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<string> AddPermissionToRoleAsync(string roleName, string permission)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return "Role does not exist.";

            // Add the permission as a claim to the role
            var result = await _roleManager.AddClaimAsync(role, new Claim("permission", permission));

            if (result.Succeeded)
            {
                // Save the permission to the database
                _context.Permissions.Add(new Permission
                {
                    RoleName = roleName,
                    PermissionName = permission
                });

                await _context.SaveChangesAsync();
            }

            return result.Succeeded ? string.Empty : "Failed to add permission.";
        }

        public async Task<string> RemovePermissionFromRoleAsync(string roleName, string permission)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return "Role does not exist.";

            // Remove the permission claim from the role
            var claims = await _roleManager.GetClaimsAsync(role);
            var claim = claims.FirstOrDefault(c => c.Type == "permission" && c.Value == permission);

            if (claim == null)
                return "Permission does not exist for this role.";

            var result = await _roleManager.RemoveClaimAsync(role, claim);

            if (result.Succeeded)
            {
                // Remove the permission from the database
                var permissionEntity = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.RoleName == roleName && p.PermissionName == permission);

                if (permissionEntity != null)
                {
                    _context.Permissions.Remove(permissionEntity);
                    await _context.SaveChangesAsync();
                }
            }

            return result.Succeeded ? string.Empty : "Failed to remove permission.";
        }

        public async Task<List<string>> GetPermissionsForRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return null;

            // Retrieve permissions for the role from the database
            var permissions = await _context.Permissions
                .Where(p => p.RoleName == roleName)
                .Select(p => p.PermissionName)
                .ToListAsync();

            return permissions;
        }

        public async Task<List<string>> GetAllPermissionsAsync()
        {
            // Retrieve all permissions from the database
            var permissions = await _context.Permissions
                .Select(p => p.PermissionName)
                .Distinct()
                .ToListAsync();

            return permissions;
        }

        public async Task<bool> RoleHasPermissionAsync(string roleName, string permission)
        {
            var permissions = await GetPermissionsForRoleAsync(roleName);
            return permissions?.Contains(permission) ?? false;
        }

        public async Task<AuthModelDto> RegisterAsync(RegisterDto model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModelDto { Message = "Email is already registered!!" };

            if (await _userManager.FindByNameAsync(model.UserName) is not null)
                return new AuthModelDto { Message = "Username is already exist!!" };

            var user = model.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                {
                    errors += $"{error.Description}, ";
                }

                return new AuthModelDto { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, Role.User);

            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthModelDto()
            {
                Email = user.Email!,
                UserName = user.UserName!,
                IsAuthenticated = true,
                ExpiresAt = jwtSecurityToken.ValidTo,
                Roles = new List<string> { Role.User },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
            };
        }

        public async Task<AuthModelDto> GetTokenAsync(TokenRequestDto model)
        {
            var authModel = new AuthModelDto();

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "Email or Password is incorrect!!";
                return authModel;
            }

            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = model.Email;
            authModel.ExpiresAt = jwtSecurityToken.ValidTo;
            authModel.UserName = user.UserName!;
            authModel.Roles = rolesList.ToList();

            // Include permissions in the AuthModel
            authModel.Permissions = await GetPermissionsForUserAsync(user);

            return authModel;
        }

        public async Task<string> AddRoleAsync(AddRoleDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null)
                return $"The user with Id: ({model.UserId}) dosen't exist!!";
            
            if (!await _roleManager.RoleExistsAsync(model.Role))
                return "The Role you want to assign doesn't exist!!";

            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "User is already assigned to this role!";

            var result = await _userManager.AddToRoleAsync(user, model.Role);

            return result.Succeeded ? string.Empty : "Some thing went wrong";
        }

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            // Include permissions in the JWT token
            var permissions = await GetPermissionsForUserAsync(user);
            var permissionClaims = permissions.Select(p => new Claim("permission", p)).ToList();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("uid", user.Id)
            }.Union(userClaims).Union(roleClaims).Union(permissionClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                claims: claims,
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                signingCredentials: signingCredentials,
                expires: DateTime.Now.AddDays(_jwt.LifeTimeInDays)
            );

            return jwtSecurityToken;
        }

        private async Task<List<string>> GetPermissionsForUserAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _context.Permissions
                .Where(p => roles.Contains(p.RoleName))
                .Select(p => p.PermissionName)
                .ToListAsync();

            return permissions;
        }
    }
}