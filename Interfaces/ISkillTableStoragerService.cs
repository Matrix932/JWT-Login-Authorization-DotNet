using JWT_Login_Authorization_DotNet.Models;

namespace JWT_Login_Authorization_DotNet.Interfaces
{
    public interface ISkillTableStoragerService
    {
        Task<Skill> GetSkillAsync(string id, string name);

        Task<List<Skill>> GetAllSkillsAsync();

        System.Threading.Tasks.Task DeleteSkillAsync(string id, string name);

        System.Threading.Tasks.Task CreateSkillAsync(Skill skill);

        System.Threading.Tasks.Task UpdateSkillAsync(Skill skill);

        Task<Skill> MapSkill(SkillDTO skillDTO);

        Task<SkillDTO> MapSkillDTO(Skill skill);

        Task<Skill> GetSkillByName(string skillName);

        Task<bool> CheckIfSkillExistsAsync(string skillName);
    }
}