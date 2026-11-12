using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RestoranSiparisVeSadakatSistemi
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Veritabanı yoksa oluşturur, varsa son migration'a günceller (exe başka bilgisayarda açıldığında gerekli)
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<Data.AppDbContext, Migrations.Configuration>());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
