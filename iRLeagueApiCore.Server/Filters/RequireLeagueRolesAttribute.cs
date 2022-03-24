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
    public class RequireLeagueRolesAttribute : Attribute
    {
        public string[] Roles { get; }
        public RequireLeagueRolesAttribute(params string[] roles)
        {
            Roles = roles;
        }
    }
}
