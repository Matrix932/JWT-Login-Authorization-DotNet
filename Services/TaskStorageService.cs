using Azure.Data.Tables;
using JWT_Login_Authorization_DotNet.Interfaces;
using JWT_Login_Authorization_DotNet.Models;

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

        public async Task<List<TaskDTO>> GetAllTaskAsync()
        {
            var tableClient = await GetTableClient();
            List<Models.Task> tasks = tableClient.Query<Models.Task>().ToList();
            List<TaskDTO> taskDTOs = new List<TaskDTO>();
            foreach (var task in tasks)
            {
                taskDTOs.Add(MapTaskDTO(task).Result);
            }
            return taskDTOs;
        }

        public async System.Threading.Tasks.Task DeleteTaskAsync(string id, string skillName)
        {
            var tableClient = await GetTableClient();
            await tableClient.DeleteEntityAsync(id, skillName);
        }

        public async System.Threading.Tasks.Task CreateTaskAsync(Models.Task task)
        {
            var tableClient = await GetTableClient();
            await tableClient.AddEntityAsync<Models.Task>(task);
        }

        public async System.Threading.Tasks.Task UpdateTaskAsync(Models.Task task)
        {
            var tableClient = await GetTableClient();
            await tableClient.UpdateEntityAsync<Models.Task>(task, Azure.ETag.All);
        }
    }
}