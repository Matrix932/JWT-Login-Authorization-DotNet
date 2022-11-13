using JWT_Login_Authorization_DotNet.Models;

namespace JWT_Login_Authorization_DotNet.Interfaces
{
    public interface ICandidateTableStorageService
    {
        Task<Candidate> GetCandidateAsync(string id, string name);

        Task<List<CandidateDTO>> GetAllCandidatesAsync();

        Task DeleteCandidateAsync(string id, string name);

        Task<Candidate> UpsertCandidateAsync(Candidate candidate);

        Task<Candidate> MapCandidate(CandidateDTO candidateDTO);

        Task<CandidateDTO> MapCandidateDTO(Candidate candidate);
    }
}