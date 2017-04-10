using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BAL;
using BAL.Model;
using DAL.Model;

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
				Exchange = Manager.LoginInExhange(email, password);
				CurrentUser = Manager.GetUser(email);
				return true;
			} catch (Exception e) {
				Exchange = null;
				CurrentUser = null;
				log.Error(e.Message);
				return false;
			}
		}

		public string GetError(string email) {
			try {
				CurrentUser = Manager.GetUser(email);
				return "Good";
			} catch (Exception e)
			{
				return e.Message + "\n\n\n\n" + e.InnerException;
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
			return PartialView("_Offices", Manager.GetOffices(CurrentUser.Email));
		}

		/// <summary>
		/// Return partial HTML for rooms view in the specified office
		/// </summary>
		/// <param name="email">Office email addres</param>
		public PartialViewResult GetRooms(string email)
		{
			Response.Cookies.Add(new HttpCookie("LastSelectedOffice", email));
			return PartialView("_Rooms", Manager.GetRooms(email, CurrentUser.Email));
		}

		public PartialViewResult GetBooking(string name, string email, int startAvailableTime, int endAvailableTime)
		{
			return PartialView("_Booking", Manager.GetBooking(name, email, startAvailableTime, endAvailableTime));
		}

		public string BookNow(string email, string subject, int start, int end)
		{
			return Manager.BookNow(CurrentUser, email, subject, start, end);
		}

		public bool ToggleFavouriteOffice(string email)
		{
			return Manager.ToggleFavouriteOffice(CurrentUser.Email, email);
		}

		public bool ToggleFavouriteRoom(string email)
		{
			return Manager.ToggleFavouriteRoom(CurrentUser.Email, email);
		}

		public PartialViewResult GetSettings()
		{
			var settings = new SettingsDTO() {
				TimeZones = TimeZoneInfo.GetSystemTimeZones()
			};
			return PartialView("_Settings", settings);
		}
	}
}