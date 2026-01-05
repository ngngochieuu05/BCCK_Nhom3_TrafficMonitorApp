using System;
using System.Drawing;

namespace TrafficMonitorApp.Utils
{
    /// <summary>
    /// Centralized Font Management - Professional Typography
    /// All fonts follow Segoe UI for Windows system consistency
    /// </summary>
    public static class UIFonts
    {
        private const string DefaultFontFamily = "Segoe UI";
        
        // ==================== TITLE & HEADING FONTS ====================
        
        /// <summary>Form title: Segoe UI 16pt Bold</summary>
        public static readonly Font FormTitle = new Font(DefaultFontFamily, 16F, FontStyle.Bold);
        
        /// <summary>Section title: Segoe UI 12pt Bold</summary>
        public static readonly Font SectionTitle = new Font(DefaultFontFamily, 12F, FontStyle.Bold);
        
        /// <summary>Subsection title: Segoe UI 11pt Bold</summary>
        public static readonly Font SubsectionTitle = new Font(DefaultFontFamily, 11F, FontStyle.Bold);
        
        // ==================== BODY TEXT FONTS ====================
        
        /// <summary>Large value/number: Segoe UI 28pt Bold</summary>
        public static readonly Font LargeValue = new Font(DefaultFontFamily, 28F, FontStyle.Bold);
        
        /// <summary>Medium value/number: Segoe UI 24pt Bold</summary>
        public static readonly Font MediumValue = new Font(DefaultFontFamily, 24F, FontStyle.Bold);
        
        /// <summary>Normal text: Segoe UI 10pt Regular</summary>
        public static readonly Font Normal = new Font(DefaultFontFamily, 10F, FontStyle.Regular);
        
        /// <summary>Normal text (bold): Segoe UI 10pt Bold</summary>
        public static readonly Font NormalBold = new Font(DefaultFontFamily, 10F, FontStyle.Bold);
        
        /// <summary>Small text: Segoe UI 9.5pt Regular</summary>
        public static readonly Font Small = new Font(DefaultFontFamily, 9.5F, FontStyle.Regular);
        
        /// <summary>Small text (bold): Segoe UI 9.5pt Bold</summary>
        public static readonly Font SmallBold = new Font(DefaultFontFamily, 9.5F, FontStyle.Bold);
        
        /// <summary>Extra small text: Segoe UI 8pt Regular</summary>
        public static readonly Font ExtraSmall = new Font(DefaultFontFamily, 8F, FontStyle.Regular);
        
        /// <summary>Extra small text (bold): Segoe UI 8pt Bold</summary>
        public static readonly Font ExtraSmallBold = new Font(DefaultFontFamily, 8F, FontStyle.Bold);
        
        // ==================== BUTTON & CONTROL FONTS ====================
        
        /// <summary>Button font: Segoe UI 10pt Bold</summary>
        public static readonly Font Button = new Font(DefaultFontFamily, 10.5F, FontStyle.Bold);
        
        /// <summary>Large button font: Segoe UI 11pt Bold</summary>
        public static readonly Font ButtonLarge = new Font(DefaultFontFamily, 11F, FontStyle.Bold);
        
        /// <summary>Label font: Segoe UI 9.5pt Regular</summary>
        public static readonly Font Label = new Font(DefaultFontFamily, 9.5F, FontStyle.Regular);
        
        /// <summary>Label font (bold): Segoe UI 9.5pt Bold</summary>
        public static readonly Font LabelBold = new Font(DefaultFontFamily, 9.5F, FontStyle.Bold);
        
        // ==================== TABLE & GRID FONTS ====================
        
        /// <summary>DataGridView header: Segoe UI 10pt Bold</summary>
        public static readonly Font GridHeader = new Font(DefaultFontFamily, 10F, FontStyle.Bold);
        
        /// <summary>DataGridView cell: Segoe UI 9pt Regular</summary>
        public static readonly Font GridCell = new Font(DefaultFontFamily, 9F, FontStyle.Regular);
        
        /// <summary>DataGridView cell (bold): Segoe UI 9pt Bold</summary>
        public static readonly Font GridCellBold = new Font(DefaultFontFamily, 9F, FontStyle.Bold);
        
        // ==================== MONOSPACE FONTS (for code/logs) ====================
        
        /// <summary>Monospace font: Consolas 9pt Regular</summary>
        public static readonly Font Monospace = new Font("Consolas", 9F, FontStyle.Regular);
        
        /// <summary>Monospace font (bold): Consolas 9pt Bold</summary>
        public static readonly Font MonospaceBold = new Font("Consolas", 9F, FontStyle.Bold);
        
        // ==================== STATUS/INDICATOR FONTS ====================
        
        /// <summary>Status indicator: Segoe UI 9pt Bold</summary>
        public static readonly Font StatusIndicator = new Font(DefaultFontFamily, 9F, FontStyle.Bold);
        
        /// <summary>Status value: Segoe UI 14pt Bold</summary>
        public static readonly Font StatusValue = new Font(DefaultFontFamily, 14F, FontStyle.Bold);
        
        // ==================== METADATA/HELPER FONTS ====================
        
        /// <summary>Placeholder text: Segoe UI 9pt Italic</summary>
        public static readonly Font Placeholder = new Font(DefaultFontFamily, 9F, FontStyle.Italic);
        
        /// <summary>Hint/Help text: Segoe UI 8pt Regular</summary>
        public static readonly Font HintText = new Font(DefaultFontFamily, 8F, FontStyle.Regular);
        
        // ==================== MENU & TOOLBAR FONTS ====================
        
        /// <summary>Menu item font: Segoe UI 10pt Regular</summary>
        public static readonly Font MenuItem = new Font(DefaultFontFamily, 10F, FontStyle.Regular);
        
        /// <summary>Toolbar button font: Segoe UI 10pt Bold</summary>
        public static readonly Font ToolbarButton = new Font(DefaultFontFamily, 10F, FontStyle.Bold);
        
        // ==================== SIZE CONSTANTS ====================
        
        public static class FontSizes
        {
            public const float FormTitle = 16F;
            public const float SectionTitle = 12F;
            public const float SubsectionTitle = 11F;
            public const float LargeValue = 28F;
            public const float MediumValue = 24F;
            public const float Normal = 10F;
            public const float Small = 9.5F;
            public const float ExtraSmall = 8F;
            public const float Button = 10.5F;
            public const float GridHeader = 10F;
            public const float GridCell = 9F;
        }
        
        // ==================== UTILITY METHODS ====================
        
        /// <summary>
        /// Create a custom font with specified family, size, and style
        /// </summary>
        public static Font Create(string fontFamily, float size, FontStyle style = FontStyle.Regular)
        {
            try
            {
                return new Font(fontFamily, size, style);
            }
            catch
            {
                // Fallback to default font if family not available
                return new Font(DefaultFontFamily, size, style);
            }
        }
        
        /// <summary>
        /// Get a bold version of the current font
        /// </summary>
        public static Font ToBold(Font font)
        {
            return new Font(font.FontFamily, font.Size, font.Style | FontStyle.Bold);
        }
        
        /// <summary>
        /// Get an italic version of the current font
        /// </summary>
        public static Font ToItalic(Font font)
        {
            return new Font(font.FontFamily, font.Size, font.Style | FontStyle.Italic);
        }
        
        /// <summary>
        /// Resize a font while maintaining style
        /// </summary>
        public static Font Resize(Font font, float newSize)
        {
            return new Font(font.FontFamily, newSize, font.Style);
        }
    }
}
