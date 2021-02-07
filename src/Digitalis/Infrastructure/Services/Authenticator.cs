using System;
using System.Linq;
using System.Security.Claims;
using Digitalis.Infrastructure.Guards;
using Digitalis.Models;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents;

namespace Digitalis.Infrastructure.Services
{
    public class Authenticator
    {
        private Lazy<User> _user;

        public User User => _user.Value;

        private readonly IHttpContextAccessor _ctx;
        private readonly IDocumentStore _store;
        private string _email;

        public Authenticator(IHttpContextAccessor ctx, IDocumentStore store)
        {
            _ctx = ctx;
            _store = store;

            _user = new Lazy<User>(new Func<User>(() => AuthenticateUser()));
        }

        public User AuthenticateUser()
        {
            AuthenticationGuard.AgainstNull(_ctx.HttpContext?.User?.Identity);
            AuthenticationGuard.Affirm(_ctx.HttpContext?.User.Identity.IsAuthenticated);

            var ci = _ctx.HttpContext?.User.Identity as ClaimsIdentity;
            AuthenticationGuard.AgainstNull(ci);

            Claim? emailClaim = ci.Claims.SingleOrDefault(c => c.Type == "email")
                                ?? ci.Claims.SingleOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            AuthenticationGuard.AgainstNull(emailClaim);

            _email = emailClaim.Value;
            AuthenticationGuard.AgainstNullOrEmpty(_email);

            using var session = _store.OpenSession();
            User user = session.Query<User>().SingleOrDefault(x => x.Email == _email);
            if (user == null)
            {
                user = new User { Email = _email };
                session.Store(user);
                session.SaveChanges();
            }

            AuthenticationGuard.AgainstNull(user);

            return user;
        }
    }
}
