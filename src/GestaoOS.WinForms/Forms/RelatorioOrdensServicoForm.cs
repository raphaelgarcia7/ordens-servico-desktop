using System;
using System.IO;
using System.Windows.Forms;
using GestaoOS.Application.Filters;
using GestaoOS.Domain.Enums;
using GestaoOS.WinForms.Infrastructure;
using Microsoft.Reporting.WinForms;

namespace GestaoOS.WinForms.Forms
{
    public class RelatorioOrdensServicoForm : Form
    {
        private readonly AppServices _services;
        private readonly DateTimePicker _inicio = new DateTimePicker();
        private readonly DateTimePicker _fim = new DateTimePicker();
        private readonly ComboBox _status = new ComboBox();
        private readonly ReportViewer _viewer = new ReportViewer();

        public RelatorioOrdensServicoForm(AppServices services)
        {
            _services = services;
            Text = "Relatório Gerencial de OS";
            Width = 1100;
            Height = 720;
            StartPosition = FormStartPosition.CenterParent;
            Build();
        }

        private void Build()
        {
            Controls.Add(new Label { Left = 12, Top = 12, Width = 80, Text = "Início" });
            _inicio.Left = 12; _inicio.Top = 32; _inicio.Width = 130; _inicio.Value = DateTime.Today.AddMonths(-1); Controls.Add(_inicio);
            Controls.Add(new Label { Left = 154, Top = 12, Width = 80, Text = "Fim" });
            _fim.Left = 154; _fim.Top = 32; _fim.Width = 130; _fim.Value = DateTime.Today; Controls.Add(_fim);
            Controls.Add(new Label { Left = 296, Top = 12, Width = 80, Text = "Status" });
            _status.Left = 296; _status.Top = 32; _status.Width = 150; _status.DropDownStyle = ComboBoxStyle.DropDownList;
            _status.Items.Add("(Todos)");
            foreach (var item in Enum.GetValues(typeof(StatusOrdemServico)))
            {
                _status.Items.Add(item);
            }
            _status.SelectedIndex = 0;
            Controls.Add(_status);

            var gerar = new Button { Left = 466, Top = 30, Width = 80, Text = "Gerar" };
            var exportar = new Button { Left = 558, Top = 30, Width = 100, Text = "Exportar PDF" };
            gerar.Click += delegate { LoadReport(); };
            exportar.Click += delegate { ExportPdf(); };
            Controls.Add(gerar); Controls.Add(exportar);

            _viewer.Left = 12; _viewer.Top = 70; _viewer.Width = 1050; _viewer.Height = 590;
            _viewer.ProcessingMode = ProcessingMode.Local;
            Controls.Add(_viewer);
        }

        private OrdemServicoFilter BuildFilter()
        {
            var filter = new OrdemServicoFilter { DataInicial = _inicio.Value.Date, DataFinal = _fim.Value.Date };
            if (_status.SelectedItem is StatusOrdemServico)
            {
                filter.Status = (StatusOrdemServico)_status.SelectedItem;
            }

            return filter;
        }

        private void LoadReport()
        {
            try
            {
                var rows = _services.Relatorios.GerarOrdensServico(BuildFilter());
                _viewer.LocalReport.DataSources.Clear();
                _viewer.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "OrdensServicoReport.rdlc");
                _viewer.LocalReport.DataSources.Add(new ReportDataSource("OrdemServicoReportRow", rows));
                _viewer.RefreshReport();
            }
            catch (Exception ex)
            {
                UiErrorHandler.Handle(ex, _services.Logger);
            }
        }

        private void ExportPdf()
        {
            try
            {
                LoadReport();
                var bytes = _viewer.LocalReport.Render("PDF");
                using (var dialog = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", FileName = "relatorio-os.pdf" })
                {
                    if (dialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    File.WriteAllBytes(dialog.FileName, bytes);
                }
            }
            catch (Exception ex)
            {
                UiErrorHandler.Handle(ex, _services.Logger);
            }
        }
    }
}
