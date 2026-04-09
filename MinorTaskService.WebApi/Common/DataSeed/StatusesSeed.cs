using MinorTaskService.DataAccess.Postgres.Models;
using System.Xml.Linq;

namespace MinorTaskService.WebApi.Common.DataSeed
{
    public static class StatusesSeed
    {
        public static readonly List<Status> Statuses = new()
        {
            new()
            {
                Id = Guid.Parse("ba8ffc8e-165c-492b-b241-10cb3b335409"),
                Name = "В поиске волонтеров"
            },
            new()
            {
                Id = Guid.Parse("8449b004-3f18-4906-b31a-4687605a49e6"),
                Name = "Выполняется"
            },
            new()
            {
                Id = Guid.Parse("b3dd4e86-0a4a-403f-8f36-6bf311b3f52f"),
                Name = "Выполнен"
            }
        };
    }
}
