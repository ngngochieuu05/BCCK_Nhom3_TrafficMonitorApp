using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficMonitorApp.Models
{
    /// <summary>
    /// Lớp đại diện cho tài khoản người dùng
    /// User account model for authentication
    /// Entity Framework entity for Users table
    /// Bảng: NguoiDung
    /// </summary>
    [Table("NguoiDung")]
    public class UserAccount
    {
        /// <summary>ID duy nhất của người dùng</summary>
        [Key]
        [Column("MaNguoiDung")]
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Tên đăng nhập (username)</summary>
        [Required]
        [MaxLength(50)]
        [Column("TenDangNhap")]
        public string Username { get; set; } = string.Empty;

        /// <summary>Mật khẩu đã mã hóa (hashed password)</summary>
        [Required]
        [MaxLength(256)]
        [Column("MatKhau")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Họ và tên đầy đủ</summary>
        [MaxLength(100)]
        [Column("HoTen")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>Email liên hệ</summary>
        [EmailAddress]
        [MaxLength(100)]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Vai trò (Admin, User)</summary>
        [Required]
        [MaxLength(20)]
        [Column("VaiTro")]
        public string Role { get; set; } = "User";

        /// <summary>Ngày tạo tài khoản</summary>
        [Required]
        [Column("NgayTao")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>Ngày đăng nhập gần nhất</summary>
        [Column("LanDangNhapCuoi")]
        public DateTime? LastLoginDate { get; set; }

        /// <summary>Trạng thái kích hoạt</summary>
        [Required]
        [Column("TrangThai")]
        public bool IsActive { get; set; } = true;
    }
}
