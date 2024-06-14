using Microsoft.AspNetCore.Mvc;
using Spotify.Models;
using System.Diagnostics;

namespace Spotify.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var authCookie = Request.Cookies["auth_token"];
            if (authCookie == null)
            {
                // Redirigir a localhost:3000 si la cookie no existe
                return Redirect("https://spotifyreact.work.gd");
            }
                return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Eliminar la cookie auth_token
            if (Request.Cookies["auth_token"] != null)
            {
                Response.Cookies.Delete("auth_token");
            }

            // Redirigir al usuario a la página de inicio de sesión
            return Redirect("https://spotifyreact.work.gd");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
