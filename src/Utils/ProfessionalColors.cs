/// <summary>
/// Professional Color Scheme for Traffic Monitor App
/// B?ng màu chu?n cho ?ng d?ng - Minimalist, Clean, Professional
/// </summary>
namespace TrafficMonitorApp.Utils
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Professional palette - Replaces hardcoded colors in UI
    /// S? d?ng thay th? cho t?t c? hardcode màu trong Forms
    /// </summary>
    public static class ProfessionalColors
    {
        // ==================== PRIMARY COLORS (N?N CHÍNH) ====================
        /// <summary>
        /// N?n chính: Xám ?en ph?ng (không gradient)
        /// Main background: #1E1E1E (Flat, no gradient)
        /// </summary>
        public static Color Background => Color.FromArgb(30, 30, 30);

        /// <summary>
        /// N?n ph?: Xám sáng h?n n?n chính, ?? phân tách các kh?i
        /// Secondary background: #282828 (Separates sections)
        /// </summary>
        public static Color BackgroundSecondary => Color.FromArgb(40, 40, 40);

        /// <summary>
        /// N?n th?/card: Thêm m?t t?ng sáng h?n n?a
        /// Card/Panel background: #323232 (For cards, panels)
        /// </summary>
        public static Color BackgroundCard => Color.FromArgb(50, 50, 50);

        /// <summary>
        /// N?n khi hover: Sáng h?n m?t chút so v?i card
        /// Hover/Focus background: #3C3C3C (For hover states)
        /// </summary>
        public static Color BackgroundHover => Color.FromArgb(60, 60, 60);

        // ==================== ACCENT COLORS (MÀU CH???O) ====================
        /// <summary>
        /// Xanh d??ng Visual Studio (Primary Action Color)
        /// Used for: Buttons, Links, Primary CTA
        /// </summary>
        public static Color AccentPrimary => Color.FromArgb(0, 120, 212);      // #0078D4

        /// <summary>
        /// Xanh d??ng sáng h?n (Hover state)
        /// Used for: Accent Primary Hover
        /// </summary>
        public static Color AccentPrimaryLight => Color.FromArgb(0, 140, 230); // #008CE6

        // ==================== STATE COLORS (MÀU TR?NG THÁI) ====================
        /// <summary>
        /// Xanh lá: Thành công
        /// Green: Success state
        /// </summary>
        public static Color Success => Color.FromArgb(76, 175, 80);            // #4CAF50

        /// <summary>
        /// Cam: C?nh báo
        /// Orange: Warning state
        /// </summary>
        public static Color Warning => Color.FromArgb(255, 152, 0);            // #FF9800

        /// <summary>
        /// ??: Nguy hi?m/L?i
        /// Red: Danger/Error state
        /// </summary>
        public static Color Danger => Color.FromArgb(244, 67, 54);             // #F44336

        /// <summary>
        /// Xanh lam nh?t: Thông tin
        /// Light Blue: Info state
        /// </summary>
        public static Color Info => Color.FromArgb(33, 150, 243);              // #2196F3

        // ==================== TEXT COLORS (MÀU CH?) ====================
        /// <summary>
        /// Ch? tr?ng: Tiêu ?? chính
        /// White: Primary text (headings, important info)
        /// </summary>
        public static Color TextPrimary => Color.FromArgb(255, 255, 255);      // #FFFFFF

        /// <summary>
        /// Ch? xám nh?t: N?i dung ph?
        /// Light gray: Secondary text (descriptions, help text)
        /// </summary>
        public static Color TextSecondary => Color.FromArgb(204, 204, 204);    // #CCCCCC

        /// <summary>
        /// Ch? xám ??m: Hint text
        /// Dark gray: Tertiary text (placeholders, hints)
        /// </summary>
        public static Color TextTertiary => Color.FromArgb(150, 150, 150);     // #969696

        /// <summary>
        /// Ch? r?t xám: Disabled text
        /// Very dark gray: Disabled text
        /// </summary>
        public static Color TextDisabled => Color.FromArgb(100, 100, 100);     // #646464

        // ==================== BORDER COLORS (MÀU VI?N) ====================
        /// <summary>
        /// Vi?n nh?: Untuk subtle borders
        /// Light border: #3C3C3C
        /// </summary>
        public static Color BorderLight => Color.FromArgb(60, 60, 60);         // #3C3C3C

        /// <summary>
        /// Vi?n trung bình: Cho borders bình th??ng
        /// Medium border: #505050
        /// </summary>
        public static Color BorderMedium => Color.FromArgb(80, 80, 80);        // #505050

        /// <summary>
        /// Vi?n ??m: Cho focus, active states
        /// Dark border: #646464
        /// </summary>
        public static Color BorderDark => Color.FromArgb(100, 100, 100);       // #646464

        // ==================== SEMANTIC COLORS (MÀU THEO NG? C?NH) ====================
        /// <summary>
        /// Màu lo?i xe: Ô tô (Blue)
        /// Vehicle type color: Car
        /// </summary>
        public static Color VehicleCar => Color.FromArgb(33, 150, 243);        // #2196F3 (Blue)

        /// <summary>
        /// Màu lo?i xe: Xe máy (Purple)
        /// Vehicle type color: Motorcycle
        /// </summary>
        public static Color VehicleMotorcycle => Color.FromArgb(156, 39, 176); // #9C27B0 (Purple)

        /// <summary>
        /// Màu lo?i xe: Buýt (Amber)
        /// Vehicle type color: Bus
        /// </summary>
        public static Color VehicleBus => Color.FromArgb(255, 193, 7);         // #FFC107 (Amber)

        /// <summary>
        /// Màu lo?i xe: Xe ??p (Green)
        /// Vehicle type color: Bicycle
        /// </summary>
        public static Color VehicleBicycle => Color.FromArgb(76, 175, 80);     // #4CAF50 (Green)

        /// <summary>
        /// Màu lo?i xe: Xe t?i (Deep Orange)
        /// Vehicle type color: Truck
        /// </summary>
        public static Color VehicleTruck => Color.FromArgb(255, 87, 34);       // #FF5722 (Deep Orange)

        // ==================== UTILITY METHODS ====================
        /// <summary>
        /// L?y màu t? lo?i xe
        /// Get color based on vehicle type
        /// </summary>
        public static Color GetVehicleTypeColor(string vehicleType)
        {
            return vehicleType?.ToLower() switch
            {
                "car" => VehicleCar,
                "motorcycle" => VehicleMotorcycle,
                "bus" => VehicleBus,
                "bicycle" => VehicleBicycle,
                "truck" => VehicleTruck,
                _ => TextSecondary
            };
        }

        /// <summary>
        /// L?y màu t? tr?ng thái
        /// Get color based on status
        /// </summary>
        public static Color GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "success" or "completed" => Success,
                "warning" or "pending" => Warning,
                "danger" or "error" or "failed" => Danger,
                "info" => Info,
                _ => TextSecondary
            };
        }

        /// <summary>
        /// T?ng ?? sáng c?a màu
        /// Lighten a color by percentage
        /// </summary>
        public static Color Lighten(Color color, double percentage = 0.2)
        {
            int r = (int)(color.R + (255 - color.R) * percentage);
            int g = (int)(color.G + (255 - color.G) * percentage);
            int b = (int)(color.B + (255 - color.B) * percentage);
            return Color.FromArgb(
                Math.Min(255, r),
                Math.Min(255, g),
                Math.Min(255, b)
            );
        }

        /// <summary>
        /// Gi?m ?? sáng c?a màu
        /// Darken a color by percentage
        /// </summary>
        public static Color Darken(Color color, double percentage = 0.2)
        {
            int r = (int)(color.R * (1 - percentage));
            int g = (int)(color.G * (1 - percentage));
            int b = (int)(color.B * (1 - percentage));
            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// T?o màu v?i alpha (transparent)
        /// Create color with alpha transparency
        /// </summary>
        public static Color WithAlpha(Color color, int alpha)
        {
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, alpha)),
                color.R,
                color.G,
                color.B
            );
        }
    }

    /// <summary>
    /// UI constants for consistent sizing and spacing
    /// H?ng s? UI cho kích th??c và spacing nh?t quán
    /// </summary>
    public static class UIConstants
    {
        // ==================== BUTTON SIZES ====================
        /// <summary>
        /// Standard button width
        /// </summary>
        public const int ButtonWidthSmall = 100;
        
        public const int ButtonWidthMedium = 160;
        
        public const int ButtonWidthLarge = 220;

        /// <summary>
        /// Standard button height
        /// </summary>
        public const int ButtonHeightSmall = 35;
        
        public const int ButtonHeightMedium = 40;
        
        public const int ButtonHeightLarge = 50;

        // ==================== PADDING & MARGINS ====================
        /// <summary>
        /// Horizontal padding for controls
        /// </summary>
        public const int PaddingHorizontal = 10;

        /// <summary>
        /// Vertical padding for controls
        /// </summary>
        public const int PaddingVertical = 8;

        /// <summary>
        /// Standard spacing between controls
        /// </summary>
        public const int SpacingSmall = 5;
        
        public const int SpacingMedium = 10;
        
        public const int SpacingLarge = 15;

        // ==================== FONT SIZES ====================
        /// <summary>
        /// Font size for large headings
        /// </summary>
        public const float FontSizeHeading = 16F;

        /// <summary>
        /// Font size for normal text
        /// </summary>
        public const float FontSizeNormal = 10F;

        /// <summary>
        /// Font size for small text (help, hints)
        /// </summary>
        public const float FontSizeSmall = 8F;

        // ==================== HEIGHTS ====================
        /// <summary>
        /// Header panel height
        /// </summary>
        public const int HeaderHeight = 60;

        /// <summary>
        /// Statistics panel height
        /// </summary>
        public const int StatsHeight = 90;

        /// <summary>
        /// GroupBox title height
        /// </summary>
        public const int GroupBoxTitleHeight = 20;
    }
}
