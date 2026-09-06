using System;
using System.Collections.Generic;

namespace FMS.Security.Entities
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string Code { get; set; } // Örn: "Flights.Create"
        public string Description { get; set; } 

        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}