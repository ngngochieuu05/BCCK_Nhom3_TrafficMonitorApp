using System;
using System.Drawing;
using System.Windows.Forms;
using TrafficMonitorApp.Services;

namespace TrafficMonitorApp.GUI
{
    /// <summary>
    /// Form đăng nhập vào hệ thống
    /// Login form for system access
    /// </summary>
    public class LoginForm : Form
    {
        private readonly AuthenticationService _authService;
        
        // Controls - Initialized in InitializeComponent()
        private Label lblTitle = null!;
        private Label lblUsername = null!;
        private Label lblPassword = null!;
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Button btnRegister = null!;
        private CheckBox chkShowPassword = null!;
        private LinkLabel lnkForgotPassword = null!;
        private Panel pnlMain = null!;
        
        public bool LoginSuccessful { get; private set; }

        public LoginForm(AuthenticationService authService)
        {
            _authService = authService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "Đăng Nhập - Traffic Monitor System";
            this.Size = new Size(450, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Main panel
            pnlMain = new Panel
            {
                Location = new Point(50, 30),
                Size = new Size(350, 400),
                BackColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.None
            };

            // Title
            lblTitle = new Label
            {
                Text = "ĐĂNG NHẬP HỆ THỐNG",
                Location = new Point(25, 30),
                Size = new Size(300, 40),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Username label
            lblUsername = new Label
            {
                Text = "Tên đăng nhập:",
                Location = new Point(50, 100),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.White
            };

            // Username textbox
            txtUsername = new TextBox
            {
                Location = new Point(50, 130),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Nhập tên đăng nhập"
            };

            // Password label
            lblPassword = new Label
            {
                Text = "Mật khẩu:",
                Location = new Point(50, 175),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.White
            };

            // Password textbox
            txtPassword = new TextBox
            {
                Location = new Point(50, 205),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true,
                PlaceholderText = "Nhập mật khẩu"
            };

            // Show password checkbox
            chkShowPassword = new CheckBox
            {
                Text = "Hiển thị mật khẩu",
                Location = new Point(50, 245),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White
            };
            chkShowPassword.CheckedChanged += ChkShowPassword_CheckedChanged;

            // Forgot password link
            lnkForgotPassword = new LinkLabel
            {
                Text = "Quên mật khẩu?",
                Location = new Point(200, 245),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9),
                LinkColor = Color.FromArgb(41, 128, 185),
                TextAlign = ContentAlignment.MiddleRight
            };
            lnkForgotPassword.LinkClicked += LnkForgotPassword_LinkClicked;

            // Login button
            btnLogin = new Button
            {
                Text = "ĐĂNG NHẬP",
                Location = new Point(50, 290),
                Size = new Size(250, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            // Register button
            btnRegister = new Button
            {
                Text = "ĐĂNG KÝ TÀI KHOẢN MỚI",
                Location = new Point(50, 340),
                Size = new Size(250, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            // Add controls to panel
            pnlMain.Controls.AddRange(new Control[]
            {
                lblTitle, lblUsername, txtUsername,
                lblPassword, txtPassword, chkShowPassword,
                lnkForgotPassword, btnLogin, btnRegister
            });

            // Add panel to form
            this.Controls.Add(pnlMain);

            // Set default button and key events
            this.AcceptButton = btnLogin;
            txtUsername.KeyPress += Txt_KeyPress;
            txtPassword.KeyPress += Txt_KeyPress;
        }

        private void Txt_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                btnLogin.PerformClick();
            }
        }

        private void ChkShowPassword_CheckedChanged(object? sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        private void LnkForgotPassword_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(
                "Vui lòng liên hệ quản trị viên để đặt lại mật khẩu.\n\n" +
                "Email: admin@trafficmonitor.com\n" +
                "Hotline: 1900-xxxx",
                "Quên mật khẩu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            // Attempt login
            btnLogin.Enabled = false;
            btnLogin.Text = "Đang đăng nhập...";

            try
            {
                if (_authService.Login(txtUsername.Text.Trim(), txtPassword.Text))
                {
                    LoginSuccessful = true;
                    MessageBox.Show(
                        $"Chào mừng {_authService.CurrentUser?.FullName}!\n\n" +
                        $"Vai trò: {_authService.CurrentUser?.Role}",
                        "Đăng nhập thành công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "Tên đăng nhập hoặc mật khẩu không đúng!\n\n" +
                        "Vui lòng kiểm tra lại thông tin đăng nhập.",
                        "Đăng nhập thất bại",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi đăng nhập: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "ĐĂNG NHẬP";
            }
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            var registerForm = new RegisterForm(_authService);
            if (registerForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show(
                    "Đăng ký tài khoản thành công!\n\n" +
                    "Bạn có thể đăng nhập ngay bây giờ.",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                
                // Auto-fill username
                txtUsername.Text = registerForm.RegisteredUsername;
                txtPassword.Focus();
            }
        }
    }
}
