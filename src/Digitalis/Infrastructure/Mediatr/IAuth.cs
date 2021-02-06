namespace Digitalis.Infrastructure.Mediatr
{
    using Features;

    public interface IAuth<in TRequest, TResponse> where TRequest : Request<TResponse>
    {
        public void Authorize(TRequest request);
    }
}
