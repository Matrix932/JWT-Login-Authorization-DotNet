using JWT_Login_Authorization_DotNet.Models;

namespace JWT_Login_Authorization_DotNet.Interfaces
{
    public interface ICandidateTableStorageService
    {
        Task<Candidate> GetCandidateAsync(string id, string name);

        Task DeleteCandidateAsync(string id, string name);

        Task<Candidate> UpsertCandidateAsync(Candidate candidate);
    }
}