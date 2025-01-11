using Microsoft.AspNetCore.Mvc.Filters;

namespace DefaultHRManagementSystem.Helpers
{
    public class PermissionAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public PermissionAuthorizeAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if the user is authenticated
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get the user's permissions from the claims
            var userPermissions = context.HttpContext.User.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToList();

            // Check if the user has the required permission
            if (!userPermissions.Contains(_permission))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}