using System.ComponentModel.DataAnnotations;

namespace JWT_Login_Authorization_DotNet.Models
{
    public class CandidateDTO
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [RegularExpression("Junior|Middle|Senior", ErrorMessage = "Seniority must be Junior,Middle or Senior")]
        public string Seniority { get; set; }
    }
}