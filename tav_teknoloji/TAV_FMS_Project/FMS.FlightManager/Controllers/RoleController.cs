using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin.Auth;
using FMS.Security.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace FMS.FlightManager.Controllers
{
    [Authorize] // Bu kilit sayesinde tokensız kimse bu sınıftaki hiçbir metoda erişemez
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        // ÖNEMLİ: Bu metot sadece geliştirme veya süper-admin kullanımı içindir!
        [HttpPost("assign-role")]
        [HasPermission("Roles.Create")]
        public async Task<IActionResult> AssignRole([FromQuery] string uid, [FromQuery] string role)
        {
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(role))
                return BadRequest("UID ve Role parametreleri zorunludur.");

            try
            {
                // Firebase kullanıcısına "role" isimli özel bir mühür (Claim) basıyoruz
                var claims = new Dictionary<string, object>()
                {
                    { "role", role.ToLower() } // Örn: "admin" veya "operator"
                };

                await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(uid, claims);

                return Ok(new { message = $"Başarılı! Kullanıcı ({uid}) artık '{role}' rolüne sahip. Değişikliğin aktif olması için kullanıcının sistemden çıkış yapıp tekrar girmesi gerekmektedir." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Rol atanırken bir hata oluştu: {ex.Message}");
            }
        }
    }
}