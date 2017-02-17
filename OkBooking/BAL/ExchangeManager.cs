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
		private Context dbContext = new Context();

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
			// Try to find user in DB
			var userFromDb = dbContext.Users.FirstOrDefault(e => e.Email == email);

			// If user exists in DB than return user.
			if (userFromDb != null) return userFromDb;
				
			// Create user in DB if it not already exists
			var newUser = new User() {Email = email};
			dbContext.Users.Add(newUser);
			dbContext.SaveChanges();

			// Return created user;
			return newUser;
		} 

		/// <summary>
		/// Return all offices
		/// </summary>
		public List<Model.Office> GetOffices(string userEmail) {
			EmailAddressCollection roomLists = service.GetRoomLists(); // GetRoomLists - return offices list
			List<Model.Office> offices = new List<Model.Office>();

			var user = GetUser(userEmail);

			foreach (EmailAddress ssAddress in roomLists) {
				//"Chernivtsi Office Meeting Rooms List"
				string name = ssAddress.Name.Replace("Meeting Rooms List", "").Trim();
				bool isFavourite = user.FavouriteOffices.FirstOrDefault(e => e.Email == ssAddress.Address) != null;

				offices.Add(new Model.Office() {
					Email = ssAddress.Address,
					Name = name,
					IsFavourite = isFavourite,
					CssClass = isFavourite ? "fav-office-star-active" : ""
				});
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
			for (int i = 0; i < roomsList.Count; i++) {
				var room = roomsList[i];
				var item = schedule.AttendeesAvailability[i];
				var message = "";
				var bookNow = true;
				bool[] reservedTime = new bool[24*60];

				// go through each meeting (event) in the room and check room availability
				if (item != null && item.CalendarEvents.Count > 0) {
					foreach (var meeting in item.CalendarEvents) {
						// set time zone
						DateTime meetingStartTime = TimeZoneInfo.ConvertTime(meeting.StartTime, userZone);
						DateTime meetingEndTime = TimeZoneInfo.ConvertTime(meeting.EndTime, userZone);

						// get minute ranges
						int startMinute = meetingStartTime.Hour * 60 + meetingStartTime.Minute;
						int endMinute = meetingEndTime.Hour * 60 + meetingEndTime.Minute;

						// mark each minute as reserved
						for (int minute = startMinute; minute <= endMinute; minute++) {
							reservedTime[minute] = true;
						}

						// skip next days
						if (meeting.StartTime.Day != DateTime.Now.Day) break;
					}
				}

				int timeMinuteNow = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
				if (reservedTime[timeMinuteNow] == true)
				{
					message = "Reserved";
				} else message = "Not reserved";

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

		private DAL.Model.Offices GetOffice(string officeEmail)
		{
			// get office from db
			var office = dbContext.FavouriteOffices.FirstOrDefault(e => e.Email == officeEmail);

			// if office not exists
			if (office == null)
			{
				// than create new office
				office = new DAL.Model.Offices() { Email = officeEmail };
				dbContext.FavouriteOffices.Add(office);
				dbContext.SaveChanges();
			}

			// return created or existing office
			return office;
		}

		public bool ToggleFavouriteOffice(string userEmail, string officeEmail) {
			try
			{
				var office = GetOffice(officeEmail);
				var user = GetUser(userEmail);
				if (!user.FavouriteOffices.Contains(office)) {
					user.FavouriteOffices.Add(office);
				} else {
					user.FavouriteOffices.Remove(office);
				}
				dbContext.SaveChanges();
				return true;
			} catch (Exception ex) {
				return false;
			}
		}
	}
}
