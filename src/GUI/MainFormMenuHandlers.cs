using System;
using System.Windows.Forms;
using TrafficMonitorApp.GUI;
using TrafficMonitorApp.Services;

namespace TrafficMonitorApp
{
    public partial class MainForm
    {
        // ==================== MENU FILE HANDLERS ====================
        private void MenuFileOpen_Click(object? sender, EventArgs e)
        {
            try
            {
                // Switch to video tab and trigger browse
                if (rbVideo.Checked)
                {
                    btnBrowseData.PerformClick();
                }
                else
                {
                    rbVideo.Checked = true;
                    btnBrowseData.PerformClick();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở file: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== MENU VIEW HANDLERS ====================
        private void MenuViewSettings_Click(object? sender, EventArgs e)
        {
            try
            {
                var settingsForm = new SettingsForm();
                settingsForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở cài đặt: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MenuViewStatistics_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_dbContext != null)
                {
                    var chartForm = new TrafficChartsForm(_dbContext);
                    chartForm.Show();
                }
                else
                {
                    MessageBox.Show("Database chưa được khởi tạo!", "Thông báo", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở thống kê: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MenuViewHistory_Click(object? sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("Xem lịch sử trong Admin Dashboard", "Lịch Sử", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xem lịch sử: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== MENU MODE HANDLERS ====================
        private void MenuModeBasic_Click(object? sender, EventArgs e)
        {
            try
            {
                // Hide parking controls
                gbParking.Visible = false;
                MessageBox.Show("Đã chuyển sang Basic Mode", "Chế Độ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chuyển chế độ: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MenuModeParking_Click(object? sender, EventArgs e)
        {
            try
            {
                // Show parking controls
                gbParking.Visible = true;
                MessageBox.Show("Đã chuyển sang Parking Mode", "Chế Độ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chuyển chế độ: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== MENU DATA HANDLERS ====================
        private void MenuDataExport_Click(object? sender, EventArgs e)
        {
            try
            {
                btnExportExcel.PerformClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất báo cáo: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MenuDataAdmin_Click(object? sender, EventArgs e)
        {
            try
            {
                btnAdminDashboard.PerformClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở Admin Dashboard: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== MENU TOOLS HANDLERS ====================
        private void MenuToolsOptions_Click(object? sender, EventArgs e)
        {
            try
            {
                MenuViewSettings_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở tùy chọn: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== MENU HELP HANDLERS ====================
        private void MenuHelpAbout_Click(object? sender, EventArgs e)
        {
            try
            {
                var aboutInfo = @"🚗 HỆ THỐNG GIÁM SÁT GIAO THÔNG AI

📌 THÔNG TIN HỆ THỐNG:
   Phiên bản: 2.0.0
   Mô hình AI: YOLOv8 (ONNX Runtime)
   Framework: .NET 6.0 + OpenCV
   Ngày phát hành: 25/11/2025

🎯 CHỨC NĂNG CHÍNH:
   ✅ Phát hiện 5 loại phương tiện
   ✅ Theo dõi và đếm xe thông minh
   ✅ Phân tích thống kê chi tiết
   ✅ Xuất báo cáo CSV/Excel
   ✅ Xử lý Video/Camera/Ảnh
   ✅ Chọn vùng phát hiện tùy chỉnh

🚀 TÍNH NĂNG NỔI BẬT:
   • AI Model: YOLOv8n (Nhanh & Chính xác)
   • Real-time Processing
   • GPU Acceleration Support
   • Advanced Vehicle Refinement
   • Frame Optimization
   • Alert System

👨‍💻 PHÁT TRIỂN BỞI:
   Nguyễn Ngọc Hiếu

📧 HỖ TRỢ:
   Email: bimax12052005@gmail.com
   Website: www.nguyenngochieu.com

© 2025 Traffic Monitor AI System. All rights reserved.";

                MessageBox.Show(aboutInfo, "🔍 Giới Thiệu Hệ Thống", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị thông tin: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Menu handler for User Guide / Help
        /// </summary>
        private void MenuHelpGuide_Click(object? sender, EventArgs e)
        {
            try
            {
                var helpMessage = @"📖 HƯỚNG DẪN SỬ DỤNG HỆ THỐNG

1️⃣ CHỌN CHẾ ĐỘ NGUỒN:
   • 📹 Video: Phát hiện từ file video
   • 📷 Camera: Phát hiện từ camera trực tiếp
   • 🖼️ Ảnh: Phát hiện từ hình ảnh

2️⃣ TẢI DỮ LIỆU:
   • Chọn file ONNX model AI (.onnx)
   • Nhấn nút 'Tải Model AI' để tải mô hình
   • Nhấn 'Tải Dữ Liệu' để kiểm tra cấu hình

3️⃣ THIẾT LẬP THAM SỐ:
   • Độ tin cậy: 0.25 (mặc định) - Tăng lên để chính xác hơn
   • IOU: 0.45 (mặc định) - Ngưỡng trùng lặp
   • Skip Frames: 2 (mặc định) - Bỏ qua khung hình

4️⃣ CHỌN VÙNG PHÁT HIỆN:
   • Click chuột trái để chọn các điểm
   • Nhấn Enter để hoàn thành
   • Nhấn ESC để hủy bỏ

5️⃣ ĐIỀU KHIỂN:
   • ▶️ Bắt Đầu: Khởi động phát hiện
   • ⏸️ Tạm Dừng: Dừng tạm thời
   • ⏹️ Dừng: Kết thúc quá trình

6️⃣ XUẤT BÁO CÁO:
   • Chọn đường dẫn lưu file
   • Bao cáo tự động xuất khi kết thúc
   • Hỗ trợ Excel, JSON, TXT

💡 MẸO:
   • Di chuột qua các nút để xem chi tiết
   • Sử dụng Hẹn Giờ để tự động khởi động
   • Bật Quản Lý Bãi Xe cho chế độ đỗ xe
   • Kiểm tra lịch sử trong Admin Dashboard";

                MessageBox.Show(helpMessage, "📖 Hướng Dẫn Sử Dụng", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị hướng dẫn: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Menu handler for Parameter Tuning Guide
        /// </summary>
        private void MenuHelpParameterGuide_Click(object? sender, EventArgs e)
        {
            try
            {
                var settingsGuide = @"⚙️ HƯỚNG DẪN TUY CHỈNH THAM SỐ

📊 ĐỘ TIN CẬY (Confidence Threshold):
   • Mặc định: 0.25
   • Thấp (0.15-0.25): Phát hiện nhiều hơn, có thể có lỗi dương tính
   • Cao (0.35-0.50): Chính xác hơn, có thể bỏ sót đối tượng
   • Khuyên nghị: 0.25 cho giao thông đô thị

🎯 IOU (Intersection over Union):
   • Mặc định: 0.45
   • Thấp (0.30-0.40): Cho phép các vùng trùng lặp nhiều hơn
   • Cao (0.50-0.70): Nghiêm ngặt hơn, giảm trùng lặp
   • Khuyên nghị: 0.45 cho giao thông đông đúc

🎬 FRAME SKIP:
   • Mặc định: 2 (xử lý mỗi frame thứ 2)
   • Tăng lên (3-5): Tăng tốc độ, giảm chính xác
   • Giảm xuống (0-1): Chậm hơn, chính xác hơn
   • Khuyên nghị: 2 cho độ cân bằng tốt

📷 CAMERA INDEX:
   • Mặc định: 0 (camera mặc định của hệ thống)
   • Thay đổi nếu có nhiều camera
   • Giá trị: 0, 1, 2... (theo số camera)

🌐 MÔ HÌNH AI:
   • YOLOv8 Nano (n): Nhanh nhất, ít chính xác
   • YOLOv8 Small (s): Cân bằng tốt
   • YOLOv8 Medium (m): Chính xác hơn, chậm hơn
   • YOLOv8 Large (l): Rất chính xác, chậm

💡 LƯU Ý:
   • Lưu cấu hình tự động khi đóng ứng dụng
   • Thử nghiệm với các giá trị khác nhau để tìm tối ưu
   • Kết quả phụ thuộc vào chất lượng video/camera
   • GPU sẽ tăng tốc độ xử lý đáng kể

⚡ TỐI ƯU HÓA HIỆU NĂNG:
   • Giảm skip frames để tăng độ chính xác
   • Tăng skip frames để tăng tốc độ
   • Sử dụng GPU nếu có sẵn
   • Chọn vùng phát hiện nhỏ hơn để giảm tải
   • Giảm độ phân giải video input
   • Sử dụng Frame Skip = 3-5 cho video HD";

                MessageBox.Show(settingsGuide, "⚙️ Hướng Dẫn Tuy Chỉnh Tham Số", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị hướng dẫn tham số: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Menu handler for GPU/CPU Settings Guide
        /// </summary>
        private void MenuHelpGPUSettings_Click(object? sender, EventArgs e)
        {
            try
            {
                var gpuGuide = @"🚀 HƯỚNG DẪN CẤU HÌNH GPU/CPU

⚙️ CHỌN THIẾT BỊ XỬ LÝ:

CPU MODE (Mặc định):
   ✅ Ưu điểm:
   • Hoạt động trên mọi máy tính
   • Không cần cài đặt driver riêng
   • Ổn định, không có vấn đề tương thích
   
   ❌ Nhược điểm:
   • Xử lý chậm (20-30 FPS)
   • Tốn điện năng cao
   • Không phù hợp cho real-time HD

GPU MODE (NVIDIA CUDA/OpenGL):
   ✅ Ưu điểm:
   • Xử lý siêu nhanh (60-120+ FPS)
   • Tiết kiệm điện năng
   • Phù hợp cho real-time HD/4K
   • Hỗ trợ xử lý song song
   
   ❌ Nhược điểm:
   • Cần GPU NVIDIA với CUDA hỗ trợ
   • Phải cài đặt NVIDIA Driver + CUDA Toolkit
   • Tiêu thụ VRAM (2-6 GB)
   • Không hỗ trợ GPU AMD/Intel

📊 SO SÁNH HIỆU NĂNG:

Xử lý Video Full HD (1920x1080):
   CPU i5-10400:        ~15-20 FPS  (không thực tế)
   CPU i9-13900K:       ~30-40 FPS  (có thể chấp nhận)
   GPU RTX 3060:        ~80-100 FPS (tuyệt vời)
   GPU RTX 4090:        ~300+ FPS   (rất nhanh)

🔧 CẤU HÌNH GPU (NVIDIA):

Bước 1: Kiểm tra GPU
   • Mở Device Manager
   • Tìm NVIDIA Graphics Card
   • Kiểm tra Driver version (phải >= 470)

Bước 2: Cài đặt Driver
   • Tải từ: https://www.nvidia.com/Download/driverDetails.aspx
   • Cài đặt NVIDIA Driver
   • Khởi động lại máy tính

Bước 3: Cài đặt CUDA Toolkit
   • Tải từ: https://developer.nvidia.com/cuda-downloads
   • Chọn phiên bản phù hợp
   • Cài đặt theo hướng dẫn

Bước 4: Cấu hình trong ứng dụng
   • Mở Settings
   • Chọn GPU Mode
   • Chọn GPU device ID (thường là 0)
   • Lưu và khởi động lại

🎯 CẤU HÌNH TỐI ƯU:

Cho máy tính phổ thông:
   • CPU Mode
   • Skip Frames: 3-5
   • Độ tin cậy: 0.25
   • Độ phân giải: 640x640

Cho máy tính gaming:
   • GPU Mode (nếu có NVIDIA)
   • Skip Frames: 1-2
   • Độ tin cậy: 0.25
   • Độ phân giải: 1280x1280

Cho máy chủ xử lý:
   • GPU Mode (multiple GPUs)
   • Skip Frames: 0
   • Độ tin cậy: 0.2
   • Độ phân giải: 1920x1920

⚡ TIẾT KIỆM ĐIỆN NĂNG:
   • Sử dụng GPU thay vì CPU (tiết kiệm 30-40%)
   • Tăng Skip Frames (giảm xử lý)
   • Giảm độ phân giải
   • Sử dụng Batch Processing (nếu hỗ trợ)

🐛 KHẮC PHỤC SỰ CỐ:

GPU không được phát hiện:
   • Cập nhật NVIDIA Driver
   • Cài đặt CUDA Toolkit
   • Kiểm tra NVIDIA GPU Computing Capability >= 3.5

Lỗi Out of Memory:
   • Giảm độ phân giải input
   • Tăng Skip Frames
   • Đóng các ứng dụng khác
   • Nâng cấp GPU (VRAM lớn hơn)

Performance thấp:
   • Kiểm tra GPU Load (task manager)
   • Cập nhật Driver
   • Cấu hình Power Settings
   • Kiểm tra nhiệt độ GPU";

                MessageBox.Show(gpuGuide, "🚀 Hướng Dẫn GPU/CPU Settings", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị hướng dẫn GPU: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
