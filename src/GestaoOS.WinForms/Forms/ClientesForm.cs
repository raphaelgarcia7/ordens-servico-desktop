using System;
using System.ComponentModel;
using System.Windows.Forms;
using GestaoOS.Application.Filters;
using GestaoOS.Domain.Entities;
using GestaoOS.Domain.Enums;
using GestaoOS.WinForms.Infrastructure;

namespace GestaoOS.WinForms.Forms
{
    public class ClientesForm : Form
    {
        private readonly AppServices _services;
        private readonly DataGridView _grid;
        private readonly TextBox _nome;
        private readonly TextBox _documento;
        private readonly CheckBox _ativo;
        private readonly BindingList<Cliente> _clientes;

        public ClientesForm(AppServices services)
        {
            _services = services;
            _clientes = new BindingList<Cliente>();
            Text = "Clientes";
            Width = 900;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;

            _nome = new TextBox { Left = 12, Top = 28, Width = 220 };
            _documento = new TextBox { Left = 244, Top = 28, Width = 160 };
            _ativo = new CheckBox { Left = 416, Top = 30, Width = 80, Text = "Ativo", Checked = true };
            var pesquisar = new Button { Left = 508, Top = 26, Width = 90, Text = "Pesquisar" };
            var novo = new Button { Left = 610, Top = 26, Width = 90, Text = "Novo" };
            var editar = new Button { Left = 712, Top = 26, Width = 90, Text = "Editar" };
            var excluir = new Button { Left = 814, Top = 26, Width = 60, Text = "Excluir" };

            _grid = new DataGridView { Left = 12, Top = 65, Width = 862, Height = 440 };
            GridFactory.ConfigureReadOnly(_grid);
            _grid.Columns.Add(GridFactory.TextColumn("Id", "Código", 70));
            _grid.Columns.Add(GridFactory.TextColumn("Nome", "Nome", 210));
            _grid.Columns.Add(GridFactory.TextColumn("Documento", "Documento", 130));
            _grid.Columns.Add(GridFactory.TextColumn("Tipo", "Tipo", 90));
            _grid.Columns.Add(GridFactory.TextColumn("Email", "E-mail", 170));
            _grid.Columns.Add(GridFactory.TextColumn("Telefone", "Telefone", 110));
            _grid.Columns.Add(GridFactory.TextColumn("DataCadastro", "Cadastro", 90, "dd/MM/yyyy"));
            _grid.Columns.Add(GridFactory.CheckColumn("Ativo", "Ativo", 60));
            _grid.DataSource = _clientes;

            Controls.Add(new Label { Left = 12, Top = 10, Width = 120, Text = "Nome" });
            Controls.Add(new Label { Left = 244, Top = 10, Width = 120, Text = "Documento" });
            Controls.AddRange(new Control[] { _nome, _documento, _ativo, pesquisar, novo, editar, excluir, _grid });

            pesquisar.Click += delegate { LoadData(); };
            novo.Click += delegate { Edit(new Cliente { Ativo = true, Tipo = TipoCliente.Fisica }); };
            editar.Click += delegate { Edit(Current()); };
            excluir.Click += delegate { DeleteCurrent(); };

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var result = _services.Clientes.Search(new ClienteFilter { Nome = _nome.Text, Documento = _documento.Text, Ativo = _ativo.Checked, Page = 1, PageSize = 100 });
                _clientes.Clear();
                foreach (var item in result.Items)
                {
                    _clientes.Add(item);
                }
            }
            catch (Exception ex)
            {
                UiErrorHandler.Handle(ex, _services.Logger);
            }
        }

        private Cliente Current()
        {
            if (_grid.CurrentRow == null)
            {
                return null;
            }

            return _grid.CurrentRow.DataBoundItem as Cliente;
        }

        private void Edit(Cliente cliente)
        {
            if (cliente == null)
            {
                return;
            }

            using (var form = new ClienteEditForm(_services, cliente))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void DeleteCurrent()
        {
            var cliente = Current();
            if (cliente == null)
            {
                return;
            }

            if (MessageBox.Show("Confirma a exclusão do cliente?", "Clientes", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _services.Clientes.Delete(cliente.Id);
                LoadData();
            }
            catch (Exception ex)
            {
                UiErrorHandler.Handle(ex, _services.Logger);
            }
        }
    }
}
