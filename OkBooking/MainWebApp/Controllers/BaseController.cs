﻿using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BAL;
using DAL.Model;
using BAL.Model;

namespace MainWebApp.Controllers {
	public class BaseController : Controller
	{

		private const string EXCHANGE_SERVICE = "EXCHANGE_SERVICE";
		protected ExchangeService Exchange {
			get {
				var obj = Session[EXCHANGE_SERVICE];
				if (obj == null) return null;
				return (ExchangeService) obj;
			}
			set { Session[EXCHANGE_SERVICE] = value; }
		}

		protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private ExchangeManager manager;

		protected ExchangeManager Manager {
			get {
				if (manager == null) {
					var httpCookie = HttpContext.Request.Cookies["TIMEZONEOFFSET"];
					int timeOffset = httpCookie != null ? int.Parse(httpCookie.Value) : 0;
					manager = new ExchangeManager(Exchange, timeOffset);
				}
				return manager;
			}
		}

		/// <summary>
		/// Current authorized user
		/// </summary>
		protected User CurrentUser {
			get { return (User) Session["USER"]; }
			set { Session["USER"] = value; }
		}

		/// <summary>
		/// Schedule for the last selected office
		/// </summary>
		protected List<RoomDTO> OfficeSchedule {
			get { return (List<RoomDTO>) Session["LAST_SELECTED_OFFICE_SCHEDULE"]; }
			set { Session["LAST_SELECTED_OFFICE_SCHEDULE"] = value; }
		}
	}
}