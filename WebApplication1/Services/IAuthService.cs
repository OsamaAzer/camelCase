using System.Collections.Generic;
using System.Threading.Tasks;

namespace DefaultHRManagementSystem.Services
{
    public interface IAuthService
    {
        Task<AuthModelDto> RegisterAsync(RegisterDto model);
        Task<AuthModelDto> GetTokenAsync(TokenRequestDto model);
        Task<string> AddRoleAsync(AddRoleDto model);
        Task<string> AddPermissionToRoleAsync(string roleName, string permission);
        Task<string> RemovePermissionFromRoleAsync(string roleName, string permission);
        Task<List<string>> GetPermissionsForRoleAsync(string roleName);
        Task<List<string>> GetAllPermissionsAsync();
        Task<bool> RoleHasPermissionAsync(string roleName, string permission);
    }
}