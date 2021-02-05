using System.Linq;
using Digitalis.Models;
using Raven.Client.Documents;

namespace Digitalis.Infrastructure.Services
{
    public class DbSeeding
    {
        public void Setup(IDocumentStore store, string superAdminEmail)
        {
            using var session = store.OpenSession();

            User superAdmin = session.Query<User>().SingleOrDefault(x => x.Email == superAdminEmail);
            if (superAdmin == null)
            {
                superAdmin = new User();
                superAdmin.Claims.Add((AppClaims.CreateNewEntry, ""));

                session.Store(superAdmin);
                session.SaveChanges();
            }
        }
    }
}
