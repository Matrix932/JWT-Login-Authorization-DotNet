using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JWT_Login_Authorization_DotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly ISkillTableStoragerService _skillService;

        public SkillController(ISkillTableStoragerService skillService)
        {
            _skillService = skillService;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Skill azureSkill = _skillService.MapSkill(skillDTO).Result;

            await _skillService.CreateSkillAsync(azureSkill);
            return Ok("You have sucessully created a task: " + azureSkill.Name + " with the following ID: " + azureSkill.Id);
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
            await _skillService.DeleteSkillAsync(SkillId, SkillName);
            return Ok("You have sucessufully deleted : " + SkillName + " " + SkillId + " from storage");
        }
    }
}