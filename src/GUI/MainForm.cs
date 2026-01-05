using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using TrafficMonitorApp.Data;
using TrafficMonitorApp.Models;
using TrafficMonitorApp.Services;
using TrafficMonitorApp.GUI;

namespace TrafficMonitorApp
{
    public partial class MainForm : Form
    {
        private AppConfig _config;
        private VehicleDetector? _detector;
        private VideoProcessor _processor;
        private List<System.Drawing.Point> _detectionZone = new List<System.Drawing.Point>();
        private List<System.Drawing.Point> _tempZonePoints = new List<System.Drawing.Point>();
        private bool _isSelectingZone = false;
        private bool _isSelectingLine = false;
        private System.Drawing.Point _countingLineStart = System.Drawing.Point.Empty;
        private System.Drawing.Point _countingLineEnd = System.Drawing.Point.Empty;
        private Bitmap? _currentFrame;
        private Bitmap? _originalFrame;
        private string _inputMode = "video"; // video, camera, image
        private bool _dataLoaded = false;
        private System.Drawing.Point _lastMousePos;
        private DateTime _scheduledTime;
        private bool _scheduleEnabled = false;
        private TrafficHistoryManager _historyManager;
        private ParkingManager _parkingManager;
        // Reserved for future FPS control implementation
        #pragma warning disable CS0414
        private int _targetFPS = 30;
        #pragma warning restore CS0414
        private bool _isSeekingVideo = false;
        
        // Database fields
        private TrafficDbContext? _dbContext;
        private TrafficRepository? _repository;
        private TrafficSessionDb? _currentSession;
        private DateTime _sessionStartTime;
        
        // Authentication field
        private AuthenticationService _authService;

        public MainForm(AuthenticationService authService)
        {
            _authService = authService;
            
            InitializeComponent();
            _config = AppConfig.Load();
            _processor = new VideoProcessor();
            _historyManager = new TrafficHistoryManager();
            _parkingManager = new ParkingManager();
            
            // Initialize database
            InitializeDatabase();
            
            InitializeEventHandlers();
            LoadConfiguration();
            SetupUI();
            
            // Update title with user info
            UpdateTitleWithUser();
            
            // Set default view mode (Basic Mode)
            InitializeDefaultViewMode();
            
            // Configure SplitContainer after form is fully displayed
            this.Shown += (s, e) => {
                try
                {
                    // Now form and splitContainer have actual dimensions
                    int availableWidth = splitContainer.Width;
                    
                    // Tăng kích thước panel bên trái để chứa các GroupBox rộng hơn (550px)
                    // Set larger minimum size for left panel (now containing 550px wide groupboxes)
                    int leftPanelMinSize = 600;  // Tăng từ 300 lên 600
                    int rightPanelMinSize = 600; // Tăng từ 500 lên 600
                    
                    // Tính kích thước an toàn
                    splitContainer.Panel1MinSize = Math.Min(leftPanelMinSize, availableWidth / 2);
                    splitContainer.Panel2MinSize = Math.Min(rightPanelMinSize, availableWidth / 2);
                    
                    // Calculate and set splitter distance (40% left, 60% right)
                    int desiredDistance = (int)(availableWidth * 0.40);
                    int minDist = splitContainer.Panel1MinSize;
                    int maxDist = availableWidth - splitContainer.Panel2MinSize - splitContainer.SplitterWidth;
                    
                    if (maxDist > minDist && desiredDistance >= minDist && desiredDistance <= maxDist)
                    {
                        splitContainer.SplitterDistance = desiredDistance;
                    }
                    else if (desiredDistance < minDist)
                    {
                        // Nếu không đủ không gian, sử dụng kích thước tối thiểu
                        splitContainer.SplitterDistance = minDist;
                    }
                }
                catch (Exception ex)
                {
                    // Fallback: use default layout
                    System.Diagnostics.Debug.WriteLine($"SplitContainer setup warning: {ex.Message}");
                }
            };
        }

        private void InitializeEventHandlers()
        {
            _processor.FrameProcessed += OnFrameProcessed;
            _processor.StatisticsUpdated += OnStatisticsUpdated;
            _processor.ProcessingCompleted += OnProcessingCompleted;
            _processor.VehicleDetected += OnVehicleDetected;
            
            // Áp dụng phân quyền dựa trên vai trò người dùng
            // Apply permission based on user role
            ApplyUserPermissions();
        }

        /// <summary>
        /// Áp dụng phân quyền cho các chức năng theo vai trò người dùng
        /// Apply permissions for features based on user role
        /// </summary>
        private void ApplyUserPermissions()
        {
            bool isAdmin = _authService.IsAdmin();
            
            if (!isAdmin)
            {
                // User bình thường - Có thể xuất báo cáo và thay đổi model, nhưng không truy cập database
                // Regular user - Can export reports and change model, but no database access
                
                // Vô hiệu hóa nút Admin Dashboard (không thể truy cập quản lý)
                // Disable Admin Dashboard button (cannot access management)
                if (btnAdminDashboard != null) 
                {
                    btnAdminDashboard.Visible = false;
                    btnAdminDashboard.Enabled = false;
                }
                
                // User CÓ THỂ thay đổi các thông số model và config
                // User CAN change model parameters and config
                // Không disable các controls này nữa
                
                // Thêm thông báo giới hạn quyền
                // Add permission notice
                var lblPermission = new Label
                {
                    Text = "👤 Tài khoản User - Có thể xuất báo cáo và thay đổi model, không truy cập quản lý hệ thống",
                    AutoSize = true,
                    ForeColor = Color.DarkOrange,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Location = new System.Drawing.Point(10, 5),
                    BackColor = Color.LightYellow,
                    Padding = new Padding(5)
                };
                
                // Tìm panel chứa các controls để thêm label
                if (this.Controls.Count > 0)
                {
                    var mainPanel = this.Controls[0];
                    if (mainPanel is Panel || mainPanel is SplitContainer)
                    {
                        lblPermission.Location = new System.Drawing.Point(
                            mainPanel.Width / 2 - 250, 
                            5
                        );
                    }
                }
                
                this.Controls.Add(lblPermission);
                lblPermission.BringToFront();
            }
            else
            {
                // Admin - Toàn quyền truy cập
                // Admin - Full access
                UpdateTitleWithUser(); // Cập nhật title hiển thị "Admin"
                
                // Hiện nút Admin Dashboard cho Admin
                // Show Admin Dashboard button for Admin
                if (btnAdminDashboard != null)
                {
                    btnAdminDashboard.Visible = true;
                }
            }
        }

        private void LoadConfiguration()
        {
            txtModelPath.Text = _config.ModelPath;
            
            // Handle both formats: 0-1 (e.g., 0.25) or 0-100 (e.g., 25)
            decimal confValue = (decimal)_config.ConfidenceThreshold;
            decimal iouValue = (decimal)_config.IouThreshold;
            
            // If values are in 0-1 range, convert to percentage
            if (confValue <= 1.0m) confValue *= 100;
            if (iouValue <= 1.0m) iouValue *= 100;
            
            // Clamp to valid range (0-100)
            numConfidence.Value = Math.Min(100, Math.Max(0, confValue));
            numIOU.Value = Math.Min(100, Math.Max(0, iouValue));
            
            numSkipFrames.Value = _config.SkipFrames;
            numCamera.Value = _config.CameraIndex;
            txtExportPath.Text = _config.ExportPath;
            chkUseCountingLine.Checked = _config.UseCountingLine;

            if (_config.DetectionZone.Count > 0)
            {
                _detectionZone = _config.DetectionZone.Select(p => 
                    new System.Drawing.Point(p.X, p.Y)).ToList();
            }
            
            if (_config.CountingLineStart.X != 0 && _config.CountingLineEnd.X != 0)
            {
                _countingLineStart = new System.Drawing.Point(_config.CountingLineStart.X, _config.CountingLineStart.Y);
                _countingLineEnd = new System.Drawing.Point(_config.CountingLineEnd.X, _config.CountingLineEnd.Y);
            }
        }

        private void SetupUI()
        {
            UpdateTitleWithUser();
            
            pbVideo.Paint += PictureBox_Paint;
            
            UpdateInputMode();
            UpdateButtonStates();
        }
        
        /// <summary>
        /// Cập nhật tiêu đề form với thông tin người dùng
        /// Update form title with user information
        /// </summary>
        private void UpdateTitleWithUser()
        {
            string userInfo = _authService.IsLoggedIn 
                ? $" - Người dùng: {_authService.CurrentUser?.FullName} ({_authService.CurrentUser?.Role})"
                : "";
            this.Text = $"Traffic Monitor App - Giám Sát Giao Thông AI v2.0{userInfo}";
        }

        private void PictureBox_Paint(object? sender, PaintEventArgs e)
        {
            if (pbVideo.Image == null)
            {
                string message = _dataLoaded 
                    ? "Dang xu ly..." 
                    : "Vui long tai du lieu de hien thi";
                
                using (var font = new Font("Segoe UI", 14))
                using (var brush = new SolidBrush(Color.White))
                {
                    var size = e.Graphics.MeasureString(message, font);
                    var x = (pbVideo.Width - size.Width) / 2;
                    var y = (pbVideo.Height - size.Height) / 2;
                    e.Graphics.DrawString(message, font, brush, x, y);
                }
            }
        }

