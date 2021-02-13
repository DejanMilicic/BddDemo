using System;
using System.Linq;
using System.Security.Claims;
using Digitalis.Infrastructure.Guards;
using Digitalis.Models;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents;

namespace Digitalis.Infrastructure.Services
{
    internal class Authenticator
    {
        public User User => _user.Value;

        private readonly IHttpContextAccessor _ctx;
        private readonly IDocumentStore _store;
        private readonly Lazy<User> _user;

        public Authenticator(IHttpContextAccessor ctx, IDocumentStore store)
        {
            _ctx = ctx;
            _store = store;

            _user ??= new Lazy<User>(AuthenticateUser);
        }

        private User AuthenticateUser()
        {
            AuthenticationGuard.AgainstNull(_ctx.HttpContext?.User?.Identity);
            AuthenticationGuard.Affirm(_ctx.HttpContext?.User.Identity.IsAuthenticated);

            var ci = _ctx.HttpContext?.User.Identity as ClaimsIdentity;
            AuthenticationGuard.AgainstNull(ci);

            Claim? emailClaim = ci.Claims.SingleOrDefault(c => c.Type == "email")
                                ?? ci.Claims.SingleOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            AuthenticationGuard.AgainstNull(emailClaim);

            string email = emailClaim.Value;
            AuthenticationGuard.AgainstNullOrEmpty(email);

            using var session = _store.OpenSession();
            User user = session.Query<User>().SingleOrDefault(x => x.Email == email);
            if (user == null)
            {
                user = new User { Email = email };
                session.Store(user);
                session.SaveChanges();
            }

            AuthenticationGuard.AgainstNull(user);

            return user;
        }
    }
}
