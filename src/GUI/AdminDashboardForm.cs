using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using TrafficMonitorApp.Data;
using TrafficMonitorApp.Services;
using TrafficMonitorApp.Models;

namespace TrafficMonitorApp.GUI
{
    /// <summary>
    /// Form quản lý dữ liệu cho Admin
    /// Admin dashboard for data management
    /// </summary>
    public partial class AdminDashboardForm : Form
    {
        private readonly TrafficDbContext _dbContext;
        private readonly AuthenticationService _authService;
        
        private MenuStrip? menuStrip;
        private TabControl? tabControl;
        
        // Users Tab
        private DataGridView? dgvUsers;
        private Button? btnAddUser;
        private Button? btnEditUser;
        private Button? btnDeleteUser;
        private Button? btnResetPassword;
        private Button? btnRefreshUsers;
        private Button? btnPromoteToAdmin;
        
        // Traffic Sessions Tab
        private DataGridView? dgvSessions;
        private Button? btnRefreshSessions;
        private Button? btnDeleteSession;
        private DateTimePicker? dtpFromDate;
        private DateTimePicker? dtpToDate;
        private Button? btnFilterSessions;
        
        // Vehicle Detections Tab
        private DataGridView? dgvDetections;
        private Button? btnRefreshDetections;
        private ComboBox? cboVehicleType;
        private Button? btnFilterDetections;
        private Label? lblDetectionCount;
        
        // Statistics Tab
        private DataGridView? dgvStatistics;
        private Button? btnRefreshStatistics;
        private Label? lblTotalSessions;
        private Label? lblTotalDetections;
        private Label? lblTotalUsers;

        public AdminDashboardForm(TrafficDbContext dbContext, AuthenticationService authService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            
            InitializeComponent();
            
            // Load data after form is shown to avoid null reference issues
            this.Shown += (s, e) => LoadAllData();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "🛡️ Quản Lý Hệ Thống - Admin Dashboard";
            this.Size = new Size(1500, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 248);
            this.MinimumSize = new Size(1200, 700);
            this.Font = new Font("Segoe UI", 9.5F);
            
            // Initialize MenuStrip
            InitializeMenuStrip();

            // Tab Control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };

            // Create tabs
            CreateUsersTab();
            CreateSessionsTab();
            CreateDetectionsTab();
            CreateStatisticsTab();

            this.Controls.Add(tabControl);
            if (menuStrip != null)
            {
                this.Controls.Add(menuStrip);
                this.MainMenuStrip = menuStrip;
            }
        }
        
        private void InitializeMenuStrip()
        {
            menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Padding = new Padding(5, 2, 0, 2)
            };

            // Menu File
            var menuFile = new ToolStripMenuItem("📁 File");
            menuFile.ForeColor = Color.White;
            
            var menuFileExportUsers = new ToolStripMenuItem("📊 Xuất Danh Sách Users");
            menuFileExportUsers.ShortcutKeys = Keys.Control | Keys.Shift | Keys.U;
            menuFileExportUsers.Click += (s, e) => ExportData("users");
            
            var menuFileExportSessions = new ToolStripMenuItem("📈 Xuất Traffic Sessions");
            menuFileExportSessions.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            menuFileExportSessions.Click += (s, e) => ExportData("sessions");
            
            var menuFileExportDetections = new ToolStripMenuItem("🚗 Xuất Vehicle Detections");
            menuFileExportDetections.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D;
            menuFileExportDetections.Click += (s, e) => ExportData("detections");
            
            var menuFileExportStatistics = new ToolStripMenuItem("📊 Xuất Thống Kê");
            menuFileExportStatistics.ShortcutKeys = Keys.Control | Keys.Shift | Keys.T;
            menuFileExportStatistics.Click += (s, e) => ExportData("statistics");
            
            menuFile.DropDownItems.Add(menuFileExportUsers);
            menuFile.DropDownItems.Add(menuFileExportSessions);
            menuFile.DropDownItems.Add(menuFileExportDetections);
            menuFile.DropDownItems.Add(menuFileExportStatistics);
            menuFile.DropDownItems.Add(new ToolStripSeparator());
            
            var menuFileClose = new ToolStripMenuItem("❌ Đóng");
            menuFileClose.ShortcutKeys = Keys.Alt | Keys.F4;
            menuFileClose.Click += (s, e) => this.Close();
            menuFile.DropDownItems.Add(menuFileClose);
            
            // Menu Data
            var menuData = new ToolStripMenuItem("💾 Dữ Liệu");
            menuData.ForeColor = Color.White;
            
            var menuDataRefresh = new ToolStripMenuItem("🔄 Làm Mới");
            menuDataRefresh.ShortcutKeys = Keys.F5;
            menuDataRefresh.Click += (s, e) => LoadAllData();
            
            var menuDataBackup = new ToolStripMenuItem("💿 Sao Lưu Database");
            menuDataBackup.Click += MenuDataBackup_Click;
            
