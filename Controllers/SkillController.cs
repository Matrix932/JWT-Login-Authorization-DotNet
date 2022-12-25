using Azure;
using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;

namespace JWT_Login_Authorization_DotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> GetAsync(string SkillId, string SkillName)
        {
            if (string.IsNullOrWhiteSpace(SkillName) || string.IsNullOrWhiteSpace(SkillId))
            {
                return BadRequest("Name or id cannot be empty ");
            }
            return Ok(await _skillService.GetSkillAsync(SkillId, SkillName));
        }

        [HttpGet("GetSkills")]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _skillService.GetAllSkillsAsync());
        }

        [HttpPost]
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("Updates the skill name and the row key of all tasks belonging to the skill","" +
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
                Skill beforeUpdateSkill = await _skillService.GetSkillById(skillId);
                Skill afterUpdateSkill = await _skillService.UpdateSkillMapper(skillId, skillDTO);

                List<Models.Task> tasksToDelete = await _taskService.GetTasksBySkillName(beforeUpdateSkill.Name);
                List<Models.Task> tasksToUpdate = await _taskService.GetTasksBySkillName(beforeUpdateSkill.Name);
                await _skillService.DeleteSkillAsync(beforeUpdateSkill.Id, beforeUpdateSkill.Name);

                foreach (Models.Task task in tasksToDelete)
                {
                    await _taskService.DeleteTaskAsync(task.Id, task.RowKey);
                }
                foreach (Models.Task task in tasksToUpdate)
                {
                    task.RowKey = skillDTO.Name;
                    await _taskService.CreateTaskAsync(task);
                }
                List<TaskDTO> afterUpdateDTO = new List<TaskDTO>();
                foreach (var task in tasksToUpdate)
                {
                    afterUpdateDTO.Add(await _taskService.MapTaskDTO(task));
                }
                afterUpdateSkill.Tasks = JsonSerializer.Serialize<List<TaskDTO>>(afterUpdateDTO);
                await _skillService.CreateSkillAsync(afterUpdateSkill);
                return Ok("Lets see if this worked");
            }
            catch (RequestFailedException azureEx)
            {
                return StatusCode(azureEx.Status, azureEx.Message);
            }
        }

        [HttpDelete("DeleteSkill")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("Deletes a Skill Entity and all asociates tasks with the skill")]
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("Deletes all skills and all task entities from storage",
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