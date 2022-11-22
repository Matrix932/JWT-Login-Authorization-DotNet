using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JWT_Login_Authorization_DotNet.Models
{
    public class TaskDTO
    {
        public string? Description { get; set; }

        [RegularExpression("Junior|Middle|Senior", ErrorMessage = "Level must be junior, middle or senior")]
        public string Level { get; set; }

        [Required]
        public string? Title { get; set; }

        public string? Code { get; set; }

        [Required]
        [RegularExpression("Not assigned|.Net|Java|Python|Typescript|Java script|React|Angular|QA", ErrorMessage = "Skill cannot be set to this value")]
        public string? SkillName { get; set; }
    }
}