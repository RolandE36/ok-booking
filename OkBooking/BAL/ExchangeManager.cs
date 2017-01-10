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

		/// <summary>
		/// Get roms list for specified office
		/// </summary>
		/// <param name="email">Office email</param>
		/// <returns></returns>
		public List<Room> GetRooms(string email) {
			var emailAddress = new EmailAddress(email);
			var roomsList = service.GetRooms(emailAddress);
			List<Room> rooms = new List<Room>();
			// get schedule for each room in the office
			var schedule = GetRoomsSchedule(roomsList);

			// convert server time to user time zone
			DateTime userTimeNow = TimeZoneInfo.ConvertTime(DateTime.Now, service.TimeZone);

			for (int i = 0; i < roomsList.Count; i++)
			{
				var room = roomsList[i];
				var item = schedule.AttendeesAvailability[i];
				var message = "";
				var bookNow = true;

				// go through each meeting in the room and check room availability
				if (item != null && item.CalendarEvents.Count > 0) {
					foreach (var meeting in item.CalendarEvents) {

						// set time zone
						DateTime meetingStartTime = TimeZoneInfo.ConvertTime(meeting.StartTime, service.TimeZone);
						DateTime meetingEndTime = TimeZoneInfo.ConvertTime(meeting.EndTime, service.TimeZone);

						// skip finished meeting
						if (meeting.StartTime < userTimeNow && meeting.EndTime < userTimeNow) continue;

						message = "" + meeting.StartTime.Hour + ":" + meeting.StartTime.Minute + " H" +
						          service.TimeZone.BaseUtcOffset.Hours + " ST:" + service.TimeZone + " H" + TimeZoneInfo.Local.BaseUtcOffset.Hours + " CT:" +
						          TimeZoneInfo.Local;
						break;

						// meeting in progress
						if (meeting.StartTime <= userTimeNow && meeting.EndTime >= userTimeNow) {
							message += "... - " + meetingEndTime.ToString("HH:mm") + "; ";
							bookNow = false;
						}

						// future meeting
						if (meeting.StartTime > userTimeNow && meeting.EndTime >= userTimeNow) {

							// we can't book room if we have only 15 minutes till the next meeting
							var span = new TimeSpan(meeting.StartTime.Ticks - userTimeNow.Ticks);
							if (bookNow && span.Minutes <= 15) { bookNow = false; }

							message = string.Format("{0} - {1}", meetingStartTime.ToString("HH:mm"), meetingEndTime.ToString("HH:mm"));
							break;
						}

						// skip next days
						if (meeting.StartTime.Day != userTimeNow.Day) break;
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
