using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BAL;

namespace MainWebApp.Controllers {
	public class HomeController : BaseController {
		public ActionResult Index() {
			// https://dcrazed.com/css-html-login-form-templates/ - Login design

			/*ViewBag.IsAuthorized = IsAuthorized();
			if (ViewBag.IsAuthorized) {
				ViewBag.Offices = Manager.GetOffices();
			}*/
			return View();
		}

		public bool Login(string email, string password) {
			try {
				Exchange = Manager.Login(email, password);
				return true;
			} catch (Exception e) {
				return false;
			}
		}

		public bool IsAuthorized() {
			return Exchange != null;
		}

		public PartialViewResult GetOffices() {
			return PartialView("_Offices", Manager.GetOffices());
		}

		public PartialViewResult GetRooms(string email)
		{
			return PartialView("_Rooms", Manager.GetRooms(email));
		}
	}
}