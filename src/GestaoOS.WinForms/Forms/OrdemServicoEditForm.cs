using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using GestaoOS.Application.Filters;
using GestaoOS.Domain.Entities;
using GestaoOS.Domain.Enums;
using GestaoOS.WinForms.Infrastructure;

namespace GestaoOS.WinForms.Forms
{
    public class OrdemServicoEditForm : Form
    {
        private readonly AppServices _services;
        private readonly OrdemServico _ordem;
        private readonly ComboBox _cliente = new ComboBox();
        private readonly ComboBox _status = new ComboBox();
        private readonly TextBox _observacao = new TextBox();
        private readonly Label _total = new Label();
        private readonly BindingList<OrdemServicoItem> _itens = new BindingList<OrdemServicoItem>();
        private readonly DataGridView _gridItens = new DataGridView();
        private readonly ComboBox _servico = new ComboBox();
        private readonly NumericUpDown _quantidade = new NumericUpDown();
        private Button _adicionar;
        private Button _remover;

        public OrdemServicoEditForm(AppServices services, OrdemServico ordem)
        {
            _services = services;
            _ordem = ordem.Id == 0 ? ordem : services.OrdensServico.GetById(ordem.Id);
            Text = "Ordem de Serviço";
            Width = 980;
            Height = 660;
            StartPosition = FormStartPosition.CenterParent;
            Build();
            Bind();
        }

        private void Build()
        {
            Controls.Add(new Label { Left = 12, Top = 14, Width = 120, Text = "Cliente" });
            _cliente.Left = 12; _cliente.Top = 34; _cliente.Width = 360; _cliente.DropDownStyle = ComboBoxStyle.DropDownList; Controls.Add(_cliente);
            Controls.Add(new Label { Left = 390, Top = 14, Width = 120, Text = "Status" });
            _status.Left = 390; _status.Top = 34; _status.Width = 160; _status.DropDownStyle = ComboBoxStyle.DropDownList; Controls.Add(_status);
            Controls.Add(new Label { Left = 570, Top = 14, Width = 120, Text = "Total" });
            _total.Left = 570; _total.Top = 38; _total.Width = 160; Controls.Add(_total);

            Controls.Add(new Label { Left = 12, Top = 70, Width = 120, Text = "Observação" });
            _observacao.Left = 12; _observacao.Top = 90; _observacao.Width = 920; _observacao.Height = 58; _observacao.Multiline = true; Controls.Add(_observacao);

            Controls.Add(new Label { Left = 12, Top = 166, Width = 120, Text = "Serviço" });
            _servico.Left = 12; _servico.Top = 186; _servico.Width = 360; _servico.DropDownStyle = ComboBoxStyle.DropDownList; Controls.Add(_servico);
            Controls.Add(new Label { Left = 390, Top = 166, Width = 120, Text = "Quantidade" });
            _quantidade.Left = 390; _quantidade.Top = 186; _quantidade.Width = 120; _quantidade.Minimum = 1; _quantidade.Maximum = 9999; Controls.Add(_quantidade);
            _adicionar = new Button { Left = 530, Top = 184, Width = 90, Text = "Adicionar" };
            _remover = new Button { Left = 632, Top = 184, Width = 90, Text = "Remover" };
            _adicionar.Click += delegate { AddItem(); };
            _remover.Click += delegate { RemoveItem(); };
            Controls.Add(_adicionar); Controls.Add(_remover);

            _gridItens.Left = 12; _gridItens.Top = 226; _gridItens.Width = 920; _gridItens.Height = 330;
            GridFactory.ConfigureReadOnly(_gridItens);
            _gridItens.Columns.Add(GridFactory.TextColumn("ServicoNome", "Serviço", 330));
            _gridItens.Columns.Add(GridFactory.TextColumn("Quantidade", "Qtd.", 70, null, DataGridViewContentAlignment.MiddleCenter));
            _gridItens.Columns.Add(GridFactory.TextColumn("ValorUnitario", "Valor unitário", 130, "C2", DataGridViewContentAlignment.MiddleRight));
            _gridItens.Columns.Add(GridFactory.TextColumn("PercentualImpostoAplicado", "Imposto (%)", 110, "N2", DataGridViewContentAlignment.MiddleRight));
            _gridItens.Columns.Add(GridFactory.TextColumn("ValorTotalItem", "Total do item", 130, "C2", DataGridViewContentAlignment.MiddleRight));
            _gridItens.DataSource = _itens;
            Controls.Add(_gridItens);

            var salvar = new Button { Left = 760, Top = 574, Width = 80, Text = "Salvar" };
            var cancelar = new Button { Left = 852, Top = 574, Width = 80, Text = "Cancelar", DialogResult = DialogResult.Cancel };
            salvar.Click += delegate { Save(); };
            Controls.Add(salvar); Controls.Add(cancelar);
        }

