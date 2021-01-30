namespace Digitalis.Infrastructure.Mediatr
{
    public interface IAuthorizer<in T>
    {
        public bool IsAuthorized(T request);
    }
}
