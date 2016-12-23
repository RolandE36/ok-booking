using BAL.Authentication;
using BAL.Model;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
			var schedule = GetRoomsSchedule(roomsList);

			for (int i = 0; i < roomsList.Count; i++)
			{
				var room = roomsList[i];
				var item = schedule.AttendeesAvailability[i];
				var message = "";
				var bookNow = true;

				if (item != null && item.CalendarEvents.Count > 0) {
					foreach (var cevent in item.CalendarEvents) {
						if (cevent.StartTime < DateTime.Now && cevent.EndTime < DateTime.Now) continue;
						if (cevent.StartTime <= DateTime.Now && cevent.EndTime >= DateTime.Now) {
							message += "... - " + cevent.EndTime.ToString("HH: mm") + "; ";
							bookNow = false;
						}

						if (cevent.StartTime > DateTime.Now && cevent.EndTime >= DateTime.Now) {
							var span = new TimeSpan(cevent.StartTime.Ticks - DateTime.Now.Ticks);
							if (bookNow && span.Minutes <= 15) {
								bookNow = false;
							}
							message = string.Format("{0} - {1}", cevent.StartTime.ToString("HH: mm"), cevent.EndTime.ToString("HH: mm"));
							break;
						}

						if (cevent.StartTime.Day != DateTime.Now.Day) break;
					}
				}

				rooms.Add(new Room() { Name = room.Name, Email = room.Address, Time = message, BookNow = bookNow});
			}
			return rooms;
		}

		private GetUserAvailabilityResults GetRoomsSchedule(Collection<EmailAddress> rooms) {
			List<AttendeeInfo> attendees = new List<AttendeeInfo>();
			AvailabilityOptions meetingOptions = new AvailabilityOptions();
			meetingOptions.MeetingDuration = 30;
			meetingOptions.MaximumNonWorkHoursSuggestionsPerDay = 0;
			meetingOptions.CurrentMeetingTime = DateTime.UtcNow;

			foreach (EmailAddress room in rooms) {
				attendees.Add(new AttendeeInfo() {
					SmtpAddress = room.Address,
					AttendeeType = MeetingAttendeeType.Room
				});
			}

			return service.GetUserAvailability(
										attendees,
										new TimeWindow(DateTime.Now, DateTime.Now.AddDays(1)),
											AvailabilityData.FreeBusyAndSuggestions,
										meetingOptions);
		}
	}
}
