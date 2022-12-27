using Microsoft.AspNetCore.Mvc;

namespace TrainPassenger.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
