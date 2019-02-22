using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonteCristo.Application.Services;
using MonteCristo.Web.Models;

namespace MonteCristo.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public HomeController(IUserService userService, CacheManager.ICacheManager cacheManager)
        {
            cacheManager.Create(1,1);
            var xx = cacheManager.Get<int>(1);
            var xxxx = cacheManager.GetOrCreate<Application.Models.Framework.ApplicationUser>(2, () => userService.GetAsync("5c6f62b83d0b093ea8ddc96a").GetAwaiter().GetResult());
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("error/{code:int}")]
        public IActionResult Error(int code)
        {
            return View($"~/Views/Shared/404.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
            //return View($"~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
