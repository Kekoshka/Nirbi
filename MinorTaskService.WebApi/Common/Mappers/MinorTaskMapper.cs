using MinorTaskService.DataAccess.Postgres.Models;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Mediator;
using Riok.Mapperly.Abstractions;

namespace MinorTaskService.WebApi.Common.Mappers
{
    [Mapper]
    public static partial class MinorTaskMapper
    {
        public static partial MinorTask ToMinorTask(this CreateMinorTaskDTO value);

        public static partial CreateMinorTaskDTO ToCreateMinorTaskDTO(this CreateMinorTaskCommand value);
     
        [MapProperty(nameof(MinorTask.Status.Name), nameof(GetMinorTaskDTO.Status))]
        public static partial GetMinorTaskDTO ToGetMinorTaskDTO(this MinorTask value);

        public static partial IQueryable<GetMinorTasksDTO> ToGetMinorTasksDTO(this IQueryable<MinorTask> value);

        [MapperIgnoreTarget(nameof(MinorTask.Id))]
        public static partial void UpdateMinorTask(UpdateMinorTaskDTO source, MinorTask target);

    }
}
