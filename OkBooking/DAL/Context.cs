using DAL.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
#if (!DEBUG)
	[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
#endif
	public class Context : DbContext {
#if DEBUG
		public Context() : base("DebugConnection") {}
#else
		public Context() : base("ReleaseConnection") { }
#endif

		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<User>().Property(u => u.Email).HasMaxLength(255);
			modelBuilder.Entity<FavouriteOffice>().Property(u => u.Email).HasMaxLength(255);
		}

		public DbSet<User> Users { get; set; }
		public DbSet<FavouriteOffice> FavouriteOffices { get; set; }
	}
}
