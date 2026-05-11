using System;
using System.Windows.Forms;
using GestaoOS.WinForms.Infrastructure;

namespace GestaoOS.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new MainForm(CompositionRoot.Build()));
        }
    }
}
