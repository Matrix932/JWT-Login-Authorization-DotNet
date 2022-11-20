using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JWT_Login_Authorization_DotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskTableStorageService _taskService;

        public TaskController(ITaskTableStorageService taskTableStorageService)
        {
            _taskService = taskTableStorageService;
        }

        [HttpGet("GetTask")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAsync([FromQuery] string id, string skillName)
        {
            if (string.IsNullOrWhiteSpace(skillName) || string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Name or id cannot be empty ");
            }
            return Ok(await _taskService.GetTaskAsync(id, skillName));
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> PostAsync([FromQuery] TaskDTO taskDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Models.Task azureTask = _taskService.MapTask(taskDTO).Result;

            await _taskService.CreateTaskAsync(azureTask);
            return Ok("You have sucessully created a task: " + azureTask.Title + " for the following skill " + azureTask.RowKey);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] string id, string skillName)
        {
            if (string.IsNullOrWhiteSpace(skillName) || string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Name or id cannot be empty ");
            }
            await _taskService.DeleteTaskAsync(id, skillName);
            return Ok("You have sucessufully deleted the task from storage");
        }

        [HttpPut("UpdateTask"), AllowAnonymous]
        public async Task<IActionResult> UpdateAsync(string whateva)
        {
            return Ok(whateva);
        }
    }
}