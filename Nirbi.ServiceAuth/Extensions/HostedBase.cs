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

        public HostedBase(
            IServiceScopeFactory scopeFactory,
            ILogger<HostedBase> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var tokenService = scope.ServiceProvider.GetRequiredService<IServiceAccessTokenProvider>();
                string token = await tokenService.RegisterAndSaveTokenAsync().ConfigureAwait(false);
                _logger.LogInformation("HostedBase: Service registered with token: {token}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError("HostedBase: {Message}", ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
