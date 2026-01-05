using System;
using System.Drawing;
using System.Windows.Forms;
using TrafficMonitorApp.Utils;

namespace TrafficMonitorApp.GUI
{
    /// <summary>
    /// Form cài đặt giao diện
    /// UI settings form
    /// </summary>
    public class SettingsForm : Form
    {
        // Display Settings
        private ComboBox cboTheme = null!;
        private ComboBox cboLanguage = null!;
        private NumericUpDown nudFontSize = null!;
        private CheckBox chkShowNotifications = null!;
        private CheckBox chkAutoSave = null!;
        
        // GPU/CPU Settings
        private ComboBox cboExecutionProvider = null!;
        private Label lblExecutionInfo = null!;
        
        // Buttons
        private Button btnSave = null!;
        private Button btnCancel = null!;
        
        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
            
            // Subscribe to theme changes
            ColorScheme.ThemeChanged += (s, e) => ApplyCurrentTheme();
        }
        
        private void InitializeComponent()
        {
            this.Text = "⚙️ Cài Đặt Giao Diện";
            this.Size = new Size(600, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColorScheme.BackgroundDark;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 10);
            
            // Header
            var lblHeader = new Label
            {
                Text = "🎨 CÀI ĐẶT GIAO DIỆN",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 20),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);
            
            int yPos = 80;
            
