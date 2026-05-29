    using System.Net.Http.Headers;

    namespace Nirbi.ServiceAuth.Http;

    public sealed class ServiceAccessTokenDelegatingHandler : DelegatingHandler
    {
        private readonly IServiceAccessTokenProvider _tokenProvider;

        public ServiceAccessTokenDelegatingHandler(IServiceAccessTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
