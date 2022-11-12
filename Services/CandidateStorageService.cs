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
    }
}