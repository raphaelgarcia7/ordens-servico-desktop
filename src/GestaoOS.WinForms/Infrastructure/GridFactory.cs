using System.Drawing;
using System.Windows.Forms;

namespace GestaoOS.WinForms.Infrastructure
{
    public static class GridFactory
    {
        public static void ConfigureReadOnly(DataGridView grid)
        {
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AutoGenerateColumns = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersHeight = 30;
            grid.EnableHeadersVisualStyles = false;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.RowTemplate.Height = 28;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(242, 244, 247);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(35, 35, 35);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        }

        public static DataGridViewTextBoxColumn TextColumn(string property, string header, int width)
        {
            return TextColumn(property, header, width, null, DataGridViewContentAlignment.MiddleLeft);
        }

        public static DataGridViewTextBoxColumn TextColumn(string property, string header, int width, string format)
        {
            return TextColumn(property, header, width, format, DataGridViewContentAlignment.MiddleLeft);
        }

        public static DataGridViewTextBoxColumn TextColumn(string property, string header, int width, string format, DataGridViewContentAlignment alignment)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = header,
                Name = property,
                Width = width
            };

            column.DefaultCellStyle.Format = format;
            column.DefaultCellStyle.Alignment = alignment;
            return column;
        }

        public static DataGridViewCheckBoxColumn CheckColumn(string property, string header, int width)
        {
            return new DataGridViewCheckBoxColumn
            {
                DataPropertyName = property,
                HeaderText = header,
                Name = property,
                Width = width
            };
        }
    }
}
