using Azure;
using Azure.Data.Tables;
using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;
using System.Linq;

namespace JWT_Login_Authorization_DotNet.Services
{
    public class TaskStorageService : ITaskTableStorageService
    {
        private const string TableName = "Task";
        private readonly IConfiguration _configuration;

        public TaskStorageService(IConfiguration configuration)
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

        public async System.Threading.Tasks.Task<Models.Task> GetTaskAsync(string id, string skillName)
        {
            var tableClient = await GetTableClient();
            return await tableClient.GetEntityAsync<Models.Task>(id, skillName);
        }

        public async System.Threading.Tasks.Task<Models.Task> MapTask(TaskDTO taskDTO)
        {
            Models.Task task = new Models.Task();
            task.Id = Guid.NewGuid().ToString();
            task.PartitionKey = task.Id;
            task.ETag = Azure.ETag.All;
            task.Timestamp = DateTime.Now;
            task.Description = taskDTO.Description;
            task.Level = taskDTO.Level;
            task.Code = taskDTO.Code;
            task.RowKey = taskDTO.SkillName;
            task.Title = taskDTO.Title;

            return task;
        }

        public async System.Threading.Tasks.Task<TaskDTO> MapTaskDTO(Models.Task task)
        {
            TaskDTO taskDTO = new TaskDTO();
            taskDTO.Title = task.Title;
            taskDTO.Description = task.Description;
            taskDTO.Level = task.Level;
            taskDTO.Code = task.Code;
            taskDTO.SkillName = task.RowKey;
            return taskDTO;
        }

        public async Task<List<Models.Task>> GetAllTaskAsync()
        {
            var tableClient = await GetTableClient();
            return await tableClient.QueryAsync<Models.Task>().ToListAsync();
        }

        public async System.Threading.Tasks.Task<Response> DeleteTaskAsync(string id, string skillName)
        {
            var tableClient = await GetTableClient();
            return await tableClient.DeleteEntityAsync(id, skillName);
        }

        public async System.Threading.Tasks.Task<Response> CreateTaskAsync(Models.Task task)
        {
            var tableClient = await GetTableClient();
            return await tableClient.AddEntityAsync<Models.Task>(task);
        }

        public async System.Threading.Tasks.Task UpdateTaskAsync(Models.Task task)
        {
            var tableClient = await GetTableClient();
            await tableClient.UpdateEntityAsync<Models.Task>(task, Azure.ETag.All);
        }

        public async Task<List<Models.Task>> GetTasksBySkillName(string skillName)
        {
            var tableClient = await GetTableClient();

            return await tableClient.QueryAsync<Models.Task>(x => x.RowKey.Equals(skillName)).ToListAsync();
        }

        public async System.Threading.Tasks.Task DeleteTasksBySkill(string skillName)
        {
            var tableClient = await GetTableClient();
            List<Models.Task> tasks = await GetTasksBySkillName(skillName);
            foreach (Models.Task task in tasks)
            {
                await DeleteTaskAsync(task.PartitionKey, task.RowKey);
            }
        }
    }
}