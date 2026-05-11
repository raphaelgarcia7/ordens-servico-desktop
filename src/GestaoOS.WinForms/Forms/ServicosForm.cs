using System;
using System.ComponentModel;
using System.Windows.Forms;
using GestaoOS.Application.Filters;
using GestaoOS.Domain.Entities;
using GestaoOS.WinForms.Infrastructure;

namespace GestaoOS.WinForms.Forms
{
    public class ServicosForm : Form
    {
        private readonly AppServices _services;
        private readonly BindingList<Servico> _servicos = new BindingList<Servico>();
        private readonly DataGridView _grid;
        private readonly TextBox _nome;
        private readonly CheckBox _ativo;

        public ServicosForm(AppServices services)
        {
            _services = services;
            Text = "Serviços";
            Width = 760;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;

            _nome = new TextBox { Left = 12, Top = 28, Width = 260 };
            _ativo = new CheckBox { Left = 284, Top = 30, Width = 80, Text = "Ativo", Checked = true };
            var pesquisar = new Button { Left = 376, Top = 26, Width = 90, Text = "Pesquisar" };
            var novo = new Button { Left = 478, Top = 26, Width = 90, Text = "Novo" };
            var editar = new Button { Left = 580, Top = 26, Width = 90, Text = "Editar" };
            _grid = new DataGridView { Left = 12, Top = 65, Width = 720, Height = 390, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, DataSource = _servicos };

            Controls.Add(new Label { Left = 12, Top = 10, Width = 120, Text = "Nome" });
            Controls.AddRange(new Control[] { _nome, _ativo, pesquisar, novo, editar, _grid });
            pesquisar.Click += delegate { LoadData(); };
            novo.Click += delegate { Edit(new Servico { Ativo = true }); };
            editar.Click += delegate { Edit(Current()); };
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var result = _services.Servicos.Search(new ServicoFilter { Nome = _nome.Text, Ativo = _ativo.Checked, Page = 1, PageSize = 100 });
                _servicos.Clear();
                foreach (var item in result.Items)
                {
                    _servicos.Add(item);
                }
            }
            catch (Exception ex)
            {
                UiErrorHandler.Handle(ex, _services.Logger);
            }
        }

        private Servico Current()
        {
            return _grid.CurrentRow == null ? null : _grid.CurrentRow.DataBoundItem as Servico;
        }

        private void Edit(Servico servico)
        {
            if (servico == null)
            {
                return;
            }

            using (var form = new ServicoEditForm(_services, servico))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }
    }
}
