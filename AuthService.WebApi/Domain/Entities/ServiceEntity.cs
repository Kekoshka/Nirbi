namespace AuthService.WebApi.Domain.Entities
{
    public class ServiceEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClientSecret { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }

        public ICollection<ServiceScopeEntity> AllowedScopes { get; set; } = new List<ServiceScopeEntity>();
    }

    public class ServiceScopeEntity
    {
        public int Id { get; set; }
        public string ServiceId { get; set; }
        public string Scope { get; set; }
        public ServiceEntity Service { get; set; }
    }
}