            var menuDataRestore = new ToolStripMenuItem("♻️ Khôi Phục Database");
            menuDataRestore.Click += MenuDataRestore_Click;
            
            menuData.DropDownItems.Add(menuDataRefresh);
            menuData.DropDownItems.Add(new ToolStripSeparator());
            menuData.DropDownItems.Add(menuDataBackup);
            menuData.DropDownItems.Add(menuDataRestore);
            
            // Menu View
            var menuView = new ToolStripMenuItem("👁️ Hiển Thị");
            menuView.ForeColor = Color.White;
            
            var menuViewUsers = new ToolStripMenuItem("👥 Người Dùng");
            menuViewUsers.Click += (s, e) => tabControl.SelectedIndex = 0;
            
            var menuViewSessions = new ToolStripMenuItem("📈 Traffic Sessions");
            menuViewSessions.Click += (s, e) => tabControl.SelectedIndex = 1;
            
            var menuViewDetections = new ToolStripMenuItem("🚗 Vehicle Detections");
            menuViewDetections.Click += (s, e) => tabControl.SelectedIndex = 2;
            
            var menuViewStatistics = new ToolStripMenuItem("📊 Thống Kê");
            menuViewStatistics.Click += (s, e) => tabControl.SelectedIndex = 3;
            
            menuView.DropDownItems.Add(menuViewUsers);
            menuView.DropDownItems.Add(menuViewSessions);
            menuView.DropDownItems.Add(menuViewDetections);
            menuView.DropDownItems.Add(menuViewStatistics);
            
            // Menu Help
            var menuHelp = new ToolStripMenuItem("❓ Trợ Giúp");
            menuHelp.ForeColor = Color.White;
            
            var menuHelpAbout = new ToolStripMenuItem("ℹ️ Giới Thiệu");
            menuHelpAbout.Click += (s, e) => MessageBox.Show(
                "Admin Dashboard - Traffic Monitor System\nVersion 2.0\n© 2025",
                "Giới Thiệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            menuHelp.DropDownItems.Add(menuHelpAbout);
            
            // Add all menus
            menuStrip.Items.Add(menuFile);
            menuStrip.Items.Add(menuData);
            menuStrip.Items.Add(menuView);
            menuStrip.Items.Add(menuHelp);
        }
        
