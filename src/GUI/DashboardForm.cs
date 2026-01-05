using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using LiveCharts;
using LiveCharts.Wpf;
using TrafficMonitorApp.Data;
using TrafficMonitorApp.Services;
using TrafficMonitorApp.Utils;

namespace TrafficMonitorApp.GUI
{
    /// <summary>
    /// Dashboard tổng quan hệ thống
    /// Main dashboard showing system overview
    /// </summary>
    public class DashboardForm : Form
    {
        private readonly TrafficDbContext _dbContext;
        private readonly AuthenticationService _authService;
        private bool _isDisposed = false;
        
        // Statistics Cards
        private Panel pnlTotalSessions = null!;
        private Panel pnlTotalVehicles = null!;
        private Panel pnlTotalUsers = null!;
        private Panel pnlUptime = null!;
        
        // Labels
        private Label lblTotalSessionsValue = null!;
        private Label lblTotalVehiclesValue = null!;
        private Label lblTotalUsersValue = null!;
        private Label lblUptimeValue = null!;
        
        // Charts
        private CartesianChart chartWeeklyTrend = null!;
        private ElementHost chartHost = null!;
        
        // Quick Actions
        private Button btnStartMonitoring = null!;
        private Button btnViewReports = null!;
        private Button btnSettings = null!;
        private Button btnManageData = null!;
        
        // Recent Activity
        private DataGridView dgvRecentSessions = null!;
        
        public DashboardForm(TrafficDbContext dbContext, AuthenticationService authService)
        {
            _dbContext = dbContext;
            _authService = authService;
            
            InitializeComponent();
            LoadDashboardData();
        }
        
        /// <summary>
        /// Kiểm tra và đảm bảo kết nối database sẵn sàng
        /// </summary>
        private bool EnsureDatabaseConnection()
        {
            if (_dbContext == null || _isDisposed)
            {
                return false;
            }

            try
            {
                // Kiểm tra kết nối bằng cách thử query đơn giản
                _ = _dbContext.Database.CanConnect();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DashboardForm] Database connection error: {ex.Message}");
                return false;
            }
        }
        
