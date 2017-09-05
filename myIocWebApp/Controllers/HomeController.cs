using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Misc;

namespace myIocWebApp.Controllers
{
    public class HomeController : Controller
    {
        private IRealm realm;

        public HomeController(IRealm realm)
        {
            this.realm = realm ?? throw new ArgumentNullException("Realm must be defined.");
        }

        public IActionResult Index()
        {
            ViewBag.Realm = realm;
            ViewBag.RealmImpl = realm.GetType();
            return View();
        }
        
        public IActionResult Error()
        {
            return View();
        }
    }
}
