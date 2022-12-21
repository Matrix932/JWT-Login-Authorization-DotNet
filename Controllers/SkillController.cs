using Azure;
using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace JWT_Login_Authorization_DotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly ISkillTableStoragerService _skillService;
        private readonly ITaskTableStorageService _taskTableStorageService;

        public SkillController(ISkillTableStoragerService skillService, ITaskTableStorageService taskTableStorageService)
        {
            _skillService = skillService;
            _taskTableStorageService = taskTableStorageService;
        }

        [HttpGet("GetSkill{SkillId}/{SkillName}")]
        public async Task<IActionResult> GetAsync(string SkillId, string SkillName)
        {
            if (string.IsNullOrWhiteSpace(SkillName) || string.IsNullOrWhiteSpace(SkillId))
            {
                return BadRequest("Name or id cannot be empty ");
            }
            return Ok(await _skillService.GetSkillAsync(SkillId, SkillName));
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

        [HttpGet("Skills")]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _skillService.GetAllSkillsAsync());
        }

        [HttpDelete("DeleteSkill")]
        public async Task<IActionResult> DeleteAsync(string SkillId, string SkillName)
        {
            if (string.IsNullOrWhiteSpace(SkillName) || string.IsNullOrWhiteSpace(SkillId))
            {
                return BadRequest("Name or id cannot be empty ");
            }
            try
            {
                await _skillService.DeleteSkillAsync(SkillId, SkillName);
                List<Models.Task> tasks = await _taskTableStorageService.GetTasksBySkillName(SkillName);
                if (tasks.Count > 0)
                {
                    foreach (Models.Task task in tasks)
                    {
                        await _taskTableStorageService.DeleteTaskAsync(task.Id, SkillName);
                    }
                }
                return Ok("You have sucessufully deleted : " + SkillName + " " + SkillId + " from storage");
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpPut("UpdateTask"), AllowAnonymous]
        public async Task<IActionResult> UpdateAsync(string skillId, [FromQuery] SkillDTO skillDTO)
        {
            if (string.IsNullOrWhiteSpace(skillId))
            {
                return BadRequest("SkillID cannot be empty");
            }

            try
            {
                List<Models.Task> tasks = await _taskTableStorageService.GetTasksBySkillName(skillDTO.Name);
                return Ok(tasks);
            }
            catch (RequestFailedException azureEx)
            {
                return StatusCode(azureEx.Status, azureEx.Message);
            }
        }
    }
}