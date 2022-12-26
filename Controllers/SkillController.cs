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
    public class SkillController : ControllerBase
    {
        private readonly ISkillTableStoragerService _skillService;
        private readonly ITaskTableStorageService _taskService;

        public SkillController(ISkillTableStoragerService skillService, ITaskTableStorageService taskTableStorageService)
        {
            _skillService = skillService;
            _taskService = taskTableStorageService;
        }

        [HttpGet("GetSkill")]
        [SwaggerOperation("Retrieve a skill entity using its id and skillName")]
        public async Task<IActionResult> GetAsync(string SkillId, string SkillName)
        {
            if (string.IsNullOrWhiteSpace(SkillName) || string.IsNullOrWhiteSpace(SkillId))
            {
                return BadRequest("Name or id cannot be empty ");
            }
            try
            {
                return Ok(await _skillService.GetSkillAsync(SkillId, SkillName));
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpGet("GetSkillById")]
        [SwaggerOperation("Retrieve a skill entity using its id")]
        public async Task<IActionResult> GetByIdAsync(string SkillId)
        {
            if (string.IsNullOrWhiteSpace(SkillId))
            {
                return BadRequest("Id cannot be empty ");
            }
            try
            {
                return Ok(await _skillService.GetSkillById(SkillId));
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpGet("GetSkills")]
        [SwaggerOperation("Retrieve all skill Entities")]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                return Ok(await _skillService.GetAllSkillsAsync());
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpPost("CreateSkill")]
        [SwaggerOperation("Create a Skill entity")]
        public async Task<IActionResult> PostAsync([FromQuery] SkillDTO skillDTO)
        {
            try
            {
                if (await _skillService.CheckIfSkillExistsAsync(skillDTO.Name) != false)
                {
                    return StatusCode(403, "Cannot create skill as it already exists");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                Skill azureSkill = _skillService.MapSkill(skillDTO).Result;

                await _skillService.CreateSkillAsync(azureSkill);
                return Ok("You have sucessully created a task: " + azureSkill.Name + " with the following ID: " + azureSkill.Id);
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpPut("UpdateSkill"), AllowAnonymous]
        [SwaggerOperation("Updates the skill name and the row key of all tasks belonging to the skill", "" +
            "Input the skillId of the skill you want to update, you can only update the skill name")]
        public async Task<IActionResult> UpdateAsync(string skillId, [FromQuery] SkillDTO skillDTO)
        {
            if (string.IsNullOrWhiteSpace(skillId))
            {
                return BadRequest("SkillID cannot be empty");
            }
            if (await _skillService.CheckIfSkillExistsAsync(skillDTO.Name) != false)
            {
                return StatusCode(403, "Cannot update skill name to : " + skillDTO.Name + "as a skill with name already exists");
            }

            try
            {
                //As one of the update parameters is the row key of the entity ,update logic is based on deleting the old entity using inputed ID
                //and creating a new one in the database

                //Retrieves  a skill entity(already existing/beforeupdate from the database )
                Skill beforeUpdateSkill = await _skillService.GetSkillById(skillId);
                Skill afterUpdateSkill = await _skillService.UpdateSkillMapper(skillId, skillDTO);

                List<Models.Task> tasksToDelete = await _taskService.GetTasksBySkillName(beforeUpdateSkill.Name);
                List<Models.Task> tasksToUpdate = await _taskService.GetTasksBySkillName(beforeUpdateSkill.Name);

                //Deletes the old skill from the database
                await _skillService.DeleteSkillAsync(beforeUpdateSkill.Id, beforeUpdateSkill.Name);

                //As one of the update paramaters(skillName) is the RowKey of the Task entity
                //task with the  old skill name must be deleted and task's with the new skill name must be created
                foreach (Models.Task task in tasksToDelete)
                {
                    await _taskService.DeleteTaskAsync(task.Id, task.RowKey);
                }
                foreach (Models.Task task in tasksToUpdate)
                {
                    task.RowKey = skillDTO.Name;
                    await _taskService.CreateTaskAsync(task);
                }

                //Mapping task with the updatedSkillName to DTO's and SerilizingThem to the new(updatedSKill)
                List<TaskDTO> afterUpdateDTO = new List<TaskDTO>();
                foreach (var task in tasksToUpdate)
                {
                    afterUpdateDTO.Add(await _taskService.MapTaskDTO(task));
                }
                afterUpdateSkill.Tasks = JsonSerializer.Serialize<List<TaskDTO>>(afterUpdateDTO);
                await _skillService.CreateSkillAsync(afterUpdateSkill);
                return Ok("Sucessfully updated the skill");
                //Mapping task with the updatedSkillName to DTO's and SerilizingThem to the new(updatedSKill)
            }
            catch (RequestFailedException azureEx)
            {
                return StatusCode(azureEx.Status, azureEx.Message);
            }
        }

        [HttpDelete("DeleteSkill")]
        [SwaggerOperation("Deletes a Skill Entity and all asociates tasks with the skill")]
        public async Task<IActionResult> DeleteAsync(string SkillId, string SkillName)
        {
            if (string.IsNullOrWhiteSpace(SkillName) || string.IsNullOrWhiteSpace(SkillId))
            {
                return BadRequest("Name or id cannot be empty ");
            }
            try
            {
                await _skillService.DeleteSkillAsync(SkillId, SkillName);
                List<Models.Task> tasks = await _taskService.GetTasksBySkillName(SkillName);
                if (tasks.Count > 0)
                {
                    foreach (Models.Task task in tasks)
                    {
                        await _taskService.DeleteTaskAsync(task.Id, SkillName);
                    }
                }
                return Ok("You have sucessufully deleted : " + SkillName + " " + SkillId + " from storage");
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpDelete("DeleteAllSkills")]
        [SwaggerOperation("Deletes all skills and all task entities from storage",
        "To keep the integrity of storage all task entites must be Deleted ")]
        public async Task<IActionResult> DeleteSkillsAsync()
        {
            try
            {
                await _skillService.DeleteAllsSkillsAsync();
                await _taskService.DeleteAllTasksAsync();
                return Ok("You have successfully deleted all skills from storage");
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }
    }
}