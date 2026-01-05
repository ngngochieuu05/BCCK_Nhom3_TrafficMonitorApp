using System;
using System.Drawing;
using System.Windows.Forms;

namespace TrafficMonitorApp.GUI
{
    /// <summary>
    /// Partial class containing Export functionality for AdminDashboardForm
    /// </summary>
    public partial class AdminDashboardForm
    {
        /// <summary>
        /// Export data from DataGridView with file type selection
        /// </summary>
        private void ExportData(string dataType)
        {
            DataGridView? dgv = dataType switch
            {
                "users" => dgvUsers,
                "sessions" => dgvSessions,
                "detections" => dgvDetections,
                "statistics" => dgvStatistics,
                _ => null
            };

            if (dgv == null || dgv.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Tab-Separated Excel|*.xls|CSV Files|*.csv|JSON Files|*.json",
                    Title = $"Xuất Dữ Liệu {dataType}",
                    FileName = $"{dataType}_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string extension = System.IO.Path.GetExtension(saveDialog.FileName).ToLower();

                    switch (extension)
                    {
                        case ".xls":
                            ExportToExcel(dgv, saveDialog.FileName);
                            break;
                        case ".csv":
                            ExportToCSV(dgv, saveDialog.FileName);
                            break;
                        case ".json":
                            ExportToJSON(dgv, saveDialog.FileName);
                            break;
                    }

                    MessageBox.Show($"Xuất dữ liệu thành công!\nFile: {saveDialog.FileName}",
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Export DataGridView to Tab-Separated format (opens in Excel)
        /// </summary>
        private void ExportToExcel(DataGridView dgv, string filePath)
        {
            using (var workbook = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                // Write header
                var headers = new System.Text.StringBuilder();
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    if (dgv.Columns[i].Visible)
                    {
                        headers.Append(dgv.Columns[i].HeaderText);
                        if (i < dgv.Columns.Count - 1)
                            headers.Append("\t");
                    }
                }
                workbook.WriteLine(headers.ToString());

                // Write rows
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.IsNewRow) continue;
                    var rowData = new System.Text.StringBuilder();

                    for (int i = 0; i < dgv.Columns.Count; i++)
                    {
                        if (dgv.Columns[i].Visible)
                        {
                            var cellValue = row.Cells[i].Value?.ToString() ?? "";
                            rowData.Append(cellValue);
                            if (i < dgv.Columns.Count - 1)
                                rowData.Append("\t");
                        }
                    }
                    workbook.WriteLine(rowData.ToString());
                }
            }
        }

        /// <summary>
        /// Export DataGridView to CSV format
        /// </summary>
        private void ExportToCSV(DataGridView dgv, string filePath)
        {
            using (var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                // Write header
                var headers = new System.Text.StringBuilder();
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    if (dgv.Columns[i].Visible)
                    {
                        headers.Append($"\"{dgv.Columns[i].HeaderText}\"");
                        if (i < dgv.Columns.Count - 1)
                            headers.Append(",");
                    }
                }
                writer.WriteLine(headers.ToString());

                // Write rows
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.IsNewRow) continue;
                    var rowData = new System.Text.StringBuilder();

                    for (int i = 0; i < dgv.Columns.Count; i++)
                    {
                        if (dgv.Columns[i].Visible)
                        {
                            var cellValue = row.Cells[i].Value?.ToString()?.Replace("\"", "\"\"") ?? "";
                            rowData.Append($"\"{cellValue}\"");
                            if (i < dgv.Columns.Count - 1)
                                rowData.Append(",");
                        }
                    }
                    writer.WriteLine(rowData.ToString());
                }
            }
        }

        /// <summary>
        /// Export DataGridView to JSON format
        /// </summary>
        private void ExportToJSON(DataGridView dgv, string filePath)
        {
            var jsonData = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, string>>();

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;
                var rowDict = new System.Collections.Generic.Dictionary<string, string>();

                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    if (dgv.Columns[i].Visible)
                    {
                        var columnName = dgv.Columns[i].HeaderText;
                        var cellValue = row.Cells[i].Value?.ToString() ?? "";
                        rowDict[columnName] = cellValue;
                    }
                }
                jsonData.Add(rowDict);
            }

            var json = System.Text.Json.JsonSerializer.Serialize(jsonData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            System.IO.File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
        }
    }
}
