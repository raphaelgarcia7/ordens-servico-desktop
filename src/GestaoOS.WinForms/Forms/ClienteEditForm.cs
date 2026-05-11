using System;
using System.Windows.Forms;
using GestaoOS.Domain.Entities;
using GestaoOS.Domain.Enums;
using GestaoOS.WinForms.Infrastructure;

namespace GestaoOS.WinForms.Forms
{
    public class ClienteEditForm : Form
    {
        private readonly AppServices _services;
        private readonly Cliente _cliente;
        private readonly TextBox _nome = new TextBox();
        private readonly TextBox _documento = new TextBox();
        private readonly ComboBox _tipo = new ComboBox();
        private readonly TextBox _email = new TextBox();
        private readonly TextBox _telefone = new TextBox();
        private readonly CheckBox _ativo = new CheckBox();

        public ClienteEditForm(AppServices services, Cliente cliente)
        {
            _services = services;
            _cliente = cliente.Id == 0 ? cliente : services.Clientes.GetById(cliente.Id);
            Text = "Cliente";
            Width = 430;
            Height = 310;
            StartPosition = FormStartPosition.CenterParent;
            Build();
            Bind();
        }

        private void Build()
        {
            AddLabel("Nome", 16, 18); Add(_nome, 16, 38, 370);
            AddLabel("Documento", 16, 68); Add(_documento, 16, 88, 160);
            AddLabel("Tipo", 190, 68); Add(_tipo, 190, 88, 196);
            AddLabel("E-mail", 16, 118); Add(_email, 16, 138, 370);
            AddLabel("Telefone", 16, 168); Add(_telefone, 16, 188, 160);
            _ativo.Left = 190; _ativo.Top = 190; _ativo.Text = "Ativo"; Controls.Add(_ativo);
            var salvar = new Button { Left = 216, Top = 228, Width = 80, Text = "Salvar" };
            var cancelar = new Button { Left = 306, Top = 228, Width = 80, Text = "Cancelar", DialogResult = DialogResult.Cancel };
            salvar.Click += delegate { Save(); };
            Controls.Add(salvar); Controls.Add(cancelar);
        }

        private void Bind()
        {
            _tipo.DataSource = Enum.GetValues(typeof(TipoCliente));
            _nome.Text = _cliente.Nome;
            _documento.Text = _cliente.Documento;
            _tipo.SelectedItem = _cliente.Tipo;
            _email.Text = _cliente.Email;
            _telefone.Text = _cliente.Telefone;
            _ativo.Checked = _cliente.Ativo;
        }

        private void Save()
        {
            try
            {
                _cliente.Nome = _nome.Text;
                _cliente.Documento = _documento.Text;
                _cliente.Tipo = (TipoCliente)_tipo.SelectedItem;
                _cliente.Email = _email.Text;
                _cliente.Telefone = _telefone.Text;
                _cliente.Ativo = _ativo.Checked;
                _services.Clientes.Save(_cliente);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                UiErrorHandler.Handle(ex, _services.Logger);
            }
        }

        private void Add(Control control, int left, int top, int width)
        {
            control.Left = left; control.Top = top; control.Width = width; Controls.Add(control);
        }

        private void AddLabel(string text, int left, int top)
        {
            Controls.Add(new Label { Text = text, Left = left, Top = top, Width = 120 });
        }
    }
}
