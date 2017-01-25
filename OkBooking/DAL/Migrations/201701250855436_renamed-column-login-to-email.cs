namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renamedcolumnlogintoemail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Email", c => c.String());
            DropColumn("dbo.Users", "Login");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "Login", c => c.String());
            DropColumn("dbo.Users", "Email");
        }
    }
}
