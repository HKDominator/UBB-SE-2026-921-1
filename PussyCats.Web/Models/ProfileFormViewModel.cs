using PussyCats.Library.Domain;

namespace PussyCats.Web.Models
{
    public class ProfileFormViewModel
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string GitHub { get; set; } = string.Empty;

        public string LinkedIn { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string University { get; set; } = string.Empty;

        public int ExpectedGraduationYear { get; set; }

        public string Address { get; set; } = string.Empty;

        public string Motivation { get; set; } = string.Empty;

        public bool HasDisabilities { get; set; }

        public List<string> Skills { get; set; } = [];

        public List<WorkExperience> WorkExperiences { get; set; } = [];

        public List<Project> Projects { get; set; } = [];

        public List<ExtraCurricularActivity> ExtraCurricularActivities { get; set; } = [];

        public List<int> GraduationYears { get; set; } = [];
    }
}
