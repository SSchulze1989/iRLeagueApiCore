using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Filters
{
    /// <summary>
    /// Sets requirement for having at least one of the listed league roles to access the resource
    /// Only works in combination with <see cref="LeagueAuthorizeAttribute"/>
    /// <para>If no league role is specified, the user is checked to be in at least one of any available league role</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireLeagueRoleAttribute : Attribute
    {
        /// <summary>
        /// Private field to prevent modifiying from outside
        /// </summary>
        private readonly string[] _roles;
        /// <summary>
        /// Roles that are required to access the decorated resource
        /// </summary>
        public string[] Roles => _roles.ToArray();
        /// <summary>
        /// </summary>
        /// <param name="roles">List of roles. Requires user to be in at leas one of the provided roles</param>
        public RequireLeagueRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }
    }
}
