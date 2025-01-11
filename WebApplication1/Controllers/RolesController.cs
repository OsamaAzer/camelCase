
namespace DefaultHRManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager) : ControllerBase
    {
        [PermissionAuthorize(Permissions.View)]
        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await roleManager.Roles.ToListAsync();
            return Ok(roles);
        }
        
        [PermissionAuthorize(Permissions.View)]
        [HttpGet("GetRoleByID/{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }
        
        [PermissionAuthorize(Permissions.View)]
        [HttpGet("GetUsersInRole/{roleName}")]
        public async Task<IActionResult> GetUsersInRole(string roleName)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return NotFound("Role not found.");
            }

            var users = await userManager.GetUsersInRoleAsync(roleName);
            return Ok(users);
        }
        
        [PermissionAuthorize(Permissions.Create)]
        [HttpPost("CreateNewRole")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return BadRequest("Role name is required.");
            }

            var role = new IdentityRole(roleName);
            var result = await roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
            }

            return BadRequest(result.Errors);
        }
        
        [PermissionAuthorize(Permissions.Create)]
        [HttpPost("AssignUserToRole")]
        public async Task<IActionResult> AssignUserToRole([FromBody] UserRoleDto request)
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.RoleName))
            {
                return BadRequest("UserId and RoleName are required.");
            }

            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var roleExists = await roleManager.RoleExistsAsync(request.RoleName);
            if (!roleExists)
            {
                return NotFound("Role not found.");
            }

            if (await userManager.IsInRoleAsync(user, request.RoleName))
            {
                return BadRequest("User is already assigned to this role!");
            }

            var result = await userManager.AddToRoleAsync(user, request.RoleName);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }
        
        [PermissionAuthorize(Permissions.Update)]
        [HttpPut("UpdateRole/{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return BadRequest("Role name is required.");
            }

            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            role.Name = roleName;
            var result = await roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        [PermissionAuthorize(Permissions.Delete)]
        [HttpDelete("DeleteRole/{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var result = await roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        [PermissionAuthorize(Permissions.Delete)]
        [HttpDelete("RemoveUserFromRole")]
        public async Task<IActionResult> RemoveUserFromRole([FromBody] UserRoleDto request)
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.RoleName))
            {
                return BadRequest("UserId and RoleName are required.");
            }

            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var roleExists = await roleManager.RoleExistsAsync(request.RoleName);
            if (!roleExists)
            {
                return NotFound("Role not found.");
            }

            var result = await userManager.RemoveFromRoleAsync(user, request.RoleName);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        //[Authorize(Roles = Role.Admin)]
        //[HttpPost("AddRole")]
        //public async Task<IActionResult> AddRole([FromBody] AddRoleModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var result = await authService.AddRoleAsync(model);

        //    if (!string.IsNullOrEmpty(result))
        //        return BadRequest(result);

        //    return Ok(model);
        //}
    }

    
}