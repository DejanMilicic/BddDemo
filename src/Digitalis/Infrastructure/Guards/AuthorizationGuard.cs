using System;
using System.Linq;
using Digitalis.Models;

namespace Digitalis.Infrastructure.Guards
{
    public static class AuthorizationGuard
    {
        public static void AffirmClaim(User user, string claim)
        {
            if (string.IsNullOrWhiteSpace(claim))
                throw new ArgumentException(nameof(claim));

            var claims = user.Claims.Select(x => x.Item1).ToArray();
            if (claims.Length == 0)
                throw new UnauthorizedAccessException();

            var exists = claims.Contains(claim);

            if (exists == false)
                throw new UnauthorizedAccessException();
        }
    }
}
