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
			return View();
		}

		/// <summary>
		/// Try to authorize user
		/// </summary>
		/// <returns>Return true if authorization completed successfully</returns>
		public bool Login(string email, string password) {
			try {
				Exchange = Manager.Login(email, password);
				return true;
			} catch (Exception e) {
				return false;
			}
		}

		/// <summary>
		/// Ansver is users already authorized
		/// </summary>
		public bool IsAuthorized() {
			return Exchange != null;
		}

		/// <summary>
		/// Return partial HTML for offices view
		/// </summary>
		public PartialViewResult GetOffices() {
			return PartialView("_Offices", Manager.GetOffices());
		}

		/// <summary>
		/// Return partial HTML for rooms view in the specified office
		/// </summary>
		/// <param name="email">Office email addres</param>
		public PartialViewResult GetRooms(string email)
		{
			return PartialView("_Rooms", Manager.GetRooms(email));
		}
	}
}