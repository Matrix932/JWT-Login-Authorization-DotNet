﻿using Azure;
using JWT_Login_Authorization_DotNet.Models;
using Task = System.Threading.Tasks.Task;

namespace JWT_Login_Authorization_DotNet.Interfaces
{
    public interface ICandidateTableStorageService
    {
        Task<Candidate> GetCandidateAsync(string id, string name);

        Task<List<CandidateDTO>> GetAllCandidatesAsync();

        Task<Response> DeleteCandidateAsync(string id, string name);

        Task<Candidate> MapCandidate(CandidateDTO candidateDTO);

        Task<CandidateDTO> MapCandidateDTO(Candidate candidate);

        Task<Response> CreateCanidateAsync(Candidate candidate);

        Task<Response> UpdateCandidateAsync(Candidate candidate);

        System.Threading.Tasks.Task DeleteAllCandidatesAsync();
    }
}