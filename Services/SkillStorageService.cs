using Azure.Data.Tables;
using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;

namespace JWT_Login_Authorization_DotNet.Services
{
    public class SkillStorageService : ISkillTableStoragerService
    {
        private const string TableName = "Skill";
        private readonly IConfiguration _configuration;

        public SkillStorageService(IConfiguration configuration)
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

        public async System.Threading.Tasks.Task CreateSkillAsync(Skill skill)
        {
            var tableClient = await GetTableClient();
            await tableClient.AddEntityAsync<Models.Skill>(skill);
        }

        public async System.Threading.Tasks.Task DeleteSkillAsync(string id, string name)
        {
            var tableClient = await GetTableClient();
            await tableClient.DeleteEntityAsync(id, name);
        }

        public async Task<List<Skill>> GetAllSkillsAsync()
        {
            var tableClient = await GetTableClient();
            List<Models.Skill> skills = tableClient.Query<Models.Skill>().ToList();
            return skills;
        }

        public async Task<Skill> GetSkillAsync(string id, string name)
        {
            var tableClient = await GetTableClient();
            return await tableClient.GetEntityAsync<Models.Skill>(id, name);
        }

        public async Task<Skill> MapSkill(SkillDTO skillDTO)
        {
            Skill skill = new Skill();
            skill.Id = Guid.NewGuid().ToString();
            skill.PartitionKey = skill.Id;
            skill.Name = skillDTO.Name;
            skill.RowKey = skill.Name;
            skill.Timestamp = DateTime.Now;
            skill.ETag = Azure.ETag.All;
            return skill;
        }

        public async Task<SkillDTO> MapSkillDTO(Skill skill)
        {
            SkillDTO skillDTO = new SkillDTO();
            skillDTO.Name = skill.Name;
            return skillDTO;
        }

        public Task<Skill> UpdateSkillAsync(Skill skill)
        {
            throw new NotImplementedException();
        }
    }
}