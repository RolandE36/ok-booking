using BAL.Authentication;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL {
	public class ExchangeManager {
		private ExchangeService service;

		public ExchangeManager(ExchangeService service) {
			this.service = service;
		}

		public ExchangeService Login(string email, string password) {
			service = Service.ConnectToService(new UserData(email, password), new TraceListener());
			return service;
		}
	}
}
