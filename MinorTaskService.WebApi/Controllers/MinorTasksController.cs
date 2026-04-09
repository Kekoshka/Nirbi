using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Mediator;

namespace MinorTaskService.WebApi.Controllers
{
    [Route("api/")]
    [ApiController]
    public class MinorTasksController : ControllerBase
    {
        [HttpPost("tasks")]
        public IActionResult CreateTask(CreateMinorTaskRequest request)
        {

        }

        [HttpGet("tasks/{minorTaskId}")]
        public IActionResult GetTask(Guid minorTaskId)
        {

        }

        [HttpGet("tasks")]
        public IActionResult GetTasks(
            [FromQuery] int? limit,
            [FromQuery] int? from,
            [FromQuery] int? to)
        {

        }

        [HttpGet("statuses")]
        public IActionResult GetStatuses()
        {

        }

        [HttpPatch("tasks/{minorTaskId}")]
        public IActionResult UpdateTask(Guid minorTaskId, UpdateMinorTaskRequest request)
        {

        }

        [HttpPut("tasks/{minorTaskId}")]
        public IActionResult UpdateTaskStatus(Guid minorTaskId, UpdateMinorTaskStatusRequest request)
        {
            
        }

        [HttpDelete("tasks/{minorTaskId}")]
        public IActionResult DeleteTask(Guid minorTaskId)
        {

        }

        [HttpDelete("tasks/{minorTaskId}/participants/{participantId}")]
        public IActionResult DeleteTaskParticipant(
            Guid minorTaskId,
            Guid participantId)
        {

        }
    }
}