        private void Bind()
        {
            var clientes = _services.Clientes.Search(new ClienteFilter { Ativo = true, Page = 1, PageSize = 1000 }).Items;
            _cliente.DataSource = clientes;
            _cliente.DisplayMember = "Nome";
            _cliente.ValueMember = "Id";
            _status.DataSource = Enum.GetValues(typeof(StatusOrdemServico));
            var servicos = _services.Servicos.Search(new ServicoFilter { Ativo = true, Page = 1, PageSize = 1000 }).Items;
            _servico.DataSource = servicos;
            _servico.DisplayMember = "Nome";
            _servico.ValueMember = "Id";

            if (_ordem.ClienteId > 0)
            {
                _cliente.SelectedValue = _ordem.ClienteId;
            }

            _status.SelectedItem = _ordem.Status;
            _status.SelectedIndexChanged += delegate { RefreshItemControls(); };
            _observacao.Text = _ordem.Observacao;
            foreach (var item in _ordem.Itens)
            {
                _itens.Add(item);
            }

            RefreshTotal();
            RefreshItemControls();
        }

        private void AddItem()
        {
            var servico = _servico.SelectedItem as Servico;
            if (servico == null)
            {
                return;
            }

            var item = new OrdemServicoItem
            {
                ServicoId = servico.Id,
                ServicoNome = servico.Nome,
                Quantidade = (int)_quantidade.Value,
                ValorUnitario = servico.ValorBase,
                PercentualImpostoAplicado = servico.PercentualImposto
            };
            item.Recalcular();
            _itens.Add(item);
            RefreshTotal();
        }

        private void RemoveItem()
        {
            if (_gridItens.CurrentRow == null)
            {
                return;
            }

            var item = _gridItens.CurrentRow.DataBoundItem as OrdemServicoItem;
            if (item == null)
            {
                return;
            }

            _itens.Remove(item);
            RefreshTotal();
        }

        private void RefreshTotal()
        {
            var total = _itens.Sum(item => item.ValorTotalItem);
            _total.Text = total.ToString("C2");
        }

        private void RefreshItemControls()
        {
            var status = (StatusOrdemServico)_status.SelectedItem;
            var canEditItems = status != StatusOrdemServico.Concluida && status != StatusOrdemServico.Cancelada;
            _servico.Enabled = canEditItems;
            _quantidade.Enabled = canEditItems;
            _adicionar.Enabled = canEditItems;
            _remover.Enabled = canEditItems;
        }

        private void Save()
        {
            try
            {
                _ordem.ClienteId = (int)_cliente.SelectedValue;
                _ordem.Status = (StatusOrdemServico)_status.SelectedItem;
                _ordem.Observacao = _observacao.Text;
                _ordem.DataConclusao = _ordem.Status == StatusOrdemServico.Concluida ? (DateTime?)DateTime.Now : null;
                _ordem.Itens.Clear();
                foreach (var item in _itens)
                {
                    _ordem.Itens.Add(item);
                }

                _services.OrdensServico.Save(_ordem, Environment.UserName);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                UiErrorHandler.Handle(ex, _services.Logger);
            }
        }
    }
}
