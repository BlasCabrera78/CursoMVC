using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CapaPresentacionTienda.Controllers
{
    public class TiendaController : Controller
    {
        // GET: Teinda
        public ActionResult Index()
        {
            return View();
        }
    }
}