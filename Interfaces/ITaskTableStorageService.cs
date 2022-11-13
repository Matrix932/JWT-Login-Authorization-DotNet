using JWT_Login_Authorization_DotNet.Models;

namespace JWT_Login_Authorization_DotNet.Interfaces
{
    public interface ITaskTableStorageService
    {
        //Skill Name will be row key
        Task<JWT_Login_Authorization_DotNet.Models.Task> GetTaskAsync(string id, string skillName);

        Task<List<TaskDTO>> GetAllTaskAsync();

        System.Threading.Tasks.Task DeleteTaskAsync(string id, string skillName);

        System.Threading.Tasks.Task CreateTaskAsync(JWT_Login_Authorization_DotNet.Models.Task task);

        System.Threading.Tasks.Task UpdateTaskAsync(JWT_Login_Authorization_DotNet.Models.Task task);

        System.Threading.Tasks.Task MapTask(JWT_Login_Authorization_DotNet.Models.Task task);

        System.Threading.Tasks.Task MapTaskDTO(TaskDTO taskDTO);
    }
}