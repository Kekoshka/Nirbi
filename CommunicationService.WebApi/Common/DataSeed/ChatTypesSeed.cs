using CommunicationService.DataAccess.Postgres.Models;

namespace CommunicationService.WebApi.Common.DataSeed
{
    public static class ChatTypesSeed
    {
        public static readonly List<ChatType> ChatTypes = new()
        {
            new()
            {
                Id = Guid.Parse("b36b1c9c-1647-4785-9573-e4b225cf35ae"),
                Name = "Групповой"
            },
            new()
            {
                Id = Guid.Parse("c0e3b007-f0bc-4c92-9f32-a87f1582812b"),
                Name = "Личный"
            }
        };
    }
}
