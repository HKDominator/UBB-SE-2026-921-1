using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserProfileService profileService;

        public ProfileController(IUserProfileService profileService)
        {
            this.profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            // Replace this with your session/auth logic
            int userId = GetCurrentUserId();

            var user = await profileService.GetProfileAsync(userId);

            if (user == null)
            {
                user = new User
                {
                    UserId = userId
                };
            }

            var viewModel = MapToViewModel(user);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateGraduationYears(model);
                return View(model);
            }

            var user = MapToDomain(model);

            await profileService.SaveAsync(user.UserId, user);

            TempData["SuccessMessage"] = "Profile saved successfully.";

            return RedirectToAction(nameof(Edit));
        }

        private static ProfileFormViewModel MapToViewModel(User user)
        {
            var currentYear = DateTime.Now.Year;

            return new ProfileFormViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Age = user.Age,
                Gender = user.Gender,
                Email = user.Email,
                PhoneNumber = user.Phone,
                GitHub = user.GitHub,
                LinkedIn = user.LinkedIn,
                Country = user.Country,
                City = user.City,
                University = user.University,
                ExpectedGraduationYear = user.ExpectedGraduationYear,
                Address = user.Address,
                Motivation = user.Motivation,
                HasDisabilities = user.HasDisabilities,

                Skills = user.Skills
                    .Select(skill => skill.Skill?.Name ?? string.Empty)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList(),

                WorkExperiences = user.WorkExperiences.ToList(),

                Projects = user.Projects.ToList(),

                ExtraCurricularActivities =
                    user.ExtraCurricularActivities.ToList(),

                GraduationYears =
                    Enumerable.Range(currentYear, 11).ToList()
            };
        }

        private static User MapToDomain(ProfileFormViewModel model)
        {
            return new User
            {
                UserId = model.UserId,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Age = model.Age,
                Gender = model.Gender,
                Email = model.Email.Trim(),
                Phone = model.PhoneNumber.Trim(),
                GitHub = model.GitHub.Trim(),
                LinkedIn = model.LinkedIn.Trim(),
                Country = model.Country.Trim(),
                City = model.City.Trim(),
                University = model.University.Trim(),
                ExpectedGraduationYear = model.ExpectedGraduationYear,
                Address = model.Address.Trim(),
                Motivation = model.Motivation.Trim(),
                HasDisabilities = model.HasDisabilities,

                Skills = model.Skills
                    .Where(skill => !string.IsNullOrWhiteSpace(skill))
                    .Select(skill => new UserSkill
                    {
                        Skill = new Skill
                        {
                            Name = skill
                        }
                    })
                    .ToList(),

                WorkExperiences = model.WorkExperiences,

                Projects = model.Projects,

                ExtraCurricularActivities =
                    model.ExtraCurricularActivities,

                LastUpdated = DateTime.UtcNow
            };
        }

        private static void PopulateGraduationYears(ProfileFormViewModel model)
        {
            var currentYear = DateTime.Now.Year;

            model.GraduationYears =
                Enumerable.Range(currentYear, 11).ToList();
        }

        private int GetCurrentUserId()
        {
            // Replace with actual auth/session logic
            return 1;
        }
    }
}
