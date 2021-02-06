using System.Linq;
using System.Security.Claims;
using Digitalis.Infrastructure.Guards;
using Digitalis.Models;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents;

namespace Digitalis.Infrastructure.Mediatr
{
    public interface IAuth<in T>
    {
        public void Authenticate(T request);

        public void Authorize(T request);
    }

    public abstract class Auth<T> : IAuth<T>
    {
        protected readonly IHttpContextAccessor Ctx;
        protected readonly IDocumentStore Store;
        protected string Email;
        protected User User;

        protected Auth(IHttpContextAccessor ctx, IDocumentStore store)
        {
            Ctx = ctx;
            Store = store;
        }

        public void Authenticate(T request)
        {
            AuthenticationGuard.AgainstNull(Ctx.HttpContext?.User?.Identity);
            AuthenticationGuard.Affirm(Ctx.HttpContext?.User.Identity.IsAuthenticated);

            var ci = Ctx.HttpContext?.User.Identity as ClaimsIdentity;
            AuthenticationGuard.AgainstNull(ci);

            Claim? emailClaim = ci.Claims.SingleOrDefault(c => c.Type == "email") 
                                ?? ci.Claims.SingleOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            AuthenticationGuard.AgainstNull(emailClaim);

            Email = emailClaim.Value;
            AuthenticationGuard.AgainstNullOrEmpty(Email);

            using var session = Store.OpenSession();
            User = session.Query<User>().SingleOrDefault(x => x.Email == Email);
            if (User == null)
            {
                User = new User {Email = Email};
                session.Store(User);
                session.SaveChanges();
            }

            AuthenticationGuard.AgainstNull(User);
        }

        public virtual void Authorize(T request)
        {

        }
    }
}
