namespace AuthService.WebApi.Configuration
{
    public class ServiceRegistryOptions
    {
        public const string Section = "ServiceRegistry";

        /// <summary>
        /// When true, registered services from <see cref="Services"/> are ensured on application startup.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// When true, scopes for an existing service are overwritten from configuration on each startup.
        /// </summary>
        public bool SyncScopesOnStartup { get; set; }

        public List<ServiceRegistryEntry> Services { get; set; } = new();
    }

    public class ServiceRegistryEntry
    {
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public List<string> Scopes { get; set; } = new();

        /// <summary>
        /// Optional. If set, used as the client secret for a new registration.
        /// Prefer user secrets or environment variables in production.
        /// </summary>
        public string ClientSecret { get; set; }
    }
}
