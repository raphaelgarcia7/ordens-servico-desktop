using System;
using System.ComponentModel;
using System.Windows.Forms;
using GestaoOS.Application.Filters;
using GestaoOS.Domain.Entities;
using GestaoOS.Domain.Enums;
using GestaoOS.WinForms.Infrastructure;

namespace GestaoOS.WinForms.Forms
{
    public class OrdensServicoForm : Form
    {
        private readonly AppServices _services;
        private readonly BindingList<OrdemServico> _ordens = new BindingList<OrdemServico>();
        private readonly DataGridView _grid;
        private readonly ComboBox _status;

        public OrdensServicoForm(AppServices services)
        {
            _services = services;
            Text = "Ordens de Serviço";
            Width = 980;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;

            _status = new ComboBox { Left = 12, Top = 28, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            _status.Items.Add("(Todos)");
            foreach (var item in Enum.GetValues(typeof(StatusOrdemServico)))
            {
                _status.Items.Add(item);
            }
            _status.SelectedIndex = 0;

            var pesquisar = new Button { Left = 174, Top = 26, Width = 90, Text = "Pesquisar" };
            var novo = new Button { Left = 276, Top = 26, Width = 90, Text = "Nova" };
            var editar = new Button { Left = 378, Top = 26, Width = 90, Text = "Abrir" };
            _grid = new DataGridView { Left = 12, Top = 65, Width = 940, Height = 430, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, DataSource = _ordens };

            Controls.Add(new Label { Left = 12, Top = 10, Width = 120, Text = "Status" });
            Controls.AddRange(new Control[] { _status, pesquisar, novo, editar, _grid });
            pesquisar.Click += delegate { LoadData(); };
            novo.Click += delegate { Edit(new OrdemServico { DataAbertura = DateTime.Now, Status = StatusOrdemServico.Aberta }); };
            editar.Click += delegate { Edit(Current()); };
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var filter = new OrdemServicoFilter { Page = 1, PageSize = 100 };
                if (_status.SelectedItem is StatusOrdemServico)
                {
                    filter.Status = (StatusOrdemServico)_status.SelectedItem;
                }

                var result = _services.OrdensServico.Search(filter);
                _ordens.Clear();
                foreach (var item in result.Items)
                {
                    _ordens.Add(item);
                }
            }
            catch (Exception ex)
            {
                UiErrorHandler.Handle(ex, _services.Logger);
            }
        }

        private OrdemServico Current()
        {
            return _grid.CurrentRow == null ? null : _grid.CurrentRow.DataBoundItem as OrdemServico;
        }

        private void Edit(OrdemServico ordem)
        {
            if (ordem == null)
            {
                return;
            }

            using (var form = new OrdemServicoEditForm(_services, ordem))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }
    }
}