        private void MenuDataBackup_Click(object sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "SQLite Database|*.db",
                    Title = "Sao Lưu Database",
                    FileName = $"traffic_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
                };
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.Copy("traffic_monitor.db", saveDialog.FileName, true);
                    MessageBox.Show("Sao lưu database thành công!", "Thành công",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sao lưu: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void MenuDataRestore_Click(object sender, EventArgs e)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "SQLite Database|*.db",
                    Title = "Khôi Phục Database"
                };
                
                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    var result = MessageBox.Show(
                        "Khôi phục sẽ ghi đè lên database hiện tại. Bạn có chắc chắn?",
                        "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        
                    if (result == DialogResult.Yes)
                    {
                        System.IO.File.Copy(openDialog.FileName, "traffic_monitor.db", true);
                        MessageBox.Show("Khôi phục database thành công! Vui lòng khởi động lại ứng dụng.",
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khôi phục: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Users Tab

        private void CreateUsersTab()
        {
            var tabUsers = new TabPage("Quản Lý Người Dùng");
            
            // Panel for buttons
            var pnlButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(15, 12, 15, 12),
                BackColor = Color.White
            };

            btnAddUser = new Button
            {
                Text = "➕ Thêm Người Dùng",
                Location = new Point(15, 15),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddUser.FlatAppearance.BorderSize = 0;
            btnAddUser.Click += BtnAddUser_Click;

            btnEditUser = new Button
            {
                Text = "✏️ Chỉnh Sửa",
                Location = new Point(165, 15),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEditUser.FlatAppearance.BorderSize = 0;
            btnEditUser.Click += BtnEditUser_Click;

            btnDeleteUser = new Button
            {
                Text = "🗑️ Xóa",
                Location = new Point(305, 15),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeleteUser.FlatAppearance.BorderSize = 0;
            btnDeleteUser.Click += BtnDeleteUser_Click;

            btnResetPassword = new Button
            {
                Text = "🔑 Reset Mật Khẩu",
                Location = new Point(425, 15),
                Size = new Size(155, 40),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnResetPassword.FlatAppearance.BorderSize = 0;
            btnResetPassword.Click += BtnResetPassword_Click;

            btnPromoteToAdmin = new Button
            {
                Text = "👑 Ủy Quyền Admin",
                Location = new Point(590, 15),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(142, 68, 173),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPromoteToAdmin.FlatAppearance.BorderSize = 0;
            btnPromoteToAdmin.Click += BtnPromoteToAdmin_Click;

            btnRefreshUsers = new Button
            {
                Text = "🔄 Tải lại",
                Location = new Point(760, 15),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefreshUsers.FlatAppearance.BorderSize = 0;
            btnRefreshUsers.Click += (s, e) => LoadUsers();

            // Add Export to Excel button
            var btnExportUsers = new Button
            {
                Text = "📊 Xuất Excel",
                Location = new Point(880, 15),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExportUsers.FlatAppearance.BorderSize = 0;
            btnExportUsers.Click += (s, e) => this.ExportData("users");

            var lblSearch = new Label
            {
                Text = "🔍 Tìm:",
                Location = new Point(1030, 22),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

            var txtSearchUser = new TextBox
            {
                Location = new Point(1085, 19),
                Width = 180,
                Font = new Font("Segoe UI", 10)
            };
            txtSearchUser.TextChanged += (s, e) => SearchUsers(txtSearchUser.Text);

            pnlButtons.Controls.AddRange(new Control[] { 
                btnAddUser, btnEditUser, btnDeleteUser, btnResetPassword, btnPromoteToAdmin, btnRefreshUsers,
                btnExportUsers, lblSearch, txtSearchUser
            });

            // DataGridView
            dgvUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
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

            tabUsers.Controls.Add(dgvUsers);
            tabUsers.Controls.Add(pnlButtons);
            if (tabControl != null)
            {
                tabControl.TabPages.Add(tabUsers);
            }
        }

        private void LoadUsers()
        {
            if (dgvUsers == null || _authService == null) return;
            
            try
            {
                var users = _authService.GetAllUsers();
                if (users == null) return;
                
                dgvUsers.DataSource = users.Select(u => new
                {
                    u.UserId,
                    Tên_Đăng_Nhập = u.Username,
                    Họ_Tên = u.FullName,
                    Email = u.Email,
                    Vai_Trò = u.Role,
                    Trạng_Thái = u.IsActive ? "Hoạt động" : "Khóa",
                    Ngày_Tạo = u.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                    Đăng_Nhập_Cuối = u.LastLoginDate?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa đăng nhập"
                }).ToList();

                try
                {
                    if (dgvUsers.Columns.Contains("UserId"))
                        dgvUsers.Columns["UserId"].Visible = false;
                    
                    // Đặt độ rộng cột cho đẹp
                    if (dgvUsers.Columns.Contains("Tên_Đăng_Nhập"))
                        dgvUsers.Columns["Tên_Đăng_Nhập"].Width = 120;
                    if (dgvUsers.Columns.Contains("Họ_Tên"))
                        dgvUsers.Columns["Họ_Tên"].Width = 150;
                    if (dgvUsers.Columns.Contains("Email"))
                        dgvUsers.Columns["Email"].Width = 180;
                    if (dgvUsers.Columns.Contains("Vai_Trò"))
                        dgvUsers.Columns["Vai_Trò"].Width = 80;
                    if (dgvUsers.Columns.Contains("Trạng_Thái"))
                        dgvUsers.Columns["Trạng_Thái"].Width = 100;
                    if (dgvUsers.Columns.Contains("Ngày_Tạo"))
                        dgvUsers.Columns["Ngày_Tạo"].Width = 130;
                    if (dgvUsers.Columns.Contains("Đăng_Nhập_Cuối"))
                        dgvUsers.Columns["Đăng_Nhập_Cuối"].Width = 130;
                }
                catch { /* Ignore column width errors */ }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải users: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddUser_Click(object? sender, EventArgs e)
        {
            var registerForm = new RegisterForm(_authService);
            if (registerForm.ShowDialog() == DialogResult.OK)
            {
                LoadUsers();
                MessageBox.Show("Thêm người dùng thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnEditUser_Click(object? sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn người dùng cần sửa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userId = dgvUsers.SelectedRows[0].Cells["UserId"].Value?.ToString();
            if (string.IsNullOrEmpty(userId))
                return;

            var user = _authService.GetUserById(userId);
            if (user == null)
                return;

            var editForm = new UserEditForm(user, _authService);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadUsers();
                MessageBox.Show("Cập nhật người dùng thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDeleteUser_Click(object? sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn người dùng cần xóa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userId = dgvUsers.SelectedRows[0].Cells["UserId"].Value?.ToString();
            var username = dgvUsers.SelectedRows[0].Cells["Tên_Đăng_Nhập"].Value?.ToString();
            
            if (string.IsNullOrEmpty(userId))
                return;

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa người dùng '{username}'?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (_authService.DeleteUser(userId))
                {
                    LoadUsers();
                    MessageBox.Show("Xóa người dùng thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Không thể xóa người dùng này!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnResetPassword_Click(object? sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn người dùng!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userId = dgvUsers.SelectedRows[0].Cells["UserId"].Value?.ToString();
            var username = dgvUsers.SelectedRows[0].Cells["Tên_Đăng_Nhập"].Value?.ToString();
            
            if (string.IsNullOrEmpty(userId))
                return;

            var newPassword = Microsoft.VisualBasic.Interaction.InputBox(
                "Nhập mật khẩu mới (tối thiểu 6 ký tự):",
                "Reset Mật Khẩu",
                "");

            if (string.IsNullOrWhiteSpace(newPassword))
                return;

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_authService.ResetPassword(userId, newPassword))
            {
                MessageBox.Show($"Reset mật khẩu thành công cho '{username}'!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Lỗi khi reset mật khẩu!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPromoteToAdmin_Click(object? sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn người dùng!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userId = dgvUsers.SelectedRows[0].Cells["UserId"].Value?.ToString();
            var username = dgvUsers.SelectedRows[0].Cells["Tên_Đăng_Nhập"].Value?.ToString();
            var currentRole = dgvUsers.SelectedRows[0].Cells["Vai_Trò"].Value?.ToString();
            
            if (string.IsNullOrEmpty(userId))
                return;

            // Kiểm tra xem user đã là Admin chưa
            if (currentRole == "Admin")
            {
                // Hạ quyền xuống User
                var confirmDowngrade = MessageBox.Show(
                    $"Bạn có chắc muốn HẠ QUYỀN tài khoản '{username}' từ Admin xuống User?\n\n" +
                    "⚠️ Tài khoản này sẽ mất toàn quyền truy cập hệ thống!",
                    "Xác nhận hạ quyền",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirmDowngrade == DialogResult.Yes)
                {
                    var user = _authService.GetUserById(userId);
                    if (user != null)
                    {
                        user.Role = "User";
                        if (_authService.UpdateUser(user))
                        {
                            LoadUsers();
                            MessageBox.Show(
                                $"✅ Đã hạ quyền tài khoản '{username}' xuống User!",
                                "Thành công",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Lỗi khi cập nhật quyền!", "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                // Thăng cấp lên Admin
                var confirmPromote = MessageBox.Show(
                    $"Bạn có chắc muốn ỦY QUYỀN ADMIN cho tài khoản '{username}'?\n\n" +
                    "👑 Tài khoản này sẽ có toàn quyền truy cập và quản lý hệ thống:\n" +
                    "  ✓ Quản lý người dùng (thêm, sửa, xóa)\n" +
                    "  ✓ Truy cập và quản lý toàn bộ database\n" +
                    "  ✓ Xóa dữ liệu traffic sessions\n" +
                    "  ✓ Xuất báo cáo và thống kê\n" +
                    "  ✓ Thay đổi cấu hình hệ thống\n\n" +
                    "⚠️ Chỉ ủy quyền cho người đáng tin cậy!",
                    "Xác nhận ủy quyền Admin",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmPromote == DialogResult.Yes)
                {
                    var user = _authService.GetUserById(userId);
                    if (user != null)
                    {
                        user.Role = "Admin";
                        if (_authService.UpdateUser(user))
                        {
                            LoadUsers();
                            MessageBox.Show(
                                $"🎉 Đã ủy quyền Admin thành công cho '{username}'!\n\n" +
                                $"Tài khoản này giờ có toàn quyền quản lý hệ thống.",
                                "Ủy quyền thành công",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Lỗi khi cập nhật quyền!", "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        #endregion

        #region Sessions Tab

        private void CreateSessionsTab()
        {
            var tabSessions = new TabPage("Phiên Giao Thông");
            
            // Panel for filters
            var pnlFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };

            var lblFrom = new Label
            {
                Text = "Từ ngày:",
                Location = new Point(10, 18),
                AutoSize = true
            };

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(80, 15),
                Width = 150,
                Format = DateTimePickerFormat.Short
            };
            dtpFromDate.Value = DateTime.Now.AddDays(-7);

            var lblTo = new Label
            {
                Text = "Đến:",
                Location = new Point(240, 18),
                AutoSize = true
            };

            dtpToDate = new DateTimePicker
            {
                Location = new Point(290, 15),
                Width = 150,
                Format = DateTimePickerFormat.Short
            };

            btnFilterSessions = new Button
            {
                Text = "🔍 Lọc",
                Location = new Point(450, 15),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnFilterSessions.Click += (s, e) => LoadSessions();

            btnRefreshSessions = new Button
            {
                Text = "🔄 Tải lại",
                Location = new Point(560, 15),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefreshSessions.Click += (s, e) => LoadSessions();

            btnDeleteSession = new Button
            {
                Text = "🗑️ Xóa",
                Location = new Point(670, 15),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDeleteSession.Click += BtnDeleteSession_Click;

            var btnExportSessions = new Button
            {
                Text = "💾 Xuất CSV",
                Location = new Point(780, 15),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExportSessions.Click += BtnExportSessions_Click;

            var btnViewDetails = new Button
            {
                Text = "🔍 Chi tiết",
                Location = new Point(900, 15),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnViewDetails.Click += BtnViewSessionDetails_Click;

            pnlFilters.Controls.AddRange(new Control[] { 
                lblFrom, dtpFromDate, lblTo, dtpToDate, 
                btnFilterSessions, btnRefreshSessions, btnDeleteSession, btnExportSessions, btnViewDetails
            });

            // DataGridView
            dgvSessions = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
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

            tabSessions.Controls.Add(dgvSessions);
            tabSessions.Controls.Add(pnlFilters);
            if (tabControl != null)
            {
                tabControl.TabPages.Add(tabSessions);
            }
        }

        private void LoadSessions()
        {
            if (dgvSessions == null || _dbContext == null) return;
            
            try
            {
                var sessions = _dbContext.TrafficSessions
                    .Where(s => s.StartTime >= dtpFromDate.Value.Date && 
                               s.StartTime <= dtpToDate.Value.Date.AddDays(1))
                    .OrderByDescending(s => s.StartTime)
                    .ToList();

                dgvSessions.DataSource = sessions.Select(s => new
                {
                    s.SessionId,
                    Bắt_Đầu = s.StartTime.ToString("dd/MM/yyyy HH:mm:ss"),
                    Kết_Thúc = s.EndTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Đang chạy",
                    Tổng_Xe = s.TotalVehicles,
                    Nguồn = s.SourceType,
                    Đường_Dẫn = System.IO.Path.GetFileName(s.SourcePath),
                    Mô_Hình = System.IO.Path.GetFileName(s.ModelPath)
                }).ToList();

                try
                {
                    if (dgvSessions.Columns.Contains("SessionId"))
                        dgvSessions.Columns["SessionId"].Visible = false;
                    
                    // Đặt độ rộng cột
                    if (dgvSessions.Columns.Contains("Bắt_Đầu"))
                        dgvSessions.Columns["Bắt_Đầu"].Width = 150;
                    if (dgvSessions.Columns.Contains("Kết_Thúc"))
                        dgvSessions.Columns["Kết_Thúc"].Width = 150;
                    if (dgvSessions.Columns.Contains("Tổng_Xe"))
                        dgvSessions.Columns["Tổng_Xe"].Width = 80;
                    if (dgvSessions.Columns.Contains("Nguồn"))
                        dgvSessions.Columns["Nguồn"].Width = 100;
                    if (dgvSessions.Columns.Contains("Đường_Dẫn"))
                        dgvSessions.Columns["Đường_Dẫn"].Width = 200;
                    if (dgvSessions.Columns.Contains("Mô_Hình"))
                        dgvSessions.Columns["Mô_Hình"].Width = 150;
                }
                catch { /* Ignore column width errors */ }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải sessions: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteSession_Click(object? sender, EventArgs e)
        {
            if (dgvSessions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phiên cần xóa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sessionId = (int?)dgvSessions.SelectedRows[0].Cells["SessionId"].Value;
            if (sessionId == null)
                return;

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa phiên này? Tất cả dữ liệu phát hiện xe sẽ bị xóa!",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var session = _dbContext.TrafficSessions.Find(sessionId.Value);
                    if (session != null)
                    {
                        _dbContext.TrafficSessions.Remove(session);
                        _dbContext.SaveChanges();
                        LoadSessions();
                        MessageBox.Show("Xóa phiên thành công!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Detections Tab

        private void CreateDetectionsTab()
        {
            var tabDetections = new TabPage("Phát Hiện Xe");
            
            // Panel for filters
            var pnlFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };

            var lblType = new Label
            {
                Text = "Loại xe:",
                Location = new Point(10, 18),
                AutoSize = true
            };

            cboVehicleType = new ComboBox
            {
                Location = new Point(80, 15),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboVehicleType.Items.AddRange(new object[] { "Tất cả", "Car", "Truck", "Bus", "Motorbike", "Bicycle" });
            cboVehicleType.SelectedIndex = 0;

            btnFilterDetections = new Button
            {
                Text = "🔍 Lọc",
                Location = new Point(240, 15),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnFilterDetections.Click += (s, e) => LoadDetections();

            btnRefreshDetections = new Button
            {
                Text = "🔄 Tải lại",
                Location = new Point(350, 15),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefreshDetections.Click += (s, e) => LoadDetections();

            lblDetectionCount = new Label
            {
                Text = "Tổng số: 0",
                Location = new Point(460, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            pnlFilters.Controls.AddRange(new Control[] { 
                lblType, cboVehicleType, btnFilterDetections, btnRefreshDetections, lblDetectionCount 
            });

            // DataGridView
            dgvDetections = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
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

            tabDetections.Controls.Add(dgvDetections);
            tabDetections.Controls.Add(pnlFilters);
            if (tabControl != null)
            {
                tabControl.TabPages.Add(tabDetections);
            }
        }

        private void LoadDetections()
        {
            if (dgvDetections == null || _dbContext == null || cboVehicleType == null) return;
            
            try
            {
                var query = _dbContext.VehicleDetections.AsQueryable();

                // Count total before filter
                var totalCount = query.Count();

                if (cboVehicleType.SelectedItem?.ToString() != "Tất cả")
                {
                    var selectedType = cboVehicleType.SelectedItem?.ToString();
                    query = query.Where(d => d.VehicleType == selectedType);
                }

                var filteredCount = query.Count();
                var detections = query
                    .OrderByDescending(d => d.DetectedTime)
                    .Take(1000)
                    .ToList();

                dgvDetections.DataSource = detections.Select(d => new
                {
                    Mã_Phát_Hiện = d.DetectionId,
                    Mã_Phiên = d.SessionId,
                    Thời_Gian = d.DetectedTime.ToString("dd/MM/yyyy HH:mm:ss"),
                    Loại_Xe = d.VehicleType,
                    Độ_Tin_Cậy = $"{d.Confidence:P1}",
                    Mã_Tracking = d.TrackerId,
                    Số_Frame = d.FrameNumber,
                    Vị_Trí = $"({d.PositionX}, {d.PositionY})",
                    Kích_Thước = $"{d.Width}x{d.Height}"
                }).ToList();

                // Update count label
                if (lblDetectionCount != null)
                {
                    if (cboVehicleType.SelectedItem?.ToString() == "Tất cả")
                        lblDetectionCount.Text = $"Tổng số: {totalCount:N0} phát hiện";
                    else
                        lblDetectionCount.Text = $"Hiển thị: {detections.Count:N0}/{filteredCount:N0} (Tổng: {totalCount:N0})";
                }
                
                try
                {
                    // Đặt độ rộng cột
                    if (dgvDetections.Columns.Contains("Mã_Phát_Hiện"))
                        dgvDetections.Columns["Mã_Phát_Hiện"].Width = 100;
                    if (dgvDetections.Columns.Contains("Mã_Phiên"))
                        dgvDetections.Columns["Mã_Phiên"].Width = 80;
                    if (dgvDetections.Columns.Contains("Thời_Gian"))
                        dgvDetections.Columns["Thời_Gian"].Width = 150;
                    if (dgvDetections.Columns.Contains("Loại_Xe"))
                        dgvDetections.Columns["Loại_Xe"].Width = 100;
                    if (dgvDetections.Columns.Contains("Độ_Tin_Cậy"))
                        dgvDetections.Columns["Độ_Tin_Cậy"].Width = 90;
                    if (dgvDetections.Columns.Contains("Mã_Tracking"))
                        dgvDetections.Columns["Mã_Tracking"].Width = 100;
                    if (dgvDetections.Columns.Contains("Số_Frame"))
                        dgvDetections.Columns["Số_Frame"].Width = 80;
                    if (dgvDetections.Columns.Contains("Vị_Trí"))
                        dgvDetections.Columns["Vị_Trí"].Width = 100;
                    if (dgvDetections.Columns.Contains("Kích_Thước"))
                        dgvDetections.Columns["Kích_Thước"].Width = 90;
                }
                catch { /* Ignore column width errors */ }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải detections: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Statistics Tab

        private void CreateStatisticsTab()
        {
            var tabStats = new TabPage("Thống Kê");
            
            // Panel for summary
            var pnlSummary = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            lblTotalSessions = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219)
            };

            lblTotalDetections = new Label
            {
                Location = new Point(20, 50),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113)
            };

            lblTotalUsers = new Label
            {
                Location = new Point(20, 80),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(155, 89, 182)
            };

            btnRefreshStatistics = new Button
            {
                Text = "🔄 Tải lại",
                Location = new Point(350, 40),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnRefreshStatistics.Click += (s, e) => LoadStatistics();

            pnlSummary.Controls.AddRange(new Control[] { 
                lblTotalSessions, lblTotalDetections, lblTotalUsers, btnRefreshStatistics 
            });

            // DataGridView for hourly stats
            dgvStatistics = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
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

            tabStats.Controls.Add(dgvStatistics);
            tabStats.Controls.Add(pnlSummary);
            if (tabControl != null)
            {
                tabControl.TabPages.Add(tabStats);
            }
        }

        private void LoadStatistics()
        {
            if (dgvStatistics == null || _dbContext == null || lblTotalSessions == null) return;
            
            try
            {
                var totalSessions = _dbContext.TrafficSessions.Count();
                var totalDetections = _dbContext.VehicleDetections.Count();
                var totalUsers = _dbContext.Users.Count();

                lblTotalSessions.Text = $"📊 Tổng phiên: {totalSessions:N0}";
                lblTotalDetections.Text = $"🚗 Tổng xe phát hiện: {totalDetections:N0}";
                lblTotalUsers.Text = $"👥 Tổng người dùng: {totalUsers}";

                var stats = _dbContext.HourlyStatistics
                    .OrderByDescending(s => s.HourTimestamp)
                    .Take(100)
                    .ToList();

                dgvStatistics.DataSource = stats.Select(s => new
                {
                    Giờ = s.HourTimestamp.ToString("dd/MM/yyyy HH:00"),
                    Tổng_Xe = s.TotalVehicles,
                    Ô_Tô = s.CarCount,
                    Xe_Máy = s.MotorcycleCount,
                    Xe_Buýt = s.BusCount,
                    Xe_Đạp = s.BicycleCount,
                    Tốc_Độ_TB = $"{s.AverageSpeed:F1} km/h",
                    Mức_Tắc = s.CongestionLevel
                }).ToList();
                
                try
                {
                    // Đặt độ rộng cột
                    if (dgvStatistics.Columns.Contains("Giờ"))
                        dgvStatistics.Columns["Giờ"].Width = 130;
                    if (dgvStatistics.Columns.Contains("Tổng_Xe"))
                        dgvStatistics.Columns["Tổng_Xe"].Width = 80;
                    if (dgvStatistics.Columns.Contains("Ô_Tô"))
                        dgvStatistics.Columns["Ô_Tô"].Width = 80;
                    if (dgvStatistics.Columns.Contains("Xe_Máy"))
                        dgvStatistics.Columns["Xe_Máy"].Width = 80;
                    if (dgvStatistics.Columns.Contains("Xe_Buýt"))
                        dgvStatistics.Columns["Xe_Buýt"].Width = 80;
                    if (dgvStatistics.Columns.Contains("Xe_Đạp"))
                        dgvStatistics.Columns["Xe_Đạp"].Width = 80;
                    if (dgvStatistics.Columns.Contains("Tốc_Độ_TB"))
                        dgvStatistics.Columns["Tốc_Độ_TB"].Width = 100;
                    if (dgvStatistics.Columns.Contains("Mức_Tắc"))
                        dgvStatistics.Columns["Mức_Tắc"].Width = 100;
                }
                catch { /* Ignore column width errors */ }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải statistics: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void LoadAllData()
        {
            LoadUsers();
            LoadSessions();
            LoadDetections();
            LoadStatistics();
        }

        #region Additional Management Functions

        /// <summary>
        /// Tìm kiếm người dùng theo tên hoặc email
        /// Search users by name or email
        /// </summary>
        private void SearchUsers(string searchText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    LoadUsers();
                    return;
                }

                var users = _authService.GetAllUsers()
                    .Where(u => u.Username.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                               (u.FullName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                               (u.Email?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();

                dgvUsers.DataSource = users.Select(u => new
                {
                    u.UserId,
                    Tên_Đăng_Nhập = u.Username,
                    Họ_Tên = u.FullName,
                    Email = u.Email,
                    Vai_Trò = u.Role,
                    Trạng_Thái = u.IsActive ? "Hoạt động" : "Khóa",
                    Ngày_Tạo = u.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                    Đăng_Nhập_Cuối = u.LastLoginDate?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa đăng nhập"
                }).ToList();

                try
                {
                    if (dgvUsers.Columns.Contains("UserId"))
                        dgvUsers.Columns["UserId"].Visible = false;
                    
                    // Đặt độ rộng cột
                    if (dgvUsers.Columns.Contains("Tên_Đăng_Nhập"))
                        dgvUsers.Columns["Tên_Đăng_Nhập"].Width = 120;
                    if (dgvUsers.Columns.Contains("Họ_Tên"))
                        dgvUsers.Columns["Họ_Tên"].Width = 150;
                    if (dgvUsers.Columns.Contains("Email"))
                        dgvUsers.Columns["Email"].Width = 180;
                    if (dgvUsers.Columns.Contains("Vai_Trò"))
                        dgvUsers.Columns["Vai_Trò"].Width = 80;
                    if (dgvUsers.Columns.Contains("Trạng_Thái"))
                        dgvUsers.Columns["Trạng_Thái"].Width = 100;
                    if (dgvUsers.Columns.Contains("Ngày_Tạo"))
                        dgvUsers.Columns["Ngày_Tạo"].Width = 130;
                    if (dgvUsers.Columns.Contains("Đăng_Nhập_Cuối"))
                        dgvUsers.Columns["Đăng_Nhập_Cuối"].Width = 130;
                }
                catch { /* Ignore column width errors */ }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Xuất danh sách sessions ra file CSV
        /// Export sessions to CSV file
        /// </summary>
        private void BtnExportSessions_Click(object? sender, EventArgs e)
        {
            try
            {
                var sessions = _dbContext.TrafficSessions
                    .Where(s => s.StartTime >= dtpFromDate.Value.Date && 
                               s.StartTime <= dtpToDate.Value.Date.AddDays(1))
                    .OrderByDescending(s => s.StartTime)
                    .ToList();

                if (sessions.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files|*.csv",
                    Title = "Lưu file CSV",
                    FileName = $"Sessions_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var writer = new System.IO.StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // Header
                        writer.WriteLine("Session ID,Bắt Đầu,Kết Thúc,Tổng Xe,Nguồn,Đường Dẫn,Mô Hình");

                        // Data
                        foreach (var s in sessions)
                        {
                            writer.WriteLine($"{s.SessionId}," +
                                           $"{s.StartTime:dd/MM/yyyy HH:mm:ss}," +
                                           $"{s.EndTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Đang chạy"}," +
                                           $"{s.TotalVehicles}," +
                                           $"{s.SourceType}," +
                                           $"\"{s.SourcePath}\"," +
                                           $"\"{s.ModelPath}\"");
                        }
                    }

                    MessageBox.Show($"Đã xuất {sessions.Count} phiên thành công!\n\nFile: {saveDialog.FileName}",
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất CSV: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Xem chi tiết session và các phát hiện xe
        /// View session details and detections
        /// </summary>
        private void BtnViewSessionDetails_Click(object? sender, EventArgs e)
        {
            if (dgvSessions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phiên cần xem!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sessionId = (int?)dgvSessions.SelectedRows[0].Cells["SessionId"].Value;
            if (sessionId == null)
                return;

            try
            {
                var session = _dbContext.TrafficSessions.Find(sessionId.Value);
                if (session == null)
                {
                    MessageBox.Show("Không tìm thấy phiên!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var detections = _dbContext.VehicleDetections
                    .Where(d => d.SessionId == sessionId.Value)
                    .OrderByDescending(d => d.DetectedTime)
                    .ToList();

                var vehicleCounts = detections.GroupBy(d => d.VehicleType)
                    .Select(g => $"{g.Key}: {g.Count()}")
                    .ToList();

                var message = $"📊 CHI TIẾT PHIÊN GIAO THÔNG\n\n" +
                            $"🆔 Session ID: {session.SessionId}\n" +
                            $"⏰ Bắt đầu: {session.StartTime:dd/MM/yyyy HH:mm:ss}\n" +
                            $"⏹️ Kết thúc: {session.EndTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Đang chạy"}\n" +
                            $"🚗 Tổng xe: {session.TotalVehicles}\n" +
                            $"📹 Nguồn: {session.SourceType}\n" +
                            $"📁 Đường dẫn: {System.IO.Path.GetFileName(session.SourcePath)}\n" +
                            $"🤖 Mô hình: {System.IO.Path.GetFileName(session.ModelPath)}\n\n" +
                            $"📈 PHÂN LOẠI XE:\n" +
                            string.Join("\n", vehicleCounts) + "\n\n" +
                            $"💾 Tổng phát hiện: {detections.Count}";

                MessageBox.Show(message, "Chi Tiết Phiên", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải chi tiết: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }

    #region User Edit Form

    /// <summary>
    /// Form sửa thông tin người dùng
    /// User edit form
    /// </summary>
    public class UserEditForm : Form
    {
        private readonly UserAccount _user;
        private readonly AuthenticationService _authService;

        private TextBox txtFullName = null!;
        private TextBox txtEmail = null!;
        private ComboBox cboRole = null!;
        private CheckBox chkIsActive = null!;
        private Button btnSave = null!;
        private Button btnCancel = null!;

        public UserEditForm(UserAccount user, AuthenticationService authService)
        {
            _user = user;
            _authService = authService;
            InitializeComponent();
            LoadUserData();
        }

        private void InitializeComponent()
        {
            this.Text = "Sửa Người Dùng";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblFullName = new Label { Text = "Họ tên:", Location = new Point(30, 30), AutoSize = true };
            txtFullName = new TextBox { Location = new Point(30, 55), Width = 320 };

            var lblEmail = new Label { Text = "Email:", Location = new Point(30, 90), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(30, 115), Width = 320 };

            var lblRole = new Label { Text = "Vai trò:", Location = new Point(30, 150), AutoSize = true };
            cboRole = new ComboBox { Location = new Point(30, 175), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboRole.Items.AddRange(new object[] { "Admin", "User" });

            chkIsActive = new CheckBox { Text = "Kích hoạt", Location = new Point(200, 175), AutoSize = true };

            btnSave = new Button
            {
                Text = "💾 Lưu",
                Location = new Point(150, 220),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "❌ Hủy",
                Location = new Point(250, 220),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] {
                lblFullName, txtFullName, lblEmail, txtEmail, lblRole, cboRole,
                chkIsActive, btnSave, btnCancel
            });
        }

        private void LoadUserData()
        {
            txtFullName.Text = _user.FullName;
            txtEmail.Text = _user.Email;
            cboRole.SelectedItem = _user.Role;
            chkIsActive.Checked = _user.IsActive;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _user.FullName = txtFullName.Text.Trim();
            _user.Email = txtEmail.Text.Trim();
            _user.Role = cboRole.SelectedItem?.ToString() ?? "User";
            _user.IsActive = chkIsActive.Checked;

            if (_authService.UpdateUser(_user))
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Lỗi khi cập nhật!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    #endregion
}
