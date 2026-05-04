namespace MinorTaskService.WebApi.Common.Options
{
    public class ExternalServicesOptions
    {
        public string CommunicationServiceAddress { get; set; } = string.Empty;
        public string KafkaAddress { get; set; } = string.Empty;
        public string SchemaRegistryAddress { get; set; } = string.Empty;
        public string MinorTaskServiceTopic { get; set; } = string.Empty;
        public string TaskParticipantServiceTopic { get; set; } = string.Empty;
    }
}
