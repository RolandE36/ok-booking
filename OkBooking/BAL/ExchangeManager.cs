using BAL.Authentication;
using BAL.Model;
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

		public List<Room> GetRoomLists() {
			EmailAddressCollection roomLists = service.GetRoomLists();
			List<Room> rooms = new List<Room>();

			foreach (EmailAddress address in roomLists) {
				//"Chernivtsi Office Meeting Rooms List"
				var name = address.Name.Replace("Meeting Rooms List", "").Trim();
				rooms.Add(new Room() { Email = address.Address, Name = name });
			}

			return rooms;
		}
	}
}
