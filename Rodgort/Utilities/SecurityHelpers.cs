using System.Security.Claims;

namespace Rodgort.Utilities
{
    public static class SecurityHelpers
    {
        public static bool HasRole(this ClaimsPrincipal claimsPrincipal, int roleId)
        {
            return claimsPrincipal.HasClaim(ClaimTypes.Role, roleId.ToString());
        }

        public static int UserId(this ClaimsPrincipal claimsPrincipal)
        {
            var claim = claimsPrincipal.FindFirst("accountId");
            return int.Parse(claim.Value);
        }
    }
}
