using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Digitalis.Infrastructure
{
    public static class ClaimsPrincipalExtensions
    {
        public static string Email(this ClaimsIdentity user)
        {
            return user != null ? user.Claims.FirstOrDefault(c => c.Type == "email")?.Value : string.Empty;
        }
    }
}
