using BAL.Authentication;
using BAL.Model;
using DAL;
using DAL.Model;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL {
	public class ExchangeManager {
		private ExchangeService service;

		public ExchangeManager(ExchangeService service) {
			this.service = service;
		}

		/// <summary>
		/// Try to authorize user. Throw Exception in case of invalid credentials
		/// </summary>
		public ExchangeService LoginInExhange(string email, string password) {
			service = Service.ConnectToService(new UserData(email, password), new TraceListener());
			return service;
		}

		/// <summary>
		/// Get user from DB or create user and return
		/// </summary>
		public User GetUser(string email)
		{
			using (var ctx = new Context()) {
				// Try to find user in DB
				var userFromDb = ctx.Users.FirstOrDefault(e => e.Email == email);

				// If user exists in DB than return user.
				if (userFromDb != null) return userFromDb;
				
				// Create user in DB if it not already exists
				var newUser = new User() {Email = email};
				ctx.Users.Add(newUser);
				ctx.SaveChanges();

				// Return created user;
				return newUser;
			}
		} 

		/// <summary>
		/// Return all offices
		/// </summary>
		public List<Office> GetOffices() {
			EmailAddressCollection roomLists = service.GetRoomLists(); // GetRoomLists - return offices list
			List<Office> offices = new List<Office>();
			
			foreach (EmailAddress address in roomLists) {
				//"Chernivtsi Office Meeting Rooms List"
				var name = address.Name.Replace("Meeting Rooms List", "").Trim();
				offices.Add(new Office() { Email = address.Address, Name = name });
			}

			return offices.OrderBy(e => e.Name).ToList();
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

			string userZoneId = "FLE Standard Time"; // +2 Kyiv
			TimeZoneInfo userZone = TimeZoneInfo.FindSystemTimeZoneById(userZoneId);

			// go through all meeting rooms
			for (int i = 0; i < roomsList.Count; i++)
			{
				var room = roomsList[i];
				var item = schedule.AttendeesAvailability[i];
				var message = "";
				var bookNow = true;

				// go through each meeting (event) in the room and check room availability
				if (item != null && item.CalendarEvents.Count > 0) {
					foreach (var meeting in item.CalendarEvents) {
						// set time zone
						DateTime meetingStartTime = TimeZoneInfo.ConvertTime(meeting.StartTime, userZone);
						DateTime meetingEndTime = TimeZoneInfo.ConvertTime(meeting.EndTime, userZone);

						// skip finished meeting
						if (meeting.StartTime < DateTime.Now && meeting.EndTime < DateTime.Now) continue;

						// meeting in progress
						if (meeting.StartTime <= DateTime.Now && meeting.EndTime >= DateTime.Now) {
							message += "... - " + meetingEndTime.ToString("HH:mm") + "; ";
							bookNow = false;
						}

						// future meeting
						if (meeting.StartTime > DateTime.Now && meeting.EndTime >= DateTime.Now) {

							// we can't book room if we have only 15 minutes till the next meeting
							var span = new TimeSpan(meeting.StartTime.Ticks - DateTime.Now.Ticks);
							if (bookNow && span.Minutes <= 15) { bookNow = false; }

							message = string.Format("{0} - {1}", meetingStartTime.ToString("HH:mm"), meetingEndTime.ToString("HH:mm"));
							break;
						}

						// skip next days
						if (meeting.StartTime.Day != DateTime.Now.Day) break;
					}
				}

				rooms.Add(new Room() { Name = room.Name, Email = room.Address, Time = message, BookNow = bookNow});
			}
			return rooms;
		}

		/// <summary>
		/// Get meeting schedule for each room for current day
		/// </summary>
		/// <param name="rooms"></param>
		/// <returns></returns>
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
										new TimeWindow(DateTime.Now, DateTime.Now.AddDays(1)), // TODO: maybe DateTime.Now we should change to current day start
											AvailabilityData.FreeBusyAndSuggestions,           // TODO: maybe Suggestions not required
										meetingOptions);
		}
	}
}
