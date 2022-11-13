using Azure.Data.Tables;
using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;

namespace JWT_Login_Authorization_DotNet.Services
{
    public class CandidateStorageService : ICandidateTableStorageService
    {
        private const string TableName = "Candidate";
        private readonly IConfiguration _configuration;

        public CandidateStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task<TableClient> GetTableClient()
        {
            var serviceClient = new TableServiceClient(_configuration["StorageConnectionString"]);
            var tableClient = serviceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

        public async Task<List<CandidateDTO>> GetAllCandidatesAsync()
        {
            var tableClient = await GetTableClient();
            List<Candidate> candidates = tableClient.Query<Candidate>().ToList();
            List<CandidateDTO> dtoCandidates = new List<CandidateDTO>();
            foreach (Candidate candidate in candidates)
            {
                dtoCandidates.Add(MapCandidateDTO(candidate).Result);
            }
            return dtoCandidates;
        }

        public async Task<Candidate> GetCandidateAsync(string id, string name)
        {
            var tableClient = await GetTableClient();
            return await tableClient.GetEntityAsync<Candidate>(id, name);
        }

        public async Task DeleteCandidateAsync(string id, string name)
        {
            var tableClient = await GetTableClient();
            await tableClient.DeleteEntityAsync(id, name);
        }

        public async Task<Candidate> UpsertCandidateAsync(Candidate candidate)
        {
            var tableClient = await GetTableClient();
            await tableClient.UpsertEntityAsync(candidate);
            return candidate;
        }

        public async Task<Candidate> MapCandidate(CandidateDTO candidateDTO)
        {
            Candidate candidate = new Candidate();
            candidate.Name = candidateDTO.Name;
            candidate.Id = candidateDTO.Id;
            candidate.Surname = candidateDTO.Surname;
            candidate.Seniority = candidateDTO.Seniority;
            candidate.RowKey = candidateDTO.Name;
            candidate.Timestamp = DateTime.UtcNow;
            candidate.ETag = Azure.ETag.All;
            candidate.PartitionKey = candidateDTO.Id;

            return candidate;
        }

        public async Task<CandidateDTO> MapCandidateDTO(Candidate candidate)
        {
            CandidateDTO candidateDto = new CandidateDTO();
            candidateDto.Name = candidate.Name;
            candidateDto.Surname = candidate.Surname;
            candidateDto.Id = candidate.Id;
            candidateDto.Seniority = candidate.Seniority;

            return candidateDto;
        }
    }
}