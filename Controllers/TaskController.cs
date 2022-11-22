using Azure;
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
        private readonly ISkillTableStoragerService _skillService;

        public TaskController(ITaskTableStorageService taskTableStorageService, ISkillTableStoragerService skillService)
        {
            _taskService = taskTableStorageService;
            _skillService = skillService;
        }

        [HttpGet("GetTask")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAsync([FromQuery] string id, string skillName)
        {
            if (string.IsNullOrWhiteSpace(skillName) || string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Name or id cannot be empty ");
            }
            try
            {
                Models.Task task = await _taskService.GetTaskAsync(id, skillName);
                return Ok(task);
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpGet("GetTasks")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTasksAsync()
        {
            try
            {
                return Ok(await _taskService.GetAllTaskAsync());
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpGet("GetTasksBySkill")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTasksBySkillAsync(string skillName)
        {
            if (string.IsNullOrWhiteSpace(skillName))
            {
                return BadRequest("Skill name cannot be empty");
            }
            try
            {
                return Ok(_taskService.GetTasksBySkillName(skillName).Result);
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> PostAsync([FromQuery] TaskDTO taskDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state");
            }
            try
            {
                Models.Task azureTask = await _taskService.MapTask(taskDTO);

                Response response = await _taskService.CreateTaskAsync(azureTask);
                if (response.IsError)
                {
                    return StatusCode(response.Status, "Failed to create candidate");
                }
                return Ok("You have sucessully created a task: " + azureTask.Title + " for the following skill " + azureTask.RowKey);
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] string id, string skillName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(skillName) || string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Name or id cannot be empty ");
                }
                Response response = await _taskService.DeleteTaskAsync(id, skillName);
                if (response.IsError)
                {
                    return StatusCode(response.Status, "Failed to delete task");
                }
                return Ok("You have sucessufully deleted the task from storage");
            }
            catch (RequestFailedException azureEx)
            {
                return StatusCode(azureEx.Status, azureEx.Message);
            }
        }

        [HttpPut("UpdateTask"), AllowAnonymous]
        public async Task<IActionResult> UpdateAsync(string taskId, [FromQuery] TaskDTO taskDTO)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                return BadRequest("Task ID cannot be empty");
            }

            try
            {
                Models.Task azureTask = await _taskService.MapTask(taskDTO);
                azureTask.PartitionKey = taskId;
                azureTask.Id = taskId;
                await _taskService.UpdateTaskAsync(azureTask);
                return Ok("You have sucessuflly updated the task");
            }
            catch (RequestFailedException azureEx)
            {
                return StatusCode(azureEx.Status, azureEx.Message);
            }
        }
    }
}