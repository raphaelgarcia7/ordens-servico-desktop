using System;
using System.Globalization;
using System.Windows.Forms;
using GestaoOS.Domain.Entities;
using GestaoOS.WinForms.Infrastructure;

namespace GestaoOS.WinForms.Forms
{
    public class ServicoEditForm : Form
    {
        private readonly AppServices _services;
        private readonly Servico _servico;
        private readonly TextBox _nome = new TextBox();
        private readonly NumericUpDown _valorBase = new NumericUpDown();
        private readonly NumericUpDown _percentualImposto = new NumericUpDown();
        private readonly CheckBox _ativo = new CheckBox();

        public ServicoEditForm(AppServices services, Servico servico)
        {
            _services = services;
            _servico = servico.Id == 0 ? servico : services.Servicos.GetById(servico.Id);
            Text = "Serviço";
            Width = 430;
            Height = 260;
            StartPosition = FormStartPosition.CenterParent;
            Build();
            Bind();
        }

        private void Build()
        {
            Controls.Add(new Label { Left = 16, Top = 18, Width = 120, Text = "Nome" });
            _nome.Left = 16; _nome.Top = 38; _nome.Width = 370; Controls.Add(_nome);
            Controls.Add(new Label { Left = 16, Top = 72, Width = 120, Text = "Valor base" });
            _valorBase.Left = 16; _valorBase.Top = 92; _valorBase.Width = 160; _valorBase.DecimalPlaces = 2; _valorBase.Maximum = 999999; Controls.Add(_valorBase);
            Controls.Add(new Label { Left = 190, Top = 72, Width = 150, Text = "Imposto (%)" });
            _percentualImposto.Left = 190; _percentualImposto.Top = 92; _percentualImposto.Width = 120; _percentualImposto.DecimalPlaces = 2; _percentualImposto.Maximum = 100; Controls.Add(_percentualImposto);
            _ativo.Left = 16; _ativo.Top = 134; _ativo.Text = "Ativo"; Controls.Add(_ativo);
            var salvar = new Button { Left = 216, Top = 174, Width = 80, Text = "Salvar" };
            var cancelar = new Button { Left = 306, Top = 174, Width = 80, Text = "Cancelar", DialogResult = DialogResult.Cancel };
            salvar.Click += delegate { Save(); };
            Controls.Add(salvar); Controls.Add(cancelar);
        }

        private void Bind()
        {
            _nome.Text = _servico.Nome;
            _valorBase.Value = _servico.ValorBase;
            _percentualImposto.Value = _servico.PercentualImposto;
            _ativo.Checked = _servico.Ativo;
        }

        private void Save()
        {
            try
            {
                _servico.Nome = _nome.Text;
                _servico.ValorBase = _valorBase.Value;
                _servico.PercentualImposto = _percentualImposto.Value;
                _servico.Ativo = _ativo.Checked;
                _services.Servicos.Save(_servico);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                UiErrorHandler.Handle(ex, _services.Logger);
            }
        }
    }
}
