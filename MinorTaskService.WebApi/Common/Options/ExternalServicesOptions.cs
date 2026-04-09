namespace MinorTaskService.WebApi.Common.Options
{
    public class ExternalServicesOptions
    {
        public string CommunicationServiceAddress { get; set; }
        public string KafkaAddress { get; set; }
        public string SchemaRegistryAddress { get; set; }
    }
}
