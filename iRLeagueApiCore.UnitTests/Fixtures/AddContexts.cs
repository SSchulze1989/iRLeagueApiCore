using iRLeagueApiCore.Server.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.UnitTests.Fixtures
{
    public static class AddContexts
    {
        public static ClaimsPrincipal User => new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "unitTestUser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, UserRoles.User),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));

        /// <summary>
        /// Add the default HttpContext to the provided controller
        /// </summary>
        /// <typeparam name="T">type of ApiController</typeparam>
        /// <param name="controller">Controller to add context</param>
        /// <returns>Controller with added context</returns>
        public static T AddMemberControllerContext<T>(T controller) where T : Controller
        {
            var user = User;
            var identity = (ClaimsIdentity)user.Identity;
            identity.AddClaim(new Claim(ClaimTypes.Role, $"{"TestLeague".ToLower()}:{LeagueRoles.Member}"));
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return controller;
        }

        public static T AddLeagueAdminControllerContext<T>(T controller, string leagueName) where T : Controller
        {
            var user = User;
            var identity = (ClaimsIdentity)user.Identity;
            identity.AddClaim(new Claim(ClaimTypes.Role, $"{leagueName.ToLower()}:{LeagueRoles.Admin}"));
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return controller;
        }

        public static T AddAdminControllerContext<T>(T controller) where T : Controller
        {
            var user = User;
            var identity = (ClaimsIdentity)user.Identity;
            identity.AddClaim(new Claim(ClaimTypes.Role, UserRoles.Admin));
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return controller;
        }

        public static T AddControllerContextWithoutLeagueRole<T>(T controller) where T : Controller
        {
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = User }
            };

            return controller;
        }
    }
}
