namespace RestoranSiparisVeSadakatSistemi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MenuVeUrunEkleme : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Kategoris", "KategoriAdi", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Uruns", "UrunAdi", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Uruns", "UrunAdi", c => c.String());
            AlterColumn("dbo.Kategoris", "KategoriAdi", c => c.String());
        }
    }
}
