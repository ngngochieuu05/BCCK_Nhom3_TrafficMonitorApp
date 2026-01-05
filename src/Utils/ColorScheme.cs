using System;
using System.Drawing;
using System.Windows.Forms;

namespace TrafficMonitorApp.Utils
{
    /// <summary>
    /// Theme mode: Dark or Light
    /// </summary>
    public enum ThemeMode
    {
        Dark,
        Light
    }

    /// <summary>
    /// Bảng màu đồng nhất cho toàn bộ ứng dụng với hỗ trợ Dark/Light mode
    /// Unified color scheme for the entire application with Dark/Light mode support
    /// </summary>
    public static class ColorScheme
    {
        private static ThemeMode _currentTheme = ThemeMode.Dark;

        public static event EventHandler? ThemeChanged;

        public static ThemeMode CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ThemeChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        // Primary Colors - Màu chính (giữ nguyên ở mọi theme)
        public static readonly Color Primary = Color.FromArgb(52, 152, 219);      // Blue
        public static readonly Color Success = Color.FromArgb(46, 204, 113);      // Green
        public static readonly Color Warning = Color.FromArgb(241, 196, 15);      // Yellow
        public static readonly Color Danger = Color.FromArgb(231, 76, 60);        // Red
        public static readonly Color Info = Color.FromArgb(52, 152, 219);         // Blue
        public static readonly Color Purple = Color.FromArgb(155, 89, 182);       // Purple
        
        // Background Colors - Màu nền (thay đổi theo theme)
        public static Color Background => CurrentTheme == ThemeMode.Dark 
            ? Color.FromArgb(30, 30, 30) 
            : Color.FromArgb(240, 244, 248);
            
        public static Color BackgroundPanel => CurrentTheme == ThemeMode.Dark 
            ? Color.FromArgb(44, 62, 80) 
            : Color.White;
            
        public static Color BackgroundCard => CurrentTheme == ThemeMode.Dark 
            ? Color.FromArgb(44, 62, 80) 
            : Color.White;
        
        // Legacy colors cho tương thích ngược
        public static readonly Color BackgroundLight = Color.FromArgb(240, 244, 248);
        public static readonly Color BackgroundWhite = Color.White;
        public static readonly Color BackgroundDark = Color.FromArgb(52, 73, 94);
        
        // Text Colors - Màu chữ (thay đổi theo theme - TƯƠNG PHẢN CAO)
        public static Color Text => CurrentTheme == ThemeMode.Dark 
            ? Color.White 
            : Color.FromArgb(33, 33, 33);
            
        public static Color TextSecondary => CurrentTheme == ThemeMode.Dark 
            ? Color.FromArgb(200, 200, 200) 
            : Color.FromArgb(100, 100, 100);
            
        public static Color TextMuted => CurrentTheme == ThemeMode.Dark 
            ? Color.FromArgb(150, 150, 150) 
            : Color.FromArgb(127, 140, 141);
        
        // Legacy text colors
        public static readonly Color TextPrimary = Color.FromArgb(44, 62, 80);
        public static readonly Color TextLight = Color.White;
        
        // Status Colors - Màu trạng thái
        public static readonly Color StatusActive = Color.FromArgb(48, 209, 88);
        public static readonly Color StatusInactive = Color.FromArgb(189, 193, 198);
        public static readonly Color StatusError = Color.FromArgb(231, 76, 60);
        public static readonly Color StatusPending = Color.FromArgb(241, 196, 15);
        
        // Chart Colors - Màu biểu đồ
        public static readonly Color Chart1 = Color.FromArgb(52, 152, 219);       // Blue
        public static readonly Color Chart2 = Color.FromArgb(46, 204, 113);       // Green
        public static readonly Color Chart3 = Color.FromArgb(241, 196, 15);       // Yellow
        public static readonly Color Chart4 = Color.FromArgb(231, 76, 60);        // Red
        public static readonly Color Chart5 = Color.FromArgb(155, 89, 182);       // Purple
        public static readonly Color Chart6 = Color.FromArgb(26, 188, 156);       // Teal
        public static readonly Color Chart7 = Color.FromArgb(230, 126, 34);       // Orange
        
