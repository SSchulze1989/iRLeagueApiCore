using iRLeagueApiCore.Server.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace iRLeagueApiCore.Server.Models
{
    public class LeagueUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Roles { get; set; }

        public LeagueUser(string leagueName, ClaimsPrincipal principal)
        {
            Id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            Name = principal.Identity?.Name ?? string.Empty;
            Roles = principal.FindAll(ClaimTypes.Role)
                .Select(x => x.Value)
                .Where(role => LeagueRoles.IsLeagueRoleName(leagueName, role));
        }

        public static LeagueUser Empty => new LeagueUser("", new ClaimsPrincipal());
    }
}
