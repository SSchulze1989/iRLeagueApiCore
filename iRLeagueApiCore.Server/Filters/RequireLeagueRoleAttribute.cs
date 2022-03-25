using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Filters
{
    /// <summary>
    /// Sets requirement for having at least one of the listed league roles to access the resource
    /// Only works in combination with <see cref="LeagueAuthorizeAttribute"/>
    /// </summary>
    public class RequireLeagueRoleAttribute : Attribute
    {
        public string[] Roles { get; }
        /// <summary>
        /// </summary>
        /// <param name="roles">List of roles. Requires user to be in at leas one of the provided roles</param>
        public RequireLeagueRoleAttribute(params string[] roles)
        {
            Roles = roles;
        }
    }
}
