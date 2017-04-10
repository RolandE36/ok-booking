using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.Model
{
	public class RoomDTO {
		public string Name { get; set; }
		public string Email { get; set; }
		public string MessageFreeTime { get; set; }
		public int StartAvailableTime { get; set; }
		public int EndAvailableTime { get; set; }
		public bool IsFavourite { get; set; }
	}
}
