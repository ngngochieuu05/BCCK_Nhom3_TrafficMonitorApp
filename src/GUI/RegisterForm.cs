using System;
using System.Drawing;
using System.Windows.Forms;
using TrafficMonitorApp.Services;
using TrafficMonitorApp.Utils;

namespace TrafficMonitorApp.GUI
{
    /// <summary>
    /// Form đăng ký tài khoản mới
    /// Registration form for new account
    /// </summary>
    public class RegisterForm : Form
    {
        private readonly AuthenticationService _authService;
        
        // Controls - Initialized in InitializeComponent()
        private Label lblTitle = null!;
        private Label lblUsername = null!;
        private Label lblPassword = null!;
        private Label lblConfirmPassword = null!;
        private Label lblFullName = null!;
        private Label lblEmail = null!;
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private TextBox txtConfirmPassword = null!;
        private TextBox txtFullName = null!;
        private TextBox txtEmail = null!;
        private Button btnRegister = null!;
        private Button btnCancel = null!;
        private CheckBox chkShowPassword = null!;
        private Panel pnlMain = null!;
        
        public string RegisteredUsername { get; private set; } = string.Empty;

        public RegisterForm(AuthenticationService authService)
        {
            _authService = authService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "Đăng Ký Tài Khoản - Traffic Monitor System";
            this.Size = new Size(450, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ColorScheme.Background;
            
            // Subscribe to theme changes
            ColorScheme.ThemeChanged += (s, e) => ColorScheme.ApplyTheme(this);

            // Main panel
            pnlMain = new Panel
            {
                Location = new Point(50, 20),
                Size = new Size(350, 570),
                BackColor = ColorScheme.BackgroundPanel,
                BorderStyle = BorderStyle.None
            };

            // Title
            lblTitle = new Label
            {
                Text = "ĐĂNG KÝ TÀI KHOẢN",
                Location = new Point(50, 20),
                Size = new Size(250, 40),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ColorScheme.Text,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Full name
            lblFullName = new Label
            {
                Text = "Họ và tên: *",
                Location = new Point(50, 80),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.White
            };

            txtFullName = new TextBox
            {
                Location = new Point(50, 110),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Nguyễn Văn A"
            };

            // Username
            lblUsername = new Label
            {
                Text = "Tên đăng nhập: *",
                Location = new Point(50, 155),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.White
            };

            txtUsername = new TextBox
            {
                Location = new Point(50, 185),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "username"
            };

            // Email
            lblEmail = new Label
            {
                Text = "Email: *",
                Location = new Point(50, 230),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.White
            };

            txtEmail = new TextBox
            {
                Location = new Point(50, 260),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "email@example.com"
            };

            // Password
            lblPassword = new Label
            {
                Text = "Mật khẩu: * (tối thiểu 6 ký tự)",
                Location = new Point(50, 305),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.White
            };

            txtPassword = new TextBox
            {
                Location = new Point(50, 335),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true,
                PlaceholderText = "Nhập mật khẩu"
            };

            // Confirm password
            lblConfirmPassword = new Label
            {
                Text = "Xác nhận mật khẩu: *",
                Location = new Point(50, 380),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.White
            };

            txtConfirmPassword = new TextBox
            {
                Location = new Point(50, 410),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true,
                PlaceholderText = "Nhập lại mật khẩu"
            };

            // Show password checkbox
            chkShowPassword = new CheckBox
            {
                Text = "Hiển thị mật khẩu",
                Location = new Point(50, 450),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White
            };
            chkShowPassword.CheckedChanged += ChkShowPassword_CheckedChanged;

            // Register button
            btnRegister = new Button
            {
                Text = "ĐĂNG KÝ",
                Location = new Point(50, 490),
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = ColorScheme.Success,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            // Cancel button
            btnCancel = new Button
            {
                Text = "HỦY",
                Location = new Point(180, 490),
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                BackColor = ColorScheme.Danger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += BtnCancel_Click;

            // Add controls to panel
            pnlMain.Controls.AddRange(new Control[]
            {
                lblTitle, lblFullName, txtFullName,
                lblUsername, txtUsername,
                lblEmail, txtEmail,
                lblPassword, txtPassword,
                lblConfirmPassword, txtConfirmPassword,
                chkShowPassword, btnRegister, btnCancel
            });

            // Add panel to form
            this.Controls.Add(pnlMain);

            // Set default button
            this.AcceptButton = btnRegister;
            this.CancelButton = btnCancel;
        }

        private void ChkShowPassword_CheckedChanged(object? sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            txtConfirmPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập họ và tên!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (txtUsername.Text.Length < 3)
            {
                MessageBox.Show("Tên đăng nhập phải có ít nhất 3 ký tự!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Vui lòng nhập email!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            if (!txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
            {
                MessageBox.Show("Email không hợp lệ!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (txtPassword.Text.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            // Attempt registration
            btnRegister.Enabled = false;
            btnRegister.Text = "Đang xử lý...";

            try
            {
                if (_authService.Register(
                    txtUsername.Text.Trim(),
                    txtPassword.Text,
                    txtFullName.Text.Trim(),
                    txtEmail.Text.Trim()))
                {
                    RegisteredUsername = txtUsername.Text.Trim();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "Tên đăng nhập đã tồn tại!\n\n" +
                        "Vui lòng chọn tên đăng nhập khác.",
                        "Đăng ký thất bại",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi đăng ký: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnRegister.Enabled = true;
                btnRegister.Text = "ĐĂNG KÝ";
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
