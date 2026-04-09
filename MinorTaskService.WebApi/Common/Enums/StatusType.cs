using MinorTaskService.WebApi.Common.DataSeed;

namespace MinorTaskService.WebApi.Common.Enums
{
    public static class StatusType
    {
        public static readonly Guid InSearch = StatusesSeed.Statuses[0].Id;
        public static readonly Guid InProgress = StatusesSeed.Statuses[1].Id;
        public static readonly Guid Completed = StatusesSeed.Statuses[2].Id;
    }
}
