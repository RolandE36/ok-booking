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

		public List<Office> GetOffices() {
			EmailAddressCollection roomLists = service.GetRoomLists();
			List<Office> offices = new List<Office>();
			
			foreach (EmailAddress address in roomLists) {
				//"Chernivtsi Office Meeting Rooms List"
				var name = address.Name.Replace("Meeting Rooms List", "").Trim();
				offices.Add(new Office() { Email = address.Address, Name = name });
			}

			return offices;
		}

		public List<Room> GetRooms(string email) {
			var emailAddress = new EmailAddress(email);
			var roomsList = service.GetRooms(emailAddress);
			List<Room> rooms = new List<Room>();

			foreach (EmailAddress r in roomsList)
			{
				rooms.Add(new Room() { Name = r.Name, Email = r.Address });
			}
			return rooms;
		}
	}
}
