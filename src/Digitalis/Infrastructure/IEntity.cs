using NUlid;

namespace Digitalis.Infrastructure
{
    public abstract class Entity
    {
        public string Id { get; set; } = Ulid.NewUlid().ToString();
    }
}
