using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace TrafficMonitorApp.Utils
{
    /// <summary>
    /// Utility class ?? áp d?ng Professional Design Style m?t cách t? ??ng
    /// Utility class to apply Professional Design Style automatically
    /// </summary>
    public static class ProfessionalUIStyler
    {
        // ===== PROFESSIONAL COLOR PALETTE =====
        
        /// <summary>
        /// Màu n?n chính (Main Background)
        /// </summary>
        public static readonly Color MainBackground = Color.FromArgb(30, 30, 30);      // #1E1E1E
        
        /// <summary>
        /// Màu n?n Panel/Surface
        /// </summary>
        public static readonly Color PanelBackground = Color.FromArgb(44, 62, 80);     // #2D2D30
        
        /// <summary>
        /// Màu n?n Input control
        /// </summary>
        public static readonly Color InputBackground = Color.FromArgb(52, 73, 94);     // #344A5E
        
        /// <summary>
        /// Màu ch? ??o xanh d??ng (Primary Blue - VS Style)
        /// </summary>
        public static readonly Color PrimaryBlue = Color.FromArgb(0, 122, 204);        // #007ACC
        
        /// <summary>
        /// Màu xanh d??ng sáng h?n (Bright Blue - Hover)
        /// </summary>
        public static readonly Color PrimaryBlueBright = Color.FromArgb(55, 148, 255); // #3794FF
        
        /// <summary>
        /// Màu thành công (Success - Green)
        /// </summary>
        public static readonly Color SuccessGreen = Color.FromArgb(46, 204, 113);      // #2ECC71
        
        /// <summary>
        /// Màu c?nh báo (Warning - Yellow)
        /// </summary>
        public static readonly Color WarningYellow = Color.FromArgb(241, 196, 15);     // #F1C40F
        
        /// <summary>
        /// Màu l?i (Error - Red)
        /// </summary>
        public static readonly Color ErrorRed = Color.FromArgb(231, 76, 60);           // #E74C3C
        
        /// <summary>
        /// Màu xám nh?t (Gray Light)
        /// </summary>
        public static readonly Color GrayLight = Color.FromArgb(60, 60, 60);           // #3C3C3C
        
        /// <summary>
        /// Màu ch? tr?ng (Text White)
        /// </summary>
        public static readonly Color TextWhite = Color.White;                          // #FFFFFF
        
        /// <summary>
        /// Màu ch? ghi chú (Text Secondary)
        /// </summary>
        public static readonly Color TextSecondary = Color.FromArgb(200, 200, 200);    // #C8C8C8
        
        /// <summary>
        /// Màu ch? m? (Text Muted)
        /// </summary>
        public static readonly Color TextMuted = Color.FromArgb(150, 150, 150);        // #969696
        
        // ===== METHODS =====
        
        /// <summary>
        /// Áp d?ng style chuyên nghi?p cho Button (Flat Style)
        /// Apply professional style to Button (Flat Style)
        /// </summary>
        public static void StyleButton(Button btn, ButtonStyle style = ButtonStyle.Default, string icon = "")
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.Padding = new Padding(8, 0, 8, 0);
            
            switch (style)
            {
                case ButtonStyle.Primary:
                    btn.BackColor = PrimaryBlue;
                    btn.ForeColor = TextWhite;
                    btn.FlatAppearance.MouseOverBackColor = PrimaryBlueBright;
                    btn.Size = new Size(150, 40);
                    break;
                    
                case ButtonStyle.Success:
                    btn.BackColor = SuccessGreen;
                    btn.ForeColor = TextWhite;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 174, 96);
                    btn.Size = new Size(150, 40);
                    break;
                    
                case ButtonStyle.Warning:
                    btn.BackColor = WarningYellow;
                    btn.ForeColor = TextWhite;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 176, 0);
                    btn.Size = new Size(150, 40);
                    break;
                    
                case ButtonStyle.Danger:
                    btn.BackColor = ErrorRed;
                    btn.ForeColor = TextWhite;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 57, 43);
                    btn.Size = new Size(150, 40);
                    break;
                    
                case ButtonStyle.Secondary:
                    btn.BackColor = GrayLight;
                    btn.ForeColor = TextWhite;
                    btn.FlatAppearance.MouseOverBackColor = PrimaryBlue;
                    btn.Size = new Size(150, 40);
                    break;
                    
                case ButtonStyle.Icon:  // Compact icon button
                    btn.BackColor = SuccessGreen;
                    btn.ForeColor = TextWhite;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 174, 96);
                    btn.Size = new Size(60, 40);
                    btn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                    break;
                    
                default:  // Default
                    btn.BackColor = GrayLight;
                    btn.ForeColor = TextWhite;
                    btn.FlatAppearance.MouseOverBackColor = PrimaryBlue;
                    btn.Size = new Size(150, 40);
                    break;
            }
            
            if (!string.IsNullOrEmpty(icon))
            {
                btn.Text = $"{icon} {btn.Text}";
            }
        }
        
        /// <summary>
        /// Áp d?ng style cho Panel
        /// Apply style to Panel
        /// </summary>
        public static void StylePanel(Panel panel, bool isContainer = false)
        {
            if (isContainer)
            {
                panel.BackColor = MainBackground;
            }
            else
            {
                panel.BackColor = PanelBackground;
            }
            panel.BorderStyle = BorderStyle.None;
        }
        
        /// <summary>
        /// Áp d?ng style cho GroupBox
        /// Apply style to GroupBox
        /// </summary>
        public static void StyleGroupBox(GroupBox gb)
        {
            gb.BackColor = PanelBackground;
            gb.ForeColor = TextSecondary;
            gb.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            gb.Padding = new Padding(15);
            gb.Margin = new Padding(10);
        }
        
        /// <summary>
        /// Áp d?ng style cho Label
        /// Apply style to Label
        /// </summary>
        public static void StyleLabel(Label lbl, LabelStyle style = LabelStyle.Regular)
        {
            switch (style)
            {
                case LabelStyle.Heading:
                    lbl.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                    lbl.ForeColor = TextWhite;
                    break;
                    
                case LabelStyle.SubHeading:
                    lbl.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                    lbl.ForeColor = TextSecondary;
                    break;
                    
                case LabelStyle.Regular:
                    lbl.Font = new Font("Segoe UI", 9);
                    lbl.ForeColor = TextWhite;
                    break;
                    
                case LabelStyle.Muted:
                    lbl.Font = new Font("Segoe UI", 9);
                    lbl.ForeColor = TextMuted;
                    break;
                    
                case LabelStyle.Value:  // Large number display
                    lbl.Font = new Font("Segoe UI", 24, FontStyle.Bold);
                    lbl.ForeColor = PrimaryBlue;
                    break;
            }
        }
        
        /// <summary>
        /// Áp d?ng style cho TextBox
        /// Apply style to TextBox
        /// </summary>
        public static void StyleTextBox(TextBox txt)
        {
            txt.BackColor = InputBackground;
            txt.ForeColor = TextWhite;
            txt.Font = new Font("Segoe UI", 9);
            txt.BorderStyle = BorderStyle.FixedSingle;
        }
        
        /// <summary>
        /// Áp d?ng style cho NumericUpDown
        /// Apply style to NumericUpDown
        /// </summary>
        public static void StyleNumericUpDown(NumericUpDown num)
        {
            num.BackColor = InputBackground;
            num.ForeColor = TextWhite;
            num.Font = new Font("Segoe UI", 9);
            num.BorderStyle = BorderStyle.FixedSingle;
        }
        
        /// <summary>
        /// Áp d?ng style cho CheckBox
        /// Apply style to CheckBox
        /// </summary>
        public static void StyleCheckBox(CheckBox chk)
        {
            chk.ForeColor = TextWhite;
            chk.Font = new Font("Segoe UI", 9);
            chk.BackColor = PanelBackground;
            chk.FlatStyle = FlatStyle.Flat;
        }
        
        /// <summary>
        /// Áp d?ng style cho ComboBox
        /// Apply style to ComboBox
        /// </summary>
        public static void StyleComboBox(ComboBox cmb)
        {
            cmb.BackColor = InputBackground;
            cmb.ForeColor = TextWhite;
            cmb.Font = new Font("Segoe UI", 9);
            cmb.FlatStyle = FlatStyle.Flat;
        }
        
        /// <summary>
        /// Áp d?ng style cho DataGridView (Zebra Styling)
        /// Apply professional style to DataGridView with Zebra styling
        /// </summary>
        public static void StyleDataGridView(DataGridView dgv)
        {
            // ===== GENERAL =====
            dgv.BackgroundColor = MainBackground;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(70, 70, 70);
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowHeadersVisible = false;
            
            // ===== HEADER =====
            dgv.ColumnHeadersHeight = 45;
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = TextWhite,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(5),
                SelectionBackColor = Color.FromArgb(52, 73, 94),
                SelectionForeColor = TextWhite
            };
            
            // ===== ROWS =====
            dgv.RowTemplate.Height = 32;
            
            // Zebra styling - Alternate row colors
            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = TextWhite,
                Font = new Font("Segoe UI", 9),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                SelectionBackColor = PrimaryBlue,
                SelectionForeColor = TextWhite
            };
            
            // Default row style
            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(44, 62, 80),
                ForeColor = TextWhite,
                Font = new Font("Segoe UI", 9),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                SelectionBackColor = PrimaryBlue,
                SelectionForeColor = TextWhite,
                Padding = new Padding(3)
            };
            
            // ===== COLUMNS =====
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Padding = new Padding(5);
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }
        
        /// <summary>
        /// T?o m?t Statistics Widget
        /// Create a Statistics Widget
        /// </summary>
        public static Panel CreateStatWidget(string label, string value, int x, int y)
        {
            int widgetWidth = 150;
            int widgetHeight = 80;
            
            var widget = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(widgetWidth, widgetHeight),
                BackColor = Color.FromArgb(52, 73, 94),
                BorderStyle = BorderStyle.None
            };
            
            // Title
            var lblTitle = new Label
            {
                Text = label,
                Location = new Point(8, 5),
                Size = new Size(widgetWidth - 16, 20),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };
            
            // Value
            var lblValue = new Label
            {
                Text = value,
                Location = new Point(8, 28),
                Size = new Size(widgetWidth - 16, 35),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = PrimaryBlue,
                BackColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };
            
            widget.Controls.Add(lblTitle);
            widget.Controls.Add(lblValue);
            
            return widget;
        }
        
        /// <summary>
        /// T?o m?t Statistics Card v?i Left Border Indicator
        /// Create a Statistics Card with Left Border Indicator
        /// </summary>
        public static Panel CreateStatCard(string title, string value, Color borderColor, int x, int y)
        {
            int cardWidth = 280;
            int cardHeight = 110;
            
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(cardWidth, cardHeight),
                BackColor = PanelBackground,
                BorderStyle = BorderStyle.None
            };
            
            // Left border indicator
            var borderPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(5, cardHeight),
                BackColor = borderColor,
                Dock = DockStyle.Left
            };
            
            card.Controls.Add(borderPanel);
            
            // Title
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = TextSecondary,
                Location = new Point(20, 10),
                AutoSize = true
            };
            
            // Value (Large font)
            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = TextWhite,
                Location = new Point(20, 35),
                AutoSize = true
            };
            
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            
            return card;
        }
        
        /// <summary>
        /// Áp d?ng style toàn b? Form
        /// Apply theme to entire Form
        /// </summary>
        public static void ApplyFormStyle(Form form)
        {
            form.BackColor = MainBackground;
            form.ForeColor = TextWhite;
            form.Font = new Font("Segoe UI", 10);
            
            ApplyControlsStyle(form);
        }
        
        /// <summary>
        /// Áp d?ng style ?? quy cho t?t c? control trong form
        /// Apply style recursively to all controls
        /// </summary>
        private static void ApplyControlsStyle(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                // Panel
                if (control is Panel && !(control is GroupBox))
                {
                    control.BackColor = PanelBackground;
                    control.ForeColor = TextWhite;
                }
                
                // GroupBox
                else if (control is GroupBox gb)
                {
                    StyleGroupBox(gb);
                }
                
                // Label
                else if (control is Label lbl)
                {
                    if (lbl.ForeColor == TextWhite || lbl.ForeColor == Color.Black)
                        lbl.ForeColor = TextWhite;
                    lbl.BackColor = Color.Transparent;
                }
                
                // TextBox
                else if (control is TextBox txt)
                {
                    StyleTextBox(txt);
                }
                
                // NumericUpDown
                else if (control is NumericUpDown num)
                {
                    StyleNumericUpDown(num);
                }
                
                // CheckBox
                else if (control is CheckBox chk)
                {
                    StyleCheckBox(chk);
                }
                
                // ComboBox
                else if (control is ComboBox cmb)
                {
                    StyleComboBox(cmb);
                }
                
                // DataGridView
                else if (control is DataGridView dgv)
                {
                    StyleDataGridView(dgv);
                }
                
                // Recursive for container controls
                if (control.HasChildren)
                {
                    ApplyControlsStyle(control);
                }
            }
        }
    }
    
    // ===== ENUMS =====
    
    /// <summary>
    /// Button Style Types
    /// </summary>
    public enum ButtonStyle
    {
        Default,    // Xám
        Primary,    // Xanh ch? ??o
        Success,    // Xanh lá
        Warning,    // Vàng
        Danger,     // ??
        Secondary,  // Xám th? c?p
        Icon        // Icon nh? g?n
    }
    
    /// <summary>
    /// Label Style Types
    /// </summary>
    public enum LabelStyle
    {
        Regular,    // Bình th??ng
        Heading,    // Tiêu ?? l?n
        SubHeading, // Tiêu ?? ph?
        Muted,      // M?
        Value       // Giá tr? l?n
    }
}