        private void UpdateInputMode()
        {
            _inputMode = rbVideo.Checked ? "video" : 
                        rbCamera.Checked ? "camera" : "image";

            // Update path display based on mode
            switch (_inputMode)
            {
                case "video":
                    txtDataPath.Text = _config.VideoPath;
                    btnBrowseData.Enabled = true;
                    break;
                case "image":
                    txtDataPath.Text = _config.ImagePath;
                    btnBrowseData.Enabled = true;
                    break;
                case "camera":
                    txtDataPath.Text = $"Camera {numCamera.Value}";
                    btnBrowseData.Enabled = false;
                    break;
            }
            
            // Reset data loaded flag when changing mode
            _dataLoaded = false;
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool isProcessing = _processor.IsProcessing;
            bool hasModel = _detector != null;
            
            // Model can always be loaded/reloaded
            btnLoadModel.Enabled = !isProcessing;
            
            // Data can be loaded if not processing
            btnLoadData.Enabled = !isProcessing;
            
            // Start requires model loaded and data loaded
            btnStart.Enabled = !isProcessing && hasModel && _dataLoaded;
            
            // Pause/Stop only when processing and not image mode
            btnPause.Enabled = isProcessing && _inputMode != "image";
            btnStop.Enabled = isProcessing;
            
            // Zone selection based on mode
            bool useLineMode = chkUseCountingLine.Checked;
            
            if (useLineMode)
            {
                // Line mode: only show line button and reset
                btnSelectLine.Enabled = !isProcessing && _dataLoaded;
                btnSelectLine.Visible = true;
                btnSelectZone.Visible = false;
                btnShowZone.Visible = false;
                btnResetZone.Enabled = !isProcessing && _countingLineStart != System.Drawing.Point.Empty;
            }
            else
            {
                // Big zone mode: show zone buttons
                btnSelectZone.Enabled = !isProcessing && _dataLoaded;
                btnSelectZone.Visible = true;
                btnSelectLine.Visible = false;
                btnShowZone.Visible = _detectionZone.Count >= 3;
                btnShowZone.Enabled = !isProcessing && _dataLoaded;
                btnResetZone.Enabled = !isProcessing && _detectionZone.Count > 0;
            }
            
            // Update pause button text
            btnPause.Text = _processor.IsPaused ? "Tiep tuc" : "Tam dung";
        }

