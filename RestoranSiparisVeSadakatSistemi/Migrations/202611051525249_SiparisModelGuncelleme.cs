namespace RestoranSiparisVeSadakatSistemi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SiparisModelGuncelleme : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Siparis", "MusteriID", "dbo.Musteris");
            DropIndex("dbo.Siparis", new[] { "MusteriID" });
            AddColumn("dbo.Siparis", "MasaID", c => c.Int());
            AddColumn("dbo.Siparis", "Durum", c => c.String());
            AddColumn("dbo.Siparis", "KazanilanPuan", c => c.Int(nullable: false));
            AlterColumn("dbo.Siparis", "MusteriID", c => c.Int());
            CreateIndex("dbo.Siparis", "MusteriID");
            AddForeignKey("dbo.Siparis", "MusteriID", "dbo.Musteris", "MusteriID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Siparis", "MusteriID", "dbo.Musteris");
            DropIndex("dbo.Siparis", new[] { "MusteriID" });
            AlterColumn("dbo.Siparis", "MusteriID", c => c.Int(nullable: false));
            DropColumn("dbo.Siparis", "KazanilanPuan");
            DropColumn("dbo.Siparis", "Durum");
            DropColumn("dbo.Siparis", "MasaID");
            CreateIndex("dbo.Siparis", "MusteriID");
            AddForeignKey("dbo.Siparis", "MusteriID", "dbo.Musteris", "MusteriID", cascadeDelete: true);
        }
    }
}
