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

		public PartialViewResult LogOut()
		{
			Exchange = null;
			CurrentUser = null;
			return PartialView("_Login");
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
		/// Return partial HTML for offices
		/// </summary>
		public PartialViewResult GetOffices() {
			return PartialView("_Offices", Manager.GetOffices(CurrentUser.Email));
		}

		/// <summary>
		/// Return partial HTML for offices is no favorite office selected
		/// In other case return romms for favorite office
		/// </summary>
		public PartialViewResult GetOfficesOrRooms() {
			if (Manager.GetUser(CurrentUser.Email).FavouriteOffice == null) {
				return GetOffices();
			} else {
				return GetRooms(Manager.GetUser(CurrentUser.Email).FavouriteOffice.Email);
			}
		}

		/// <summary>
		/// Return partial HTML for rooms view in the specified office
		/// </summary>
		/// <param name="email">Office email addres</param>
		public PartialViewResult GetRooms(string email)
		{
			Response.Cookies.Add(new HttpCookie("LastSelectedOffice", email));
			OfficeSchedule = Manager.GetRooms(email, CurrentUser.Email);
			return PartialView("_Rooms", OfficeSchedule);
		}

		public PartialViewResult GetBooking(string name, string email, int startAvailableTime, int endAvailableTime)
		{
			return PartialView("_Booking", Manager.GetBooking(name, email, startAvailableTime, endAvailableTime, OfficeSchedule));
		}

		public string BookNow(string email, string subject, int start, int end)
		{
			return Manager.BookNow(CurrentUser, email, subject, start, end);
		}

		public bool SetFavouriteOffice(string email)
		{
			return Manager.SetFavouriteOffice(CurrentUser.Email, email);
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