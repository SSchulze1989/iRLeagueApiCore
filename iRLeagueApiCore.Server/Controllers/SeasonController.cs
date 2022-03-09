using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers
{
    public class SeasonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
