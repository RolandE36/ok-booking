namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbinit : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Rooms",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(unicode: false),
                        FavouriteOffice_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Offices", t => t.FavouriteOffice_Id)
                .Index(t => t.FavouriteOffice_Id);
            
            CreateTable(
                "dbo.Offices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserRooms",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        Room_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.Room_Id })
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.Rooms", t => t.Room_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.Room_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserRooms", "Room_Id", "dbo.Rooms");
            DropForeignKey("dbo.UserRooms", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Users", "FavouriteOffice_Id", "dbo.Offices");
            DropIndex("dbo.UserRooms", new[] { "Room_Id" });
            DropIndex("dbo.UserRooms", new[] { "User_Id" });
            DropIndex("dbo.Users", new[] { "FavouriteOffice_Id" });
            DropTable("dbo.UserRooms");
            DropTable("dbo.Offices");
            DropTable("dbo.Users");
            DropTable("dbo.Rooms");
        }
    }
}
