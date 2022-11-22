using Azure;
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

            try
            {
                Candidate candidate = await _candidateService.GetCandidateAsync(id, name);
                return Ok(candidate);
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, ex.Message);
            }
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> GetAllCandidates()
        {
            return Ok(await _candidateService.GetAllCandidatesAsync());
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> PostAsync([FromQuery] CandidateDTO candidate)
        {
            try
            {
                Candidate azureCandidate = await _candidateService.MapCandidate(candidate);

                Response response = await _candidateService.CreateCanidateAsync(azureCandidate);

                if (response.IsError)
                {
                    return StatusCode(response.Status, "Failed to create cadidate");
                }
                return Ok("You have sucessully created candidate: " + azureCandidate.Name + " " + azureCandidate.Surname);
            }
            catch (RequestFailedException azureEx)
            {
                return StatusCode(azureEx.Status, azureEx.Message);
            }
        }

        [HttpDelete, AllowAnonymous]
        public async Task<IActionResult> DeleteAsync([FromQuery] string id, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Name or id cannot be empty ");
            }
            try
            {
                //Overiding Azure default behavior where 404's are swallowed when executing delete methods
                Response response = await _candidateService.DeleteCandidateAsync(id, name);
                if (response.Status == 404)
                {
                    return NotFound("The provided candidate does not exist in the database");
                }
                return Ok("You have sucessufully deleted : " + id + " " + name + " from storage");
            }
            catch (RequestFailedException azureEx)
            {
                return StatusCode(azureEx.Status, azureEx.Message);
            }
        }

        [HttpPut, AllowAnonymous]
        public async Task<IActionResult> UpdateAsync([FromQuery] CandidateDTO candidateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Ivalid Model State");
            }
            try
            {
                Candidate azureCandidate = await _candidateService.MapCandidate(candidateDTO);
                Response response = await _candidateService.UpdateCandidateAsync(azureCandidate);
                if (response.IsError)
                {
                    return StatusCode(response.Status, "Failed to update Candidate");
                }
                return Ok("You have sucessufully updated canidate :" + azureCandidate.Name + " ID :  " + azureCandidate.Id);
            }
            catch (RequestFailedException azureEx)
            {
                return StatusCode(azureEx.Status, azureEx.Message);
            }
        }
    }
}