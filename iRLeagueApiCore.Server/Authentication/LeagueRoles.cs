using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public static readonly string[] RolesAvailable = new string[] { Admin, Organizer, Member, Steward };

        /// <summary>
        /// Get the full league role name for a provided league name and role name
        /// </summary>
        /// <returns></returns>
        public static string GetLeagueRoleName(string leagueName, string roleName)
        {
            return $"{leagueName.ToLower()}:{roleName}";
        }
    }
}