            // Theme
            var lblTheme = new Label
            {
                Text = "Giao Diện:",
                Location = new Point(30, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(lblTheme);
            
            cboTheme = new ComboBox
            {
                Location = new Point(200, yPos),
                Size = new Size(350, 30),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White
            };
            cboTheme.Items.AddRange(new string[] { "Tối (Dark)", "Sáng (Light)" });
            cboTheme.SelectedIndex = 0;
            this.Controls.Add(cboTheme);
            
            yPos += 60;
            
            // Language
            var lblLanguage = new Label
            {
                Text = "Ngôn Ngữ:",
                Location = new Point(30, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(lblLanguage);
            
            cboLanguage = new ComboBox
            {
                Location = new Point(200, yPos),
                Size = new Size(350, 30),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White
            };
            cboLanguage.Items.AddRange(new string[] { "Tiếng Việt", "English" });
            cboLanguage.SelectedIndex = 0;
            this.Controls.Add(cboLanguage);
            
            yPos += 60;
            
            // Font Size
            var lblFontSize = new Label
            {
                Text = "Cỡ Chữ:",
                Location = new Point(30, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(lblFontSize);
            
            nudFontSize = new NumericUpDown
            {
                Location = new Point(200, yPos),
                Size = new Size(150, 30),
                Minimum = 8,
                Maximum = 16,
                Value = 10,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White
            };
            this.Controls.Add(nudFontSize);
            
            yPos += 60;
            
            // Notifications
            chkShowNotifications = new CheckBox
            {
                Text = "🔔 Hiển Thị Thông Báo",
                Location = new Point(30, yPos),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Checked = true
            };
            this.Controls.Add(chkShowNotifications);
            
            yPos += 50;
            
            // Auto Save
            chkAutoSave = new CheckBox
            {
                Text = "💾 Tự Động Lưu Cấu Hình",
                Location = new Point(30, yPos),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Checked = true
            };
            this.Controls.Add(chkAutoSave);
            
            yPos += 50;
            
            // GPU/CPU Settings
            var lblExecutionProvider = new Label
            {
                Text = "⚡ Bộ Xử Lý AI (GPU/CPU):",
                Location = new Point(30, yPos),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(lblExecutionProvider);
            
            cboExecutionProvider = new ComboBox
            {
                Location = new Point(330, yPos),
                Size = new Size(220, 30),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White
            };
            cboExecutionProvider.Items.AddRange(new string[] { 
                "CPU - Chạy trên CPU (tất cả máy)",
                "CUDA - GPU NVIDIA (nhanh hơn)",
                "TensorRT - NVIDIA GPU tối ưu (cực nhanh)"
            });
            cboExecutionProvider.SelectedIndex = 1; // Default to CUDA (GPU)
            cboExecutionProvider.SelectedIndexChanged += (s, e) => UpdateExecutionInfo();
            this.Controls.Add(cboExecutionProvider);
            
            yPos += 60;
            
            // Execution Info
            lblExecutionInfo = new Label
            {
                Text = "ℹ️ CPU: An toàn, tương thích với mọi máy, tốc độ chậm hơn",
                Location = new Point(30, yPos),
                Size = new Size(520, 60),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(189, 220, 240),
                AutoSize = false
            };
            this.Controls.Add(lblExecutionInfo);
            
            // Bottom panel for buttons
            var pnlButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.FromArgb(44, 62, 80),
                Padding = new Padding(20)
            };
            
            btnSave = new Button
            {
                Text = "💾 Lưu Cài Đặt",
                Location = new Point(250, 18),
                Size = new Size(150, 45),
                BackColor = ColorScheme.Success,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            
            btnCancel = new Button
            {
                Text = "❌ Hủy",
                Location = new Point(410, 18),
                Size = new Size(150, 45),
                BackColor = ColorScheme.Danger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();
            
            pnlButtons.Controls.AddRange(new Control[] { btnSave, btnCancel });
            
            this.Controls.Add(pnlButtons);
        }
        
        private void LoadSettings()
        {
            // Load saved settings if available
            var config = AppConfig.Load();
            
            nudFontSize.Value = 10;
            cboTheme.SelectedIndex = ColorScheme.CurrentTheme == ThemeMode.Dark ? 0 : 1;
            cboLanguage.SelectedIndex = 0;
            
            // Load GPU/CPU settings - Default is now CUDA (GPU)
            if (config.ExecutionProvider == "cuda")
                cboExecutionProvider.SelectedIndex = 1;
            else if (config.ExecutionProvider == "tensorrt")
                cboExecutionProvider.SelectedIndex = 2;
            else
                cboExecutionProvider.SelectedIndex = 0;
            
            UpdateExecutionInfo();
            
            // Apply theme khi load
            ApplyCurrentTheme();
        }
        
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                // Load config và lưu GPU/CPU settings
                var config = AppConfig.Load();
                config.ExecutionProvider = cboExecutionProvider.SelectedIndex switch
                {
                    0 => "cpu",
                    1 => "cuda",
                    2 => "tensorrt",
                    _ => "cpu"
                };
                config.Save();
                
                // Áp dụng theme ngay lập tức
                ColorScheme.CurrentTheme = cboTheme.SelectedIndex == 0 ? ThemeMode.Dark : ThemeMode.Light;
                
                // Áp dụng lại theme cho form hiện tại
                ApplyCurrentTheme();
                
                string providerName = cboExecutionProvider.SelectedIndex switch
                {
                    0 => "CPU",
                    1 => "CUDA (NVIDIA GPU)",
                    2 => "TensorRT (NVIDIA GPU Tối Ưu)",
                    _ => "CPU"
                };
                
                MessageBox.Show(
                    $"✅ Cài đặt đã được áp dụng!\n\n" +
                    $"Bộ xử lý: {providerName}\n" +
                    $"Theme: {(cboTheme.SelectedIndex == 0 ? "Tối" : "Sáng")}\n\n" +
                    $"⚠️ Khởi động lại ứng dụng để áp dụng bộ xử lý AI.",
                    "Thành Công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Lỗi khi lưu cài đặt:\n{ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        
        private void ApplyCurrentTheme()
        {
            // Cập nhật màu nền và các control
            this.BackColor = ColorScheme.Background;
            
            foreach (Control control in this.Controls)
            {
                UpdateControlTheme(control);
            }
        }
        
        private void UpdateControlTheme(Control control)
        {
            if (control is Label label && !label.Text.Contains("🎨"))
            {
                label.ForeColor = ColorScheme.Text;
            }
            else if (control is ComboBox combo)
            {
                combo.BackColor = ColorScheme.InputBackground;
                combo.ForeColor = ColorScheme.InputText;
            }
            else if (control is NumericUpDown numeric)
            {
                numeric.BackColor = ColorScheme.InputBackground;
                numeric.ForeColor = ColorScheme.InputText;
            }
            else if (control is CheckBox checkbox)
            {
                checkbox.ForeColor = ColorScheme.Text;
            }
            else if (control is Panel panel)
            {
                panel.BackColor = ColorScheme.BackgroundPanel;
                
                // Cập nhật các control con trong panel
                foreach (Control child in panel.Controls)
                {
                    UpdateControlTheme(child);
                }
            }
        }
        
        private void UpdateExecutionInfo()
        {
            string info = cboExecutionProvider.SelectedIndex switch
            {
                0 => "ℹ️ CPU: Chạy trên bộ vi xử lý CPU. An toàn, tương thích với mọi máy. Tốc độ chậm hơn GPU.",
                1 => "⚡ CUDA: Dùng GPU NVIDIA. Cần cài NVIDIA CUDA Toolkit. Nhanh hơn CPU ~5-10 lần.",
                2 => "🚀 TensorRT: NVIDIA GPU tối ưu cao. Cần NVIDIA Driver + CUDA + TensorRT. Cực nhanh (~10-20x).",
                _ => "?"
            };
            lblExecutionInfo.Text = info;
        }
    }
}