        // Detection Colors - Màu phát hiện
        public static readonly Color DetectionBox = Color.FromArgb(0, 255, 0);    // Green
        public static readonly Color CountingLine = Color.FromArgb(255, 0, 0);    // Red
        public static readonly Color DetectionZone = Color.FromArgb(255, 255, 0); // Yellow
        
        // Grid/Table Colors - Màu bảng (thay đổi theo theme)
        public static Color GridHeader => CurrentTheme == ThemeMode.Dark 
            ? Color.FromArgb(52, 73, 94) 
            : Color.FromArgb(52, 73, 94);
            
        public static Color GridAlternate => CurrentTheme == ThemeMode.Dark 
            ? Color.FromArgb(50, 50, 50) 
            : Color.FromArgb(245, 247, 250);
            
        public static Color GridSelected => CurrentTheme == ThemeMode.Dark 
            ? Color.FromArgb(41, 128, 185) 
            : Color.FromArgb(41, 128, 185);
        
        // Border Colors - Màu viền (thay đổi theo theme)
        public static Color Border => CurrentTheme == ThemeMode.Dark 
            ? Color.FromArgb(70, 70, 70) 
            : Color.FromArgb(220, 220, 220);
            
        public static readonly Color BorderLight = Color.FromArgb(220, 220, 220);
        public static readonly Color BorderMedium = Color.FromArgb(200, 200, 200);
        public static readonly Color BorderDark = Color.FromArgb(52, 73, 94);
        
        // Input/Control Colors (thay đổi theo theme)
        public static Color InputBackground => CurrentTheme == ThemeMode.Dark 
            ? Color.FromArgb(52, 73, 94) 
            : Color.White;
            
        public static Color InputText => CurrentTheme == ThemeMode.Dark 
            ? Color.White 
            : Color.FromArgb(33, 33, 33);
        
        // Hover/Active States - Màu khi hover
        public static readonly Color HoverPrimary = Color.FromArgb(41, 128, 185);
        public static readonly Color HoverSuccess = Color.FromArgb(39, 174, 96);
        public static readonly Color HoverWarning = Color.FromArgb(243, 156, 18);
        public static readonly Color HoverDanger = Color.FromArgb(192, 57, 43);
        
        /// <summary>
        /// Áp dụng theme cho một Form
        /// Apply theme to a Form
        /// </summary>
        public static void ApplyTheme(Form form)
        {
            form.BackColor = Background;
            
            foreach (Control control in form.Controls)
            {
                ApplyThemeToControl(control);
            }
        }

        private static void ApplyThemeToControl(Control control)
        {
            // Panel, GroupBox
            if (control is Panel || control is GroupBox)
            {
                control.BackColor = BackgroundPanel;
                control.ForeColor = Text;
            }
            // Label
            else if (control is Label label && label.ForeColor != Primary && 
                     label.ForeColor != Success && label.ForeColor != Danger && 
                     label.ForeColor != Warning)
            {
                control.ForeColor = Text;
            }
            // TextBox, ComboBox
            else if (control is TextBox || control is ComboBox)
            {
                control.BackColor = InputBackground;
                control.ForeColor = InputText;
            }
            // CheckBox, RadioButton
            else if (control is CheckBox || control is RadioButton)
            {
                control.ForeColor = Text;
            }
            // DataGridView
            else if (control is DataGridView grid)
            {
                grid.BackgroundColor = Background;
                grid.DefaultCellStyle.BackColor = BackgroundPanel;
                grid.DefaultCellStyle.ForeColor = Text;
                grid.AlternatingRowsDefaultCellStyle.BackColor = GridAlternate;
                grid.ColumnHeadersDefaultCellStyle.BackColor = GridHeader;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.EnableHeadersVisualStyles = false;
            }
            
            // Đệ quy cho các control con
            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child);
            }
        }
    }
}
