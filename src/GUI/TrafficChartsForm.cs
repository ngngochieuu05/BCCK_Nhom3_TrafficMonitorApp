using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using LiveCharts;
using LiveCharts.Wpf;
using TrafficMonitorApp.Data;

namespace TrafficMonitorApp.GUI
{
    /// <summary>
    /// Form hiển thị biểu đồ thống kê giao thông
    /// Traffic statistics charts form
    /// </summary>
    public class TrafficChartsForm : Form
    {
        private readonly TrafficDbContext _dbContext;
        
        private TabControl tabCharts = null!;
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private Button btnRefresh = null!;
        
        // Charts
        private CartesianChart chartTrendLine = null!;
        private CartesianChart chartVehicleTypes = null!;
        private PieChart chartPieDistribution = null!;
        private CartesianChart chartHourlyHeatmap = null!;
        
        // ElementHosts for WPF charts
        private ElementHost hostTrend = null!;
        private ElementHost hostTypes = null!;
        private ElementHost hostPie = null!;
        private ElementHost hostHeatmap = null!;
        
        public TrafficChartsForm(TrafficDbContext dbContext)
        {
            _dbContext = dbContext;
            InitializeComponent();
            LoadCharts();
        }
        
        private void InitializeComponent()
        {
            this.Text = "📊 Biểu Đồ Thống Kê Giao Thông";
            this.Size = new Size(1600, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 248);
            this.MinimumSize = new Size(1200, 700);
            this.Font = new Font("Segoe UI", 9.5F);
            
            // Filter Panel
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(30, 20, 30, 20)
            };
            
            var lblFrom = new Label
            {
                Text = "📅 Từ ngày:",
                Location = new Point(20, 25),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White
            };
            
            dtpFrom = new DateTimePicker
            {
                Location = new Point(120, 22),
                Width = 180,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddDays(-30),
                Font = new Font("Segoe UI", 10)
            };
            
            var lblTo = new Label
            {
                Text = "📅 Đến ngày:",
                Location = new Point(330, 25),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White
            };
            
            dtpTo = new DateTimePicker
            {
                Location = new Point(440, 22),
                Width = 180,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10)
            };
            
