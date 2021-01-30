using NUlid;

namespace Digitalis.Models
{
    public abstract class Entity
    {
        public string Id { get; set; } = Ulid.NewUlid().ToString();
    }
}
