
namespace DefaultHRManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = Role.Admin)] // Only Admin can access these endpoints
    public class PermissionsController(IAuthService _authService) : ControllerBase
    {
        [PermissionAuthorize(Permissions.View)]
        [HttpGet("GetPermissionsForRole/{roleName}")]
        public async Task<IActionResult> GetPermissionsForRole(string roleName)
        {
            var permissions = await _authService.GetPermissionsForRoleAsync(roleName);
            if (permissions == null)
                return NotFound($"Role '{roleName}' not found.");
            return Ok(permissions);
        }

        [PermissionAuthorize(Permissions.View)]
        [HttpGet("GetAllPermissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _authService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        [PermissionAuthorize(Permissions.Create)]
        [HttpPost("AddPermissionToRole")]
        public async Task<IActionResult> AddPermission([FromBody] PermissionDto model)
        {
            var result = await _authService.AddPermissionToRoleAsync(model.RoleName, model.Permission);
            if (string.IsNullOrEmpty(result))
                return Ok("Permission added successfully.");
            return BadRequest(result);
        }

        [PermissionAuthorize(Permissions.Delete)]
        [HttpDelete("RemovePermissionFromRole")]
        public async Task<IActionResult> RemovePermission([FromBody] PermissionDto model)
        {
            var result = await _authService.RemovePermissionFromRoleAsync(model.RoleName, model.Permission);
            if (string.IsNullOrEmpty(result))
                return Ok("Permission removed successfully.");
            return BadRequest(result);
        }

        //[HttpGet("HasPermission/{roleName}/{permission}")]
        //public async Task<IActionResult> HasPermission(string roleName, string permission)
        //{
        //    var hasPermission = await _authService.RoleHasPermissionAsync(roleName, permission);
        //    return Ok(hasPermission);
        //}
    }
}