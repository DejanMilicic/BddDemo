namespace Digitalis.Infrastructure
{
    public interface IAuthorizer<in T>
    {
        public bool IsAuthorized(T request);
    }
}
