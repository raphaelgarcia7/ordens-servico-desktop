using System;
using System.Windows.Forms;
using GestaoOS.WinForms.Forms;
using GestaoOS.WinForms.Infrastructure;

namespace GestaoOS.WinForms
{
    public class MainForm : Form
    {
        private readonly AppServices _services;

        public MainForm(AppServices services)
        {
            _services = services;
            Text = "Gestão de Ordens de Serviço";
            Width = 1080;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;
            BuildMenu();
        }

        private void BuildMenu()
        {
            var menu = new MenuStrip();
            var cadastro = new ToolStripMenuItem("Cadastros");
            cadastro.DropDownItems.Add("Clientes", null, delegate { Open(new ClientesForm(_services)); });
            cadastro.DropDownItems.Add("Serviços", null, delegate { Open(new ServicosForm(_services)); });

            var operacao = new ToolStripMenuItem("Operação");
            operacao.DropDownItems.Add("Ordens de Serviço", null, delegate { Open(new OrdensServicoForm(_services)); });

            var relatorios = new ToolStripMenuItem("Relatórios");
            relatorios.DropDownItems.Add("Gerencial de OS", null, delegate { Open(new RelatorioOrdensServicoForm(_services)); });

            menu.Items.Add(cadastro);
            menu.Items.Add(operacao);
            menu.Items.Add(relatorios);
            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        private static void Open(Form form)
        {
            form.ShowDialog();
        }
    }
}
