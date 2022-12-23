using Azure;
using Azure.Data.Tables;
using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;
using Task = System.Threading.Tasks.Task;
using System.Linq;

namespace JWT_Login_Authorization_DotNet.Services
{
    public class CandidateStorageService : ICandidateTableStorageService
    {
        private const string TableName = "Candidate";
        private readonly IConfiguration _configuration;

        //DI
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
            List<Candidate> candidates = await tableClient.QueryAsync<Candidate>().ToListAsync();
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

        public async Task<Response> DeleteCandidateAsync(string id, string name)
        {
            var tableClient = await GetTableClient();
            return await tableClient.DeleteEntityAsync(id, name);
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

        public async Task<Response> CreateCanidateAsync(Candidate candidate)
        {
            var tableClient = await GetTableClient();
            Response response = await tableClient.AddEntityAsync<Models.Candidate>(candidate);
            return response;
        }

        public async Task<Response> UpdateCandidateAsync(Candidate candidate)
        {
            var tableClient = await GetTableClient();
            Response reseponse = await tableClient.UpdateEntityAsync<Models.Candidate>(candidate, Azure.ETag.All);
            return reseponse;
        }

        public async Task DeleteAllCandidatesAsync()
        {
            //Found that this is better practice then droping whole table as droping may colide with insert opearations
            var tableClient = await GetTableClient();
            List<Candidate> candidates = await tableClient.QueryAsync<Candidate>().ToListAsync();
            foreach (Candidate candidate in candidates)
            {
                await tableClient.DeleteEntityAsync(candidate.Id, candidate.Name);
            }
        }
    }
}