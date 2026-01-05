namespace TrafficMonitorApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // MenuStrip
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripMenuItem menuFileOpen;
        private System.Windows.Forms.ToolStripMenuItem menuFileExit;
        private System.Windows.Forms.ToolStripMenuItem menuView;
        private System.Windows.Forms.ToolStripMenuItem menuViewSettings;
        private System.Windows.Forms.ToolStripMenuItem menuViewStatistics;
        private System.Windows.Forms.ToolStripMenuItem menuViewHistory;
        private System.Windows.Forms.ToolStripMenuItem menuMode;
        private System.Windows.Forms.ToolStripMenuItem menuModeBasic;
        private System.Windows.Forms.ToolStripMenuItem menuModeParking;
        private System.Windows.Forms.ToolStripMenuItem menuData;
        private System.Windows.Forms.ToolStripMenuItem menuDataExport;
        private System.Windows.Forms.ToolStripMenuItem menuDataAdmin;
        private System.Windows.Forms.ToolStripMenuItem menuTools;
        private System.Windows.Forms.ToolStripMenuItem menuToolsOptions;
        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripMenuItem menuHelpAbout;
        
        // Main Layout
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.FlowLayoutPanel flowPanelLeft;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.PictureBox pbVideo;
        
        // Status and Statistics (below video, not overlay)
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TableLayoutPanel tableStats;
        
        // Statistics Cards
        private System.Windows.Forms.Panel cardTotal;
        private System.Windows.Forms.Panel cardCar;
        private System.Windows.Forms.Panel cardMotor;
        private System.Windows.Forms.Panel cardBus;
        private System.Windows.Forms.Panel cardBicycle;
        private System.Windows.Forms.TextBox txtTotalCount;
        private System.Windows.Forms.TextBox txtCarCount;
        private System.Windows.Forms.TextBox txtMotorCount;
        private System.Windows.Forms.TextBox txtBusCount;
        private System.Windows.Forms.TextBox txtBicycleCount;
        private System.Windows.Forms.Label lblFPS;
        
        // GroupBox 1: Source Selection
        private System.Windows.Forms.GroupBox gbSource;
        private System.Windows.Forms.RadioButton rbVideo;
        private System.Windows.Forms.RadioButton rbCamera;
        private System.Windows.Forms.RadioButton rbImage;
        private System.Windows.Forms.TextBox txtDataPath;
        private System.Windows.Forms.Button btnBrowseData;
        private System.Windows.Forms.Button btnLoadData;
        
        // GroupBox 2: Model AI
        private System.Windows.Forms.GroupBox gbModel;
        private System.Windows.Forms.TextBox txtModelPath;
        private System.Windows.Forms.Button btnBrowseModel;
        private System.Windows.Forms.Button btnLoadModel;
        
        // GroupBox 3: Processing Config
        private System.Windows.Forms.GroupBox gbConfig;
        private System.Windows.Forms.Label lblConfidence;
        private System.Windows.Forms.NumericUpDown numConfidence;
        private System.Windows.Forms.TrackBar trackConfidence;
        private System.Windows.Forms.Label lblIOU;
        private System.Windows.Forms.NumericUpDown numIOU;
        private System.Windows.Forms.TrackBar trackIOU;
        private System.Windows.Forms.Label lblSkipFrames;
        private System.Windows.Forms.NumericUpDown numSkipFrames;
        private System.Windows.Forms.NumericUpDown numCamera;
        
        // GroupBox 4: Detection Zone
        private System.Windows.Forms.GroupBox gbZone;
        private System.Windows.Forms.CheckBox chkUseCountingLine;
        private System.Windows.Forms.Button btnSelectZone;
        private System.Windows.Forms.Button btnSelectLine;
        private System.Windows.Forms.Button btnShowZone;
        private System.Windows.Forms.Button btnResetZone;
        
        // GroupBox 5: Main Controls
        private System.Windows.Forms.GroupBox gbControl;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.ProgressBar progressVideo;
        private System.Windows.Forms.TrackBar trackBarVideo;
        private System.Windows.Forms.Label lblVideoTime;
        
        // GroupBox 6: Export & Reports
        private System.Windows.Forms.GroupBox gbExport;
        private System.Windows.Forms.TextBox txtExportPath;
        private System.Windows.Forms.Button btnBrowseExport;
        private System.Windows.Forms.Button btnGenerateCharts;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.Button btnExportJSON;
        private System.Windows.Forms.Button btnAdminDashboard;
        
        // GroupBox 7: Schedule
        private System.Windows.Forms.GroupBox gbSchedule;
        private System.Windows.Forms.CheckBox chkEnableSchedule;
        private System.Windows.Forms.DateTimePicker dtpScheduleTime;
        private System.Windows.Forms.Button btnSetSchedule;
        private System.Windows.Forms.Label lblScheduleStatus;
        
        // GroupBox 8: Parking Management
        private System.Windows.Forms.GroupBox gbParking;
        private System.Windows.Forms.Button btnStartParking;
        private System.Windows.Forms.Button btnStopParking;
        private System.Windows.Forms.Button btnViewParkingReport;
        private System.Windows.Forms.Label lblParkingStatus;
        private System.Windows.Forms.Label lblCurrentVehicles;
        
        // Timers
        private System.Windows.Forms.Timer timerSchedule;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            
            // Dispose database resources
            _repository?.Dispose();
            
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();
            
            // Initialize MenuStrip
            InitializeMenuStrip();

            // ==================== COLORS & CONSTANTS ====================
            var colorBackground = System.Drawing.Color.FromArgb(18, 18, 18);
            var colorPanel = System.Drawing.Color.FromArgb(30, 30, 30);
            var colorCard = System.Drawing.Color.FromArgb(40, 40, 40);
            var colorAccent = System.Drawing.Color.FromArgb(0, 212, 170); // #00D4AA
            var colorText = System.Drawing.Color.FromArgb(255, 255, 255);
            var colorTextSecondary = System.Drawing.Color.FromArgb(189, 193, 198);
            var colorBorder = System.Drawing.Color.FromArgb(51, 51, 51);
            var colorSuccess = System.Drawing.Color.FromArgb(76, 175, 80);
            var colorWarning = System.Drawing.Color.FromArgb(255, 152, 0);
            var colorDanger = System.Drawing.Color.FromArgb(244, 67, 54);
            
            var fontTitle = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            var fontNormal = new System.Drawing.Font("Segoe UI", 9.5F);
            var fontButton = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            var fontClock = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Regular);
            var fontClockDate = new System.Drawing.Font("Segoe UI", 12F);

            // ==================== MAIN FORM ====================
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 1000);
            this.MinimumSize = new System.Drawing.Size(1200, 700);
            this.BackColor = colorBackground;
            this.Text = "Traffic Monitor AI v2.0 - Professional Edition";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Font = fontNormal;

            // ==================== SPLIT CONTAINER ====================
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.BackColor = colorBackground;
            this.splitContainer.SplitterWidth = 6;
            this.splitContainer.Panel1MinSize = 100;
            this.splitContainer.Panel2MinSize = 100;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.IsSplitterFixed = false;
            // MinSize and SplitterDistance will be adjusted after form loads

            // ==================== LEFT PANEL (Controls) ====================
            this.flowPanelLeft = new System.Windows.Forms.FlowLayoutPanel();
            this.flowPanelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPanelLeft.AutoScroll = true;
            this.flowPanelLeft.BackColor = colorPanel;
            this.flowPanelLeft.Padding = new System.Windows.Forms.Padding(10);
            this.flowPanelLeft.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPanelLeft.WrapContents = false;

            // ==================== GROUP 1: SOURCE SELECTION ====================
            this.gbSource = new System.Windows.Forms.GroupBox();
            this.gbSource.Text = "📥 NGUỒN DỮ LIỆU";
            this.gbSource.ForeColor = colorAccent;
            this.gbSource.Font = fontTitle;
            this.gbSource.Size = new System.Drawing.Size(550, 180);
            this.gbSource.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbSource.AutoSize = true;
            this.gbSource.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            
            this.rbVideo = new System.Windows.Forms.RadioButton();
            this.rbVideo.Text = "📹 Video";
            this.rbVideo.ForeColor = colorText;
            this.rbVideo.Font = fontNormal;
            this.rbVideo.Location = new System.Drawing.Point(15, 30);
            this.rbVideo.Size = new System.Drawing.Size(100, 35);
            this.rbVideo.Checked = true;
            this.rbVideo.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbVideo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbVideo.BackColor = colorCard;
            this.rbVideo.CheckedChanged += new System.EventHandler(this.rbInput_CheckedChanged);
            
            this.rbCamera = new System.Windows.Forms.RadioButton();
            this.rbCamera.Text = "📷 Camera";
            this.rbCamera.ForeColor = colorText;
            this.rbCamera.Font = fontNormal;
            this.rbCamera.Location = new System.Drawing.Point(125, 30);
            this.rbCamera.Size = new System.Drawing.Size(100, 35);
            this.rbCamera.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbCamera.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbCamera.BackColor = colorCard;
            this.rbCamera.CheckedChanged += new System.EventHandler(this.rbInput_CheckedChanged);
            
            this.rbImage = new System.Windows.Forms.RadioButton();
            this.rbImage.Text = "🖼️ Ảnh";
            this.rbImage.ForeColor = colorText;
            this.rbImage.Font = fontNormal;
            this.rbImage.Location = new System.Drawing.Point(235, 30);
            this.rbImage.Size = new System.Drawing.Size(100, 35);
            this.rbImage.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbImage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbImage.BackColor = colorCard;
            this.rbImage.CheckedChanged += new System.EventHandler(this.rbInput_CheckedChanged);
            
            var lblDataPath = new System.Windows.Forms.Label();
            lblDataPath.Text = "Đường dẫn:";
            lblDataPath.ForeColor = colorTextSecondary;
            lblDataPath.Font = fontNormal;
            lblDataPath.Location = new System.Drawing.Point(15, 75);
            lblDataPath.Size = new System.Drawing.Size(85, 25);
            lblDataPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            
            this.txtDataPath = new System.Windows.Forms.TextBox();
            this.txtDataPath.Location = new System.Drawing.Point(15, 100);
            this.txtDataPath.Size = new System.Drawing.Size(410, 30);
            this.txtDataPath.BackColor = colorCard;
            this.txtDataPath.ForeColor = colorText;
            this.txtDataPath.Font = fontNormal;
            this.txtDataPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDataPath.ReadOnly = true;
            
            this.btnBrowseData = new System.Windows.Forms.Button();
            this.btnBrowseData.Text = "📁";
            this.btnBrowseData.Location = new System.Drawing.Point(435, 100);
            this.btnBrowseData.Size = new System.Drawing.Size(90, 30);
            this.btnBrowseData.BackColor = colorCard;
            this.btnBrowseData.ForeColor = colorText;
            this.btnBrowseData.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseData.Font = fontButton;
            this.btnBrowseData.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseData.Click += new System.EventHandler(this.btnBrowseData_Click);
            
            this.btnLoadData = new System.Windows.Forms.Button();
            this.btnLoadData.Text = "✓ Tải Dữ Liệu";
            this.btnLoadData.Location = new System.Drawing.Point(15, 140);
            this.btnLoadData.Size = new System.Drawing.Size(510, 35);
            this.btnLoadData.BackColor = colorSuccess;
            this.btnLoadData.ForeColor = System.Drawing.Color.White;
            this.btnLoadData.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoadData.Font = fontButton;
            this.btnLoadData.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLoadData.Click += new System.EventHandler(this.btnLoadData_Click);
            
            this.gbSource.Controls.Add(this.rbVideo);
            this.gbSource.Controls.Add(this.rbCamera);
            this.gbSource.Controls.Add(this.rbImage);
            this.gbSource.Controls.Add(lblDataPath);
            this.gbSource.Controls.Add(this.txtDataPath);
            this.gbSource.Controls.Add(this.btnBrowseData);
            this.gbSource.Controls.Add(this.btnLoadData);

            // ==================== GROUP 2: MODEL AI ====================
            this.gbModel = new System.Windows.Forms.GroupBox();
            this.gbModel.Text = "🤖 MODEL AI";
            this.gbModel.ForeColor = colorAccent;
            this.gbModel.Font = fontTitle;
            this.gbModel.Size = new System.Drawing.Size(550, 140);
            this.gbModel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbModel.AutoSize = true;
            this.gbModel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            
            var lblModel = new System.Windows.Forms.Label();
            lblModel.Text = "Model Path (.onnx, .pt):";
            lblModel.ForeColor = colorTextSecondary;
            lblModel.Font = fontNormal;
            lblModel.Location = new System.Drawing.Point(15, 30);
            lblModel.Size = new System.Drawing.Size(510, 20);
            
            this.txtModelPath = new System.Windows.Forms.TextBox();
            this.txtModelPath.Location = new System.Drawing.Point(15, 55);
            this.txtModelPath.Size = new System.Drawing.Size(390, 28);
            this.txtModelPath.BackColor = colorCard;
            this.txtModelPath.ForeColor = colorText;
            this.txtModelPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            
            this.btnBrowseModel = new System.Windows.Forms.Button();
            this.btnBrowseModel.Text = "📁";
            this.btnBrowseModel.Location = new System.Drawing.Point(415, 55);
            this.btnBrowseModel.Size = new System.Drawing.Size(110, 28);
            this.btnBrowseModel.BackColor = colorCard;
            this.btnBrowseModel.ForeColor = colorText;
            this.btnBrowseModel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseModel.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnBrowseModel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseModel.Click += new System.EventHandler(this.btnBrowseModel_Click);
            
            this.btnLoadModel = new System.Windows.Forms.Button();
            this.btnLoadModel.Text = "✓ TẢI MODEL AI";
            this.btnLoadModel.Location = new System.Drawing.Point(15, 95);
            this.btnLoadModel.Size = new System.Drawing.Size(510, 35);
            this.btnLoadModel.BackColor = colorSuccess;
            this.btnLoadModel.ForeColor = System.Drawing.Color.Black;
            this.btnLoadModel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoadModel.Font = fontButton;
            this.btnLoadModel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLoadModel.Click += new System.EventHandler(this.btnLoadModel_Click);
            
            this.gbModel.Controls.Add(lblModel);
            this.gbModel.Controls.Add(this.txtModelPath);
            this.gbModel.Controls.Add(this.btnBrowseModel);
            this.gbModel.Controls.Add(this.btnLoadModel);

            // ==================== GROUP 3: PROCESSING CONFIG ====================
            this.gbConfig = new System.Windows.Forms.GroupBox();
            this.gbConfig.Text = "⚙️ CẤU HÌNH XỬ LÝ";
            this.gbConfig.ForeColor = colorAccent;
            this.gbConfig.Font = fontTitle;
            this.gbConfig.Size = new System.Drawing.Size(550, 245);
            this.gbConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbConfig.AutoSize = true;
            this.gbConfig.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            
            // Confidence Threshold
            this.lblConfidence = new System.Windows.Forms.Label();
            this.lblConfidence.Text = "Confidence: 0.25";
            this.lblConfidence.ForeColor = colorTextSecondary;
            this.lblConfidence.Font = fontNormal;
            this.lblConfidence.Location = new System.Drawing.Point(15, 30);
            this.lblConfidence.Size = new System.Drawing.Size(400, 20);
            
            this.numConfidence = new System.Windows.Forms.NumericUpDown();
            this.numConfidence.Location = new System.Drawing.Point(420, 28);
            this.numConfidence.Size = new System.Drawing.Size(105, 26);
            this.numConfidence.Minimum = 0;
            this.numConfidence.Maximum = 100;
            this.numConfidence.Value = 25;
            this.numConfidence.DecimalPlaces = 0;
            this.numConfidence.BackColor = colorCard;
            this.numConfidence.ForeColor = colorText;
            this.numConfidence.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            
            this.trackConfidence = new System.Windows.Forms.TrackBar();
            this.trackConfidence.Location = new System.Drawing.Point(15, 55);
            this.trackConfidence.Size = new System.Drawing.Size(510, 45);
            this.trackConfidence.Minimum = 0;
            this.trackConfidence.Maximum = 100;
            this.trackConfidence.Value = 25;
            this.trackConfidence.TickFrequency = 10;
            this.trackConfidence.ValueChanged += new System.EventHandler(this.trackConfidence_ValueChanged);
            
            // IOU Threshold
            this.lblIOU = new System.Windows.Forms.Label();
            this.lblIOU.Text = "IOU: 0.45";
            this.lblIOU.ForeColor = colorTextSecondary;
            this.lblIOU.Font = fontNormal;
            this.lblIOU.Location = new System.Drawing.Point(15, 105);
            this.lblIOU.Size = new System.Drawing.Size(400, 20);
            
            this.numIOU = new System.Windows.Forms.NumericUpDown();
            this.numIOU.Location = new System.Drawing.Point(420, 103);
            this.numIOU.Size = new System.Drawing.Size(105, 26);
            this.numIOU.Minimum = 0;
            this.numIOU.Maximum = 100;
            this.numIOU.Value = 45;
            this.numIOU.DecimalPlaces = 0;
            this.numIOU.BackColor = colorCard;
            this.numIOU.ForeColor = colorText;
            this.numIOU.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            
            this.trackIOU = new System.Windows.Forms.TrackBar();
            this.trackIOU.Location = new System.Drawing.Point(15, 130);
            this.trackIOU.Size = new System.Drawing.Size(510, 45);
            this.trackIOU.Minimum = 0;
            this.trackIOU.Maximum = 100;
            this.trackIOU.Value = 45;
            this.trackIOU.TickFrequency = 10;
            this.trackIOU.ValueChanged += new System.EventHandler(this.trackIOU_ValueChanged);
            
            // Skip Frames
            this.lblSkipFrames = new System.Windows.Forms.Label();
            this.lblSkipFrames.Text = "Skip Frames:";
            this.lblSkipFrames.ForeColor = colorTextSecondary;
            this.lblSkipFrames.Font = fontNormal;
            this.lblSkipFrames.Location = new System.Drawing.Point(15, 180);
            this.lblSkipFrames.Size = new System.Drawing.Size(200, 20);
            
            this.numSkipFrames = new System.Windows.Forms.NumericUpDown();
            this.numSkipFrames.Location = new System.Drawing.Point(220, 178);
            this.numSkipFrames.Size = new System.Drawing.Size(105, 26);
            this.numSkipFrames.Minimum = 0;
            this.numSkipFrames.Maximum = 30;
            this.numSkipFrames.Value = 2;
            this.numSkipFrames.BackColor = colorCard;
            this.numSkipFrames.ForeColor = colorText;
            this.numSkipFrames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            
            // Camera Index (hidden by default)
            this.numCamera = new System.Windows.Forms.NumericUpDown();
            this.numCamera.Location = new System.Drawing.Point(220, 208);
            this.numCamera.Size = new System.Drawing.Size(105, 26);
            this.numCamera.Minimum = 0;
            this.numCamera.Maximum = 10;
            this.numCamera.Value = 0;
            this.numCamera.BackColor = colorCard;
            this.numCamera.ForeColor = colorText;
            this.numCamera.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numCamera.Visible = false;
            
            this.gbConfig.Controls.Add(this.lblConfidence);
            this.gbConfig.Controls.Add(this.numConfidence);
            this.gbConfig.Controls.Add(this.trackConfidence);
            this.gbConfig.Controls.Add(this.lblIOU);
            this.gbConfig.Controls.Add(this.numIOU);
            this.gbConfig.Controls.Add(this.trackIOU);
            this.gbConfig.Controls.Add(this.lblSkipFrames);
            this.gbConfig.Controls.Add(this.numSkipFrames);
            this.gbConfig.Controls.Add(this.numCamera);

            // ==================== GROUP 4: DETECTION ZONE ====================
            this.gbZone = new System.Windows.Forms.GroupBox();
            this.gbZone.Text = "🎯 VÙNG PHÁT HIỆN";
            this.gbZone.ForeColor = colorAccent;
            this.gbZone.Font = fontTitle;
            this.gbZone.Size = new System.Drawing.Size(550, 170);
            this.gbZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbZone.AutoSize = true;
            this.gbZone.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            
            this.chkUseCountingLine = new System.Windows.Forms.CheckBox();
            this.chkUseCountingLine.Text = "✓ Sử dụng Đường Đếm (Line Counter)";
            this.chkUseCountingLine.ForeColor = colorText;
            this.chkUseCountingLine.Font = fontNormal;
            this.chkUseCountingLine.Location = new System.Drawing.Point(15, 30);
            this.chkUseCountingLine.Size = new System.Drawing.Size(510, 25);
            this.chkUseCountingLine.CheckedChanged += new System.EventHandler(this.chkUseCountingLine_CheckedChanged);
            
            this.btnSelectZone = new System.Windows.Forms.Button();
            this.btnSelectZone.Text = "🔶 Chọn Vùng Polygon";
            this.btnSelectZone.Location = new System.Drawing.Point(15, 65);
            this.btnSelectZone.Size = new System.Drawing.Size(245, 38);
            this.btnSelectZone.BackColor = colorWarning;
            this.btnSelectZone.ForeColor = System.Drawing.Color.Black;
            this.btnSelectZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectZone.Font = fontNormal;
            this.btnSelectZone.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSelectZone.Click += new System.EventHandler(this.btnSelectZone_Click);
            
            this.btnSelectLine = new System.Windows.Forms.Button();
            this.btnSelectLine.Text = "📏 Chọn Đường Đếm";
            this.btnSelectLine.Location = new System.Drawing.Point(270, 65);
            this.btnSelectLine.Size = new System.Drawing.Size(255, 38);
            this.btnSelectLine.BackColor = colorDanger;
            this.btnSelectLine.ForeColor = System.Drawing.Color.White;
            this.btnSelectLine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectLine.Font = fontNormal;
            this.btnSelectLine.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSelectLine.Visible = false;
            this.btnSelectLine.Click += new System.EventHandler(this.btnSelectLine_Click);
            
            this.btnShowZone = new System.Windows.Forms.Button();
            this.btnShowZone.Text = "👁️ Hiển Thị";
            this.btnShowZone.Location = new System.Drawing.Point(15, 115);
            this.btnShowZone.Size = new System.Drawing.Size(245, 38);
            this.btnShowZone.BackColor = colorCard;
            this.btnShowZone.ForeColor = colorText;
            this.btnShowZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShowZone.Font = fontNormal;
            this.btnShowZone.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShowZone.Click += new System.EventHandler(this.btnShowZone_Click);
            
            this.btnResetZone = new System.Windows.Forms.Button();
            this.btnResetZone.Text = "🔄 Reset";
            this.btnResetZone.Location = new System.Drawing.Point(270, 115);
            this.btnResetZone.Size = new System.Drawing.Size(255, 38);
            this.btnResetZone.BackColor = colorCard;
            this.btnResetZone.ForeColor = colorText;
            this.btnResetZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetZone.Font = fontNormal;
            this.btnResetZone.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnResetZone.Click += new System.EventHandler(this.btnResetZone_Click);
            
            this.gbZone.Controls.Add(this.chkUseCountingLine);
            this.gbZone.Controls.Add(this.btnSelectZone);
            this.gbZone.Controls.Add(this.btnSelectLine);
            this.gbZone.Controls.Add(this.btnShowZone);
            this.gbZone.Controls.Add(this.btnResetZone);

            // ==================== GROUP 5: MAIN CONTROLS ====================
            this.gbControl = new System.Windows.Forms.GroupBox();
            this.gbControl.Text = "🎮 ĐIỀU KHIỂN";
            this.gbControl.ForeColor = colorAccent;
            this.gbControl.Font = fontTitle;
            this.gbControl.Size = new System.Drawing.Size(550, 175);
            this.gbControl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbControl.AutoSize = true;
            this.gbControl.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStart.Text = "▶️ BẮT ĐẦU";
            this.btnStart.Location = new System.Drawing.Point(15, 30);
            this.btnStart.Size = new System.Drawing.Size(160, 45);
            this.btnStart.BackColor = colorSuccess;
            this.btnStart.ForeColor = System.Drawing.Color.Black;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Font = fontButton;
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            
            this.btnPause = new System.Windows.Forms.Button();
            this.btnPause.Text = "⏸️ TẠM DỪNG";
            this.btnPause.Location = new System.Drawing.Point(185, 30);
            this.btnPause.Size = new System.Drawing.Size(160, 45);
            this.btnPause.BackColor = colorWarning;
            this.btnPause.ForeColor = System.Drawing.Color.Black;
            this.btnPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPause.Font = fontNormal;
            this.btnPause.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStop.Text = "⏹️ DỪNG";
            this.btnStop.Location = new System.Drawing.Point(355, 30);
            this.btnStop.Size = new System.Drawing.Size(170, 45);
            this.btnStop.BackColor = colorDanger;
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = fontNormal;
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            
            this.progressVideo = new System.Windows.Forms.ProgressBar();
            this.progressVideo.Location = new System.Drawing.Point(15, 85);
            this.progressVideo.Size = new System.Drawing.Size(510, 10);
            this.progressVideo.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            
            this.trackBarVideo = new System.Windows.Forms.TrackBar();
            this.trackBarVideo.Location = new System.Drawing.Point(15, 100);
            this.trackBarVideo.Size = new System.Drawing.Size(510, 45);
            this.trackBarVideo.Minimum = 0;
            this.trackBarVideo.Maximum = 100;
            this.trackBarVideo.TickFrequency = 10;
            this.trackBarVideo.Enabled = false;
            this.trackBarVideo.Scroll += new System.EventHandler(this.trackBarVideo_Scroll);
            
            this.lblVideoTime = new System.Windows.Forms.Label();
            this.lblVideoTime.Text = "00:00 / 00:00";
            this.lblVideoTime.ForeColor = colorTextSecondary;
            this.lblVideoTime.Font = fontNormal;
            this.lblVideoTime.Location = new System.Drawing.Point(15, 145);
            this.lblVideoTime.Size = new System.Drawing.Size(510, 20);
            this.lblVideoTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            
            this.gbControl.Controls.Add(this.btnStart);
            this.gbControl.Controls.Add(this.btnPause);
            this.gbControl.Controls.Add(this.btnStop);
            this.gbControl.Controls.Add(this.progressVideo);
            this.gbControl.Controls.Add(this.trackBarVideo);
            this.gbControl.Controls.Add(this.lblVideoTime);

            // ==================== GROUP 6: EXPORT & REPORTS ====================
            this.gbExport = new System.Windows.Forms.GroupBox();
            this.gbExport.Text = "📊 XUẤT BÁO CÁO & BIỂU ĐỒ";
            this.gbExport.ForeColor = colorAccent;
            this.gbExport.Font = fontTitle;
            this.gbExport.Size = new System.Drawing.Size(550, 200);
            this.gbExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbExport.AutoSize = true;
            this.gbExport.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            
            var lblExport = new System.Windows.Forms.Label();
            lblExport.Text = "Thư mục xuất:";
            lblExport.ForeColor = colorTextSecondary;
            lblExport.Font = fontNormal;
            lblExport.Location = new System.Drawing.Point(15, 30);
            lblExport.Size = new System.Drawing.Size(510, 20);
            
            this.txtExportPath = new System.Windows.Forms.TextBox();
            this.txtExportPath.Location = new System.Drawing.Point(15, 55);
            this.txtExportPath.Size = new System.Drawing.Size(415, 28);
            this.txtExportPath.BackColor = colorCard;
            this.txtExportPath.ForeColor = colorText;
            this.txtExportPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            
            this.btnBrowseExport = new System.Windows.Forms.Button();
            this.btnBrowseExport.Text = "📁";
            this.btnBrowseExport.Location = new System.Drawing.Point(440, 55);
            this.btnBrowseExport.Size = new System.Drawing.Size(85, 28);
            this.btnBrowseExport.BackColor = colorCard;
            this.btnBrowseExport.ForeColor = colorText;
            this.btnBrowseExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseExport.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnBrowseExport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseExport.Click += new System.EventHandler(this.btnBrowseExport_Click);
            
            this.btnGenerateCharts = new System.Windows.Forms.Button();
            this.btnGenerateCharts.Text = "📈 Tạo Biểu Đồ";
            this.btnGenerateCharts.Location = new System.Drawing.Point(15, 95);
            this.btnGenerateCharts.Size = new System.Drawing.Size(170, 35);
            this.btnGenerateCharts.BackColor = colorCard;
            this.btnGenerateCharts.ForeColor = colorText;
            this.btnGenerateCharts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerateCharts.Font = fontNormal;
            this.btnGenerateCharts.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGenerateCharts.Click += new System.EventHandler(this.btnGenerateCharts_Click);
            
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.btnExportExcel.Text = "📑 Excel";
            this.btnExportExcel.Location = new System.Drawing.Point(195, 95);
            this.btnExportExcel.Size = new System.Drawing.Size(165, 35);
            this.btnExportExcel.BackColor = colorCard;
            this.btnExportExcel.ForeColor = colorText;
            this.btnExportExcel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportExcel.Font = fontNormal;
            this.btnExportExcel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            
            this.btnExportJSON = new System.Windows.Forms.Button();
            this.btnExportJSON.Text = "📄 JSON";
            this.btnExportJSON.Location = new System.Drawing.Point(370, 95);
            this.btnExportJSON.Size = new System.Drawing.Size(155, 35);
            this.btnExportJSON.BackColor = colorCard;
            this.btnExportJSON.ForeColor = colorText;
            this.btnExportJSON.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportJSON.Font = fontNormal;
            this.btnExportJSON.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportJSON.Click += new System.EventHandler(this.btnExportJSON_Click);
            
            this.btnAdminDashboard = new System.Windows.Forms.Button();
            this.btnAdminDashboard.Text = "📊 Quản Lý Hệ Thống (Admin)";
            this.btnAdminDashboard.Location = new System.Drawing.Point(15, 140);
            this.btnAdminDashboard.Size = new System.Drawing.Size(510, 35);
            this.btnAdminDashboard.BackColor = System.Drawing.Color.FromArgb(230, 126, 34);
            this.btnAdminDashboard.ForeColor = System.Drawing.Color.White;
            this.btnAdminDashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdminDashboard.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAdminDashboard.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAdminDashboard.Visible = false; // Ẩn mặc định, chỉ hiện với Admin
            this.btnAdminDashboard.Click += new System.EventHandler(this.btnAdminDashboard_Click);
            
            this.gbExport.Controls.Add(lblExport);
            this.gbExport.Controls.Add(this.txtExportPath);
            this.gbExport.Controls.Add(this.btnBrowseExport);
            this.gbExport.Controls.Add(this.btnGenerateCharts);
            this.gbExport.Controls.Add(this.btnExportExcel);
            this.gbExport.Controls.Add(this.btnExportJSON);
            this.gbExport.Controls.Add(this.btnAdminDashboard);

            // ==================== GROUP 7: SCHEDULE ====================
            this.gbSchedule = new System.Windows.Forms.GroupBox();
            this.gbSchedule.Text = "⏰ HẸN GIỜ TỰ ĐỘNG";
            this.gbSchedule.ForeColor = colorAccent;
            this.gbSchedule.Font = fontTitle;
            this.gbSchedule.Size = new System.Drawing.Size(550, 165);
            this.gbSchedule.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbSchedule.AutoSize = true;
            this.gbSchedule.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            
            this.chkEnableSchedule = new System.Windows.Forms.CheckBox();
            this.chkEnableSchedule.Text = "✓ Bật Hẹn Giờ";
            this.chkEnableSchedule.ForeColor = colorText;
            this.chkEnableSchedule.Font = fontNormal;
            this.chkEnableSchedule.Location = new System.Drawing.Point(15, 30);
            this.chkEnableSchedule.Size = new System.Drawing.Size(150, 25);
            this.chkEnableSchedule.CheckedChanged += new System.EventHandler(this.chkEnableSchedule_CheckedChanged);
            
            this.dtpScheduleTime = new System.Windows.Forms.DateTimePicker();
            this.dtpScheduleTime.Location = new System.Drawing.Point(15, 60);
            this.dtpScheduleTime.Size = new System.Drawing.Size(340, 28);
            this.dtpScheduleTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpScheduleTime.CustomFormat = "dd/MM/yyyy HH:mm:ss";
            this.dtpScheduleTime.BackColor = colorCard;
            this.dtpScheduleTime.ForeColor = colorText;
            
            this.btnSetSchedule = new System.Windows.Forms.Button();
            this.btnSetSchedule.Text = "⏱️ Đặt Lịch";
            this.btnSetSchedule.Location = new System.Drawing.Point(365, 60);
            this.btnSetSchedule.Size = new System.Drawing.Size(160, 28);
            this.btnSetSchedule.BackColor = colorCard;
            this.btnSetSchedule.ForeColor = colorText;
            this.btnSetSchedule.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetSchedule.Font = fontNormal;
            this.btnSetSchedule.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSetSchedule.Click += new System.EventHandler(this.btnSetSchedule_Click);
            
            this.lblScheduleStatus = new System.Windows.Forms.Label();
            this.lblScheduleStatus.Text = "Chưa đặt lịch";
            this.lblScheduleStatus.ForeColor = colorTextSecondary;
            this.lblScheduleStatus.Font = fontNormal;
            this.lblScheduleStatus.Location = new System.Drawing.Point(15, 100);
            this.lblScheduleStatus.Size = new System.Drawing.Size(510, 50);
            this.lblScheduleStatus.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            
            this.gbSchedule.Controls.Add(this.chkEnableSchedule);
            this.gbSchedule.Controls.Add(this.dtpScheduleTime);
            this.gbSchedule.Controls.Add(this.btnSetSchedule);
            this.gbSchedule.Controls.Add(this.lblScheduleStatus);

            // ==================== GROUP 8: PARKING MANAGEMENT ====================
            this.gbParking = new System.Windows.Forms.GroupBox();
            this.gbParking.Text = "🏍️ QUẢN LÝ BÃI XE";
            this.gbParking.ForeColor = colorAccent;
            this.gbParking.Font = fontTitle;
            this.gbParking.Size = new System.Drawing.Size(550, 190);
            this.gbParking.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbParking.AutoSize = true;
            this.gbParking.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            
            this.btnStartParking = new System.Windows.Forms.Button();
            this.btnStartParking.Text = "▶️ Bắt Đầu Quản Lý";
            this.btnStartParking.Location = new System.Drawing.Point(15, 30);
            this.btnStartParking.Size = new System.Drawing.Size(245, 38);
            this.btnStartParking.BackColor = colorSuccess;
            this.btnStartParking.ForeColor = System.Drawing.Color.White;
            this.btnStartParking.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartParking.Font = fontButton;
            this.btnStartParking.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStartParking.Click += new System.EventHandler(this.btnStartParking_Click);
            
            this.btnStopParking = new System.Windows.Forms.Button();
            this.btnStopParking.Text = "⏹️ Dừng Quản Lý";
            this.btnStopParking.Location = new System.Drawing.Point(270, 30);
            this.btnStopParking.Size = new System.Drawing.Size(255, 38);
            this.btnStopParking.BackColor = colorDanger;
            this.btnStopParking.ForeColor = System.Drawing.Color.White;
            this.btnStopParking.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopParking.Font = fontButton;
            this.btnStopParking.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStopParking.Enabled = false;
            this.btnStopParking.Click += new System.EventHandler(this.btnStopParking_Click);
            
            this.btnViewParkingReport = new System.Windows.Forms.Button();
            this.btnViewParkingReport.Text = "📊 Xem Báo Cáo";
            this.btnViewParkingReport.Location = new System.Drawing.Point(15, 80);
            this.btnViewParkingReport.Size = new System.Drawing.Size(510, 35);
            this.btnViewParkingReport.BackColor = colorCard;
            this.btnViewParkingReport.ForeColor = colorText;
            this.btnViewParkingReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewParkingReport.Font = fontNormal;
            this.btnViewParkingReport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewParkingReport.Click += new System.EventHandler(this.btnViewParkingReport_Click);
            
            this.lblParkingStatus = new System.Windows.Forms.Label();
            this.lblParkingStatus.Text = "🚫 Chế độ bãi xe: Tắt";
            this.lblParkingStatus.ForeColor = colorTextSecondary;
            this.lblParkingStatus.Font = fontNormal;
            this.lblParkingStatus.Location = new System.Drawing.Point(15, 125);
            this.lblParkingStatus.Size = new System.Drawing.Size(510, 22);
            this.lblParkingStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            
            this.lblCurrentVehicles = new System.Windows.Forms.Label();
            this.lblCurrentVehicles.Text = "Xe trong bãi: 0 | Tổng vào: 0";
            this.lblCurrentVehicles.ForeColor = colorAccent;
            this.lblCurrentVehicles.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCurrentVehicles.Location = new System.Drawing.Point(15, 155);
            this.lblCurrentVehicles.Size = new System.Drawing.Size(510, 25);
            this.lblCurrentVehicles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            
            this.gbParking.Controls.Add(this.btnStartParking);
            this.gbParking.Controls.Add(this.btnStopParking);
            this.gbParking.Controls.Add(this.btnViewParkingReport);
            this.gbParking.Controls.Add(this.lblParkingStatus);
            this.gbParking.Controls.Add(this.lblCurrentVehicles);

            // Add all GroupBoxes to FlowPanel
            this.flowPanelLeft.Controls.Add(this.gbSource);
            this.flowPanelLeft.Controls.Add(this.gbModel);
            this.flowPanelLeft.Controls.Add(this.gbConfig);
            this.flowPanelLeft.Controls.Add(this.gbZone);
            this.flowPanelLeft.Controls.Add(this.gbControl);
            this.flowPanelLeft.Controls.Add(this.gbExport);
            this.flowPanelLeft.Controls.Add(this.gbParking);
            this.flowPanelLeft.Controls.Add(this.gbSchedule);

            // ==================== RIGHT PANEL (Video Display) ====================
            this.panelRight = new System.Windows.Forms.Panel();
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.BackColor = System.Drawing.Color.FromArgb(18, 18, 18);
            this.panelRight.Padding = new System.Windows.Forms.Padding(10);

            // Header Panel with App Title
            var panelHeader = new System.Windows.Forms.Panel();
            panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            panelHeader.Height = 60;
            panelHeader.BackColor = colorCard;
            panelHeader.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);

            var lblAppTitle = new System.Windows.Forms.Label();
            lblAppTitle.Text = "🚦 TRAFFIC MONITOR AI v2.0";
            lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            lblAppTitle.ForeColor = colorAccent;
            lblAppTitle.Dock = System.Windows.Forms.DockStyle.Left;
            lblAppTitle.AutoSize = true;
            lblAppTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            var lblAppSubtitle = new System.Windows.Forms.Label();
            lblAppSubtitle.Text = "Giám Sát Giao Thông Thông Minh";
            lblAppSubtitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblAppSubtitle.ForeColor = colorTextSecondary;
            lblAppSubtitle.Dock = System.Windows.Forms.DockStyle.Right;
            lblAppSubtitle.AutoSize = true;
            lblAppSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            panelHeader.Controls.Add(lblAppTitle);
            panelHeader.Controls.Add(lblAppSubtitle);

            // Main PictureBox
            this.pbVideo = new System.Windows.Forms.PictureBox();
            this.pbVideo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbVideo.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.pbVideo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbVideo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbVideo.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseClick);
            this.pbVideo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
            this.pbVideo.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBox_Paint);

            // Status Label - below video
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblStatus.Text = "⚡ Sẵn sàng";
            this.lblStatus.ForeColor = colorAccent;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblStatus.Height = 35;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStatus.BackColor = colorCard;

            // Statistics Panel - below status
            var panelStats = new System.Windows.Forms.Panel();
            panelStats.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelStats.Height = 90;
            panelStats.BackColor = colorPanel;
            panelStats.Padding = new System.Windows.Forms.Padding(3);

            // Statistics Cards (5 cards in a row)
            this.tableStats = new System.Windows.Forms.TableLayoutPanel();
            this.tableStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableStats.ColumnCount = 5;
            this.tableStats.RowCount = 1;
            this.tableStats.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            
            for (int i = 0; i < 5; i++)
            {
                this.tableStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            }
            this.tableStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            // Card 1: Total
            this.cardTotal = CreateStatCard("🚦 TỔNG", "0", colorAccent);
            this.txtTotalCount = (System.Windows.Forms.TextBox)this.cardTotal.Controls[0];
            
            // Card 2: Cars
            this.cardCar = CreateStatCard("🚗 Ô TÔ", "0", System.Drawing.Color.FromArgb(33, 150, 243));
            this.txtCarCount = (System.Windows.Forms.TextBox)this.cardCar.Controls[0];
            
            // Card 3: Motorcycles
            this.cardMotor = CreateStatCard("🏍️ XE MÁY", "0", System.Drawing.Color.FromArgb(156, 39, 176));
            this.txtMotorCount = (System.Windows.Forms.TextBox)this.cardMotor.Controls[0];
            
            // Card 4: Bus
            this.cardBus = CreateStatCard("🚌 BUÝT", "0", System.Drawing.Color.FromArgb(255, 193, 7));
            this.txtBusCount = (System.Windows.Forms.TextBox)this.cardBus.Controls[0];
            
            // Card 5: Bicycle + FPS
            this.cardBicycle = new System.Windows.Forms.Panel();
            this.cardBicycle.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.cardBicycle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardBicycle.Margin = new System.Windows.Forms.Padding(3);
            this.cardBicycle.Padding = new System.Windows.Forms.Padding(5);
            
            var lblBicycleTitle = new System.Windows.Forms.Label();
            lblBicycleTitle.Text = "🚲 XE ĐẠP";
            lblBicycleTitle.ForeColor = System.Drawing.Color.FromArgb(0, 200, 83);
            lblBicycleTitle.Font = new System.Drawing.Font("Segoe UI", 7F, System.Drawing.FontStyle.Bold);
            lblBicycleTitle.Dock = System.Windows.Forms.DockStyle.Top;
            lblBicycleTitle.Height = 15;
            lblBicycleTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            
            this.txtBicycleCount = new System.Windows.Forms.TextBox();
            this.txtBicycleCount.Text = "0";
            this.txtBicycleCount.ForeColor = System.Drawing.Color.White;
            this.txtBicycleCount.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.txtBicycleCount.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            this.txtBicycleCount.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtBicycleCount.Height = 38;
            this.txtBicycleCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtBicycleCount.ReadOnly = true;
            this.txtBicycleCount.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtBicycleCount.Cursor = System.Windows.Forms.Cursors.Arrow;
            
            this.lblFPS = new System.Windows.Forms.Label();
            this.lblFPS.Text = "FPS: 0";
            this.lblFPS.ForeColor = System.Drawing.Color.LightGray;
            this.lblFPS.Font = new System.Drawing.Font("Segoe UI", 7F);
            this.lblFPS.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblFPS.Height = 15;
            this.lblFPS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            
            this.cardBicycle.Controls.Add(this.txtBicycleCount);
            this.cardBicycle.Controls.Add(lblBicycleTitle);
            this.cardBicycle.Controls.Add(this.lblFPS);
            
            this.tableStats.Controls.Add(this.cardTotal, 0, 0);
            this.tableStats.Controls.Add(this.cardCar, 1, 0);
            this.tableStats.Controls.Add(this.cardMotor, 2, 0);
            this.tableStats.Controls.Add(this.cardBus, 3, 0);
            this.tableStats.Controls.Add(this.cardBicycle, 4, 0);
            
            panelStats.Controls.Add(this.tableStats);
            
            // Add to panel right
            this.panelRight.Controls.Add(this.pbVideo);
            this.panelRight.Controls.Add(this.lblStatus);
            this.panelRight.Controls.Add(panelStats);
            this.panelRight.Controls.Add(panelHeader);

            // ==================== TIMERS ====================
            this.timerSchedule = new System.Windows.Forms.Timer(this.components);
            this.timerSchedule.Interval = 1000;
            this.timerSchedule.Tick += new System.EventHandler(this.timerSchedule_Tick);

            // ==================== FINAL ASSEMBLY ====================
            this.splitContainer.Panel1.Controls.Add(this.flowPanelLeft);
            this.splitContainer.Panel2.Controls.Add(this.panelRight);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        private void InitializeMenuStrip()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            
            // File Menu
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            
            // View Menu
            this.menuView = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewStatistics = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewHistory = new System.Windows.Forms.ToolStripMenuItem();
            
            // Mode Menu
            this.menuMode = new System.Windows.Forms.ToolStripMenuItem();
            this.menuModeBasic = new System.Windows.Forms.ToolStripMenuItem();
            this.menuModeParking = new System.Windows.Forms.ToolStripMenuItem();
            
            // Data Menu
            this.menuData = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDataExport = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDataAdmin = new System.Windows.Forms.ToolStripMenuItem();
            
            // Tools Menu
            this.menuTools = new System.Windows.Forms.ToolStripMenuItem();
            this.menuToolsOptions = new System.Windows.Forms.ToolStripMenuItem();
            
            // Help Menu
            this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            
            // Thêm menu items mới cho Help
            var menuHelpGuide = new System.Windows.Forms.ToolStripMenuItem();
            var menuHelpParameterGuide = new System.Windows.Forms.ToolStripMenuItem();
            var menuHelpGPUSettings = new System.Windows.Forms.ToolStripMenuItem();
            
            // MenuStrip
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuFile,
                this.menuView,
                this.menuMode,
                this.menuData,
                this.menuTools,
                this.menuHelp
            });
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1600, 28);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.menuStrip.ForeColor = System.Drawing.Color.White;
            this.menuStrip.Font = new System.Drawing.Font("Segoe UI", 10F);
            
            // File Menu Items
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuFileOpen,
                new System.Windows.Forms.ToolStripSeparator(),
                this.menuFileExit
            });
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(80, 24);
            this.menuFile.Text = "📁 File";
            this.menuFile.ForeColor = System.Drawing.Color.White;
            
            this.menuFileOpen.Name = "menuFileOpen";
            this.menuFileOpen.Size = new System.Drawing.Size(250, 26);
            this.menuFileOpen.Text = "📂 Mở Video/Camera";
            this.menuFileOpen.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            this.menuFileOpen.Click += MenuFileOpen_Click;
            
            this.menuFileExit.Name = "menuFileExit";
            this.menuFileExit.Size = new System.Drawing.Size(250, 26);
            this.menuFileExit.Text = "❌ Thoát";
            this.menuFileExit.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4;
            this.menuFileExit.Click += (s, e) => System.Windows.Forms.Application.Exit();
            
            // View Menu Items
            this.menuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuViewSettings,
                this.menuViewStatistics,
                this.menuViewHistory
            });
            this.menuView.Name = "menuView";
            this.menuView.Size = new System.Drawing.Size(110, 24);
            this.menuView.Text = "👁️ Hiển Thị";
            this.menuView.ForeColor = System.Drawing.Color.White;
            
            this.menuViewSettings.Name = "menuViewSettings";
            this.menuViewSettings.Size = new System.Drawing.Size(200, 26);
            this.menuViewSettings.Text = "⚙️ Cài Đặt";
            this.menuViewSettings.Click += MenuViewSettings_Click;
            
            this.menuViewStatistics.Name = "menuViewStatistics";
            this.menuViewStatistics.Size = new System.Drawing.Size(200, 26);
            this.menuViewStatistics.Text = "📊 Thống Kê";
            this.menuViewStatistics.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.menuViewStatistics.Click += MenuViewStatistics_Click;
            
            this.menuViewHistory.Name = "menuViewHistory";
            this.menuViewHistory.Size = new System.Drawing.Size(200, 26);
            this.menuViewHistory.Text = "📋 Lịch Sử";
            this.menuViewHistory.ShortcutKeys = System.Windows.Forms.Keys.F4;
            this.menuViewHistory.Click += MenuViewHistory_Click;
            
            // Mode Menu Items
            this.menuMode.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuModeBasic,
                this.menuModeParking
            });
            this.menuMode.Name = "menuMode";
            this.menuMode.Size = new System.Drawing.Size(110, 24);
            this.menuMode.Text = "🎯 Chế Độ";
            this.menuMode.ForeColor = System.Drawing.Color.White;
            
            this.menuModeBasic.Name = "menuModeBasic";
            this.menuModeBasic.Size = new System.Drawing.Size(200, 26);
            this.menuModeBasic.Text = "📹 Basic Mode";
            this.menuModeBasic.Click += MenuModeBasic_Click;
            
            this.menuModeParking.Name = "menuModeParking";
            this.menuModeParking.Size = new System.Drawing.Size(200, 26);
            this.menuModeParking.Text = "🅿️ Parking Mode";
            this.menuModeParking.Click += MenuModeParking_Click;
            
            // Data Menu Items
            this.menuData.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuDataExport,
                this.menuDataAdmin
            });
            this.menuData.Name = "menuData";
            this.menuData.Size = new System.Drawing.Size(110, 24);
            this.menuData.Text = "💾 Dữ Liệu";
            this.menuData.ForeColor = System.Drawing.Color.White;
            
            this.menuDataExport.Name = "menuDataExport";
            this.menuDataExport.Size = new System.Drawing.Size(250, 26);
            this.menuDataExport.Text = "📊 Xuất Báo Cáo";
            this.menuDataExport.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
            this.menuDataExport.Click += MenuDataExport_Click;
            
            this.menuDataAdmin.Name = "menuDataAdmin";
            this.menuDataAdmin.Size = new System.Drawing.Size(250, 26);
            this.menuDataAdmin.Text = "🛡️ Admin Dashboard";
            this.menuDataAdmin.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D;
            this.menuDataAdmin.Click += MenuDataAdmin_Click;
            
            // Tools Menu Items
            this.menuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuToolsOptions
            });
            this.menuTools.Name = "menuTools";
            this.menuTools.Size = new System.Drawing.Size(110, 24);
            this.menuTools.Text = "🔧 Công Cụ";
            this.menuTools.ForeColor = System.Drawing.Color.White;
            
            this.menuToolsOptions.Name = "menuToolsOptions";
            this.menuToolsOptions.Size = new System.Drawing.Size(200, 26);
            this.menuToolsOptions.Text = "⚙️ Tùy Chọn";
            this.menuToolsOptions.Click += MenuToolsOptions_Click;
            
            // Help Menu Items - Cập nhật với 4 menu items
            this.menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                menuHelpGuide,
                menuHelpParameterGuide,
                menuHelpGPUSettings,
                new System.Windows.Forms.ToolStripSeparator(),
                this.menuHelpAbout
            });
            this.menuHelp.Name = "menuHelp";
            this.menuHelp.Size = new System.Drawing.Size(100, 24);
            this.menuHelp.Text = "❓ Trợ Giúp";
            this.menuHelp.ForeColor = System.Drawing.Color.White;
            
            menuHelpGuide.Name = "menuHelpGuide";
            menuHelpGuide.Size = new System.Drawing.Size(300, 26);
            menuHelpGuide.Text = "📖 Hướng Dẫn Sử Dụng";
            menuHelpGuide.ShortcutKeys = System.Windows.Forms.Keys.F1;
            menuHelpGuide.Click += MenuHelpGuide_Click;
            
            menuHelpParameterGuide.Name = "menuHelpParameterGuide";
            menuHelpParameterGuide.Size = new System.Drawing.Size(300, 26);
            menuHelpParameterGuide.Text = "⚙️ Hướng Dẫn Tuy Chỉnh Tham Số";
            menuHelpParameterGuide.Click += MenuHelpParameterGuide_Click;
            
            menuHelpGPUSettings.Name = "menuHelpGPUSettings";
            menuHelpGPUSettings.Size = new System.Drawing.Size(300, 26);
            menuHelpGPUSettings.Text = "🚀 Hướng Dẫn GPU/CPU Settings";
            menuHelpGPUSettings.Click += MenuHelpGPUSettings_Click;
            
            this.menuHelpAbout.Name = "menuHelpAbout";
            this.menuHelpAbout.Size = new System.Drawing.Size(300, 26);
            this.menuHelpAbout.Text = "ℹ️ Về Hệ Thống";
            this.menuHelpAbout.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.menuHelpAbout.Click += MenuHelpAbout_Click;
        }
        
        private System.Windows.Forms.Panel CreateStatCard(string title, string value, System.Drawing.Color accentColor)
        {
            var card = new System.Windows.Forms.Panel();
            card.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            card.Dock = System.Windows.Forms.DockStyle.Fill;
            card.Margin = new System.Windows.Forms.Padding(3);
            card.Padding = new System.Windows.Forms.Padding(5);
            
            var lblTitle = new System.Windows.Forms.Label();
            lblTitle.Text = title;
            lblTitle.ForeColor = accentColor;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            lblTitle.Height = 20;
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            
            var txtValue = new System.Windows.Forms.TextBox();
            txtValue.Text = value;
            txtValue.ForeColor = System.Drawing.Color.White;
            txtValue.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            txtValue.Font = new System.Drawing.Font("Segoe UI", 30F, System.Drawing.FontStyle.Bold);
            txtValue.Dock = System.Windows.Forms.DockStyle.Fill;
            txtValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            txtValue.ReadOnly = true;
            txtValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtValue.Cursor = System.Windows.Forms.Cursors.Arrow;
            txtValue.Multiline = false;
            
            card.Controls.Add(txtValue);
            card.Controls.Add(lblTitle);
            
            return card;
        }

        // Event handlers for new controls
        private void trackConfidence_ValueChanged(object sender, System.EventArgs e)
        {
            numConfidence.Value = trackConfidence.Value;
            lblConfidence.Text = $"Confidence: {trackConfidence.Value / 100.0:F2}";
        }

        private void trackIOU_ValueChanged(object sender, System.EventArgs e)
        {
            numIOU.Value = trackIOU.Value;
            lblIOU.Text = $"IOU: {trackIOU.Value / 100.0:F2}";
        }

        private void btnExportExcel_Click(object sender, System.EventArgs e)
        {
            // Will be implemented in code-behind
        }

        private void btnExportJSON_Click(object sender, System.EventArgs e)
        {
            // Will be implemented in code-behind
        }
    }
}
