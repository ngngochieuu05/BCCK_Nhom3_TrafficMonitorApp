using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TrafficMonitorApp.Data;
using TrafficMonitorApp.Models;

namespace TrafficMonitorApp.Services
{
    /// <summary>
    /// Dịch vụ xác thực và quản lý tài khoản người dùng sử dụng Database
    /// Authentication service for user management using Database
    /// </summary>
    public class AuthenticationService
    {
        private readonly TrafficDbContext _dbContext;
        private UserAccount? _currentUser;

        /// <summary>Người dùng hiện tại đang đăng nhập</summary>
        public UserAccount? CurrentUser => _currentUser;

        /// <summary>Kiểm tra xem có người dùng đang đăng nhập hay không</summary>
        public bool IsLoggedIn => _currentUser != null;

        public AuthenticationService(TrafficDbContext dbContext)
        {
            _dbContext = dbContext;
            
            // Tạo tài khoản admin mặc định nếu chưa có
            CreateDefaultAdmin();
        }

        #region User Management - Quản lý tài khoản

        /// <summary>
        /// Tạo tài khoản admin mặc định
        /// Create default admin account
        /// </summary>
        private void CreateDefaultAdmin()
        {
            try
            {
                if (!_dbContext.Users.Any(u => u.Role == "Admin"))
                {
                    var admin = new UserAccount
                    {
                        UserId = Guid.NewGuid().ToString(),
                        Username = "admin",
                        PasswordHash = HashPassword("admin"),
                        FullName = "Administrator",
                        Email = "admin@trafficmonitor.com",
                        Role = "Admin",
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };
                    _dbContext.Users.Add(admin);
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tạo admin: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy tất cả người dùng
        /// Get all users
        /// </summary>
        public List<UserAccount> GetAllUsers()
        {
            return _dbContext.Users.ToList();
        }

        /// <summary>
        /// Lấy người dùng theo ID
        /// Get user by ID
        /// </summary>
        public UserAccount? GetUserById(string userId)
        {
            return _dbContext.Users.FirstOrDefault(u => u.UserId == userId);
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// Update user information
        /// </summary>
        public bool UpdateUser(UserAccount user)
        {
            try
            {
                var existingUser = _dbContext.Users.FirstOrDefault(u => u.UserId == user.UserId);
                if (existingUser == null)
                    return false;

                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.Role = user.Role;
                existingUser.IsActive = user.IsActive;

                _dbContext.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Xóa người dùng
        /// Delete user
        /// </summary>
        public bool DeleteUser(string userId)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null || user.Role == "Admin")
                    return false;

                _dbContext.Users.Remove(user);
                _dbContext.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Authentication - Xác thực

        /// <summary>
        /// Đăng nhập với tên đăng nhập và mật khẩu
        /// Login with username and password
        /// </summary>
        public bool Login(string username, string password)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(u => 
                    u.Username.ToLower() == username.ToLower() && 
                    u.IsActive);

                if (user == null)
                    return false;

                if (!VerifyPassword(password, user.PasswordHash))
                    return false;

                // Cập nhật thời gian đăng nhập
                user.LastLoginDate = DateTime.Now;
                _currentUser = user;
                _dbContext.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Đăng xuất người dùng hiện tại
        /// Logout current user
        /// </summary>
        public void Logout()
        {
            _currentUser = null;
        }

        /// <summary>
        /// Đăng ký tài khoản mới
        /// Register new account
        /// </summary>
        public bool Register(string username, string password, string fullName, string email)
        {
            try
            {
                // Kiểm tra username đã tồn tại chưa
                if (_dbContext.Users.Any(u => u.Username.ToLower() == username.ToLower()))
                {
                    throw new Exception("Tên đăng nhập đã tồn tại!");
                }

                // Kiểm tra độ dài mật khẩu
                if (password.Length < 6)
                {
                    throw new Exception("Mật khẩu phải có ít nhất 6 ký tự!");
                }

                var newUser = new UserAccount
                {
                    UserId = Guid.NewGuid().ToString(),
                    Username = username,
                    PasswordHash = HashPassword(password),
                    FullName = fullName,
                    Email = email,
                    Role = "User",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _dbContext.Users.Add(newUser);
                _dbContext.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Đổi mật khẩu cho người dùng hiện tại
        /// Change password for current user
        /// </summary>
        public bool ChangePassword(string oldPassword, string newPassword)
        {
            if (_currentUser == null)
                return false;

            if (!VerifyPassword(oldPassword, _currentUser.PasswordHash))
                return false;

            if (newPassword.Length < 6)
                return false;

            _currentUser.PasswordHash = HashPassword(newPassword);
            _dbContext.SaveChanges();

            return true;
        }

        /// <summary>
        /// Reset mật khẩu cho người dùng (chỉ Admin)
        /// Reset password for user (Admin only)
        /// </summary>
        public bool ResetPassword(string userId, string newPassword)
        {
            if (_currentUser?.Role != "Admin")
                return false;

            var user = _dbContext.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return false;

            user.PasswordHash = HashPassword(newPassword);
            _dbContext.SaveChanges();
            return true;
        }

        #endregion

        #region Password Hashing - Mã hóa mật khẩu

        /// <summary>
        /// Mã hóa mật khẩu sử dụng SHA256
        /// Hash password using SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Xác minh mật khẩu
        /// Verify password against hash
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            string hashOfInput = HashPassword(password);
            return hashOfInput.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Permission Check - Kiểm tra quyền

        /// <summary>
        /// Kiểm tra quyền truy cập database (chỉ Admin)
        /// Check database access permission (Admin only)
        /// </summary>
        public bool CanAccessDatabase()
        {
            return _currentUser?.Role == "Admin";
        }

        /// <summary>
        /// Kiểm tra quyền sửa dữ liệu (chỉ Admin)
        /// Check data modification permission (Admin only)
        /// </summary>
        public bool CanModifyData()
        {
            return _currentUser?.Role == "Admin";
        }

        /// <summary>
        /// Kiểm tra quyền xóa dữ liệu (chỉ Admin)
        /// Check data deletion permission (Admin only)
        /// </summary>
        public bool CanDeleteData()
        {
            return _currentUser?.Role == "Admin";
        }

        /// <summary>
        /// Kiểm tra quyền xuất báo cáo (User và Admin đều có quyền)
        /// Check report export permission (Both User and Admin can export)
        /// </summary>
        public bool CanExportReports()
        {
            // User và Admin đều có thể xuất báo cáo
            return _currentUser != null && _currentUser.IsActive;
        }

        /// <summary>
        /// Kiểm tra quyền thay đổi cấu hình hệ thống (User và Admin đều có quyền)
        /// Check system configuration change permission (Both User and Admin can change)
        /// </summary>
        public bool CanChangeSystemConfig()
        {
            // User và Admin đều có thể thay đổi model và config
            return _currentUser != null && _currentUser.IsActive;
        }

        /// <summary>
        /// Kiểm tra có phải Admin không
        /// Check if user is Admin
        /// </summary>
        public bool IsAdmin()
        {
            return _currentUser?.Role == "Admin";
        }

        #endregion
    }
}
