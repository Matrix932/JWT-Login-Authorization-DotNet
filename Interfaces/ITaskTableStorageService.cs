using Azure;
using JWT_Login_Authorization_DotNet.Models;

namespace JWT_Login_Authorization_DotNet.Interfaces
{
    public interface ITaskTableStorageService
    {
        //Skill Name will be row key
        //
        Task<Models.Task> GetTaskAsync(string id, string skillName);

        Task<List<Models.Task>> GetAllTaskAsync();

        System.Threading.Tasks.Task<Response> DeleteTaskAsync(string id, string skillName);

        System.Threading.Tasks.Task<Response> CreateTaskAsync(Models.Task task);

        System.Threading.Tasks.Task UpdateTaskAsync(Models.Task task);

        System.Threading.Tasks.Task<Models.Task> MapTask(TaskDTO taskDTO);

        System.Threading.Tasks.Task<TaskDTO> MapTaskDTO(Models.Task task);

        Task<List<Models.Task>> GetTasksBySkillName(string skillName);

        System.Threading.Tasks.Task DeleteTasksBySkill(string skillName);

        System.Threading.Tasks.Task DeleteAllTasksAsync();

        System.Threading.Tasks.Task<Models.Task> GetTaskById(string id);
    }
}