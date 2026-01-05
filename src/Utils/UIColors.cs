using System.Drawing;

namespace TrafficMonitorApp.Utils
{
    /// <summary>
    /// Professional Color Palette - Minimalist & Clean Design
    /// Unified color management for entire application
    /// </summary>
    public static class UIColors
    {
        // ==================== BACKGROUND COLORS ====================
        
        /// <summary>Main application background: #1E1E1E</summary>
        public static readonly Color Background = Color.FromArgb(30, 30, 30);
        
        /// <summary>Panel/Container background: #252526</summary>
        public static readonly Color BackgroundPanel = Color.FromArgb(37, 37, 38);
        
        /// <summary>Dark background for headers: #2D2D30</summary>
        public static readonly Color BackgroundDark = Color.FromArgb(45, 45, 48);
        
        /// <summary>Alternative background for alternating rows: #1F1F1F</summary>
        public static readonly Color BackgroundAlt = Color.FromArgb(31, 31, 31);
        
        // ==================== TEXT COLORS ====================
        
        /// <summary>Primary text color: #FFFFFF (White)</summary>
        public static readonly Color TextPrimary = Color.FromArgb(255, 255, 255);
        
        /// <summary>Secondary text color: #CCCCCC (Light Gray)</summary>
        public static readonly Color TextSecondary = Color.FromArgb(204, 204, 204);
        
        /// <summary>Tertiary/Muted text color: #969696 (Medium Gray)</summary>
        public static readonly Color TextTertiary = Color.FromArgb(150, 150, 150);
        
        // ==================== ACCENT COLORS (Status Indicators) ====================
        
        /// <summary>Primary accent color: #007ACC (VS Code Blue)</summary>
        public static readonly Color Primary = Color.FromArgb(0, 122, 204);
        
        /// <summary>Success/Positive color: #4CAF50 (Green)</summary>
        public static readonly Color Success = Color.FromArgb(76, 175, 80);
        
        /// <summary>Warning/Caution color: #FF9800 (Orange)</summary>
        public static readonly Color Warning = Color.FromArgb(255, 152, 0);
        
        /// <summary>Danger/Negative color: #F44336 (Red)</summary>
        public static readonly Color Danger = Color.FromArgb(244, 67, 54);
        
        /// <summary>Info/Secondary accent color: #2196F3 (Light Blue)</summary>
        public static readonly Color Info = Color.FromArgb(33, 150, 243);
        
        /// <summary>Secondary accent color: #9C27B0 (Purple)</summary>
        public static readonly Color Secondary = Color.FromArgb(156, 39, 176);
        
        // ==================== BORDER & DIVIDER COLORS ====================
        
        /// <summary>Border color: #404040 (Dark Gray)</summary>
        public static readonly Color Border = Color.FromArgb(64, 64, 64);
        
        /// <summary>Divider/Separator color: #333333 (Very Dark Gray)</summary>
        public static readonly Color Divider = Color.FromArgb(51, 51, 51);
        
        /// <summary>Subtle divider: #2A2A2A</summary>
        public static readonly Color DividerSubtle = Color.FromArgb(42, 42, 42);
        
        // ==================== CONTROL COLORS ====================
        
        /// <summary>Button background (default): #333333</summary>
        public static readonly Color ButtonBackground = Color.FromArgb(51, 51, 51);
        
        /// <summary>Button hover state: #404040</summary>
        public static readonly Color ButtonHover = Color.FromArgb(64, 64, 64);
        
        /// <summary>Button pressed state: #505050</summary>
        public static readonly Color ButtonPressed = Color.FromArgb(80, 80, 80);
        
        /// <summary>Button disabled state: #404040 (Grayed out)</summary>
        public static readonly Color ButtonDisabled = Color.FromArgb(64, 64, 64);
        
        /// <summary>Input field background: #2D2D30</summary>
        public static readonly Color InputBackground = Color.FromArgb(45, 45, 48);
        
        /// <summary>Input field text: #CCCCCC</summary>
        public static readonly Color InputText = Color.FromArgb(204, 204, 204);
        
        // ==================== CHART COLORS ====================
        
        /// <summary>Chart line color (primary): #007ACC</summary>
        public static readonly Color ChartPrimary = Color.FromArgb(0, 122, 204);
        
        /// <summary>Chart background (transparent): lighter gray</summary>
        public static readonly Color ChartBackground = Color.FromArgb(37, 37, 38);
        
        /// <summary>Chart grid line color: #404040</summary>
        public static readonly Color ChartGrid = Color.FromArgb(64, 64, 64);
        
        // ==================== UTILITY METHODS ====================
        
        /// <summary>
        /// Get a lighter shade of a color
        /// </summary>
        public static Color Lighten(Color color, float amount = 0.1f)
        {
            int r = (int)System.Math.Min(255, color.R + (255 * amount));
            int g = (int)System.Math.Min(255, color.G + (255 * amount));
            int b = (int)System.Math.Min(255, color.B + (255 * amount));
            return Color.FromArgb(r, g, b);
        }
        
        /// <summary>
        /// Get a darker shade of a color
        /// </summary>
        public static Color Darken(Color color, float amount = 0.1f)
        {
            int r = (int)System.Math.Max(0, color.R - (255 * amount));
            int g = (int)System.Math.Max(0, color.G - (255 * amount));
            int b = (int)System.Math.Max(0, color.B - (255 * amount));
            return Color.FromArgb(r, g, b);
        }
        
        /// <summary>
        /// Get a semi-transparent color
        /// </summary>
        public static Color WithAlpha(Color color, byte alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }
    }
}
