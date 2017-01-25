using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BAL;
using DAL.Model;

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

		private ExchangeManager manager;

		protected ExchangeManager Manager {
			get {
				if (manager == null) { manager = new ExchangeManager(Exchange); }
				return manager;
			}
		}

		/// <summary>
		/// Current authorized user
		/// </summary>
		public User CurrentUser {
			get { return (User) Session["USER"]; }
			set { Session["USER"] = value; }
		}
	}
}