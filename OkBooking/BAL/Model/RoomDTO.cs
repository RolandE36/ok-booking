using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.Model {
	public class RoomDTO {
		public string Name { get; set; }
		public string Email { get; set; }
		public bool IsFavourite { get; set; }

		public string MessageFreeTime {
			get {
				var message = "";
				if (AvalaibleTime.Count > 0) {
					foreach (var time in AvalaibleTime) {
						message += time.Message + "; ";
					}
				} else {
					message = "Sorry, this meeting room have been reserved for today.";
				}

				return message.Trim().Trim(';');
			}
		}

		public int StartAvailableTime { get { return AvalaibleTime.Count > 0 ? AvalaibleTime[0].StartTime : 0; } }
		public int EndAvailableTime { get { return AvalaibleTime.Count > 0 ? AvalaibleTime[0].EndTime : 0; } }
		public List<RoomAvalaibleTimeDTO> AvalaibleTime = new List<RoomAvalaibleTimeDTO>();
	}
}
