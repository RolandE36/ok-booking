namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Offices",
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
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserOffices",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        Offices_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.Offices_Id })
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.Offices", t => t.Offices_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.Offices_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserOffices", "Offices_Id", "dbo.Offices");
            DropForeignKey("dbo.UserOffices", "User_Id", "dbo.Users");
            DropIndex("dbo.UserOffices", new[] { "Offices_Id" });
            DropIndex("dbo.UserOffices", new[] { "User_Id" });
            DropTable("dbo.UserOffices");
            DropTable("dbo.Users");
            DropTable("dbo.Offices");
        }
    }
}
