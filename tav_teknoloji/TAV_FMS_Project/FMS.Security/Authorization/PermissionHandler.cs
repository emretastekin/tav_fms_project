using FMS.Security.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FMS.Security.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly FmsAuthDbContext _dbContext;

        public PermissionHandler(FmsAuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                return; 
            }

            // 1. Firebase UID'yi doğru claim anahtarlarından ("sub" dahil) arıyoruz
            var firebaseUid = context.User.FindFirst("sub")?.Value 
                              ?? context.User.FindFirst("user_id")?.Value 
                              ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
            {
                return; 
            }

            // 2. PostgreSQL sorgusu (Büyük/küçük harf duyarsız ve güvenli hale getirildi)
            var hasPermission = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Where(u => u.FirebaseUid == firebaseUid && u.IsActive)
                .SelectMany(u => u.UserRoles)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .AnyAsync(code => code.ToLower() == requirement.Permission.ToLower());

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}