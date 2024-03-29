﻿using Azure.Data.Tables;
using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;
using System.Linq;

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
            return await tableClient.QueryAsync<Models.Skill>().ToListAsync();
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

        public async System.Threading.Tasks.Task UpdateSkillAsync(Skill skill)
        {
            var tableClient = await GetTableClient();
            await tableClient.UpdateEntityAsync<Models.Skill>(skill, Azure.ETag.All);
        }

        public async Task<Skill> GetSkillByName(string skillName)
        {
            var tableClient = await GetTableClient();
            Skill skill = await tableClient.QueryAsync<Skill>(x => x.RowKey.Equals(skillName)).SingleAsync();
            return skill;
        }

        public async Task<bool> CheckIfSkillExistsAsync(string skillName)
        {
            var tableClient = await GetTableClient();
            int count = await tableClient.QueryAsync<Skill>(x => x.RowKey.Equals(skillName)).CountAsync();
            return count > 0 ? true : false;
        }

        public async System.Threading.Tasks.Task DeleteAllsSkillsAsync()
        {
            //Found that this is better practice then droping whole table as droping may colide with insert opearations
            var tableClient = await GetTableClient();
            List<Skill> skills = await tableClient.QueryAsync<Skill>().ToListAsync();
            foreach (Skill skill in skills)
            {
                await tableClient.DeleteEntityAsync(skill.PartitionKey, skill.RowKey);
            }
        }

        public async System.Threading.Tasks.Task DeleteAllTasksFromSkills()
        {
            var tableClient = await GetTableClient();
            List<Skill> skills = await tableClient.QueryAsync<Skill>().ToListAsync();
            foreach (Skill skill in skills)
            {
                skill.Tasks = String.Empty;

                await tableClient.UpdateEntityAsync<Skill>(skill, Azure.ETag.All);
            }
        }

        public async Task<Skill> GetSkillById(string id)
        {
            var tableClient = await GetTableClient();
            Skill skill = await tableClient.QueryAsync<Skill>(x => x.PartitionKey.Equals(id)).SingleAsync();
            return skill;
        }

        public async Task<Skill> UpdateSkillMapper(string id, SkillDTO skillDTO)
        {
            Skill skill = new();
            skill.Id = id;
            skill.PartitionKey = id;
            skill.Name = skillDTO.Name;
            skill.RowKey = skill.Name;
            skill.Timestamp = DateTime.Now;
            skill.ETag = Azure.ETag.All;
            return skill;
        }
    }
}