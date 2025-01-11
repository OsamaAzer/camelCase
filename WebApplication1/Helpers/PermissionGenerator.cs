using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace DefaultHRManagementSystem.Helpers
{
    public static class PermissionGenerator
    {
        public static List<string> GeneratePermissions()
        {
            var permissions = new List<string>();

            // Get all controllers in the application
            var controllers = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type));

            // Define the permission types based on HTTP methods
            var permissionTypes = new Dictionary<Type, string>
            {
                { typeof(HttpGetAttribute), "View" },
                { typeof(HttpPostAttribute), "Create" },
                { typeof(HttpPutAttribute), "Update" },
                { typeof(HttpDeleteAttribute), "Delete" }
            };

            foreach (var controller in controllers)
            {
                // Get all public actions in the controller
                var actions = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(method => method.IsDefined(typeof(HttpGetAttribute)) ||
                                    method.IsDefined(typeof(HttpPostAttribute)) ||
                                    method.IsDefined(typeof(HttpPutAttribute)) ||
                                    method.IsDefined(typeof(HttpDeleteAttribute)));

                foreach (var action in actions)
                {
                    // Determine the permission type based on the HTTP method
                    foreach (var attribute in action.GetCustomAttributes())
                    {
                        if (permissionTypes.ContainsKey(attribute.GetType()))
                        {
                            var permissionType = permissionTypes[attribute.GetType()];

                            // Generate permission name (e.g., "View")
                            var permission = permissionType;

                            // Add the permission if it doesn't already exist
                            if (!permissions.Contains(permission))
                            {
                                permissions.Add(permission);
                            }

                            break; // Exit the loop after finding the first matching attribute
                        }
                    }
                }
            }

            return permissions;
        }
    }
}