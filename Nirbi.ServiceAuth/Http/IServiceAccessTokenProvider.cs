namespace Nirbi.ServiceAuth.Http;

public interface IServiceAccessTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task SaveAccessTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<string> GetAndSaveTokenAsync(CancellationToken cancellationToken = default);
    Task<string> RegisterAndSaveTokenAsync(CancellationToken cancellationToken = default);
}
