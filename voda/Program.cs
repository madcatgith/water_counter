using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace voda
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0)
            {
                Application.Run(new Form1(args));
            }
            else
            {
                Application.Run(new Form1());
            }
        }
    }
}
