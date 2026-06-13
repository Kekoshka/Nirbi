using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nirbi.ServiceAuth.Http;

namespace Nirbi.ServiceAuth.Extensions
{
    public sealed class HostedBase : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HostedBase> _logger;
        private int n = 4;

        public HostedBase(
            IServiceScopeFactory scopeFactory,
            ILogger<HostedBase> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                string token = "";
                using var scope = _scopeFactory.CreateScope();
                var tokenService = scope.ServiceProvider.GetRequiredService<IServiceAccessTokenProvider>();
                for (int i = 0; i < n; i++)
                {
                    token = await tokenService.RegisterAndSaveTokenAsync(cancellationToken).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(token))
                    {
                        break;
                    }
                    _logger.LogError("HostedBase: try {n}. Token is empty.", n);
                    Task.Delay(30000).Wait();
                }
                _logger.LogInformation("HostedBase: Service registered with token: {token}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError("HostedBase: {Message},", ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
