using System.Linq;
using System.Text.RegularExpressions;

namespace iRLeagueApiCore.Server.Authentication
{
    public static class LeagueRoles
    {
        /// <summary>
        /// Administrator of the league with full privileges
        /// </summary>
        public const string Admin = "Admin";
        /// <summary>
        /// Organizer of the league
        /// Write privileges but not allowed to delete seasons or assign roles
        /// </summary>
        public const string Organizer = "Organizer";
        /// <summary>
        /// Member with restricted read access to public information:
        /// schedules, sessions, results - but not incident reviews
        /// </summary>
        public const string Member = "Member";
        /// <summary>
        /// Member with read access to reviews and allowed to create and edit own reviews
        /// No write access to schedules, sessions or results
        /// </summary>
        public const string Steward = "Steward";

        /// <summary>
        /// Array of all available league roles
        /// </summary>
        public static string[] RolesAvailable { get; } = new string[] { Admin, Organizer, Member, Steward };

        /// <summary>
        /// Delimiter used to separate between league name and role name
        /// </summary>
        public const char RoleDelimiter = ':';

        /// <summary>
        /// Get the full league role name for a provided league name and role name
        /// </summary>
        /// <returns><see langword="null"/> if <paramref name="roleName"/> is not valid</returns>
        public static string? GetLeagueRoleName(string leagueName, string roleName)
        {
            if (IsValidRole(roleName) == false)
            {
                return null;
            }

            return $"{leagueName.ToLower()}{RoleDelimiter}{CapitalizeRoleName(roleName)}";
        }

        public static string? GetRoleName(string leagueRoleName)
        {
            return leagueRoleName.Split(RoleDelimiter).ElementAtOrDefault(1);
        }

        /// <summary>
        /// Check wether a full league role name is a valid role name for the provided league
        /// </summary>
        /// <returns></returns>
        public static bool IsLeagueRoleName(string leagueName, string roleName)
        {
            var pattern = $"({leagueName})({RoleDelimiter})({string.Join('|', RolesAvailable)})";   
            return Regex.IsMatch(roleName, pattern, RegexOptions.IgnoreCase);
        }

        public static bool IsValidRole(string roleName)
        {
            return RolesAvailable.Any(x => x.Equals(roleName, System.StringComparison.OrdinalIgnoreCase));
        }

        private static string CapitalizeRoleName(string roleName)
        {
            return char.ToUpper(roleName[0]) + roleName[1..].ToLower();
        }
    }
}
