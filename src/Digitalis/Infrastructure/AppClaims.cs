using Ardalis.SmartEnum;

namespace Digitalis.Infrastructure
{
    public sealed class AppClaims : SmartEnum<AppClaims, string>
    {
        public static readonly AppClaims CreateNewEntry = new AppClaims(nameof(CreateNewEntry));
        public static readonly AppClaims FetchEntry = new AppClaims(nameof(FetchEntry));
        public static readonly AppClaims CreateUser = new AppClaims(nameof(CreateUser));

        private AppClaims(string name) : base(name, name)
        {
        }
    }
}
