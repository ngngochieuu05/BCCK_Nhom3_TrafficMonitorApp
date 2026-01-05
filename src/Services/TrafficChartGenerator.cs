using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace TrafficMonitorApp
{
    /// <summary>
    /// Tạo biểu đồ lưu lượng xe theo thời gian
    /// </summary>
    public class TrafficChartGenerator
    {
        private readonly int _chartWidth;
        private readonly int _chartHeight;
        private readonly Color _backgroundColor;
        private readonly Color _gridColor;
        private readonly Color _textColor;
        private readonly Font _titleFont;
        private readonly Font _labelFont;

        public TrafficChartGenerator(int width = 1200, int height = 800)
        {
            _chartWidth = width;
            _chartHeight = height;
            _backgroundColor = Color.FromArgb(32, 33, 36);
            _gridColor = Color.FromArgb(60, 63, 68);
            _textColor = Color.FromArgb(225, 228, 232);
            _titleFont = new Font("Segoe UI", 18, FontStyle.Bold);
            _labelFont = new Font("Segoe UI", 10);
        }

        /// <summary>
        /// Vẽ biểu đồ cột theo loại xe
        /// </summary>
        public Bitmap GenerateVehicleTypeBarChart(TrafficStatistics statistics, string title = "THỐNG KÊ PHƯƠNG TIỆN")
        {
            var bitmap = new Bitmap(_chartWidth, _chartHeight);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Background
                g.Clear(_backgroundColor);

                // Title
                var titleSize = g.MeasureString(title, _titleFont);
                g.DrawString(title, _titleFont, new SolidBrush(_textColor),
                    (_chartWidth - titleSize.Width) / 2, 30);

                // Info panel
                string info = $"Tổng số xe: {statistics.TotalVehicles} | " +
                             $"Thời gian: {statistics.StartTime:HH:mm:ss} - {statistics.EndTime:HH:mm:ss} | " +
                             $"FPS: {statistics.AverageFPS:F1}";
                g.DrawString(info, _labelFont, new SolidBrush(_textColor), 50, 80);

                if (statistics.VehicleCounts.Count == 0)
                {
                    g.DrawString("Không có dữ liệu", _titleFont, new SolidBrush(_textColor),
                        _chartWidth / 2 - 100, _chartHeight / 2);
                    return bitmap;
                }

                // Chart area
                int chartLeft = 150;
                int chartTop = 150;
                int chartWidth = _chartWidth - 200;
                int chartHeight = _chartHeight - 250;

                // Get max count for scaling
                int maxCount = statistics.VehicleCounts.Values.Max();
                if (maxCount == 0) maxCount = 1;

                // Draw grid
                DrawGrid(g, chartLeft, chartTop, chartWidth, chartHeight, maxCount);

                // Draw bars
                var sortedCounts = statistics.VehicleCounts.OrderByDescending(x => x.Value).ToList();
                int barCount = sortedCounts.Count;
                int barWidth = Math.Min(120, (chartWidth - 50) / barCount);
                int spacing = Math.Max(20, (chartWidth - barWidth * barCount) / (barCount + 1));

                for (int i = 0; i < sortedCounts.Count; i++)
                {
                    var kvp = sortedCounts[i];
                    int x = chartLeft + spacing + i * (barWidth + spacing);
                    int barHeight = (int)((double)kvp.Value / maxCount * chartHeight);
                    int y = chartTop + chartHeight - barHeight;

                    // Get color for vehicle type
                    Color barColor = GetVehicleColor(kvp.Key);

                    // Draw bar with gradient
                    using (var brush = new LinearGradientBrush(
                        new Rectangle(x, y, barWidth, barHeight),
                        barColor,
                        Color.FromArgb(barColor.A / 2, barColor.R, barColor.G, barColor.B),
                        LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(brush, x, y, barWidth, barHeight);
                    }

                    // Border
                    g.DrawRectangle(new Pen(Color.White, 2), x, y, barWidth, barHeight);

                    // Value on top
                    string valueText = kvp.Value.ToString();
                    var valueSize = g.MeasureString(valueText, _labelFont);
                    g.DrawString(valueText, _labelFont, new SolidBrush(_textColor),
                        x + (barWidth - valueSize.Width) / 2, y - 25);

                    // Percentage
                    double percentage = (double)kvp.Value / statistics.TotalVehicles * 100;
                    string percentText = $"{percentage:F1}%";
                    var percentSize = g.MeasureString(percentText, _labelFont);
                    g.DrawString(percentText, new Font("Segoe UI", 9), new SolidBrush(Color.LightGray),
                        x + (barWidth - percentSize.Width) / 2, y - 45);

                    // Label
                    string displayName = GetDisplayName(kvp.Key);
                    var labelSize = g.MeasureString(displayName, _labelFont);
                    
                    // Rotate label if needed
                    if (barWidth < 80)
                    {
                        g.TranslateTransform(x + barWidth / 2, chartTop + chartHeight + 20);
                        g.RotateTransform(-45);
                        g.DrawString(displayName, _labelFont, new SolidBrush(_textColor), 0, 0);
                        g.ResetTransform();
                    }
                    else
                    {
                        g.DrawString(displayName, _labelFont, new SolidBrush(_textColor),
                            x + (barWidth - labelSize.Width) / 2, chartTop + chartHeight + 15);
                    }
                }

                // Legend
                DrawLegend(g, sortedCounts, 50, _chartHeight - 80);
            }

            return bitmap;
        }

        /// <summary>
        /// Vẽ biểu đồ đường theo thời gian (hourly trend)
        /// </summary>
        public Bitmap GenerateHourlyTrendChart(AdvancedStatistics advStats, string title = "LƯU LƯỢNG XE THEO GIỜ")
        {
            var bitmap = new Bitmap(_chartWidth, _chartHeight);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Background
                g.Clear(_backgroundColor);

                // Title
                var titleSize = g.MeasureString(title, _titleFont);
                g.DrawString(title, _titleFont, new SolidBrush(_textColor),
                    (_chartWidth - titleSize.Width) / 2, 30);

                // Get hourly data
                var hourlyData = advStats.GetHourlyData();
                if (hourlyData.Count == 0)
                {
                    g.DrawString("Không có dữ liệu theo giờ", _titleFont, new SolidBrush(_textColor),
                        _chartWidth / 2 - 150, _chartHeight / 2);
                    return bitmap;
                }

                // Chart area
                int chartLeft = 100;
                int chartTop = 120;
                int chartWidth = _chartWidth - 150;
                int chartHeight = _chartHeight - 200;

                // Get max count
                int maxCount = hourlyData.Values.Max();
                if (maxCount == 0) maxCount = 1;

                // Draw grid
                DrawGrid(g, chartLeft, chartTop, chartWidth, chartHeight, maxCount);

                // Draw line chart
                var sortedHours = hourlyData.OrderBy(x => int.Parse(x.Key)).ToList();
                var points = new List<PointF>();

                for (int i = 0; i < sortedHours.Count; i++)
                {
                    var kvp = sortedHours[i];
                    float x = chartLeft + (float)i / (sortedHours.Count - 1) * chartWidth;
                    float y = chartTop + chartHeight - (float)kvp.Value / maxCount * chartHeight;
                    points.Add(new PointF(x, y));
                }

                // Draw area under line
                if (points.Count > 1)
                {
                    var areaPoints = new List<PointF>(points);
                    areaPoints.Add(new PointF(points[points.Count - 1].X, chartTop + chartHeight));
                    areaPoints.Add(new PointF(points[0].X, chartTop + chartHeight));

                    using (var areaBrush = new LinearGradientBrush(
                        new RectangleF(chartLeft, chartTop, chartWidth, chartHeight),
                        Color.FromArgb(100, 66, 135, 245),
                        Color.FromArgb(20, 66, 135, 245),
                        LinearGradientMode.Vertical))
                    {
                        g.FillPolygon(areaBrush, areaPoints.ToArray());
                    }

                    // Draw line
                    g.DrawLines(new Pen(Color.FromArgb(66, 135, 245), 3), points.ToArray());

                    // Draw points
                    foreach (var point in points)
                    {
                        g.FillEllipse(Brushes.White, point.X - 5, point.Y - 5, 10, 10);
                        g.DrawEllipse(new Pen(Color.FromArgb(66, 135, 245), 2), point.X - 5, point.Y - 5, 10, 10);
                    }

                    // Draw values
                    for (int i = 0; i < points.Count; i++)
                    {
                        var point = points[i];
                        var hour = sortedHours[i];
                        
                        // Value
                        string valueText = hour.Value.ToString();
                        var valueSize = g.MeasureString(valueText, _labelFont);
                        g.DrawString(valueText, _labelFont, new SolidBrush(_textColor),
                            point.X - valueSize.Width / 2, point.Y - 25);

                        // Hour label
                        string hourLabel = $"{hour.Key}:00";
                        var hourSize = g.MeasureString(hourLabel, _labelFont);
                        g.DrawString(hourLabel, _labelFont, new SolidBrush(_textColor),
                            point.X - hourSize.Width / 2, chartTop + chartHeight + 10);
                    }
                }

                // Statistics info
                DrawStatisticsInfo(g, advStats);
            }

            return bitmap;
        }

        /// <summary>
        /// Vẽ biểu đồ tròn (pie chart) phân bố loại xe
        /// </summary>
        public Bitmap GeneratePieChart(TrafficStatistics statistics, string title = "PHÂN BỐ LOẠI PHƯƠNG TIỆN")
        {
            var bitmap = new Bitmap(_chartWidth, _chartHeight);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Background
                g.Clear(_backgroundColor);

                // Title
                var titleSize = g.MeasureString(title, _titleFont);
                g.DrawString(title, _titleFont, new SolidBrush(_textColor),
                    (_chartWidth - titleSize.Width) / 2, 30);

                if (statistics.TotalVehicles == 0)
                {
                    g.DrawString("Không có dữ liệu", _titleFont, new SolidBrush(_textColor),
                        _chartWidth / 2 - 100, _chartHeight / 2);
                    return bitmap;
                }

                // Pie chart position
                int centerX = _chartWidth / 2 - 150;
                int centerY = _chartHeight / 2;
                int radius = Math.Min(_chartWidth, _chartHeight) / 3;

                Rectangle pieRect = new Rectangle(centerX - radius, centerY - radius, radius * 2, radius * 2);

                // Draw pie slices
                float startAngle = 0;
                var sortedCounts = statistics.VehicleCounts.OrderByDescending(x => x.Value).ToList();

                foreach (var kvp in sortedCounts)
                {
                    float sweepAngle = (float)kvp.Value / statistics.TotalVehicles * 360;
                    Color sliceColor = GetVehicleColor(kvp.Key);

                    using (var brush = new SolidBrush(sliceColor))
                    {
                        g.FillPie(brush, pieRect, startAngle, sweepAngle);
                    }

                    // Border
                    g.DrawPie(new Pen(Color.White, 2), pieRect, startAngle, sweepAngle);

                    // Label on slice
                    double angle = (startAngle + sweepAngle / 2) * Math.PI / 180;
                    int labelX = (int)(centerX + Math.Cos(angle) * radius * 0.7);
                    int labelY = (int)(centerY + Math.Sin(angle) * radius * 0.7);

                    double percentage = (double)kvp.Value / statistics.TotalVehicles * 100;
                    string label = $"{percentage:F1}%";
                    var labelSize = g.MeasureString(label, _labelFont);
                    
                    // Draw label with background
                    var labelRect = new RectangleF(labelX - labelSize.Width / 2 - 5, 
                                                   labelY - labelSize.Height / 2 - 2,
                                                   labelSize.Width + 10, labelSize.Height + 4);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(200, 32, 33, 36)), labelRect);
                    g.DrawString(label, _labelFont, new SolidBrush(Color.White),
                        labelX - labelSize.Width / 2, labelY - labelSize.Height / 2);

                    startAngle += sweepAngle;
                }

                // Legend on the right
                int legendX = _chartWidth / 2 + 200;
                int legendY = 150;
                int legendItemHeight = 40;

                foreach (var kvp in sortedCounts)
                {
                    Color color = GetVehicleColor(kvp.Key);
                    
                    // Color box
                    g.FillRectangle(new SolidBrush(color), legendX, legendY, 30, 25);
                    g.DrawRectangle(new Pen(Color.White, 2), legendX, legendY, 30, 25);

                    // Text
                    string displayName = GetDisplayName(kvp.Key);
                    string legendText = $"{displayName}: {kvp.Value}";
                    g.DrawString(legendText, _labelFont, new SolidBrush(_textColor), legendX + 40, legendY + 2);

                    legendY += legendItemHeight;
                }

                // Total at center
                string totalText = statistics.TotalVehicles.ToString();
                var totalSize = g.MeasureString(totalText, new Font("Segoe UI", 24, FontStyle.Bold));
                g.DrawString(totalText, new Font("Segoe UI", 24, FontStyle.Bold), 
                    new SolidBrush(_textColor), centerX - totalSize.Width / 2, centerY - totalSize.Height / 2);
                
                string totalLabel = "Tổng số xe";
                var totalLabelSize = g.MeasureString(totalLabel, _labelFont);
                g.DrawString(totalLabel, _labelFont, new SolidBrush(Color.LightGray),
                    centerX - totalLabelSize.Width / 2, centerY + totalSize.Height / 2);
            }

            return bitmap;
        }

        private void DrawGrid(Graphics g, int left, int top, int width, int height, int maxValue)
        {
            // Vertical grid lines
            for (int i = 0; i <= 10; i++)
            {
                int x = left + i * width / 10;
                g.DrawLine(new Pen(_gridColor, 1), x, top, x, top + height);
            }

            // Horizontal grid lines and labels
            for (int i = 0; i <= 5; i++)
            {
                int y = top + i * height / 5;
                g.DrawLine(new Pen(_gridColor, 1), left, y, left + width, y);

                // Y-axis labels
                int value = maxValue - (i * maxValue / 5);
                g.DrawString(value.ToString(), _labelFont, new SolidBrush(_textColor), left - 50, y - 8);
            }

            // Axes
            g.DrawLine(new Pen(_textColor, 2), left, top, left, top + height);
            g.DrawLine(new Pen(_textColor, 2), left, top + height, left + width, top + height);
        }

        private void DrawLegend(Graphics g, List<KeyValuePair<string, int>> items, int x, int y)
        {
            int boxSize = 20;
            int spacing = 150;
            int currentX = x;

            foreach (var item in items)
            {
                Color color = GetVehicleColor(item.Key);
                g.FillRectangle(new SolidBrush(color), currentX, y, boxSize, boxSize);
                g.DrawRectangle(new Pen(Color.White, 1), currentX, y, boxSize, boxSize);

                string text = GetDisplayName(item.Key);
                g.DrawString(text, _labelFont, new SolidBrush(_textColor), currentX + boxSize + 8, y + 2);

                currentX += spacing;
                if (currentX > _chartWidth - 100)
                {
                    currentX = x;
                    y += 30;
                }
            }
        }

        private void DrawStatisticsInfo(Graphics g, AdvancedStatistics stats)
        {
            int infoX = 50;
            int infoY = 80;
            var infoFont = new Font("Segoe UI", 10);

            string[] infos = new[]
            {
                $"📊 Tổng phương tiện: {stats.GetTotalVehicles()}",
                $"⚡ Lưu lượng: {stats.GetVehiclesPerMinute():F1} xe/phút",
                $"🚦 Mức độ tắc nghẽn: {stats.GetCongestionLevel():F1}%",
                $"⏰ Giờ cao điểm: {stats.GetBusiestPeriod()}"
            };

            foreach (var info in infos)
            {
                g.DrawString(info, infoFont, new SolidBrush(_textColor), infoX, infoY);
                infoY += 25;
            }
        }

        private Color GetVehicleColor(string vehicleType)
        {
            return vehicleType.ToLower() switch
            {
                "car" => Color.FromArgb(94, 92, 230),
                "motorcycle" => Color.FromArgb(66, 135, 245),
                "bus" => Color.FromArgb(255, 159, 10),
                "bicycle" => Color.FromArgb(48, 209, 88),
                "truck" => Color.FromArgb(255, 69, 58),
                _ => Color.FromArgb(138, 180, 248)
            };
        }

        private string GetDisplayName(string vehicleType)
        {
            return vehicleType.ToLower() switch
            {
                "car" => "Ô tô",
                "motorcycle" => "Xe máy",
                "bus" => "Xe buýt",
                "bicycle" => "Xe đạp",
                "truck" => "Xe tải",
                _ => vehicleType
            };
        }

        /// <summary>
        /// Lưu biểu đồ ra file
        /// </summary>
        public void SaveChart(Bitmap chart, string filePath)
        {
            string? directory = System.IO.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            chart.Save(filePath, ImageFormat.Png);
        }

        /// <summary>
        /// Tạo tất cả các biểu đồ và lưu vào thư mục
        /// </summary>
        public void GenerateAllCharts(TrafficStatistics statistics, AdvancedStatistics advStats, string outputFolder)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            // Bar chart
            using (var barChart = GenerateVehicleTypeBarChart(statistics))
            {
                SaveChart(barChart, System.IO.Path.Combine(outputFolder, $"bar_chart_{timestamp}.png"));
            }

            // Pie chart
            using (var pieChart = GeneratePieChart(statistics))
            {
                SaveChart(pieChart, System.IO.Path.Combine(outputFolder, $"pie_chart_{timestamp}.png"));
            }

            // Hourly trend (if data available)
            if (advStats.GetTotalVehicles() > 0)
            {
                using (var trendChart = GenerateHourlyTrendChart(advStats))
                {
                    SaveChart(trendChart, System.IO.Path.Combine(outputFolder, $"trend_chart_{timestamp}.png"));
                }
            }
        }
    }
}
