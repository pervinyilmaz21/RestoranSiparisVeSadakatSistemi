namespace RestoranSiparisVeSadakatSistemi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UrunAlanlariGuncelleme : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Uruns", "Aciklama", c => c.String());
            AddColumn("dbo.Uruns", "AktifMi", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Uruns", "AktifMi");
            DropColumn("dbo.Uruns", "Aciklama");
        }
    }
}
