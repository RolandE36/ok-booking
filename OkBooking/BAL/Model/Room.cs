﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.Model
{
	public class Room {
		public string Name { get; set; }
		public string Email { get; set; }
		public string MessageFreeTime { get; set; }
		public bool BookNow { get; set; }
		public int FirstAvailableTime { get; set; }
	}
}
