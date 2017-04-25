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
		private const int TOTAL_MINUTES = 24 * 60;

		private ExchangeService service;
		private readonly Context dbContext = new Context();

		private readonly int UserTimeZoneOffset;

		public ExchangeManager(ExchangeService service, int timeOffset) {
			this.service = service;
			this.UserTimeZoneOffset = timeOffset;
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
				bool isFavourite = true;
				if (user.FavouriteOffice == null || user.FavouriteOffice.Email != ssAddress.Address) isFavourite = false;

				offices.Add(new Model.Office() {
					Email = ssAddress.Address,
					Name = name,
					IsFavourite = isFavourite
				});
			}

			return offices.OrderBy(e => !e.IsFavourite).ThenBy(e => e.Name).ToList();
		}

		/// <summary>
		/// Get roms list for specified office
		/// </summary>
		/// <param name="roomEmail">Office email</param>
		/// <param name="userEmail">Current user email</param>
		/// <returns></returns>
		public List<RoomDTO> GetRooms(string roomEmail, string userEmail) {

			var emailAddress = new EmailAddress(roomEmail);
			var roomsList = service.GetRooms(emailAddress);

			List<RoomDTO> rooms = new List<RoomDTO>();
			// get schedule for each room in the office
			var schedule = GetRoomsSchedule(roomsList);
			var user = GetUser(userEmail); // current app user

			DateTime serverTimeNow = DateTime.Now;
			TimeSpan timeSpan;

			// go through all meeting rooms
			for (int i = 0; i < roomsList.Count; i++) {
				var room = roomsList[i];
				var item = schedule.AttendeesAvailability[i];
				var message = "";
				bool[] reservedTime = new bool[TOTAL_MINUTES];

				// go through each meeting (event) in the room and check room availability
				if (item != null && item.CalendarEvents.Count > 0) {
					foreach (var meeting in item.CalendarEvents) {
						// set time zone
						DateTime meetingStartTime = meeting.StartTime;
						DateTime meetingEndTime = meeting.EndTime;

						// get minute ranges
						int startMinute = meetingStartTime.Hour * 60 + meetingStartTime.Minute;
						int endMinute = meetingEndTime.Hour * 60 + meetingEndTime.Minute;
						if (endMinute == 0) endMinute = TOTAL_MINUTES - 1;

						// mark each minute as reserved
						for (int minute = startMinute; minute <= endMinute; minute++) {
							reservedTime[minute] = true;
						}

						// skip next days
						if (meeting.StartTime.Day != serverTimeNow.Day) break;
					}
				}

				int roomEmptyStartTime = serverTimeNow.Hour * 60 + serverTimeNow.Minute;
				if (reservedTime[roomEmptyStartTime] == true) {
					// if room is reserver now than lets find first empty time
					
					// while not array end and room is reserved
					while (roomEmptyStartTime < TOTAL_MINUTES && reservedTime[roomEmptyStartTime] == true) roomEmptyStartTime++;

					// Get message
					timeSpan = new TimeSpan(roomEmptyStartTime / 60, roomEmptyStartTime % 60, 0);
					timeSpan = ToClientTime(timeSpan);
					message = timeSpan.ToString(@"hh\:mm");
				} else {
					// if room is empty now
					message = "now";
				}

				message += " - ";
				// finding end of free time
				int roomEmptyEndTime = roomEmptyStartTime + 1;
				while (roomEmptyEndTime < TOTAL_MINUTES && reservedTime[roomEmptyEndTime] == false) roomEmptyEndTime++;
				roomEmptyEndTime--; // 00:00 -> 23:59
				timeSpan = new TimeSpan(roomEmptyEndTime / 60, roomEmptyEndTime % 60, 0);
				timeSpan = ToClientTime(timeSpan);
				message += timeSpan.ToString(@"hh\:mm");
				// if all rooms are booked
				if (roomEmptyStartTime == TOTAL_MINUTES) message = "Sorry, this meeting room have been reserved for today.";

				// We can't show 00:00 on UI. 
				if (roomEmptyEndTime == TOTAL_MINUTES) roomEmptyEndTime--;

				rooms.Add(new RoomDTO() {
					Name = room.Name,
					Email = room.Address,
					MessageFreeTime = message,
					StartAvailableTime = (int) ToClientTime(roomEmptyStartTime).TotalMinutes,
					EndAvailableTime = (int)ToClientTime(roomEmptyEndTime).TotalMinutes,
					IsFavourite = user.FavouriteRooms.FirstOrDefault(e => e.Email == room.Address) != null
				});
			}

			return rooms.OrderBy(e => !e.IsFavourite).ThenBy(e => e.StartAvailableTime).ThenBy(e => e.Name).ToList();
		}

		public BookingDTO GetBooking(string name, string email, int startAvailableTime, int endAvailableTime)
		{
			var booking = new BookingDTO();
			booking.Name = name;
			booking.Email = email;
			booking.StartTime = TimeSpan.FromMinutes(startAvailableTime);
			booking.EndTime = TimeSpan.FromMinutes(endAvailableTime);

			return booking;
		}

		public string BookNow(User user, string roomEmail, string subject, int start, int end) {
			try {
				Appointment meeting = new Appointment(service);

				// Set the properties on the meeting object to create the meeting.
				meeting.Subject = subject;
				meeting.Body = "";

				meeting.Start = ToServerTime(start);
				meeting.End = ToServerTime(end);

				meeting.Location = subject;
				meeting.RequiredAttendees.Add(roomEmail);
				meeting.RequiredAttendees.Add(user.Email);
				//meeting.OptionalAttendees.Add("Magdalena@contoso.com");
				meeting.ReminderMinutesBeforeStart = 15;

				// Save the meeting to the Calendar folder and send the meeting request.
				meeting.Save(SendInvitationsMode.SendToAllAndSaveCopy);

				// TODO: Check it later:
				// Verify that the meeting was created.
				// Item item = Item.Bind(service, meeting.Id, new PropertySet(ItemSchema.Subject));
				// Console.WriteLine("\nMeeting created: " + item.Subject + "\n");

				return meeting.Subject + "was reserved successfully.";
			} catch {
				return "Oops, something went wrong. Please try again.";
			}
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
			meetingOptions.CurrentMeetingTime = DateTime.Now;

			foreach (EmailAddress room in rooms) {
				attendees.Add(new AttendeeInfo() {
					SmtpAddress = room.Address,
					AttendeeType = MeetingAttendeeType.Room
				});
			}

			return service.GetUserAvailability(
										attendees,
										new TimeWindow(DateTime.Now, DateTime.Now.AddDays(1)),
											AvailabilityData.FreeBusyAndSuggestions, // TODO: maybe Suggestions not required
										meetingOptions);
		}

		private DAL.Model.Office GetOffice(string officeEmail)
		{
			// get office from db
			var office = dbContext.Offices.FirstOrDefault(e => e.Email == officeEmail);

			// if office not exists
			if (office == null)
			{
				// than create new office
				office = new DAL.Model.Office() { Email = officeEmail };
				dbContext.Offices.Add(office);
				dbContext.SaveChanges();
			}

			// return created or existing office
			return office;
		}
		
		public bool SetFavouriteOffice(string userEmail, string officeEmail) {
			try
			{
				var office = GetOffice(officeEmail);
				var user = GetUser(userEmail);
				if (user.FavouriteOffice == office) {
					user.FavouriteOffice = null;
				} else {
					user.FavouriteOffice = office;
				}
				dbContext.SaveChanges();
				return true;
			} catch (Exception ex) {
				return false;
			}
		}

		private DAL.Model.Room GetRoom(string roomEmail)
		{
			// get office from db
			var room = dbContext.FavouriteRooms.FirstOrDefault(e => e.Email == roomEmail);

			// if office not exists
			if (room == null)
			{
				// than create new office
				room = new DAL.Model.Room() { Email = roomEmail };
				dbContext.FavouriteRooms.Add(room);
				dbContext.SaveChanges();
			}

			// return created or existing office
			return room;
		}

		public bool ToggleFavouriteRoom(string userEmail, string roomEmail)
		{
			try
			{
				var room = GetRoom(roomEmail);
				var user = GetUser(userEmail);
				if (!user.FavouriteRooms.Contains(room))
				{
					user.FavouriteRooms.Add(room);
				}
				else {
					user.FavouriteRooms.Remove(room);
				}
				dbContext.SaveChanges();
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}


		#region Helpers

		private DateTime ToServerTime(int minutes) {
			return DateTime.Now.Date.AddMinutes(minutes + DateTimeOffset.Now.Offset.TotalMinutes + UserTimeZoneOffset);
		}

		private TimeSpan ToClientTime(TimeSpan date) {
			var userOffset = new TimeSpan(0, - UserTimeZoneOffset, 0);
			return date - DateTimeOffset.Now.Offset + userOffset;
		}

		private TimeSpan ToClientTime(int minutes) {
			return ToClientTime(new TimeSpan(minutes / 60, minutes % 60, 0));
		}

		#endregion
	}
}
