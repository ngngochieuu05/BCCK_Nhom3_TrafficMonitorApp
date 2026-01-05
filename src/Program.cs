using System;
using System.Windows.Forms;
using TrafficMonitorApp.GUI;
using TrafficMonitorApp.Services;
using TrafficMonitorApp.Data;

namespace TrafficMonitorApp
{
    /// <summary>
    /// Entry point for Traffic Monitor Application with Database Authentication
    /// Ứng dụng giám sát giao thông với xác thực database
    /// </summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                ConfigureApplication();
                
                var dbContext = InitializeDatabase();
                if (dbContext == null) return;

                var authService = new AuthenticationService(dbContext);
                
                if (ShowLoginAndAuthenticate(authService))
                {
                    ShowDashboard(dbContext, authService);
                }
                else
                {
                    ShowLoginRequiredMessage();
                }
            }
            catch (Exception ex)
            {
                ShowFatalError(ex);
            }
        }

        private static void ConfigureApplication()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
        }

        private static TrafficDbContext? InitializeDatabase()
        {
            var dbContext = new TrafficDbContext();
            
            try
            {
                if (!dbContext.CanConnect())
                {
                    dbContext.EnsureDatabaseCreated();
                }
                return dbContext;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể kết nối database:\n\n{ex.Message}\n\n" +
                    "Vui lòng kiểm tra SQL Server LocalDB đã được cài đặt.",
                    "Lỗi Database",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return null;
            }
        }

        private static bool ShowLoginAndAuthenticate(AuthenticationService authService)
        {
            var loginForm = new LoginForm(authService);
            return loginForm.ShowDialog() == DialogResult.OK;
        }
        
        private static void ShowDashboard(TrafficDbContext dbContext, AuthenticationService authService)
        {
            var dashboardForm = new DashboardForm(dbContext, authService);
            Application.Run(dashboardForm);
        }

        private static void ShowLoginRequiredMessage()
        {
            MessageBox.Show(
                "Bạn cần đăng nhập để sử dụng hệ thống.\n\n" +
                "Tài khoản Admin mặc định:\n" +
                "Username: admin\n" +
                "Password: admin\n\n" +
                "⚠️ Lưu ý: Chỉ Admin mới có toàn quyền truy cập hệ thống.",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private static void ShowFatalError(Exception ex)
        {
            MessageBox.Show(
                $"Lỗi khởi động ứng dụng:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                "Lỗi nghiêm trọng", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error
            );
        }
    }
}