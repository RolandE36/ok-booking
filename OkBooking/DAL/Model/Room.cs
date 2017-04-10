using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
	public class Room
	{
		public Room()
		{
			FavouriteUsers = new HashSet<User>();
		}

		[Key]
		public int Id { get; set; }
		public string Email { get; set; }
		public virtual ICollection<User> FavouriteUsers { get; set; }
	}
}
