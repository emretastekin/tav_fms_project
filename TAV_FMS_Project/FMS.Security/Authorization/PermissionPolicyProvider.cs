using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace FMS.Security.Authorization
{
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Önce sistemin kendi standart policy'leri var mı diye kontrol et
            var policy = await base.GetPolicyAsync(policyName);
            
            if (policy == null)
            {
                // Yoksa (yani bizim yazdığımız Flights.Create vb. dinamik bir yetkiyse), bunu Requirement'a dönüştür.
                policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(policyName))
                    .Build();
            }
            
            return policy;
        }
    }
}