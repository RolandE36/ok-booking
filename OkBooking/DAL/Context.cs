using DAL.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
	public class Context : DbContext
	{
		public Context() : base() {}

		public DbSet<User> Users { get; set; }
		public DbSet<FavouriteOffice> FavouriteOffices { get; set; }
	}
}
