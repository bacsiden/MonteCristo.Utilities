using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonteCristo.Application.Services;
using MonteCristo.Web.Models;

namespace MonteCristo.Web.Controllers
{
    public class BaseController : Controller
    {
        public IActionResult CustomRedirect(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Redirect("/");
            else
                return Redirect(url);
        }
    }
}
