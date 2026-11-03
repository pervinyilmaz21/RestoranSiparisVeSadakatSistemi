namespace RestoranSiparisVeSadakatSistemi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class KullaniciGuncelleme : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Musteris", "AdSoyad", c => c.String(nullable: false));
            AddColumn("dbo.Musteris", "Eposta", c => c.String());
            AddColumn("dbo.Musteris", "SifreHash", c => c.String());
            AddColumn("dbo.Musteris", "Rol", c => c.String());
            DropColumn("dbo.Musteris", "Ad");
            DropColumn("dbo.Musteris", "Soyad");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Musteris", "Soyad", c => c.String());
            AddColumn("dbo.Musteris", "Ad", c => c.String());
            DropColumn("dbo.Musteris", "Rol");
            DropColumn("dbo.Musteris", "SifreHash");
            DropColumn("dbo.Musteris", "Eposta");
            DropColumn("dbo.Musteris", "AdSoyad");
        }
    }
}
