using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.Model {
	public class SettingsDTO {
		public IReadOnlyCollection<TimeZoneInfo> TimeZones { get; set; }
		public string UserTimeZone { get; set; }
	}
}
