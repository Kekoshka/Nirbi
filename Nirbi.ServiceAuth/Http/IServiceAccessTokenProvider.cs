namespace Nirbi.ServiceAuth.Http;

public interface IServiceAccessTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