            btnRefresh = new Button
            {
                Text = "🔄 Làm Mới Dữ Liệu",
                Location = new Point(650, 18),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(39, 174, 96);
            btnRefresh.Click += (s, e) => LoadCharts();
            
            pnlFilter.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, btnRefresh });
            
            // Tab Control
            tabCharts = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                Padding = new Point(20, 5)
            };
            
            // Tab 1: Trend Line
            var tabTrend = new TabPage("📈 Xu Hướng Theo Thời Gian");
            chartTrendLine = new CartesianChart { Background = System.Windows.Media.Brushes.White };
            hostTrend = new ElementHost { Dock = DockStyle.Fill, Child = chartTrendLine };
            tabTrend.Controls.Add(hostTrend);
            tabCharts.TabPages.Add(tabTrend);
            
            // Tab 2: Vehicle Types Comparison
            var tabVehicleTypes = new TabPage("🚗 So Sánh Loại Xe");
            chartVehicleTypes = new CartesianChart { Background = System.Windows.Media.Brushes.White };
            hostTypes = new ElementHost { Dock = DockStyle.Fill, Child = chartVehicleTypes };
            tabVehicleTypes.Controls.Add(hostTypes);
            tabCharts.TabPages.Add(tabVehicleTypes);
            
            // Tab 3: Pie Chart Distribution
            var tabPie = new TabPage("🥧 Phân Bố Phần Trăm");
            chartPieDistribution = new PieChart
            {
                Background = System.Windows.Media.Brushes.White,
                LegendLocation = LegendLocation.Right
            };
            hostPie = new ElementHost { Dock = DockStyle.Fill, Child = chartPieDistribution };
            tabPie.Controls.Add(hostPie);
            tabCharts.TabPages.Add(tabPie);
            
            // Tab 4: Heatmap
            var tabHeatmap = new TabPage("🔥 Mật Độ Theo Giờ");
            chartHourlyHeatmap = new CartesianChart { Background = System.Windows.Media.Brushes.White };
            hostHeatmap = new ElementHost { Dock = DockStyle.Fill, Child = chartHourlyHeatmap };
            tabHeatmap.Controls.Add(hostHeatmap);
            tabCharts.TabPages.Add(tabHeatmap);
            
            this.Controls.AddRange(new Control[] { tabCharts, pnlFilter });
        }
        
        private void LoadCharts()
        {
            try
            {
                LoadTrendLineChart();
                LoadVehicleTypesChart();
                LoadPieChart();
                LoadHourlyHeatmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải biểu đồ: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadTrendLineChart()
        {
            var fromDate = dtpFrom.Value.Date;
            var toDate = dtpTo.Value.Date.AddDays(1);
            
            var dailyData = _dbContext.TrafficSessions
                .Where(s => s.StartTime >= fromDate && s.StartTime < toDate)
                .GroupBy(s => s.StartTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalVehicles = g.Sum(s => s.TotalVehicles)
                })
                .OrderBy(d => d.Date)
                .ToList();
            
            if (dailyData.Count == 0)
            {
                chartTrendLine.Series = new SeriesCollection();
                return;
            }
            
            var values = new ChartValues<int>(dailyData.Select(d => d.TotalVehicles));
            var labels = dailyData.Select(d => d.Date.ToString("dd/MM")).ToArray();
            
            chartTrendLine.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Số xe theo ngày",
                    Values = values,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 8,
                    StrokeThickness = 3,
                    Fill = System.Windows.Media.Brushes.LightBlue,
                    Stroke = System.Windows.Media.Brushes.DodgerBlue
                }
            };
            
            chartTrendLine.AxisX.Clear();
            chartTrendLine.AxisX.Add(new Axis
            {
                Title = "Ngày",
                Labels = labels,
                Separator = new Separator { Step = 1 }
            });
            
            chartTrendLine.AxisY.Clear();
            chartTrendLine.AxisY.Add(new Axis
            {
                Title = "Số lượng xe",
                LabelFormatter = value => value.ToString("N0"),
                MinValue = 0
            });
            
            chartTrendLine.LegendLocation = LegendLocation.Top;
        }
        
        private void LoadVehicleTypesChart()
        {
            var fromDate = dtpFrom.Value.Date;
            var toDate = dtpTo.Value.Date.AddDays(1);
            
            var vehicleData = _dbContext.VehicleDetections
                .Where(d => d.DetectedTime >= fromDate && d.DetectedTime < toDate)
                .GroupBy(d => d.VehicleType)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(v => v.Count)
                .ToList();
            
            if (vehicleData.Count == 0)
            {
                chartVehicleTypes.Series = new SeriesCollection();
                return;
            }
            
            var values = new ChartValues<int>(vehicleData.Select(v => v.Count));
            var labels = vehicleData.Select(v => v.Type).ToArray();
            
            chartVehicleTypes.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Số lượng",
                    Values = values,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("N0"),
                    MaxColumnWidth = 60
                }
            };
            
            chartVehicleTypes.AxisX.Clear();
            chartVehicleTypes.AxisX.Add(new Axis
            {
                Title = "Loại xe",
                Labels = labels,
                Separator = new Separator { Step = 1 }
            });
            
            chartVehicleTypes.AxisY.Clear();
            chartVehicleTypes.AxisY.Add(new Axis
            {
                Title = "Số lượng",
                LabelFormatter = value => value.ToString("N0"),
                MinValue = 0
            });
            
            chartVehicleTypes.LegendLocation = LegendLocation.None;
        }
        
        private void LoadPieChart()
        {
            var fromDate = dtpFrom.Value.Date;
            var toDate = dtpTo.Value.Date.AddDays(1);
            
            var vehicleData = _dbContext.VehicleDetections
                .Where(d => d.DetectedTime >= fromDate && d.DetectedTime < toDate)
                .GroupBy(d => d.VehicleType)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToList();
            
            if (vehicleData.Count == 0)
            {
                chartPieDistribution.Series = new SeriesCollection();
                return;
            }
            
            var total = vehicleData.Sum(v => v.Count);
            
            var series = new SeriesCollection();
            foreach (var item in vehicleData)
            {
                var percentage = (double)item.Count / total * 100;
                series.Add(new PieSeries
                {
                    Title = item.Type,
                    Values = new ChartValues<int> { item.Count },
                    DataLabels = true,
                    LabelPoint = point => $"{item.Count:N0} ({percentage:F1}%)"
                });
            }
            
            chartPieDistribution.Series = series;
            chartPieDistribution.LegendLocation = LegendLocation.Right;
        }
        
        private void LoadHourlyHeatmap()
        {
            var fromDate = dtpFrom.Value.Date;
            var toDate = dtpTo.Value.Date.AddDays(1);
            
            var hourlyData = _dbContext.VehicleDetections
                .Where(d => d.DetectedTime >= fromDate && d.DetectedTime < toDate)
                .GroupBy(d => d.DetectedTime.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderBy(h => h.Hour)
                .ToList();
            
            if (hourlyData.Count == 0)
            {
                chartHourlyHeatmap.Series = new SeriesCollection();
                return;
            }
            
            var values = new ChartValues<int>(hourlyData.Select(h => h.Count));
            var labels = hourlyData.Select(h => $"{h.Hour:00}:00").ToArray();
            
            chartHourlyHeatmap.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Mật độ xe",
                    Values = values,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("N0"),
                    Fill = System.Windows.Media.Brushes.OrangeRed
                }
            };
            
            chartHourlyHeatmap.AxisX.Clear();
            chartHourlyHeatmap.AxisX.Add(new Axis
            {
                Title = "Giờ trong ngày",
                Labels = labels,
                Separator = new Separator { Step = 1 }
            });
            
            chartHourlyHeatmap.AxisY.Clear();
            chartHourlyHeatmap.AxisY.Add(new Axis
            {
                Title = "Số lượng xe",
                LabelFormatter = value => value.ToString("N0"),
                MinValue = 0
            });
            
            chartHourlyHeatmap.LegendLocation = LegendLocation.Top;
        }
    }
}