        private void InitializeComponent()
        {
            // Form settings
            this.Text = "🏠 Dashboard - Tổng Quan Hệ Thống";
            this.Size = new Size(1600, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColorScheme.Background;
            this.Font = new Font("Segoe UI", 10);
            
            // Subscribe to theme changes
            ColorScheme.ThemeChanged += (s, e) => ColorScheme.ApplyTheme(this);
            this.MinimumSize = new Size(1400, 800);
            
            // Header
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = ColorScheme.BackgroundDark,
                Padding = new Padding(30, 20, 30, 20)
            };
            
            var lblHeader = new Label
            {
                Text = $"Chào mừng, {_authService.CurrentUser?.FullName ?? "User"}!",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };
            
            var lblSubHeader = new Label
            {
                Text = $"Role: {_authService.CurrentUser?.Role} | {DateTime.Now:dddd, dd/MM/yyyy HH:mm}",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(200, 220, 240),
                AutoSize = true,
                Location = new Point(20, 48)
            };
            
            pnlHeader.Controls.AddRange(new Control[] { lblHeader, lblSubHeader });
            
            // Main Container
            var pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 20, 30, 20),
                AutoScroll = true,
                BackColor = Color.FromArgb(30, 30, 30)
            };
            
            int yPos = 20;
            
            // Statistics Cards Row
            CreateStatisticsCards(pnlMain, ref yPos);
            
            // Charts Section
            CreateChartsSection(pnlMain, ref yPos);
            
            // Quick Actions
            CreateQuickActions(pnlMain, ref yPos);
            
            // Recent Activity
            CreateRecentActivity(pnlMain, ref yPos);
            
            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlHeader);
        }
        
        private void CreateStatisticsCards(Panel parent, ref int yPos)
        {
            int cardWidth = 280;
            int cardHeight = 110;
            int spacing = 25;
            int xPos = 10;
            
            // Card 1: Total Sessions
            pnlTotalSessions = CreateStatCard(
                "📊 Tổng Phiên Giám Sát",
                "0",
                Color.FromArgb(52, 152, 219),
                xPos, yPos, cardWidth, cardHeight
            );
            lblTotalSessionsValue = (Label)pnlTotalSessions.Controls[1];
            parent.Controls.Add(pnlTotalSessions);
            
            xPos += cardWidth + spacing;
            
            // Card 2: Total Vehicles
            pnlTotalVehicles = CreateStatCard(
                "🚗 Tổng Xe Phát Hiện",
                "0",
                Color.FromArgb(46, 204, 113),
                xPos, yPos, cardWidth, cardHeight
            );
            lblTotalVehiclesValue = (Label)pnlTotalVehicles.Controls[1];
            parent.Controls.Add(pnlTotalVehicles);
            
            xPos += cardWidth + spacing;
            
            // Card 3: Total Users
            pnlTotalUsers = CreateStatCard(
                "👥 Người Dùng",
                "0",
                Color.FromArgb(155, 89, 182),
                xPos, yPos, cardWidth, cardHeight
            );
            lblTotalUsersValue = (Label)pnlTotalUsers.Controls[1];
            parent.Controls.Add(pnlTotalUsers);
            
            xPos += cardWidth + spacing;
            
            // Card 4: Uptime
            pnlUptime = CreateStatCard(
                "⏱️ Hoạt Động",
                "100%",
                Color.FromArgb(241, 196, 15),
                xPos, yPos, cardWidth, cardHeight
            );
            lblUptimeValue = (Label)pnlUptime.Controls[1];
            parent.Controls.Add(pnlUptime);
            
            yPos += cardHeight + 40;
        }
        
        private Panel CreateStatCard(string title, string value, Color color, int x, int y, int width, int height)
        {
            var panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.FromArgb(44, 62, 80),
                BorderStyle = BorderStyle.None
            };
            
            // Add shadow effect simulation
            panel.Paint += (s, e) =>
            {
                var rect = panel.ClientRectangle;
                rect.Inflate(-1, -1);
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };
            
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(15, 12),
                AutoSize = true
            };
            
            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 40),
                AutoSize = true
            };
            
            panel.Controls.AddRange(new Control[] { lblTitle, lblValue });
            
            return panel;
        }
        
        private void CreateChartsSection(Panel parent, ref int yPos)
        {
            var lblSectionTitle = new Label
            {
                Text = "📈 Xu Hướng 7 Ngày Gần Nhất",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(0, yPos),
                AutoSize = true
            };
            parent.Controls.Add(lblSectionTitle);
            
            yPos += 35;
            
            chartWeeklyTrend = new CartesianChart
            {
                Background = System.Windows.Media.Brushes.White,
                DisableAnimations = false
            };

            chartHost = new ElementHost
            {
                Location = new Point(10, yPos),
                Size = new Size(1500, 250),
                Child = chartWeeklyTrend,
                BackColor = Color.White
            };
            
            parent.Controls.Add(chartHost);
            
            yPos += 290;
        }
        
        private void CreateQuickActions(Panel parent, ref int yPos)
        {
            var lblSectionTitle = new Label
            {
                Text = "🎯 Thao Tác Nhanh",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(0, yPos),
                AutoSize = true
            };
            parent.Controls.Add(lblSectionTitle);
            
            yPos += 35;
            
            int btnWidth = 220;
            int btnHeight = 55;
            int spacing = 25;
            int xPos = 10;
            
            btnStartMonitoring = new Button
            {
                Text = "▶️ Bắt Đầu Giám Sát",
                Location = new Point(xPos, yPos),
                Size = new Size(btnWidth, btnHeight),
                BackColor = ColorScheme.Success,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStartMonitoring.FlatAppearance.BorderSize = 0;
            btnStartMonitoring.Click += (s, e) => OpenMainForm();
            parent.Controls.Add(btnStartMonitoring);
            
            xPos += btnWidth + spacing;
            
            btnViewReports = new Button
            {
                Text = "📊 Xem Báo Cáo",
                Location = new Point(xPos, yPos),
                Size = new Size(btnWidth, btnHeight),
                BackColor = ColorScheme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnViewReports.FlatAppearance.BorderSize = 0;
            btnViewReports.Click += (s, e) => OpenChartsForm();
            parent.Controls.Add(btnViewReports);
            
            xPos += btnWidth + spacing;
            
            btnSettings = new Button
            {
                Text = "⚙️ Cài Đặt",
                Location = new Point(xPos, yPos),
                Size = new Size(btnWidth, btnHeight),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.Click += (s, e) => OpenSettings();
            parent.Controls.Add(btnSettings);
            
            xPos += btnWidth + spacing;
            
            if (_authService.IsAdmin())
            {
                btnManageData = new Button
                {
                    Text = "📋 Quản Lý Dữ Liệu",
                    Location = new Point(xPos, yPos),
                    Size = new Size(btnWidth, btnHeight),
                    BackColor = Color.FromArgb(142, 68, 173),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnManageData.FlatAppearance.BorderSize = 0;
                btnManageData.Click += (s, e) => OpenAdminDashboard();
                parent.Controls.Add(btnManageData);
            }
            
            yPos += btnHeight + 40;
        }
        
        private void CreateRecentActivity(Panel parent, ref int yPos)
        {
            var lblSectionTitle = new Label
            {
                Text = "📋 Hoạt Động Gần Đây",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(0, yPos),
                AutoSize = true
            };
            parent.Controls.Add(lblSectionTitle);
            
            yPos += 35;
            
            dgvRecentSessions = new DataGridView
            {
                Location = new Point(10, yPos),
                Size = new Size(1500, 180),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 10),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Padding = new Padding(5)
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(245, 247, 250)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(41, 128, 185),
                    SelectionForeColor = Color.White,
                    Padding = new Padding(3)
                },
                RowTemplate = { Height = 35 },
                EnableHeadersVisualStyles = false
            };
            
            parent.Controls.Add(dgvRecentSessions);
            
            yPos += 220;
        }
        
        private void LoadDashboardData()
        {
            try
            {
                // Kiểm tra DbContext có sẵn sàng không
                if (!EnsureDatabaseConnection())
                {
                    MessageBox.Show("Lỗi: Không thể kết nối database.\n\nVui lòng kiểm tra:\n" +
                        "1. SQL Server đang chạy\n" +
                        "2. Database 'QuanLyGiaoThong' đã được tạo\n" +
                        "3. Connection string đúng", 
                        "Lỗi Kết Nối Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Load statistics
                var totalSessions = _dbContext.TrafficSessions.Count();
                var totalVehicles = _dbContext.VehicleDetections.Count();
                var totalUsers = _dbContext.Users.Count();
                
                lblTotalSessionsValue.Text = totalSessions.ToString("N0");
                lblTotalVehiclesValue.Text = totalVehicles.ToString("N0");
                lblTotalUsersValue.Text = totalUsers.ToString();
                
                // Load weekly trend chart
                LoadWeeklyTrendChart();
                
                // Load recent sessions
                LoadRecentSessions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}\n\nChi tiết: {ex.InnerException?.Message}", 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadWeeklyTrendChart()
        {
            try
            {
                if (_dbContext == null || chartWeeklyTrend == null)
                    return;

                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => DateTime.Now.Date.AddDays(-6 + i))
                    .ToList();
                
                var dailyData = last7Days.Select(date =>
                {
                    try
                    {
                        var count = _dbContext.TrafficSessions
                            .Where(s => s.StartTime.Date == date)
                            .Sum(s => s.TotalVehicles);
                        return new { Date = date, Count = count };
                    }
                    catch
                    {
                        return new { Date = date, Count = 0 };
                    }
                }).ToList();
                
                var values = new ChartValues<int>(dailyData.Select(d => d.Count));
                var labels = dailyData.Select(d => d.Date.ToString("dd/MM")).ToArray();
                
                chartWeeklyTrend.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Số xe",
                        Values = values,
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 10,
                        Fill = System.Windows.Media.Brushes.Transparent,
                        Stroke = System.Windows.Media.Brushes.DodgerBlue,
                        StrokeThickness = 3
                    }
                };
                
                chartWeeklyTrend.AxisX.Add(new Axis
                {
                    Title = "Ngày",
                    Labels = labels,
                    Separator = new Separator { Step = 1 }
                });
                
                chartWeeklyTrend.AxisY.Add(new Axis
                {
                    Title = "Số lượng xe",
                    LabelFormatter = value => value.ToString("N0")
                });
                
                chartWeeklyTrend.LegendLocation = LegendLocation.Top;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading chart: {ex.Message}");
            }
        }
        
        private void LoadRecentSessions()
        {
            try
            {
                if (_dbContext == null || dgvRecentSessions == null)
                    return;

                var recentSessions = _dbContext.TrafficSessions
                    .OrderByDescending(s => s.StartTime)
                    .Take(5)
                    .Select(s => new
                    {
                        ID = s.SessionId,
                        Thời_Gian = s.StartTime.ToString("dd/MM/yyyy HH:mm"),
                        Tổng_Xe = s.TotalVehicles,
                        Nguồn = s.SourceType,
                        File = System.IO.Path.GetFileName(s.SourcePath),
                        Trạng_Thái = s.EndTime.HasValue ? "✅ Hoàn thành" : "⏳ Đang chạy"
                    })
                    .ToList();
                
                dgvRecentSessions.DataSource = recentSessions;
                
                if (dgvRecentSessions.Columns.Contains("ID"))
                    dgvRecentSessions.Columns["ID"].Width = 60;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recent sessions: {ex.Message}");
                // Không throw exception để không làm crash form
            }
        }
        
        private void OpenMainForm()
        {
            var mainForm = new MainForm(_authService);
            
            // Khi MainForm đóng, hiện lại Dashboard
            mainForm.FormClosed += (s, e) => {
                this.Show();
                this.BringToFront();
                LoadDashboardData(); // Refresh dữ liệu
            };
            
            mainForm.Show();
            this.Hide();
        }
        
        private void OpenChartsForm()
        {
            var chartsForm = new TrafficChartsForm(_dbContext);
            chartsForm.ShowDialog();
        }
        
        private void OpenAdminDashboard()
        {
            var adminForm = new AdminDashboardForm(_dbContext, _authService);
            adminForm.ShowDialog();
        }
        
        private void OpenSettings()
        {
            var settingsForm = new SettingsForm();
            var result = settingsForm.ShowDialog();
            
            if (result == DialogResult.OK)
            {
                MessageBox.Show(
                    "✅ Cài đặt đã được cập nhật!\n\nKhởi động lại ứng dụng để áp dụng thay đổi.",
                    "Thông Báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Xác nhận thoát
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn thoát ứng dụng?",
                "Xác nhận thoát",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            
            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
            
            try
            {
                // Đóng tất cả form con
                foreach (Form form in Application.OpenForms.Cast<Form>().ToList())
                {
                    if (form != this && !form.IsDisposed)
                    {
                        form.Close();
                    }
                }
                
                // KHÔNG dispose DbContext ở đây vì có thể được tái sử dụng khi quay lại
                // _dbContext sẽ được dispose khi ứng dụng thoát hoàn toàn
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DashboardForm] Error during cleanup: {ex.Message}");
            }
            
            base.OnFormClosing(e);
            
            // Thoát hoàn toàn
            Application.Exit();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                _isDisposed = true;
                // Dispose các resource được quản lý
                chartHost?.Dispose();
                dgvRecentSessions?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
