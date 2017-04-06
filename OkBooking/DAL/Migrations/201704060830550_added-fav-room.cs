namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedfavroom : DbMigration
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
                "dbo.RoomUsers",
                c => new
                    {
                        Room_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Room_Id, t.User_Id })
                .ForeignKey("dbo.Rooms", t => t.Room_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Room_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RoomUsers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.RoomUsers", "Room_Id", "dbo.Rooms");
            DropIndex("dbo.RoomUsers", new[] { "User_Id" });
            DropIndex("dbo.RoomUsers", new[] { "Room_Id" });
            DropTable("dbo.RoomUsers");
            DropTable("dbo.Rooms");
        }
    }
}
