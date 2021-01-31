using Ardalis.SmartEnum;

namespace Digitalis.Infrastructure
{
    public sealed class DigitalisClaims : SmartEnum<DigitalisClaims, string>
    {
        public static readonly DigitalisClaims CreateNewEntry = new DigitalisClaims(nameof(CreateNewEntry), "CreateNewEntry");
        public static readonly DigitalisClaims FetchEntry = new DigitalisClaims(nameof(FetchEntry), "FetchEntry");

        private DigitalisClaims(string name, string value) : base(name, value)
        {
        }
    }
}
