namespace DefaultHRManagementSystem.Helpers
{
    public static class RolePermissionMapping
    {
        public static Dictionary<string, Func<string, bool>> RolePermissionRules = new()
        {
            { "Admin", (permission) => true }, // Admin gets all permissions
            { "User", (permission) => permission == "View" } // User gets only "view" permissions
        };
    }
}
