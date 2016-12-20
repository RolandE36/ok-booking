using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MainWebApp.Controllers {
	public class HomeController : Controller {
		public ActionResult Index() {
			// https://dcrazed.com/css-html-login-form-templates/ - Login design
			return View();
		}
	}
}