using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JWT_Login_Authorization_DotNet.Models
{
    public class SkillDTO
    {
        [Required]
        [RegularExpression(".Net|Java|Python|Typescript|Java script|React|Angular|QA", ErrorMessage = "Skill cannot be set to this value")]
        public string Name { get; set; }
    }
}