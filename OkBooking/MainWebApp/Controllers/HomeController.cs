using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MainWebApp.Controllers {
	public class HomeController : BaseController {
		public ActionResult Index() {
			// https://dcrazed.com/css-html-login-form-templates/ - Login design


			// TODO: Exchange = Manager.Login("email", "password");
			
			return View();
		}
	}
}