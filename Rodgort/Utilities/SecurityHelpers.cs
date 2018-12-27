using System.Security.Claims;

namespace Rodgort.Utilities
{
    public static class SecurityHelpers
    {
        public static bool HasClaim(this ClaimsPrincipal claimsPrincipal, string type)
        {
            return claimsPrincipal.HasClaim(c => string.Equals(c.Type, type));
        }
    }
}
