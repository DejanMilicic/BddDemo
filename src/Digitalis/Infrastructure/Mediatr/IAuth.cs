namespace Digitalis.Infrastructure.Mediatr
{
    public interface IAuth<in TRequest, TResponse> where TRequest : AuthRequest<TResponse>
    {
        public void Authorize(TRequest request);
    }

    public interface IAuth<in TRequest> where TRequest : AuthRequest
    {
        public void Authorize(TRequest request);
    }
}
