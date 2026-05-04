namespace AuthService.WebApi.Configuration
{
    public class ServiceCommunicationOptions
    {
        public const string Section = "ServiceCommunication";

        public string ServiceId { get; set; }
        public string ServiceSecret { get; set; }
        public string AuthServiceUrl { get; set; }

    }
}
