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
		public Context() : base("DefaultConnection") {}

		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder);

			modelBuilder.Properties<String>().Configure(c => c.HasColumnType("longtext"));
			modelBuilder.Properties<string>().Configure(c => c.HasColumnType("longtext"));
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Office> Offices { get; set; }
		public DbSet<Room> FavouriteRooms { get; set; }
	}
}