        // Event Handlers
        private void btnBrowseModel_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "YOLO Models (*.pt;*.weights;*.onnx)|*.pt;*.weights;*.onnx|All files (*.*)|*.*";
                dialog.Title = "Chon file model YOLO";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtModelPath.Text = dialog.FileName;
                    _config.ModelPath = dialog.FileName;
                }
            }
        }

        private void btnBrowseVideo_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Video files (*.mp4;*.avi;*.mov;*.mkv)|*.mp4;*.avi;*.mov;*.mkv|All files (*.*)|*.*";
                dialog.Title = "Chon file video";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _config.VideoPath = dialog.FileName;
                    lblStatus.Text = $"Da chon video: {Path.GetFileName(dialog.FileName)}";
                    lblStatus.ForeColor = Color.Green;
                }
            }
        }

        private void btnBrowseImage_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*";
                dialog.Title = "Chon file anh";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _config.ImagePath = dialog.FileName;
                    lblStatus.Text = $"Da chon anh: {Path.GetFileName(dialog.FileName)}";
                    lblStatus.ForeColor = Color.Green;
                }
            }
        }

        private void btnBrowseData_Click(object sender, EventArgs e)
        {
            if (_inputMode == "video")
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Video files (*.mp4;*.avi;*.mov;*.mkv)|*.mp4;*.avi;*.mov;*.mkv|All files (*.*)|*.*";
                    dialog.Title = "Chon file video";
                    
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _config.VideoPath = dialog.FileName;
                        txtDataPath.Text = dialog.FileName;
                        lblStatus.Text = $"Da chon video: {Path.GetFileName(dialog.FileName)}";
                        lblStatus.ForeColor = Color.Green;
                    }
                }
            }
            else if (_inputMode == "image")
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*";
                    dialog.Title = "Chon file anh";
                    
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _config.ImagePath = dialog.FileName;
                        txtDataPath.Text = dialog.FileName;
                        lblStatus.Text = $"Da chon anh: {Path.GetFileName(dialog.FileName)}";
                        lblStatus.ForeColor = Color.Green;
                    }
                }
            }
            else // camera
            {
                txtDataPath.Text = $"Camera {numCamera.Value}";
                lblStatus.Text = "Che do Camera";
                lblStatus.ForeColor = Color.Green;
            }
        }

        private void btnBrowseExport_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Chon thu muc xuat bao cao";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtExportPath.Text = dialog.SelectedPath;
                    _config.ExportPath = dialog.SelectedPath;
                }
            }
        }

        private async void btnLoadModel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtModelPath.Text) || !File.Exists(txtModelPath.Text))
            {
                MessageBox.Show("Vui long chon file model hop le!", "Canh bao", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Console.WriteLine($"[MainForm] Loading model from: {txtModelPath.Text}");
                lblStatus.Text = "Dang tai model...";
                lblStatus.ForeColor = Color.Orange;
                UpdateButtonStates();
                Application.DoEvents();

                UpdateConfiguration();
                Console.WriteLine($"[MainForm] Confidence: {_config.ConfidenceThreshold}, IOU: {_config.IouThreshold}");

                await Task.Run(() =>
                {
                    _detector?.Dispose();
                    _detector = new VehicleDetector(
                        txtModelPath.Text,
                        _config.ConfidenceThreshold,
                        _config.IouThreshold
                    );
                });
                
                Console.WriteLine("[MainForm] Model loaded successfully");

                lblStatus.Text = "Model da duoc tai thanh cong!";
                lblStatus.ForeColor = Color.Green;
                MessageBox.Show("Model da duoc tai thanh cong!", "Thanh cong", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Loi tai model";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"Khong the tai model:\n{ex.Message}", "Loi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _detector = null;
                UpdateButtonStates();
            }
        }

        private void btnLoadData_Click(object sender, EventArgs e)
        {
            try
            {
                string path = _inputMode switch
                {
                    "camera" => numCamera.Value.ToString(),
                    "image" => _config.ImagePath,
                    _ => _config.VideoPath
                };

                Console.WriteLine($"[MainForm] Loading data - Mode: {_inputMode}, Path: {path}");

                if (_inputMode != "camera" && (string.IsNullOrWhiteSpace(path) || !File.Exists(path)))
                {
                    MessageBox.Show("Vui long chon nguon du lieu hop le!", "Canh bao", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                lblStatus.Text = "Dang tai du lieu...";
                lblStatus.ForeColor = Color.Orange;
                Application.DoEvents();

                // Load first frame for preview
                if (_inputMode == "image")
                {
                    using (var mat = Cv2.ImRead(path))
                    {
                        if (mat.Empty())
                            throw new Exception("Khong the doc file anh");
                            
                        _originalFrame?.Dispose();
                        _originalFrame = BitmapConverter.ToBitmap(mat);
                        _currentFrame?.Dispose();
                        _currentFrame = new Bitmap(_originalFrame);
                        DisplayFrame(_currentFrame);
                    }
                }
                else
                {
                    int source = _inputMode == "camera" ? (int)numCamera.Value : 0;
                    using (var cap = _inputMode == "camera" 
                        ? new VideoCapture(source) 
                        : new VideoCapture(path))
                    {
                        if (!cap.IsOpened())
                            throw new Exception("Khong the mo nguon video");

                        // Get video info for trackbar
                        if (_inputMode == "video")
                        {
                            int totalFrames = (int)cap.Get(VideoCaptureProperties.FrameCount);
                            if (totalFrames > 0)
                            {
                                trackBarVideo.Maximum = totalFrames;
                                trackBarVideo.Value = 0;
                                trackBarVideo.Enabled = true;
                                progressVideo.Maximum = 100;
                                progressVideo.Value = 0;
                            }
                        }
                        else
                        {
                            trackBarVideo.Enabled = false;
                        }

                        using (var mat = new Mat())
                        {
                            cap.Read(mat);
                            if (!mat.Empty())
                            {
                                _originalFrame?.Dispose();
                                _originalFrame = BitmapConverter.ToBitmap(mat);
                                _currentFrame?.Dispose();
                                _currentFrame = new Bitmap(_originalFrame);
                                DisplayFrame(_currentFrame);
                            }
                            else
                            {
                                throw new Exception("Khong the doc khung hinh tu nguon");
                            }
                        }
                    }
                }

                _dataLoaded = true;
                Console.WriteLine($"[MainForm] Data loaded successfully - Mode: {_inputMode}");
                lblStatus.Text = "Da tai du lieu thanh cong";
                lblStatus.ForeColor = Color.Green;
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                _dataLoaded = false;
                lblStatus.Text = "Loi tai du lieu";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"Khong the tai du lieu:\n{ex.Message}", "Loi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateButtonStates();
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (_detector == null)
            {
                MessageBox.Show("Vui long tai model truoc!", "Canh bao", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_dataLoaded)
            {
                MessageBox.Show("Vui long tai du lieu truoc!", "Canh bao", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Console.WriteLine($"[MainForm] Starting processing - Mode: {_inputMode}");
                UpdateConfiguration();
                
                Console.WriteLine($"[MainForm] Config - Conf: {_config.ConfidenceThreshold}, IOU: {_config.IouThreshold}, SkipFrames: {_config.SkipFrames}");
                Console.WriteLine($"[MainForm] Use counting line: {_config.UseCountingLine}");
                
                string source = _inputMode switch
                {
                    "camera" => numCamera.Value.ToString(),
                    "image" => _config.ImagePath,
                    _ => _config.VideoPath
                };

                bool isCamera = _inputMode == "camera";
                var zone = _detectionZone.Count >= 3 
                    ? _detectionZone.Select(p => new OpenCvSharp.Point(p.X, p.Y)).ToList() 
                    : null;

                lblStatus.Text = "Đang xử lý...";
                lblStatus.ForeColor = Color.Blue;
                
                // Reset statistics
                txtTotalCount.Text = "0";
                txtCarCount.Text = "0";
                txtMotorCount.Text = "0";
                txtBusCount.Text = "0";
                txtBicycleCount.Text = "0";
                lblFPS.Text = "FPS: 0";
                
                // Create database session
                await CreateDatabaseSessionAsync(source, isCamera);

                if (_inputMode == "image")
                {
                    UpdateButtonStates();
                    var result = _processor.ProcessSingleImage(source, zone);
                    if (result != null)
                    {
                        _currentFrame?.Dispose();
                        _currentFrame = BitmapConverter.ToBitmap(result);
                        DisplayFrame(_currentFrame);
                        result.Dispose();
                        
                        // Update statistics for image
                        UpdateStatisticsDisplay(_processor.Statistics);
                        
                        ShowCompletionDialog();
                    }
                }
                else
                {
                    // Start async processing - this returns immediately
                    var task = _processor.StartProcessingAsync(source, _detector, zone, 
                        (int)numSkipFrames.Value, isCamera);
                    
                    // Update button states AFTER starting processing
                    UpdateButtonStates();
                    
                    // Wait for completion
                    await task;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi xu ly:\n{ex.Message}", "Loi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Loi xu ly";
                lblStatus.ForeColor = Color.Red;
                UpdateButtonStates();
            }
            finally
            {
                // Ensure button states are correct after processing ends
                UpdateButtonStates();
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (_processor.IsPaused)
            {
                _processor.Resume();
                lblStatus.Text = "Dang xu ly...";
                lblStatus.ForeColor = Color.Blue;
            }
            else
            {
                _processor.Pause();
                lblStatus.Text = "Da tam dung";
                lblStatus.ForeColor = Color.Orange;
            }
            UpdateButtonStates();
        }

        private async void btnStop_Click(object sender, EventArgs e)
        {
            _processor.Stop();
            lblStatus.Text = "Dang luu du lieu...";
            lblStatus.ForeColor = Color.Orange;
            
            // Wait a moment for any pending detection saves to complete
            await Task.Delay(500);
            
            // Update database session when manually stopped
            await UpdateDatabaseSessionAsync();
            
            lblStatus.Text = "Da dung xu ly - Du lieu da duoc luu";
            lblStatus.ForeColor = Color.Orange;
            UpdateButtonStates();
            
            // Show summary
            if (_processor.Statistics.TotalVehicles > 0)
            {
                var stats = _processor.Statistics;
                MessageBox.Show(
                    $"Ket thuc phien giam sat!\n\n" +
                    $"Tong so xe: {stats.TotalVehicles}\n" +
                    $"Khung hinh xu ly: {stats.ProcessedFrames}\n" +
                    $"Thoi gian: {stats.ProcessingTime:F2}s\n" +
                    $"FPS trung binh: {stats.AverageFPS:F2}\n\n" +
                    $"Du lieu da duoc luu vao co so du lieu.",
                    "Phien giam sat ket thuc",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void btnSelectZone_Click(object sender, EventArgs e)
        {
            if (!_dataLoaded || _originalFrame == null)
            {
                MessageBox.Show("Vui long tai du lieu truoc!", "Canh bao", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isSelectingZone = true;
            _tempZonePoints.Clear();
            
            // Reset to original frame without any annotations
            _currentFrame?.Dispose();
            _currentFrame = new Bitmap(_originalFrame);
            DisplayFrame(_currentFrame);
            
            lblStatus.Text = "Click chuot trai de chon diem, chuot phai de hoan thanh (toi thieu 3 diem)";
            lblStatus.ForeColor = Color.Yellow;
            
            MessageBox.Show(
                "Huong dan chon vung:\n\n" +
                "✅ CHI QUET TRONG VUNG\n" +
                "✅ DEM NGAY KHI PHAT HIEN\n\n" +
                "Cach chon:\n" +
                "• Click chuot TRAI: Them diem\n" +
                "• Click chuot PHAI: Hoan thanh (it nhat 3 diem)\n" +
                "• ESC: Huy bo\n\n" +
                "Luu y: Vung nho tiet kiem CPU hon!",
                "Chon vung dem - Big Zone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnResetZone_Click(object sender, EventArgs e)
        {
            _detectionZone.Clear();
            _tempZonePoints.Clear();
            _isSelectingZone = false;
            _isSelectingLine = false;
            _countingLineStart = System.Drawing.Point.Empty;
            _countingLineEnd = System.Drawing.Point.Empty;
            
            if (_originalFrame != null)
            {
                _currentFrame?.Dispose();
                _currentFrame = new Bitmap(_originalFrame);
                DisplayFrame(_currentFrame);
            }
            
            // Save empty zone to config
            UpdateConfiguration();
            
            lblStatus.Text = chkUseCountingLine.Checked ? "Da reset duong dem" : "Da reset vung dem";
            lblStatus.ForeColor = Color.Green;
            UpdateButtonStates();
        }

        private void btnShowZone_Click(object sender, EventArgs e)
        {
            if (!_dataLoaded || _originalFrame == null)
            {
                MessageBox.Show("Vui long tai du lieu truoc!", "Canh bao", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_detectionZone.Count < 3)
            {
                MessageBox.Show("Chua co vung dem nao duoc chon!", "Thong bao", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DrawZoneOnFrame(_detectionZone, true);
            lblStatus.Text = $"Hien thi vung dem voi {_detectionZone.Count} diem";
            lblStatus.ForeColor = Color.Green;
        }

        private void OnFrameProcessed(object? sender, Mat frame)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnFrameProcessed(sender, frame)));
                return;
            }

            try
            {
                _currentFrame?.Dispose();
                _currentFrame = BitmapConverter.ToBitmap(frame);
                DisplayFrame(_currentFrame);
                frame.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Frame display error: {ex.Message}");
            }
        }

        private void OnStatisticsUpdated(object? sender, TrafficStatistics stats)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnStatisticsUpdated(sender, stats)));
                return;
            }

            UpdateStatisticsDisplay(stats);
            
            // Update trackbar and progress if not seeking
            if (!_isSeekingVideo && trackBarVideo.Enabled)
            {
                int currentFrame = stats.ProcessedFrames;
                if (currentFrame <= trackBarVideo.Maximum)
                {
                    trackBarVideo.Value = currentFrame;
                    progressVideo.Value = (int)((double)currentFrame / trackBarVideo.Maximum * 100);
                    
                    // Update time label
                    double fps = stats.AverageFPS > 0 ? stats.AverageFPS : 30;
                    int currentSec = (int)(currentFrame / fps);
                    int totalSec = (int)(trackBarVideo.Maximum / fps);
                    lblVideoTime.Text = $"{currentSec / 60:D2}:{currentSec % 60:D2} / {totalSec / 60:D2}:{totalSec % 60:D2}";
                }
            }
        }

        private async void OnProcessingCompleted(object? sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(async () => await OnProcessingCompletedAsync()));
                return;
            }

            await OnProcessingCompletedAsync();
        }
        
        private async Task OnProcessingCompletedAsync()
        {
            lblStatus.Text = "Xu ly hoan tat";
            lblStatus.ForeColor = Color.Green;
            UpdateButtonStates();
            
            // Save session to history
            string sourceType = _inputMode; // "video", "camera", "image"
            string sourcePath = _inputMode == "camera" ? $"Camera {numCamera.Value}" : 
                               _inputMode == "image" ? _config.ImagePath : _config.VideoPath;
            _historyManager.SaveSession(_processor.Statistics, _processor.AdvancedStats, sourceType, sourcePath);
            
            // Update database session
            await UpdateDatabaseSessionAsync();
            
            ShowCompletionDialog();
        }

        private void UpdateStatisticsDisplay(TrafficStatistics stats)
        {
            txtTotalCount.Text = stats.TotalVehicles.ToString("N0");
            lblFPS.Text = $"FPS: {stats.AverageFPS:F1}";
            
            // Update vehicle type counts
            UpdateVehicleTypeCounts(stats.VehicleCounts);
        }

        private void UpdateVehicleTypeCounts(Dictionary<string, int> counts)
        {
            txtCarCount.Text = counts.ContainsKey(VehicleType.Car) ? counts[VehicleType.Car].ToString("N0") : "0";
            txtMotorCount.Text = counts.ContainsKey(VehicleType.Motorcycle) ? counts[VehicleType.Motorcycle].ToString("N0") : "0";
            txtBusCount.Text = counts.ContainsKey(VehicleType.Bus) ? counts[VehicleType.Bus].ToString("N0") : "0";
            txtBicycleCount.Text = counts.ContainsKey(VehicleType.Bicycle) ? counts[VehicleType.Bicycle].ToString("N0") : "0";
        }

        private void DisplayFrame(Bitmap frame)
        {
            if (pbVideo.InvokeRequired)
            {
                pbVideo.Invoke(new Action(() => DisplayFrame(frame)));
                return;
            }

            if (pbVideo.Image != null && pbVideo.Image != frame)
            {
                var oldImage = pbVideo.Image;
                pbVideo.Image = null;
                oldImage.Dispose();
            }
            pbVideo.Image = new Bitmap(frame);
        }

        private void DrawZoneOnFrame(List<System.Drawing.Point> points, bool showLabels)
        {
            if (_originalFrame == null || points.Count < 3) return;

            _currentFrame?.Dispose();
            _currentFrame = new Bitmap(_originalFrame);

            using (var g = Graphics.FromImage(_currentFrame))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw polygon
                using (var pen = new Pen(Color.Yellow, 3))
                {
                    g.DrawPolygon(pen, points.ToArray());
                }

                // Draw points
                for (int i = 0; i < points.Count; i++)
                {
                    // Draw point circle
                    using (var brush = new SolidBrush(Color.Red))
                    {
                        g.FillEllipse(brush, points[i].X - 6, points[i].Y - 6, 12, 12);
                    }

                    // Draw point border
                    using (var pen = new Pen(Color.White, 2))
                    {
                        g.DrawEllipse(pen, points[i].X - 6, points[i].Y - 6, 12, 12);
                    }

                    // Draw label
                    if (showLabels)
                    {
                        string label = (i + 1).ToString();
                        using (var font = new Font("Arial", 10, FontStyle.Bold))
                        using (var brush = new SolidBrush(Color.White))
                        {
                            var size = g.MeasureString(label, font);
                            g.DrawString(label, font, brush, 
                                points[i].X - size.Width / 2, 
                                points[i].Y - size.Height - 10);
                        }
                    }
                }

                // Draw info text
                if (showLabels)
                {
                    string info = $"Vùng đếm: {points.Count} điểm";
                    using (var font = new Font("Arial", 12, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.Yellow))
                    using (var bgBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                    {
                        var size = g.MeasureString(info, font);
                        g.FillRectangle(bgBrush, 10, 10, size.Width + 10, size.Height + 5);
                        g.DrawString(info, font, brush, 15, 12);
                    }
                }
            }

            DisplayFrame(_currentFrame);
        }

        private void ShowCompletionDialog()
        {
            var stats = _processor.Statistics;
            string message = $"Xu ly hoan tat!\n\n" +
                           $"Tong so xe: {stats.TotalVehicles}\n" +
                           $"Khung hinh xu ly: {stats.ProcessedFrames}\n" +
                           $"Thoi gian: {stats.ProcessingTime:F2}s\n" +
                           $"FPS trung binh: {stats.AverageFPS:F2}\n\n" +
                           $"Ban co muon xuat bao cao khong?";

            var result = MessageBox.Show(message, "Hoan thanh", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                ShowExportDialog();
            }
        }

        private void ShowExportDialog()
        {
            using (var dialog = new Form())
            {
                dialog.Text = "Xuat bao cao";
                dialog.Size = new System.Drawing.Size(350, 180);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                var btnJSON = new Button { Text = "Xuat JSON", Location = new System.Drawing.Point(20, 20), Size = new System.Drawing.Size(120, 40) };
                var btnTXT = new Button { Text = "Xuat TXT", Location = new System.Drawing.Point(150, 20), Size = new System.Drawing.Size(120, 40) };
                var btnExcel = new Button { Text = "Xuat Excel", Location = new System.Drawing.Point(20, 70), Size = new System.Drawing.Size(120, 40) };
                var btnClose = new Button { Text = "Dong", Location = new System.Drawing.Point(150, 70), Size = new System.Drawing.Size(120, 40), DialogResult = DialogResult.Cancel };

                btnJSON.Click += (s, e) => ExportReport("json");
                btnTXT.Click += (s, e) => ExportReport("txt");
                btnExcel.Click += (s, e) => ExportReport("excel");

                dialog.Controls.AddRange(new Control[] { btnJSON, btnTXT, btnExcel, btnClose });
                dialog.ShowDialog(this);
            }
        }

        private void ExportReport(string format)
        {
            // Kiểm tra quyền xuất báo cáo (chỉ Admin)
            // Check export permission (Admin only)
            if (!_authService.CanExportReports())
            {
                MessageBox.Show(
                    "⚠️ BẠN KHÔNG CÓ QUYỀN XUẤT BÁO CÁO!\n\n" +
                    "Chức năng này chỉ dành cho Admin.\n" +
                    "Vui lòng liên hệ quản trị viên để được hỗ trợ.\n\n" +
                    "Tài khoản hiện tại: " + _authService.CurrentUser?.Username + " (User)",
                    "Không Có Quyền Truy Cập",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
            
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string defaultPath = !string.IsNullOrWhiteSpace(txtExportPath.Text) 
                    ? txtExportPath.Text 
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                string extension = format == "excel" ? ".xlsx" : format == "json" ? ".json" : ".txt";
                string fileName = $"traffic_report_{timestamp}{extension}";
                string fullPath = Path.Combine(defaultPath, fileName);

                switch (format)
                {
                    case "json":
                        ReportExporter.ExportToJson(_processor.Statistics, fullPath, _config);
                        break;
                    case "txt":
                        ReportExporter.ExportToText(_processor.Statistics, fullPath, _config);
                        break;
                    case "excel":
                        ReportExporter.ExportToExcel(_processor.Statistics, fullPath, _config);
                        break;
                }

                MessageBox.Show($"Da xuat bao cao thanh cong:\n{fileName}", "Thanh cong", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                lblStatus.Text = $"Da xuat bao cao: {fileName}";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi xuat bao cao:\n{ex.Message}", "Loi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateConfiguration()
        {
            // Kiểm tra quyền thay đổi cấu hình hệ thống (chỉ Admin)
            // Check system configuration permission (Admin only)
            if (!_authService.CanChangeSystemConfig())
            {
                MessageBox.Show(
                    "⚠️ BẠN KHÔNG CÓ QUYỀN THAY ĐỔI CẤU HÌNH!\n\n" +
                    "Chức năng này chỉ dành cho Admin.\n" +
                    "Vui lòng liên hệ quản trị viên để được hỗ trợ.\n\n" +
                    "Tài khoản hiện tại: " + _authService.CurrentUser?.Username + " (User)",
                    "Không Có Quyền Truy Cập",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
            
            _config.ModelPath = txtModelPath.Text;
            // _config.VideoPath = txtVideoPath.Text; // Already set via dialog
            // _config.ImagePath = txtImagePath.Text; // Already set via dialog
            // Convert from percentage (0-100) to decimal (0-1) for detector
            _config.ConfidenceThreshold = (float)((double)numConfidence.Value / 100.0);
            _config.IouThreshold = (float)((double)numIOU.Value / 100.0);
            _config.SkipFrames = (int)numSkipFrames.Value;
            _config.CameraIndex = (int)numCamera.Value;
            _config.ExportPath = txtExportPath.Text;
            // _config.ShowCoordinates = chkShowCoordinates.Checked; // Control removed
            _config.UseCountingLine = chkUseCountingLine.Checked;
            
            // Save detection zone or counting line based on mode
            if (_config.UseCountingLine)
            {
                if (_countingLineStart != System.Drawing.Point.Empty && _countingLineEnd != System.Drawing.Point.Empty)
                {
                    _config.CountingLineStart = new OpenCvSharp.Point(_countingLineStart.X, _countingLineStart.Y);
                    _config.CountingLineEnd = new OpenCvSharp.Point(_countingLineEnd.X, _countingLineEnd.Y);
                }
            }
            else
            {
                // Always update detection zone - clear if less than 3 points, otherwise save new zone
                if (_detectionZone.Count >= 3)
                {
                    _config.DetectionZone = _detectionZone.Select(p => 
                        new OpenCvSharp.Point(p.X, p.Y)).ToList();
                }
                else
                {
                    // Clear the zone in config when reset or invalid
                    _config.DetectionZone.Clear();
                }
            }
            
            _config.Save();
        }

        // PictureBox mouse events for zone selection
        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if ((!_isSelectingZone && !_isSelectingLine) || _originalFrame == null) return;

            // Convert PictureBox coordinates to image coordinates
            var imagePoint = ConvertPictureBoxToImageCoordinates(e.Location);

            if (_isSelectingLine)
            {
                // Line selection mode
                if (e.Button == MouseButtons.Left)
                {
                    if (_countingLineStart == System.Drawing.Point.Empty)
                    {
                        _countingLineStart = imagePoint;
                        DrawLineOnFrame(_countingLineStart, imagePoint, false);
                        lblStatus.Text = "Da chon diem dau. Click tiep de chon diem cuoi";
                        lblStatus.ForeColor = Color.Yellow;
                    }
                    else
                    {
                        _countingLineEnd = imagePoint;
                        _isSelectingLine = false;
                        
                        DrawLineOnFrame(_countingLineStart, _countingLineEnd, true);
                        UpdateConfiguration();
                        
                        lblStatus.Text = "Da hoan thanh chon duong dem";
                        lblStatus.ForeColor = Color.Green;
                        UpdateButtonStates();
                        
                        MessageBox.Show("Da chon duong dem thanh cong!", 
                            "Thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else if (_isSelectingZone)
            {
                // Polygon zone selection mode
                if (e.Button == MouseButtons.Left)
                {
                    _tempZonePoints.Add(imagePoint);
                    
                    // Redraw with current points
                    DrawZoneOnFrame(_tempZonePoints, true);
                    
                    lblStatus.Text = $"Da chon {_tempZonePoints.Count} diem. Click chuot phai de hoan thanh (can it nhat 3 diem)";
                    lblStatus.ForeColor = Color.Yellow;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (_tempZonePoints.Count >= 3)
                    {
                        _detectionZone = new List<System.Drawing.Point>(_tempZonePoints);
                        _tempZonePoints.Clear();
                        _isSelectingZone = false;
                        
                        DrawZoneOnFrame(_detectionZone, true);
                        
                        // Save zone to config immediately
                        UpdateConfiguration();
                        
                        lblStatus.Text = $"Da hoan thanh chon vung voi {_detectionZone.Count} diem";
                        lblStatus.ForeColor = Color.Green;
                        UpdateButtonStates();
                        
                        MessageBox.Show($"Da chon vung dem thanh cong voi {_detectionZone.Count} diem!", 
                            "Thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Can it nhat 3 diem de tao vung dem!", "Canh bao", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            _lastMousePos = e.Location;
            
            if (_isSelectingZone && _tempZonePoints.Count > 0)
            {
                // Show preview line when selecting zone
                pbVideo.Invalidate();
            }
            else if (_isSelectingLine && _countingLineStart != System.Drawing.Point.Empty)
            {
                // Show preview line when selecting counting line
                var imagePoint = ConvertPictureBoxToImageCoordinates(e.Location);
                DrawLineOnFrame(_countingLineStart, imagePoint, false);
            }
        }

        private System.Drawing.Point ConvertPictureBoxToImageCoordinates(System.Drawing.Point pictureBoxPoint)
        {
            if (pbVideo.Image == null || _originalFrame == null)
                return pictureBoxPoint;

            // Get the actual display area of the image in the PictureBox
            float imageWidth = _originalFrame.Width;
            float imageHeight = _originalFrame.Height;
            float pictureBoxWidth = pbVideo.ClientSize.Width;
            float pictureBoxHeight = pbVideo.ClientSize.Height;

            // Calculate the scaling factor (PictureBox uses Zoom mode)
            float scaleX = pictureBoxWidth / imageWidth;
            float scaleY = pictureBoxHeight / imageHeight;
            float scale = Math.Min(scaleX, scaleY);

            // Calculate the actual size and position of the image
            float scaledWidth = imageWidth * scale;
            float scaledHeight = imageHeight * scale;
            float offsetX = (pictureBoxWidth - scaledWidth) / 2;
            float offsetY = (pictureBoxHeight - scaledHeight) / 2;

            // Convert coordinates
            float imageX = (pictureBoxPoint.X - offsetX) / scale;
            float imageY = (pictureBoxPoint.Y - offsetY) / scale;

            // Clamp to image bounds
            imageX = Math.Max(0, Math.Min(imageX, imageWidth - 1));
            imageY = Math.Max(0, Math.Min(imageY, imageHeight - 1));

            return new System.Drawing.Point((int)imageX, (int)imageY);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // ESC to cancel zone/line selection
            if (keyData == Keys.Escape && (_isSelectingZone || _isSelectingLine))
            {
                _isSelectingZone = false;
                _isSelectingLine = false;
                _tempZonePoints.Clear();
                _countingLineStart = System.Drawing.Point.Empty;
                
                if (_originalFrame != null)
                {
                    _currentFrame?.Dispose();
                    _currentFrame = new Bitmap(_originalFrame);
                    DisplayFrame(_currentFrame);
                }
                
                lblStatus.Text = "Da huy chon vung";
                lblStatus.ForeColor = Color.Orange;
                UpdateButtonStates();
                
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Form closing
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_processor.IsProcessing)
            {
                var result = MessageBox.Show(
                    "Dang co xu ly dang chay. Ban co chac muon thoat?",
                    "Xac nhan thoat",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                
                // Dừng processor và chờ hoàn thành
                _processor.Stop();
                System.Threading.Thread.Sleep(500); // Chờ thread dừng
            }

            try
            {
                // Cleanup resources
                _processor.Stop();
                UpdateConfiguration();
                _detector?.Dispose();
                _currentFrame?.Dispose();
                _originalFrame?.Dispose();
                
                // Dispose database context
                _dbContext?.Dispose();
                _repository?.Dispose();
                
                // Đóng các form con nếu có
                var childForms = Application.OpenForms.Cast<Form>()
                    .Where(f => f != this && f.Owner == this && !f.IsDisposed)
                    .ToList();
                    
                foreach (var form in childForms)
                {
                    form.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainForm] Error during cleanup: {ex.Message}");
            }

            base.OnFormClosing(e);
            
            // Không gọi Application.Exit() ở đây vì sẽ quay lại Dashboard
        }

        // Input mode changed
        private void rbInput_CheckedChanged(object sender, EventArgs e)
        {
            UpdateInputMode();
        }

        // TrackBar scroll event - Seek video position
        private void trackBarVideo_Scroll(object? sender, EventArgs e)
        {
            if (_processor == null || !trackBarVideo.Enabled || _processor.IsProcessing)
            {
                // Don't allow seeking while processing
                if (_processor != null && _processor.IsProcessing)
                {
                    MessageBox.Show("Vui lòng dừng video trước khi tua!", "Thông báo", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }
                
            try
            {
                _isSeekingVideo = true;
                int framePosition = trackBarVideo.Value;
                
                // Update time label only (don't actually seek during playback)
                int totalFrames = trackBarVideo.Maximum;
                if (totalFrames > 0)
                {
                    double fps = 30; // Default FPS for time calculation
                    if (_processor.Statistics != null && _processor.Statistics.AverageFPS > 0)
                        fps = _processor.Statistics.AverageFPS;
                    
                    int currentSec = (int)(framePosition / fps);
                    int totalSec = (int)(totalFrames / fps);
                    
                    string current = $"{currentSec / 60:D2}:{currentSec % 60:D2}";
                    string total = $"{totalSec / 60:D2}:{totalSec % 60:D2}";
                    lblVideoTime.Text = $"{current} / {total}";
                }
                
                _isSeekingVideo = false;
            }
            catch (Exception ex)
            {
                _isSeekingVideo = false;
                Console.WriteLine($"[MainForm] Video seek error: {ex.Message}");
            }
        }

        // Help button - Show user guide
        private void btnHelp_Click(object sender, EventArgs e)
        {
            var helpMessage = @"📖 HUONG DAN SU DUNG HE THONG

1️⃣ CHON CHE DO NGUON:
   • Video: Phat hien tu file video
   • Camera: Phat hien tu camera truc tiep
   • Image: Phat hien tu hinh anh

2️⃣ TAI DU LIEU:
   • Chon file ONNX model AI (.onnx)
   • Nhan nut 'Tai model AI' de tai mo hinh
   • Nhan 'Tai du lieu' de kiem tra cau hinh

3️⃣ THIET LAP THAM SO:
   • Do tin cay: 0.25 (mac dinh) - Tang len de chinh xac hon
   • IOU: 0.45 (mac dinh) - Nguong trung lap

4️⃣ CHON  HIEN:
   • Click chuot trai de chon cac diem
   • Nhan Enter de hoan thanh
   • Nhan ESC de huy bo

5️⃣ DIEU KHIEN:
   • Bat dau: Khoi dong phat hien
   • Tam dung: Dung tam thoi
   • Dung: Ket thuc qua trinh

6️⃣ XUAT BAO CAO:
   • Chon duong dan luu file CSV
   • Bao cao tu dong xuat khi ket thuc

💡 MEO: Di chuot qua cac nut de xem chi tiet!";

            MessageBox.Show(helpMessage, "Huong dan su dung", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Settings button - Parameter tuning guide
        private void btnSettings_Click(object sender, EventArgs e)
        {
            var settingsGuide = @"⚙️ HUONG DAN TUY CHINH THAM SO

📊 DO TIN CAY (Confidence Threshold):
   • Mac dinh: 0.25
   • Thap (0.15-0.25): Phat hien nhieu hon, co the co loi duong tinh
   • Cao (0.35-0.50): Chinh xac hon, co the bo sot doi tuong
   • Khuyen nghi: 0.25 cho giao thong do thi

🎯 IOU (Intersection over Union):
   • Mac dinh: 0.45
   • Thap (0.30-0.40): Cho phep cac vung trung lap nhieu hon
   • Cao (0.50-0.70): Nghiem ngat hon, giam trung lap
   • Khuyen nghi: 0.45 cho giao thong dong duc

🎬 FRAME SKIP:
   • Mac dinh: 2 (xu ly moi frame thu 2)
   • Tang len (3-5): Tang toc do, giam chinh xac
   • Giam xuong (0-1): Cham hon, chinh xac hon

📷 CAMERA INDEX:
   • Mac dinh: 0 (camera mac dinh cua he thong)
   • Thay doi neu co nhieu camera

💡 LUU Y: 
   • Luu cau hinh tu dong khi dong ung dung
   • Thu nghiem voi cac gia tri khac nhau de tim ra toi uu
   • Ket qua phu thuoc vao chat luong video/camera";

            MessageBox.Show(settingsGuide, "Tuy chinh tham so", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // About button - System information
        private void btnAbout_Click(object sender, EventArgs e)
        {
            var aboutInfo = @"🚗 HE THONG GIAM SAT GIAO THONG AI

📌 THONG TIN HE THONG:
   Phien ban: 2.0.0
   Mo hinh AI: YOLOv8 (ONNX Runtime)
   Framework: .NET 6.0 + OpenCV
   Ngay phat hanh: 25/11/2025

🎯 CHUC NANG CHINH:
   ✅ Phat hien 5 loai phuong tien
   ✅ Theo do va dem xe thong minh
   ✅ Phan tich thong ke chi tiet
   ✅ Xuat bao cao CSV/Excel
   ✅ Xu ly Video/Camera/Anh
   ✅ Chon vung phat hien tuy chinh

🚀 TINH NANG NOI BAT:
   • AI Model: YOLOv8n (Nhanh & Chinh xac)
   • Real-time Processing
   • GPU Acceleration Support
   • Advanced Vehicle Refinement
   • Frame Optimization
   • Alert System

👨‍💻 PHAT TRIEN BOI:
   Nguyễn Ngọc Hiếu

📧 HO TRO:
   Email: bimax12052005@gmail.com
   Website: www..com

© 2025 Traffic Monitor AI System. All rights reserved.";

            MessageBox.Show(aboutInfo, "Gioi thieu", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InitializeDefaultViewMode()
        {
            // Start with Basic Mode - hide Parking and Schedule
            gbParking.Visible = false;
            gbSchedule.Visible = false;
        }

        // Admin Dashboard button - Open admin management interface
        private void btnAdminDashboard_Click(object sender, EventArgs e)
        {
            if (!_authService.IsAdmin())
            {
                MessageBox.Show(
                    "Chỉ Admin mới có quyền truy cập Quản Lý Hệ Thống!",
                    "Không có quyền",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (_dbContext == null)
            {
                MessageBox.Show(
                    "Chưa kết nối database!",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            try
            {
                var dashboardForm = new AdminDashboardForm(_dbContext, _authService);
                dashboardForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi mở Dashboard:\n\n{ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // Schedule Timer Event Handlers
        private void chkEnableSchedule_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnableSchedule.Checked)
            {
                dtpScheduleTime.Enabled = true;
                btnSetSchedule.Enabled = true;
            }
            else
            {
                dtpScheduleTime.Enabled = false;
                btnSetSchedule.Enabled = false;
                timerSchedule.Stop();
                _scheduleEnabled = false;
                lblScheduleStatus.Text = "⏸️ Chua dat lich";
                lblScheduleStatus.ForeColor = System.Drawing.Color.FromArgb(189, 193, 198);
            }
        }

        private void btnSetSchedule_Click(object sender, EventArgs e)
        {
            if (!chkEnableSchedule.Checked)
            {
                MessageBox.Show("Vui long kich hoat hen gio truoc!", "Canh bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _scheduledTime = dtpScheduleTime.Value;
            
            // Check if scheduled time is in the past
            if (_scheduledTime <= DateTime.Now)
            {
                MessageBox.Show("Thoi gian hen phai lon hon thoi gian hien tai!", "Loi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _scheduleEnabled = true;
            timerSchedule.Start();
            
            TimeSpan timeUntil = _scheduledTime - DateTime.Now;
            lblScheduleStatus.Text = $"⏰ Hen gio: {_scheduledTime:dd/MM/yyyy HH:mm}\n⏳ Con {timeUntil.Hours}h {timeUntil.Minutes}m {timeUntil.Seconds}s";
            lblScheduleStatus.ForeColor = System.Drawing.Color.FromArgb(48, 209, 88);
            
            MessageBox.Show($"Da(dat lich thanh cong!\n\nThoi gian bat dau: {_scheduledTime:dd/MM/yyyy HH:mm}\n\nUng dung se tu dong bat dau xu ly tai thoi diem nay.",
                "Dat lich thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void timerSchedule_Tick(object sender, EventArgs e)
        {
            if (!_scheduleEnabled) return;

            TimeSpan timeUntil = _scheduledTime - DateTime.Now;
            
            if (timeUntil.TotalSeconds <= 0)
            {
                // Time to start processing
                timerSchedule.Stop();
                _scheduleEnabled = false;
                
                lblScheduleStatus.Text = "✅ Bat dau xu ly theo lich...";
                lblScheduleStatus.ForeColor = System.Drawing.Color.FromArgb(48, 209, 88);
                
                // Automatically start processing
                if (_dataLoaded && _detector != null)
                {
                    btnStart_Click(sender, e);
                    
                    // Nếu đang bật chế độ parking, thông báo
                    if (_parkingManager.IsParkingMode)
                    {
                        MessageBox.Show("⏰ Hết thời gian hẹn giờ!\n\n" +
                            "Đang tạo báo cáo bãi xe...",
                            "Hẹn giờ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Show parking report
                        btnViewParkingReport_Click(sender, e);
                    }
                    else
                    {
                        MessageBox.Show("Bat dau xu ly theo lich da hen!", "Thong bao",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Khong the bat dau! Vui long tai model va du lieu truoc.", "Loi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblScheduleStatus.Text = "❌ Loi: Chua tai model/du lieu";
                    lblScheduleStatus.ForeColor = System.Drawing.Color.Red;
                }
                
                chkEnableSchedule.Checked = false;
            }
            else
            {
                // Update countdown
                lblScheduleStatus.Text = $"⏰ Hen gio: {_scheduledTime:dd/MM/yyyy HH:mm}\n⏳ Con {timeUntil.Hours}h {timeUntil.Minutes}m {timeUntil.Seconds}s";
            }
        }

        // Parking Management Event Handlers
        private void btnStartParking_Click(object sender, EventArgs e)
        {
            if (!_dataLoaded || _detector == null)
            {
                MessageBox.Show("Vui lòng tải model và dữ liệu trước khi bật chế độ bãi xe!",
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra có sử dụng counting line không (bắt buộc cho parking mode)
            if (!_config.UseCountingLine || (_config.CountingLineStart.X == 0 && _config.CountingLineStart.Y == 0))
            {
                MessageBox.Show("Chế độ bãi xe yêu cầu:\n\n" +
                    "1. Bật 'Đường Đếm' (Counting Line)\n" +
                    "2. Vẽ đường đếm tại cổng vào/ra bãi xe\n\n" +
                    "Vui lòng cấu hình trong mục 'Vùng Phát Hiện'!",
                    "Cấu hình chưa đủ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _parkingManager.StartParkingSession();
            
            btnStartParking.Enabled = false;
            btnStopParking.Enabled = true;
            lblParkingStatus.Text = "✅ Chế độ bãi xe: Đang hoạt động";
            lblParkingStatus.ForeColor = Color.FromArgb(48, 209, 88);
            
            MessageBox.Show("Đã bật chế độ quản lý bãi xe!\n\n" +
                "Hệ thống sẽ tự động:\n" +
                "• Ghi nhận xe vào khi phát hiện\n" +
                "• Ghi nhận xe ra khi vượt qua đường đếm\n" +
                "• Thống kê số xe trong bãi\n\n" +
                "Bắt đầu xử lý video để theo dõi!",
                "Bãi xe", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnStopParking_Click(object sender, EventArgs e)
        {
            var report = _parkingManager.GenerateReport();
            _parkingManager.StopParkingSession();
            
            btnStartParking.Enabled = true;
            btnStopParking.Enabled = false;
            lblParkingStatus.Text = "🛑 Chế độ bãi xe: Tắt";
            lblParkingStatus.ForeColor = Color.FromArgb(189, 193, 198);
            lblCurrentVehicles.Text = "Xe trong bãi: 0 | Tổng vào: 0";
            
            // Hiển thị báo cáo tổng kết
            var summary = $"📊 BÃO CÁO TỔNG KẾT BÃI XE\n\n" +
                $"⏱️ Thời gian: {report.SessionStartTime:HH:mm:ss} - {report.SessionEndTime:HH:mm:ss}\n" +
                $"⏰ Tổng thời gian: {report.TotalDuration.Hours}h {report.TotalDuration.Minutes}m\n\n" +
                $"📥 Tổng xe vào: {report.TotalVehiclesEntered}\n" +
                $"📤 Tổng xe ra: {report.TotalVehiclesExited}\n" +
                $"🚗 Xe còn trong bãi: {report.VehiclesStillInParking}\n\n";

            if (report.VehicleTypeCount.Any())
            {
                summary += "📋 Thống kê theo loại xe:\n";
                foreach (var type in report.VehicleTypeCount.OrderByDescending(x => x.Value))
                {
                    summary += $"   {type.Key}: {type.Value}\n";
                }
            }

            MessageBox.Show(summary, "Báo cáo bãi xe", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnViewParkingReport_Click(object sender, EventArgs e)
        {
            if (!_parkingManager.IsParkingMode && _parkingManager.TotalEntered == 0)
            {
                MessageBox.Show("Chưa có dữ liệu bãi xe!\n\nVui lòng bật chế độ quản lý bãi xe và xử lý video.",
                    "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var report = _parkingManager.GenerateReport();
            
            // Tạo form hiển thị báo cáo chi tiết
            var reportForm = new Form
            {
                Text = "📊 Báo Cáo Chi Tiết Bãi Xe",
                Size = new System.Drawing.Size(900, 700),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var lblTitle = new Label
            {
                Text = "BÁO CÁO QUẢN LÝ BÃI XE",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 212, 170),
                AutoSize = true,
                Location = new System.Drawing.Point(20, 20)
            };

            var lblInfo = new Label
            {
                Text = $"Thời gian: {report.SessionStartTime:dd/MM/yyyy HH:mm:ss} - {report.SessionEndTime:dd/MM/yyyy HH:mm:ss}\n" +
                       $"Tổng thời gian: {report.TotalDuration.Hours}h {report.TotalDuration.Minutes}m {report.TotalDuration.Seconds}s\n\n" +
                       $"📥 Tổng xe vào: {report.TotalVehiclesEntered}\n" +
                       $"📤 Tổng xe ra: {report.TotalVehiclesExited}\n" +
                       $"🚗 Xe còn trong bãi: {report.VehiclesStillInParking}",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new System.Drawing.Point(20, 60)
            };

            // DataGridView để hiển thị chi tiết
            var dgv = new DataGridView
            {
                Location = new System.Drawing.Point(20, 200),
                Size = new System.Drawing.Size(840, 400),
                BackgroundColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 212, 170),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(40, 40, 40),
                    ForeColor = Color.White,
                    SelectionBackColor = Color.FromArgb(0, 150, 120),
                    Font = new Font("Segoe UI", 9)
                }
            };

            dgv.Columns.Add("TrackerId", "ID");
            dgv.Columns.Add("VehicleType", "Loại xe");
            dgv.Columns.Add("EntryTime", "Giờ vào");
            dgv.Columns.Add("ExitTime", "Giờ ra");
            dgv.Columns.Add("Duration", "Thời gian (phút)");
            dgv.Columns.Add("Status", "Trạng thái");

            foreach (var record in report.DetailedRecords.OrderByDescending(r => r.EntryTime))
            {
                string exitTime = record.ExitTime?.ToString("HH:mm:ss") ?? "-";
                string duration = record.Duration?.TotalMinutes.ToString("F1") ?? "-";
                string status = record.Status == "InParking" ? "🚗 Trong bãi" : "✓ Đã ra";
                
                dgv.Rows.Add(
                    record.TrackerId,
                    record.VehicleType,
                    record.EntryTime.ToString("HH:mm:ss"),
                    exitTime,
                    duration,
                    status
                );
            }

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblInfo);
            panel.Controls.Add(dgv);
            reportForm.Controls.Add(panel);
            reportForm.ShowDialog();
        }

        private void btnGenerateCharts_Click(object sender, EventArgs e)
        {
            try
            {
                if (_processor.Statistics.TotalVehicles == 0)
                {
                    MessageBox.Show("Chua co du lieu thong ke!\n\nVui long xu ly video/camera truoc khi tao bieu do.",
                        "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Select output folder
                using (var folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Chon thu muc luu bieu do";
                    folderDialog.SelectedPath = txtExportPath.Text;

                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        lblStatus.Text = "Dang tao bieu do...";
                        lblStatus.ForeColor = Color.Blue;
                        Application.DoEvents();

                        string outputFolder = folderDialog.SelectedPath;
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        
                        var chartGen = new TrafficChartGenerator(1200, 800);

                        // Generate bar chart
                        using (var barChart = chartGen.GenerateVehicleTypeBarChart(_processor.Statistics))
                        {
                            string barPath = System.IO.Path.Combine(outputFolder, $"bar_chart_{timestamp}.png");
                            chartGen.SaveChart(barChart, barPath);
                        }

                        // Generate pie chart
                        using (var pieChart = chartGen.GeneratePieChart(_processor.Statistics))
                        {
                            string piePath = System.IO.Path.Combine(outputFolder, $"pie_chart_{timestamp}.png");
                            chartGen.SaveChart(pieChart, piePath);
                        }

                        // Generate hourly trend if advanced stats available
                        if (_processor.AdvancedStats.GetTotalVehicles() > 0)
                        {
                            using (var trendChart = chartGen.GenerateHourlyTrendChart(_processor.AdvancedStats))
                            {
                                string trendPath = System.IO.Path.Combine(outputFolder, $"trend_chart_{timestamp}.png");
                                chartGen.SaveChart(trendChart, trendPath);
                            }
                        }

                        lblStatus.Text = "✅ Da tao bieu do thanh cong!";
                        lblStatus.ForeColor = Color.Green;

                        var result = MessageBox.Show(
                            $"Da tao bieu do thanh cong!\n\n" +
                            $"📊 Bar Chart: bar_chart_{timestamp}.png\n" +
                            $"🥧 Pie Chart: pie_chart_{timestamp}.png\n" +
                            $"📈 Trend Chart: trend_chart_{timestamp}.png\n\n" +
                            $"Thu muc: {outputFolder}\n\n" +
                            $"Ban co muon mo thu muc khong?",
                            "Thanh cong",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start("explorer.exe", outputFolder);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi tao bieu do:\n{ex.Message}", "Loi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Loi tao bieu do";
                lblStatus.ForeColor = Color.Red;
            }
        }
        
        // Counting line mode toggle
        private void chkUseCountingLine_CheckedChanged(object sender, EventArgs e)
        {
            bool useLineMode = chkUseCountingLine.Checked;
            
            // Update button visibility
            UpdateButtonStates();
            
            // Update reset button text
            btnResetZone.Text = "🔄 Reset";
            
            UpdateConfiguration();
        }
        
        // Select counting line button
        private void btnSelectLine_Click(object sender, EventArgs e)
        {
            if (!_dataLoaded || _originalFrame == null)
            {
                MessageBox.Show("Vui long tai du lieu truoc!", "Canh bao", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isSelectingLine = true;
            _countingLineStart = System.Drawing.Point.Empty;
            _countingLineEnd = System.Drawing.Point.Empty;
            
            _currentFrame?.Dispose();
            _currentFrame = new Bitmap(_originalFrame);
            DisplayFrame(_currentFrame);
            
            lblStatus.Text = "Click 2 diem de tao duong dem";
            lblStatus.ForeColor = Color.Yellow;
            
            MessageBox.Show(
                "Huong dan chon duong dem:\n\n" +
                "✅ QUET TOAN BO MAN HINH\n" +
                "✅ DEM CA 2 CHIEU (Tren->Duoi & Duoi->Tren)\n\n" +
                "Cach chon:\n" +
                "• Click chuot TRAI lan 1: Chon diem dau\n" +
                "• Click chuot TRAI lan 2: Chon diem cuoi\n" +
                "• Xe se duoc dem khi cat qua duong\n" +
                "• ESC: Huy bo\n\n" +
                "Luu y: Dat duong vuong goc voi huong di chuyen!",
                "Chon duong dem - Line Zone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        
        // Draw counting line on frame
        private void DrawLineOnFrame(System.Drawing.Point start, System.Drawing.Point end, bool finalized)
        {
            if (_originalFrame == null) return;

            _currentFrame?.Dispose();
            _currentFrame = new Bitmap(_originalFrame);

            using (Graphics g = Graphics.FromImage(_currentFrame))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Pen linePen = finalized 
                    ? new Pen(Color.Red, 4) 
                    : new Pen(Color.Yellow, 2);
                linePen.DashStyle = finalized 
                    ? System.Drawing.Drawing2D.DashStyle.Solid 
                    : System.Drawing.Drawing2D.DashStyle.Dash;

                g.DrawLine(linePen, start, end);

                // Draw start/end points
                using (Brush startBrush = new SolidBrush(Color.Lime))
                using (Brush endBrush = new SolidBrush(Color.Red))
                {
                    g.FillEllipse(startBrush, start.X - 6, start.Y - 6, 12, 12);
                    g.DrawEllipse(Pens.White, start.X - 6, start.Y - 6, 12, 12);
                    
                    if (finalized)
                    {
                        g.FillEllipse(endBrush, end.X - 6, end.Y - 6, 12, 12);
                        g.DrawEllipse(Pens.White, end.X - 6, end.Y - 6, 12, 12);
                    }
                }

                // Draw label
                if (finalized)
                {
                    using (Font font = new Font("Segoe UI", 14, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.Red))
                    {
                        string label = "COUNTING LINE";
                        var size = g.MeasureString(label, font);
                        float labelX = (start.X + end.X) / 2 - size.Width / 2;
                        float labelY = Math.Min(start.Y, end.Y) - size.Height - 10;
                        
                        // Background
                        g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), 
                            labelX - 5, labelY - 5, size.Width + 10, size.Height + 10);
                        g.DrawString(label, font, brush, labelX, labelY);
                    }
                }

                linePen.Dispose();
            }

            DisplayFrame(_currentFrame);
        }
        
        #region Database Methods
        
        /// <summary>
        /// Initialize database connection and create database if needed
        /// </summary>
        private void InitializeDatabase()
        {
            // Kiểm tra quyền truy cập database (chỉ Admin)
            // Check database access permission (Admin only)
            if (!_authService.CanAccessDatabase())
            {
                Console.WriteLine("[Database] User does not have database access permission - Skipped initialization");
                MessageBox.Show(
                    "⚠️ THÔNG BÁO GIỚI HẠN QUYỀN\n\n" +
                    "Tài khoản User không có quyền truy cập Database.\n" +
                    "Các chức năng lưu trữ lịch sử và thống kê sẽ bị tắt.\n\n" +
                    "Liên hệ Admin để được cấp quyền nếu cần thiết.",
                    "Giới Hạn Quyền Truy Cập",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }
            
            try
            {
                _dbContext = new TrafficDbContext();
                _repository = new TrafficRepository(_dbContext);
                
                // Check if database can be connected
                if (!_dbContext.CanConnect())
                {
                    // Try to create database
                    _dbContext.EnsureDatabaseCreated();
                    
                    // Show success message
                    if (_dbContext.CanConnect())
                    {
                        Console.WriteLine("[Database] Connected and initialized successfully");
                    }
                    else
                    {
                        Console.WriteLine("[Database] Warning: Could not establish connection");
                    }
                }
                else
                {
                    Console.WriteLine("[Database] Connected to existing database");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Database] Error initializing: {ex.Message}");
                MessageBox.Show(
                    $"Không thể kết nối database. Ứng dụng sẽ chạy mà không lưu dữ liệu vào database.\\n\\nLỗi: {ex.Message}",
                    "Cảnh báo Database",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                
                // Dispose failed context
                _dbContext?.Dispose();
                _dbContext = null;
                _repository = null;
            }
        }
        
        /// <summary>
        /// Create a new session record in database when processing starts
        /// </summary>
        private async Task CreateDatabaseSessionAsync(string source, bool isCamera)
        {
            if (_repository == null || _dbContext == null)
            {
                Console.WriteLine("[Database] Skipping session creation - database not initialized");
                return;
            }
            
            try
            {
                _sessionStartTime = DateTime.Now;
                
                _currentSession = new TrafficSessionDb
                {
                    StartTime = _sessionStartTime,
                    SourceType = _inputMode, // "video", "camera", "image"
                    SourcePath = source,
                    ModelPath = _config.ModelPath,
                    ConfidenceThreshold = _config.ConfidenceThreshold,
                    IouThreshold = _config.IouThreshold,
                    TotalVehicles = 0,
                    ProcessedFrames = 0,
                    ProcessingTime = 0,
                    AverageFPS = 0
                };
                
                _currentSession = await _repository.CreateSessionAsync(_currentSession);
                Console.WriteLine($"[Database] Created session ID: {_currentSession.SessionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Database] Error creating session: {ex.Message}");
                _currentSession = null;
            }
        }
        
        /// <summary>
        /// Update session record when processing completes
        /// </summary>
        private async Task UpdateDatabaseSessionAsync()
        {
            if (_repository == null || _currentSession == null)
            {
                Console.WriteLine("[Database] Skipping session update - no active session");
                return;
            }
            
            try
            {
                var stats = _processor.Statistics;
                
                // Update session with final statistics
                _currentSession.EndTime = DateTime.Now;
                _currentSession.TotalVehicles = stats.TotalVehicles;
                _currentSession.ProcessedFrames = stats.ProcessedFrames;
                _currentSession.ProcessingTime = (DateTime.Now - _sessionStartTime).TotalSeconds;
                _currentSession.AverageFPS = stats.AverageFPS;
                
                await _repository.UpdateSessionAsync(_currentSession);
                Console.WriteLine($"[Database] Updated session ID: {_currentSession.SessionId} - Total vehicles: {_currentSession.TotalVehicles}");
                
                // Optionally save hourly statistics
                await SaveHourlyStatisticsAsync();
                
                _currentSession = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Database] Error updating session: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Save hourly statistics aggregated from current session
        /// </summary>
        private async Task SaveHourlyStatisticsAsync()
        {
            if (_repository == null)
                return;
                
            try
            {
                var stats = _processor.Statistics;
                var hour = DateTime.Now;
                hour = new DateTime(hour.Year, hour.Month, hour.Day, hour.Hour, 0, 0);
                
                // Calculate congestion level based on total vehicles
                int congestionLevel = CalculateCongestionLevel(stats.TotalVehicles);
                
                var hourlyStats = new HourlyStatisticsDb
                {
                    HourTimestamp = hour,
                    TotalVehicles = stats.TotalVehicles,
                    CarCount = stats.VehicleCounts.ContainsKey(VehicleType.Car) ? stats.VehicleCounts[VehicleType.Car] : 0,
                    MotorcycleCount = stats.VehicleCounts.ContainsKey(VehicleType.Motorcycle) ? stats.VehicleCounts[VehicleType.Motorcycle] : 0,
                    BusCount = stats.VehicleCounts.ContainsKey(VehicleType.Bus) ? stats.VehicleCounts[VehicleType.Bus] : 0,
                    BicycleCount = stats.VehicleCounts.ContainsKey(VehicleType.Bicycle) ? stats.VehicleCounts[VehicleType.Bicycle] : 0,
                    AverageSpeed = 0, // Would need to calculate from tracking data
                    CongestionLevel = congestionLevel
                };
                
                await _repository.UpsertHourlyStatisticsAsync(hourlyStats);
                Console.WriteLine($"[Database] Saved hourly statistics for {hour:yyyy-MM-dd HH:mm}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Database] Error saving hourly statistics: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Calculate congestion level based on vehicle count
        /// </summary>
        private int CalculateCongestionLevel(int vehicleCount)
        {
            // Simple algorithm - adjust thresholds as needed
            if (vehicleCount < 10) return 0; // No congestion
            if (vehicleCount < 30) return 1; // Light
            if (vehicleCount < 60) return 2; // Moderate
            if (vehicleCount < 100) return 3; // Heavy
            if (vehicleCount < 150) return 4; // Very heavy
            return 5; // Extreme congestion
        }

        /// <summary>
        /// Save vehicle detection to database in realtime
        /// </summary>
        private async void OnVehicleDetected(object? sender, DetectionResult detection)
        {
            // Parking management: Record vehicle entry
            if (_parkingManager.IsParkingMode)
            {
                // Ghi nhận xe vào khi phát hiện lần đầu
                if (!_parkingManager.IsVehicleInParking(detection.TrackerId))
                {
                    _parkingManager.RecordVehicleEntry(
                        detection.TrackerId,
                        detection.VehicleType,
                        _processor?.Statistics?.ProcessedFrames ?? 0
                    );
                    
                    // Cập nhật UI
                    UpdateParkingUI();
                }
            }
            
            if (_repository == null || _currentSession == null)
                return;
            
            try
            {
                var vehicleDetection = new VehicleDetectionDb
                {
                    SessionId = _currentSession.SessionId,
                    DetectedTime = detection.DetectedTime,
                    VehicleType = detection.VehicleType,
                    Confidence = detection.Confidence,
                    TrackerId = detection.TrackerId,
                    PositionX = (int)detection.Center.X,
                    PositionY = (int)detection.Center.Y,
                    Width = detection.BoundingBox.Width,
                    Height = detection.BoundingBox.Height,
                    FrameNumber = _processor?.Statistics?.ProcessedFrames ?? 0
                };
                
                await _repository.AddDetectionAsync(vehicleDetection);
                Console.WriteLine($"[Database] Saved detection ID: {vehicleDetection.DetectionId}, Type: {detection.VehicleType}, TrackerId: {detection.TrackerId}, Frame: {vehicleDetection.FrameNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Database] Error saving detection: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Update parking UI with current statistics
        /// </summary>
        private void UpdateParkingUI()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateParkingUI));
                return;
            }
            
            if (_parkingManager.IsParkingMode)
            {
                int currentCount = _parkingManager.CurrentVehicleCount;
                int totalEntered = _parkingManager.TotalEntered;
                lblCurrentVehicles.Text = $"🚗 Xe trong bãi: {currentCount} | 📝 Tổng vào: {totalEntered}";
            }
        }
        
        /// <summary>
        /// Handle vehicle exiting parking lot (line crossing detected)
        /// </summary>
        private void OnVehicleExited(object? sender, DetectionResult detection)
        {
            if (_parkingManager.IsParkingMode)
            {
                _parkingManager.RecordVehicleExit(
                    detection.TrackerId,
                    _processor?.Statistics?.ProcessedFrames ?? 0
                );
                
                // Update UI
                UpdateParkingUI();
            }
        }
        
        #endregion
        
        #region Status Strip Management
        
        #endregion
    }
}

