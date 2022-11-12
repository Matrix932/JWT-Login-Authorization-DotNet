using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JWT_Login_Authorization_DotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateTableStorageService _candidateService;

        public CandidateController(ICandidateTableStorageService candidateService)
        {
            _candidateService = candidateService;
        }

        [HttpGet]
        [ActionName(nameof(GetAsync))]
        public async Task<IActionResult> GetAsync([FromQuery] string id, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id))
            {
                return NotFound("Name or id cannot be empty ");
            }
            return Ok(await _candidateService.GetCandidateAsync(id, name));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CandidateDTO candidate)
        {
            Candidate azureCandidate = new Candidate();
            azureCandidate.Name = candidate.Name;
            azureCandidate.Id = candidate.Id;
            azureCandidate.Surname = candidate.Surname;
            azureCandidate.Seniority = candidate.Seniority;
            azureCandidate.RowKey = candidate.Name;
            azureCandidate.Timestamp = DateTime.UtcNow;
            azureCandidate.ETag = Azure.ETag.All;
            azureCandidate.PartitionKey = candidate.Id;
            await _candidateService.UpsertCandidateAsync(azureCandidate);
            return Ok("You have sucessully created candidate: " + azureCandidate.Name + " " + azureCandidate.Surname);

            //var createdEntity = await _storageService.UpsertEntityAsync(entity);
            //return CreatedAtAction(nameof(GetAsync), createdEntity);
        }
    }
}