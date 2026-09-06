using Microsoft.AspNetCore.Authorization;

namespace FMS.Security.Authorization
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        // Constructor: Dışarıdan gelen izni (Örn: "Flights.Create") Policy olarak ayarlar
        public HasPermissionAttribute(string permission) : base(policy: permission)
        {
        }
    }
}