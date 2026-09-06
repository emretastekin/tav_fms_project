using FMS.Security.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FMS.Security.Authorization;
using FMS.Security.Entities;

namespace FMS.FlightManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
    public class AuthController : ControllerBase
    {
        private readonly FmsAuthDbContext _authDbContext;

        public AuthController(FmsAuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        [HttpGet("my-permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            // 1. Token'dan kullanıcının Firebase UID'sini okuyoruz
            var firebaseUid = User.FindFirst("sub")?.Value 
                              ?? User.FindFirst("user_id")?.Value 
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");

            // 2. Email adresini token içerisinden alıyoruz (Otomatik kayıt için gerekli)
            var email = User.FindFirst(ClaimTypes.Email)?.Value 
                        ?? User.FindFirst("email")?.Value 
                        ?? "";

            // 3. Kullanıcı veritabanında var mı kontrol et, yoksa OTOMATİK OLUŞTUR (Auto-Sync)
            var user = await _authDbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null)
            {
                // Yeni kullanıcıyı oluştur
                user = new User
                {
                    Id = Guid.NewGuid(),
                    FirebaseUid = firebaseUid,
                    Email = email,
                    IsActive = true
                };

                _authDbContext.Users.Add(user);
                await _authDbContext.SaveChangesAsync();

                // Varsayılan olarak "Viewer" rolünü bu kullanıcıya otomatik bağla
                var viewerRole = await _authDbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Viewer");
                if (viewerRole != null)
                {
                    _authDbContext.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = viewerRole.Id
                    });
                    await _authDbContext.SaveChangesAsync();
                }

                // Eklenen rol ve izinlerin güncel nesneye yansıması için kullanıcıyı tekrar sorgula
                user = await _authDbContext.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstAsync(u => u.Id == user.Id);
            }

            // Eğer kullanıcı pasif durumdaysa içeri alma
            if (!user.IsActive)
                return Forbid("Kullanıcı hesabınız pasif durumdadır.");

            // 4. Veritabanından bu kullanıcıya atanmış TÜM yetki kodlarını çekiyoruz
            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToList();

            // 5. İzinleri JSON dizisi olarak frontend'e gönderiyoruz
            return Ok(permissions);
        }
        
        // --- DİĞER ROL VE YETKİ METOTLARI AYNEN KALIYOR ---
        
        [HttpGet("roles")]
        [HasPermission("Roles.Manage")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _authDbContext.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpGet("permissions")]
        [HasPermission("Roles.Manage")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _authDbContext.Permissions.ToListAsync();
            return Ok(permissions);
        }

        [HttpGet("roles/{roleId}/permissions")]
        [HasPermission("Roles.Manage")]
        public async Task<IActionResult> GetRolePermissions(Guid roleId)
        {
            var permissionIds = await _authDbContext.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();
        
            return Ok(permissionIds);
        }

        [HttpPost("roles/{roleId}/permissions")]
        [HasPermission("Roles.Manage")]
        public async Task<IActionResult> UpdateRolePermissions(Guid roleId, [FromBody] List<Guid> selectedPermissionIds)
        {
            var existingPermissions = _authDbContext.RolePermissions.Where(rp => rp.RoleId == roleId);
            _authDbContext.RolePermissions.RemoveRange(existingPermissions);

            var newPermissions = selectedPermissionIds.Select(permissionId => new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });
    
            await _authDbContext.RolePermissions.AddRangeAsync(newPermissions);
            await _authDbContext.SaveChangesAsync();

            return Ok(new { message = "Yetkiler başarıyla güncellendi!" });
        }
    }
}