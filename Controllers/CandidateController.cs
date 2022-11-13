using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWT_Login_Authorization_DotNet.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateTableStorageService _candidateService;

        public CandidateController(ICandidateTableStorageService candidateService)
        {
            _candidateService = candidateService;
        }

        [HttpGet]
        [ActionName("GetCandidate")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAsync([FromQuery] string id, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Name or id cannot be empty ");
            }
            return Ok(await _candidateService.GetCandidateAsync(id, name));
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> GetAllCandidates()
        {
            return Ok(await _candidateService.GetAllCandidatesAsync());
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> PostAsync([FromQuery] CandidateDTO candidate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Candidate azureCandidate = _candidateService.MapCandidate(candidate).Result;

            await _candidateService.UpsertCandidateAsync(azureCandidate);
            return Ok("You have sucessully created candidate: " + azureCandidate.Name + " " + azureCandidate.Surname);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] string id, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Name or id cannot be empty ");
            }
            await _candidateService.DeleteCandidateAsync(id, name);
            return Ok("You have sucessufully deleted : " + id + " " + name + " from storage");
        }
    }
}