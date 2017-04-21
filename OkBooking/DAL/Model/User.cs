using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
	public class User {
		public User() {
			FavouriteRooms = new HashSet<Room>();
		}

		[Key]
		public int Id { get; set; }
		public string Email { get; set; }
		public Offices FavouriteOffice { get; set; }
		public virtual ICollection<Room> FavouriteRooms { get; set; }
	}
}
