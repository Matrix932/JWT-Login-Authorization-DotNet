using Azure;
using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;

namespace JWT_Login_Authorization_DotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskTableStorageService _taskService;
        private readonly ISkillTableStoragerService _skillService;

        public TaskController(ITaskTableStorageService taskTableStorageService, ISkillTableStoragerService skillService)
        {
            _taskService = taskTableStorageService;
            _skillService = skillService;
        }

        [HttpGet("{id}/{name}")]
        [SwaggerOperation("Retrive task entity by id and name", "Retrive Task Entity")]
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

        [HttpGet("{id}")]
        [SwaggerOperation("Retrieve a task entity using its id ")]
        public async Task<IActionResult> GetByIdAsync(string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                return BadRequest("Id cannot be empty ");
            }
            try
            {
                return Ok(await _taskService.GetTaskById(taskId));
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpGet("tasks")]
        [SwaggerOperation("Retrieve all Task entities from storage")]
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

        [HttpGet("{skillId}/tasks")]
        [SwaggerOperation("Retrieve all Task entities from storage that have the inputed skill name")]
        public async Task<IActionResult> GetTasksBySkillAsync(string skillName)
        {
            if (string.IsNullOrWhiteSpace(skillName))
            {
                return BadRequest("Skill name cannot be empty");
            }
            try
            {
                return Ok(await _taskService.GetTasksBySkillName(skillName));
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpPost("task")]
        [SwaggerOperation("Create a task entity",
            "You cannot create a task for a Skill that doesnt exist in the database, task will automatically be added to corresponding skill after creation")]
        public async Task<IActionResult> PostAsync([FromQuery] TaskDTO taskDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state");
            }

            try
            {
                Models.Task azureTask = await _taskService.MapTask(taskDTO);
                if (await _skillService.CheckIfSkillExistsAsync(taskDTO.SkillName) == false)
                {
                    return StatusCode(400, "Cannot create atask as the following skill has not been created");
                }
                Response response = await _taskService.CreateTaskAsync(azureTask);

                Skill skill = await _skillService.GetSkillByName(taskDTO.SkillName);
                skill.Tasks += Newtonsoft.Json.JsonConvert.SerializeObject(taskDTO);
                await _skillService.UpdateSkillAsync(skill);

                return Ok("You have sucessully created a task: " + azureTask.Title + " for the following skill " + azureTask.RowKey);
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpPut("task")]
        [SwaggerOperation("Update a Task entity",
            "Input the ID of the Task that you want to update, all other inputs are new update values." +
            "You cannot update the Task's skill name to a skill that doesnt exist in the database")]
        public async Task<IActionResult> UpdateAsync(string taskId, [FromQuery] TaskDTO taskDTO)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                return BadRequest("Task ID cannot be empty");
            }
            if (await _skillService.CheckIfSkillExistsAsync(taskDTO.SkillName) != true)
            {
                return BadRequest("Cannot update Task skill name to : " + taskDTO.SkillName + " because that skill hasnt been created yet, please create the skill entity first");
            }
            try
            {
                //As one of the update parameters is the row key of the entity ,update logic is based on deleting the old entity using inputed ID
                //and creating a new one in the database
                Models.Task taskBeforeUpdate = await _taskService.GetTaskById(taskId);
                await _taskService.DeleteTaskAsync(taskBeforeUpdate.Id, taskBeforeUpdate.RowKey);

                Models.Task azureTask = await _taskService.MapTask(taskDTO);
                azureTask.PartitionKey = taskId;
                azureTask.Id = taskId;
                await _taskService.CreateTaskAsync(azureTask);
                //
                try
                {
                    //Removing  the deletedTask(beforeUpdate) from its coresponding
                    TaskDTO deletedTaskDTO = await _taskService.MapTaskDTO(taskBeforeUpdate);

                    Skill oldTaskSkill = await _skillService.GetSkillByName(taskBeforeUpdate.RowKey);
                    string deletedTaskSerilized = JsonSerializer.Serialize<TaskDTO>(deletedTaskDTO);

                    //Doesnt work in single line of code(skillBeforeUpdate.Tasks..Replace(deletedTaskSerilized, null).Trim();
                    var replaceHelper = oldTaskSkill.Tasks.Replace(deletedTaskSerilized, null).Trim();
                    oldTaskSkill.Tasks = replaceHelper;
                    await _skillService.UpdateSkillAsync(oldTaskSkill);
                    //Removing  the deletedTask(beforeUpdate) from its coresponding

                    //Adding the updated(newTask) to its corresponding skill
                    Skill newTaskSkill = await _skillService.GetSkillByName(taskDTO.SkillName);
                    string afterUpdateSeriliazed = JsonSerializer.Serialize<TaskDTO>(taskDTO);
                    newTaskSkill.Tasks += afterUpdateSeriliazed;
                    await _skillService.UpdateSkillAsync(newTaskSkill);
                    //Adding the updated(newTask) to its corresponding skill
                }
                catch (RequestFailedException azureEx)
                {
                    return StatusCode(azureEx.Status, "Error from updates" + azureEx.Message);
                }
                return Ok("You have sucessuflly updated the task");
            }
            catch (RequestFailedException azureEx)
            {
                return StatusCode(azureEx.Status, azureEx.Message);
            }
        }

        [HttpDelete("{id}/{skillName}")]
        [SwaggerOperation("Delete a Task entity and remove it from its coresponding skill")]
        public async Task<IActionResult> DeleteAsync([FromQuery] string id, string skillName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(skillName) || string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Name or id cannot be empty ");
                }

                Models.Task task = await _taskService.GetTaskAsync(id, skillName);
                TaskDTO taskDTO = await _taskService.MapTaskDTO(task);
                Response response = await _taskService.DeleteTaskAsync(id, skillName);
                if (response.IsError)
                {
                    return StatusCode(response.Status, "Failed to delete task");
                }
                try
                {
                    Skill skill = await _skillService.GetSkillByName(skillName);

                    if (!String.IsNullOrEmpty(skill.Tasks))
                    {
                        var taskString = JsonSerializer.Serialize(taskDTO);

                        var s1 = skill.Tasks.Replace(taskString, null).Trim();
                        skill.Tasks = s1;
                        await _skillService.UpdateSkillAsync(skill);
                    }
                    else
                    {
                        return Ok("Task was not part of any skill but it was deleted from storatge ");
                    }
                }
                catch (RequestFailedException azureEx)
                {
                    return StatusCode(azureEx.Status, azureEx.Message);
                }

                return Ok("You have sucessufully deleted the task from storage");
            }
            catch (RequestFailedException azureEx)
            {
                return StatusCode(azureEx.Status, azureEx.Message);
            }
        }

        [HttpDelete("tasks")]
        [SwaggerOperation("Deletes all task entities from storage",
        "Also removes Task entites from their coresponding skill")]
        public async Task<IActionResult> DeleteAllTasksAsync()
        {
            try
            {
                await _taskService.DeleteAllTasksAsync();
                try
                {
                    await _skillService.DeleteAllTasksFromSkills();
                }
                catch (RequestFailedException azureEx)
                {
                    return StatusCode(azureEx.Status, azureEx.Message);
                }
                return Ok("You have sucessfully deleted all tasks");
            }
            catch (RequestFailedException azureEx)
            {
                return StatusCode(azureEx.Status, azureEx.Message);
            }
        }
    }
}