namespace Digitalis.Infrastructure.Mediatr
{
    public interface IAuth<in TRequest, TResponse> where TRequest : AuthRequest<TResponse>
    {
    }

    public interface IAuth<in TRequest> where TRequest : AuthRequest
    {
    }
}
