using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
	public class FavouriteOffice
	{
		[Key]
		public int Id { get; set; }
		public string Email { get; set; }
		public User User { get; set; }
	}
}
