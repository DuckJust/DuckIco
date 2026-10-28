using System;
using System.Windows.Forms;

namespace DuckIco
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // فایل‌هایی که روی آیکون exe رها شوند در args می‌آیند
            Application.Run(new MainForm(args));
        }
    }
}
