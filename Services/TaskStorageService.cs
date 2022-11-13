using Azure.Data.Tables;
using JWT_Login_Authorization_DotNet.Models;

namespace JWT_Login_Authorization_DotNet.Services
{
    public class TaskStorageService
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

        //public async Task<List<TaskDTO>> GetAllTasks()
        //{
        //    var tableClient = await GetTableClient();

        //    List<JWT_Login_Authorization_DotNet.Models.Task> tasks = tableClient.Query<JWT_Login_Authorization_DotNet.Models.Task>().ToList();

        //}
    }
}