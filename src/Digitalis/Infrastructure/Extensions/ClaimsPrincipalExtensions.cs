using System.Linq;
using System.Security.Claims;

namespace Digitalis.Infrastructure.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string Email(this ClaimsIdentity user)
        {
            return user != null ? user.Claims.FirstOrDefault(c => c.Type == "email")?.Value : string.Empty;
        }
    }
}
