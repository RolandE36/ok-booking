namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedfavouriteoffices : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FavouriteOffices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FavouriteOffices", "User_Id", "dbo.Users");
            DropIndex("dbo.FavouriteOffices", new[] { "User_Id" });
            DropTable("dbo.FavouriteOffices");
        }
    }
}
