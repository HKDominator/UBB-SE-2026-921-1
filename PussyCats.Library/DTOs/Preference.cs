// Preserved for IPreferenceService.GetByUserIdAsync's legacy return
// shape. The merged model stores preferences as fields on User
// (PreferredEmploymentType, WorkModePreference, LocationPreference).
// PreferenceService translates between the two to keep view-model
// surfaces stable through Phase 5. After view-model migration,
// consider replacing the IPreferenceService surface with a flat
// UserPreferences record and deleting this DTO.

namespace PussyCats.Library.DTOs;

public class Preference
{
    public int PreferenceId { get; set; }
    public int UserId { get; set; }
    public string PreferenceType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
